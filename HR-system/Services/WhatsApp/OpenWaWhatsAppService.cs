using System.Net;
using System.Text;
using System.Text.Json;
using HR_system.Middleware;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using QRCoder;

namespace HR_system.Services
{
    public class OpenWaWhatsAppService : IWhatsAppService
    {
        private const string EngineName = "OPENWA";

        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenWaWhatsAppService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl;
        private readonly string _apiKey;
        private readonly string _sessionId;
        private readonly string _sessionName;
        private readonly string _publicFileBaseUrl;
        private readonly string _webRootPath;
        private string? _resolvedSessionId;

        public OpenWaWhatsAppService(
            HttpClient httpClient,
            ILogger<OpenWaWhatsAppService> logger,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _baseUrl = (configuration["OpenWAApi:BaseUrl"] ?? "http://localhost:2785/api").TrimEnd('/');
            _apiKey = configuration["OpenWAApi:ApiKey"] ?? "dev-admin-key";
            _sessionId = configuration["OpenWAApi:SessionId"] ?? "hr-system-default";
            _sessionName = configuration["OpenWAApi:SessionName"] ?? "HR System";
            _publicFileBaseUrl = (configuration["OpenWAApi:PublicFileBaseUrl"] ?? "http://host.docker.internal:5000").TrimEnd('/');
            _webRootPath = environment.WebRootPath;
        }

        public async Task<bool> StartSessionAsync()
        {
            try
            {
                var sessionId = await EnsureSessionExistsAsync();
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return false;
                }

                for (var attempt = 1; attempt <= 2; attempt++)
                {
                    try
                    {
                        var response = await PostNoPayloadAsync($"{_baseUrl}/sessions/{sessionId}/start");

                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }

                        var body = await response.Content.ReadAsStringAsync();
                        if ((int)response.StatusCode == 400 && body.Contains("already", StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }

                        var isTransientServerError = (int)response.StatusCode >= 500;
                        if (isTransientServerError && attempt < 2)
                        {
                            _logger.LogWarning("OpenWA start session failed transiently. Attempt: {Attempt}. Status: {StatusCode}. Retrying...", attempt, (int)response.StatusCode);
                            await Task.Delay(TimeSpan.FromSeconds(3));
                            continue;
                        }

                        _logger.LogWarning("OpenWA start session failed. Status: {StatusCode}. Response: {Body}", (int)response.StatusCode, body);
                        return false;
                    }
                    catch (TaskCanceledException ex) when (attempt < 2)
                    {
                        _logger.LogWarning(ex, "OpenWA start session timed out on attempt {Attempt}. Retrying...", attempt);
                        await Task.Delay(TimeSpan.FromSeconds(3));
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting OpenWA session");
                return false;
            }
        }

        public async Task<bool> RestartSessionAsync()
        {
            try
            {
                var sessionId = await EnsureSessionExistsAsync();
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return false;
                }

                var response = await PostNoPayloadAsync($"{_baseUrl}/sessions/{sessionId}/restart");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restarting OpenWA session");
                return false;
            }
        }

        public async Task<bool> LogoutSessionAsync()
        {
            try
            {
                var sessionId = await ResolveSessionIdAsync();
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return true;
                }

                // Swagger-supported endpoint: stop first to disconnect active socket cleanly.
                var stopResponse = await PostNoPayloadAsync($"{_baseUrl}/sessions/{sessionId}/stop");
                if (!stopResponse.IsSuccessStatusCode && stopResponse.StatusCode != HttpStatusCode.NotFound)
                {
                    var stopBody = await stopResponse.Content.ReadAsStringAsync();
                    _logger.LogWarning("OpenWA stop session returned non-success status. Status: {StatusCode}. Response: {Body}", (int)stopResponse.StatusCode, stopBody);
                }

                var response = await PostNoPayloadAsync($"{_baseUrl}/sessions/{sessionId}/logout");
                if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("OpenWA logout returned non-success status. Status: {StatusCode}. Response: {Body}", (int)response.StatusCode, body);
                }

                // Always force stop + delete with keepAuth=false to avoid silent auto-reconnect.
                return await ForceDeleteSessionAsync(sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging out OpenWA session");
                return false;
            }
        }

        public async Task<string?> GetQRCodeAsync()
        {
            try
            {
                var sessionId = await ResolveSessionIdAsync();
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return null;
                }

                var request = BuildRequest(HttpMethod.Get, $"{_baseUrl}/sessions/{sessionId}/qr");
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var body = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(body))
                {
                    return null;
                }

                using var doc = JsonDocument.Parse(body);
                var payload = UnwrapData(doc.RootElement);

                if (TryGetString(payload, "qrCode", out var qrCode) || TryGetString(payload, "image", out qrCode) || TryGetString(payload, "qr", out qrCode) || TryGetString(payload, "code", out qrCode))
                {
                    if (qrCode.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
                    {
                        return qrCode;
                    }

                    return BuildQrDataUri(qrCode);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OpenWA QR code");
                return null;
            }
        }

        public async Task<WhatsAppSessionInfo?> GetSessionInfoAsync()
        {
            try
            {
                var sessionId = await ResolveSessionIdAsync();
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return CreateSessionInfo("STOPPED", _sessionId);
                }

                var request = BuildRequest(HttpMethod.Get, $"{_baseUrl}/sessions/{sessionId}");
                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return CreateSessionInfo("STOPPED", _sessionId);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return CreateSessionInfo("UNKNOWN", _sessionId);
                }

                var body = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(body);
                var payload = UnwrapData(doc.RootElement);

                var rawStatus = TryGetString(payload, "status", out var statusValue) ? statusValue : string.Empty;
                var sessionName = TryGetString(payload, "name", out var name) ? name : _sessionId;
                var phone = TryGetString(payload, "phone", out var phoneValue) ? phoneValue : null;
                var pushName = TryGetString(payload, "pushName", out var pushNameValue) ? pushNameValue : null;

                return CreateSessionInfo(MapStatus(rawStatus), sessionName, phone, pushName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OpenWA session info");
                return null;
            }
        }

        public async Task<string> GetSessionStatusAsync()
        {
            var info = await GetSessionInfoAsync();
            return info?.Status ?? "STOPPED";
        }

        public async Task<WhatsAppOperationResult> SendMessageAsync(string phoneNumber, string message)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = GetCorrelationId(),
                ["Operation"] = "SendMessage"
            });

            try
            {
                await StartSessionAsync();

                var payload = new Dictionary<string, object?>
                {
                    ["chatId"] = ToChatId(phoneNumber),
                    ["text"] = message
                };

                var sessionId = await ResolveSessionIdAsync();
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return WhatsAppOperationResult.Fail("Session not found");
                }

                var request = BuildJsonRequest(HttpMethod.Post, $"{_baseUrl}/sessions/{sessionId}/messages/send-text", payload);
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return WhatsAppOperationResult.Ok();
                }

                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("OpenWA send-text failed. Status: {StatusCode}. Response: {Body}", (int)response.StatusCode, body);
                return WhatsAppOperationResult.Fail($"send-text failed with status {(int)response.StatusCode}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp message via OpenWA");
                return WhatsAppOperationResult.Fail(ex.Message);
            }
        }

        public async Task<WhatsAppOperationResult> SendFileAsync(string phoneNumber, string fileName, string mimeType, byte[] fileContent, string? caption = null)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = GetCorrelationId(),
                ["Operation"] = "SendFile"
            });

            (string FullPath, string StoredName)? stagedFile = null;
            try
            {
                await StartSessionAsync();

                stagedFile = await StageTempFileAsync(fileName, fileContent);

                var fileUrl = $"{_publicFileBaseUrl}/wa-temp/{Uri.EscapeDataString(stagedFile.Value.StoredName)}";

                var payload = new Dictionary<string, object?>
                {
                    ["chatId"] = ToChatId(phoneNumber),
                    ["url"] = fileUrl,
                    ["filename"] = fileName,
                    ["caption"] = caption
                };

                var sessionId = await ResolveSessionIdAsync();
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    return WhatsAppOperationResult.Fail("Session not found");
                }

                var request = BuildJsonRequest(HttpMethod.Post, $"{_baseUrl}/sessions/{sessionId}/messages/send-document", payload);
                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return WhatsAppOperationResult.Ok();
                }

                // Some OpenWA builds use nested "document" schema. Retry once for compatibility.
                var body = await response.Content.ReadAsStringAsync();
                if ((int)response.StatusCode == 400 && body.Contains("property document", StringComparison.OrdinalIgnoreCase))
                {
                    var fallbackPayload = new Dictionary<string, object?>
                    {
                        ["chatId"] = ToChatId(phoneNumber),
                        ["document"] = new Dictionary<string, object?>
                        {
                            ["url"] = fileUrl,
                            ["mimetype"] = mimeType
                        },
                        ["filename"] = fileName,
                        ["caption"] = caption
                    };

                    var fallbackRequest = BuildJsonRequest(HttpMethod.Post, $"{_baseUrl}/sessions/{sessionId}/messages/send-document", fallbackPayload);
                    response = await _httpClient.SendAsync(fallbackRequest);
                    if (response.IsSuccessStatusCode)
                    {
                        return WhatsAppOperationResult.Ok();
                    }

                    body = await response.Content.ReadAsStringAsync();
                }
                _logger.LogWarning("OpenWA send-document failed. Status: {StatusCode}. Response: {Body}", (int)response.StatusCode, body);
                return WhatsAppOperationResult.Fail($"send-document failed with status {(int)response.StatusCode}", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WhatsApp document via OpenWA");
                return WhatsAppOperationResult.Fail(ex.Message);
            }
            finally
            {
                if (stagedFile.HasValue)
                {
                    await DeleteTempFileWithRetryAsync(stagedFile.Value.FullPath);
                }
            }
        }

        private async Task<(string FullPath, string StoredName)> StageTempFileAsync(string fileName, byte[] fileContent)
        {
            var folderPath = Path.Combine(_webRootPath, "wa-temp");
            Directory.CreateDirectory(folderPath);

            var extension = Path.GetExtension(fileName);
            var storedName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(folderPath, storedName);

            await File.WriteAllBytesAsync(fullPath, fileContent);
            return (fullPath, storedName);
        }

        private async Task DeleteTempFileWithRetryAsync(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return;
            }

            for (var attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }

                    return;
                }
                catch (IOException)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                catch (UnauthorizedAccessException)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
                catch
                {
                    return;
                }
            }
        }

        private async Task<string?> EnsureSessionExistsAsync()
        {
            var existingId = await ResolveSessionIdAsync();
            if (!string.IsNullOrWhiteSpace(existingId))
            {
                return existingId;
            }

            var createPayload = new
            {
                name = SanitizeSessionName(_sessionName)
            };

            var createRequest = BuildJsonRequest(HttpMethod.Post, $"{_baseUrl}/sessions", createPayload);
            var createResponse = await _httpClient.SendAsync(createRequest);
            if (!createResponse.IsSuccessStatusCode)
            {
                var body = await createResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("OpenWA create session failed. Status: {StatusCode}. Response: {Body}", (int)createResponse.StatusCode, body);
                return await ResolveSessionIdAsync();
            }

            var json = await createResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                return await ResolveSessionIdAsync();
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var payload = UnwrapData(doc.RootElement);
                if (TryGetString(payload, "id", out var createdId))
                {
                    _resolvedSessionId = createdId;
                    return createdId;
                }
            }
            catch
            {
                // Ignore response parsing and retry through list endpoint.
            }

            return await ResolveSessionIdAsync();
        }

        private async Task<string?> ResolveSessionIdAsync()
        {
            if (!string.IsNullOrWhiteSpace(_resolvedSessionId))
            {
                return _resolvedSessionId;
            }

            if (!string.IsNullOrWhiteSpace(_sessionId))
            {
                var getByIdRequest = BuildRequest(HttpMethod.Get, $"{_baseUrl}/sessions/{_sessionId}");
                var getByIdResponse = await _httpClient.SendAsync(getByIdRequest);
                if (getByIdResponse.IsSuccessStatusCode)
                {
                    _resolvedSessionId = _sessionId;
                    return _sessionId;
                }
            }

            var listRequest = BuildRequest(HttpMethod.Get, $"{_baseUrl}/sessions");
            var listResponse = await _httpClient.SendAsync(listRequest);
            if (!listResponse.IsSuccessStatusCode)
            {
                return null;
            }

            var json = await listResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                JsonElement list = root;
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Array)
                {
                    list = data;
                }

                if (list.ValueKind != JsonValueKind.Array)
                {
                    return null;
                }

                var expectedName = SanitizeSessionName(_sessionName);
                foreach (var item in list.EnumerateArray())
                {
                    if (!TryGetString(item, "id", out var id))
                    {
                        continue;
                    }

                    if (id.Equals(_sessionId, StringComparison.OrdinalIgnoreCase))
                    {
                        _resolvedSessionId = id;
                        return id;
                    }

                    if (TryGetString(item, "name", out var name) && name.Equals(expectedName, StringComparison.OrdinalIgnoreCase))
                    {
                        _resolvedSessionId = id;
                        return id;
                    }
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private HttpRequestMessage BuildRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Add("X-API-Key", _apiKey);
            return request;
        }

        private Task<HttpResponseMessage> PostNoPayloadAsync(string url)
        {
            var request = BuildJsonRequest(HttpMethod.Post, url, new { });
            return _httpClient.SendAsync(request);
        }

        private async Task<bool> ForceDeleteSessionAsync(string sessionId)
        {
            var stopResponse = await PostNoPayloadAsync($"{_baseUrl}/sessions/{sessionId}/stop");
            if (!stopResponse.IsSuccessStatusCode && stopResponse.StatusCode != HttpStatusCode.NotFound)
            {
                var stopBody = await stopResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("OpenWA stop session during forced delete failed. Status: {StatusCode}. Response: {Body}", (int)stopResponse.StatusCode, stopBody);
            }

            var deleteResponse = await _httpClient.SendAsync(BuildRequest(HttpMethod.Delete, $"{_baseUrl}/sessions/{sessionId}?keepAuth=false"));
            if (deleteResponse.IsSuccessStatusCode || deleteResponse.StatusCode == HttpStatusCode.NotFound)
            {
                _resolvedSessionId = null;
                return true;
            }

            var deleteBody = await deleteResponse.Content.ReadAsStringAsync();
            _logger.LogWarning("OpenWA forced delete failed. Status: {StatusCode}. Response: {Body}", (int)deleteResponse.StatusCode, deleteBody);
            return false;
        }

        private HttpRequestMessage BuildJsonRequest(HttpMethod method, string url, object payload)
        {
            var request = BuildRequest(method, url);
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            return request;
        }

        private static JsonElement UnwrapData(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object)
            {
                return data;
            }

            return root;
        }

        private static bool TryGetString(JsonElement root, string propertyName, out string value)
        {
            value = string.Empty;
            if (!root.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var text = element.GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            value = text;
            return true;
        }

        private static string ToChatId(string phoneNumber)
        {
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
            return $"{digits}@c.us";
        }

        private static WhatsAppSessionInfo CreateSessionInfo(string status, string sessionName, string? whatsAppId = null, string? pushName = null)
        {
            return new WhatsAppSessionInfo
            {
                SessionName = sessionName,
                Status = status,
                WhatsAppId = whatsAppId,
                PushName = pushName,
                Engine = EngineName
            };
        }

        private static string SanitizeSessionName(string source)
        {
            var value = string.IsNullOrWhiteSpace(source) ? "hr-system" : source;
            var chars = value
                .ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) || c == '-' ? c : '-')
                .ToArray();

            var sanitized = new string(chars).Trim('-');
            return string.IsNullOrWhiteSpace(sanitized) ? "hr-system" : sanitized;
        }

        private static string MapStatus(string status)
        {
            return status.ToLowerInvariant() switch
            {
                "ready" => "WORKING",
                "connected" => "WORKING",
                "qr_ready" => "SCAN_QR_CODE",
                "scan_qr" => "SCAN_QR_CODE",
                "initializing" => "STARTING",
                "connecting" => "STARTING",
                "authenticating" => "STARTING",
                "disconnected" => "STOPPED",
                "failed" => "FAILED",
                _ => "UNKNOWN"
            };
        }

        private static string BuildQrDataUri(string qrText)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data).GetGraphic(20);
            return $"data:image/png;base64,{Convert.ToBase64String(png)}";
        }

        private string GetCorrelationId()
        {
            return _httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemKey]?.ToString()
                   ?? _httpContextAccessor.HttpContext?.TraceIdentifier
                   ?? Guid.NewGuid().ToString("N");
        }
    }
}
