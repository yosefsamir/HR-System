using HR_system.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Middleware
{
    /// <summary>
    /// Enforces authentication ONLY when at least one user account exists.
    /// If the database has no users yet, every page is freely accessible so
    /// the operator can tour the system and create the first account at their
    /// own pace via Settings → Security (or the /Account/Setup route).
    /// Once the first account is created the cookie is written and all
    /// subsequent requests require a valid session.
    /// </summary>
    public class ConditionalAuthMiddleware
    {
        private readonly RequestDelegate _next;

        // Cached flag — once we know users exist we never need to query again.
        // Volatile so it is visible across threads without a full lock.
        private static volatile bool _usersExistCache = false;

        public ConditionalAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db)
        {
            // ── 1. Always allow static files, auth pages, and API endpoints ──
            var path = context.Request.Path.Value ?? "";

            bool isExempt =
                path.StartsWith("/Account", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/css",     StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/js",      StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/lib",     StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/fonts",   StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/images",  StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase);

            if (isExempt)
            {
                await _next(context);
                return;
            }

            // ── 2. Check whether any users exist (with cache) ─────────────────
            if (!_usersExistCache)
            {
                // Re-query DB; if still empty, let the request through freely.
                _usersExistCache = await db.Users.AnyAsync();

                if (!_usersExistCache)
                {
                    // No users yet → free access; pass a flag so the layout
                    // can show a "create first account" banner.
                    context.Items["NoUsersYet"] = true;
                    await _next(context);
                    return;
                }
            }

            // ── 3. Users exist — enforce authentication ───────────────────────
            if (context.User.Identity?.IsAuthenticated != true)
            {
                var returnUrl = Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
                context.Response.Redirect($"/Account/Login?returnUrl={returnUrl}");
                return;
            }

            await _next(context);
        }

        /// <summary>
        /// Call this after the first user is successfully created so the
        /// cache is refreshed immediately without waiting for the next request.
        /// </summary>
        public static void InvalidateCache() => _usersExistCache = false;

        /// <summary>
        /// Call this after a user is created to mark the cache as populated.
        /// </summary>
        public static void MarkUsersExist() => _usersExistCache = true;
    }
}
