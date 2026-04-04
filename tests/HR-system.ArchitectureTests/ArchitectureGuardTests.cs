using System.Reflection;
using HR_system.Services;
using Xunit;

namespace HR_system.ArchitectureTests;

public class ArchitectureGuardTests
{
    private static readonly string RepoRoot = GetRepositoryRoot();
    private static readonly string AppRoot = Path.Combine(RepoRoot, "HR-system");

    [Fact]
    public void SalaryService_Should_Depend_On_Interfaces_For_Repositories()
    {
        var salaryServiceType = typeof(SalaryService);
        var ctor = salaryServiceType.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Single();
        var parameterTypes = ctor.GetParameters().Select(p => p.ParameterType.FullName).ToArray();

        Assert.Contains("HR_system.Repositories.IPayrollRepository", parameterTypes);
        Assert.Contains("HR_system.Repositories.IMonthlyAttendanceRepository", parameterTypes);
        Assert.DoesNotContain("HR_system.Repositories.PayrollRepository", parameterTypes);
    }

    [Fact]
    public void Services_Should_Not_Instantiate_Repositories_Manually()
    {
        var serviceFiles = Directory.GetFiles(Path.Combine(AppRoot, "Services"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase));

        foreach (var file in serviceFiles)
        {
            var content = File.ReadAllText(file);
            Assert.DoesNotContain("new PayrollRepository(", content, StringComparison.Ordinal);
            Assert.DoesNotContain("new MonthlyAttendanceRepository(", content, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void CorrelationIdMiddleware_Should_Be_Registered_In_Program()
    {
        var programFile = Path.Combine(AppRoot, "Program.cs");
        var content = File.ReadAllText(programFile);

        Assert.Contains("builder.Services.AddHttpContextAccessor();", content, StringComparison.Ordinal);
        Assert.Contains("app.UseMiddleware<CorrelationIdMiddleware>();", content, StringComparison.Ordinal);
    }

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "HR-system")) &&
                Directory.Exists(Path.Combine(current.FullName, "tests")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Repository root not found.");
    }
}
