using HR_system.Data;
using HR_system.Models;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Services.Security;

public sealed class MonetaryEncryptionBackfillService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<MonetaryEncryptionBackfillService> _logger;

    public MonetaryEncryptionBackfillService(
        ApplicationDbContext db,
        ILogger<MonetaryEncryptionBackfillService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var updatedRows = 0;

        updatedRows += await BackfillEmployeesAsync(cancellationToken);
        updatedRows += await BackfillAdvancesAsync(cancellationToken);
        updatedRows += await BackfillBounesAsync(cancellationToken);
        updatedRows += await BackfillDeductionsAsync(cancellationToken);
        updatedRows += await BackfillPayrollsAsync(cancellationToken);

        _logger.LogInformation("Monetary encryption backfill finished. Updated rows: {Rows}", updatedRows);
    }

    private async Task<int> BackfillEmployeesAsync(CancellationToken cancellationToken)
    {
        var rows = await _db.Employees.ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            var entry = _db.Entry(row);
            entry.Property(e => e.Salary).IsModified = true;
            entry.Property(e => e.MonthlyFixedAllowance).IsModified = true;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
        return rows.Count;
    }

    private async Task<int> BackfillAdvancesAsync(CancellationToken cancellationToken)
    {
        var rows = await _db.Advances.ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            _db.Entry(row).Property(e => e.Amount).IsModified = true;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
        return rows.Count;
    }

    private async Task<int> BackfillBounesAsync(CancellationToken cancellationToken)
    {
        var rows = await _db.Bounes.ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            _db.Entry(row).Property(e => e.Amount).IsModified = true;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
        return rows.Count;
    }

    private async Task<int> BackfillDeductionsAsync(CancellationToken cancellationToken)
    {
        var rows = await _db.Deductions.ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            _db.Entry(row).Property(e => e.Amount).IsModified = true;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
        return rows.Count;
    }

    private async Task<int> BackfillPayrollsAsync(CancellationToken cancellationToken)
    {
        var rows = await _db.PayRolls.ToListAsync(cancellationToken);
        foreach (var row in rows)
        {
            var entry = _db.Entry(row);
            entry.Property(e => e.BaseSalary).IsModified = true;
            entry.Property(e => e.SalaryPerHour).IsModified = true;
            entry.Property(e => e.SalaryPerDay).IsModified = true;
            entry.Property(e => e.OvertimeAmount).IsModified = true;
            entry.Property(e => e.LateTimeDeduction).IsModified = true;
            entry.Property(e => e.EarlyDepartureDeduction).IsModified = true;
            entry.Property(e => e.NetTimeDifferenceAmount).IsModified = true;
            entry.Property(e => e.TotalBonuses).IsModified = true;
            entry.Property(e => e.TotalDeductions).IsModified = true;
            entry.Property(e => e.TotalAdvances).IsModified = true;
            entry.Property(e => e.MonthlyFixedAllowance).IsModified = true;
            entry.Property(e => e.TotalAttendanceAdjustment).IsModified = true;
            entry.Property(e => e.WorkedHoursSalary).IsModified = true;
            entry.Property(e => e.GrossSalary).IsModified = true;
            entry.Property(e => e.TotalDeductionsAmount).IsModified = true;
            entry.Property(e => e.NetSalary).IsModified = true;
            entry.Property(e => e.ActualPaidAmount).IsModified = true;
            entry.Property(e => e.SalaryCarryOver).IsModified = true;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _db.ChangeTracker.Clear();
        return rows.Count;
    }
}
