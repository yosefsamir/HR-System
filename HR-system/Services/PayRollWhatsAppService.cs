using HR_system.Data;
using HR_system.DTOs.PayRoll;
using HR_system.Middleware;
using Microsoft.EntityFrameworkCore;
using HR_system.Services.Interfaces;
using HR_system.Services.Payroll;

namespace HR_system.Services
{
    public class PayRollWhatsAppService : IPayRollWhatsAppService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<PayRollWhatsAppService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;

        public PayRollWhatsAppService(
            ApplicationDbContext context,
            IWhatsAppService whatsAppService,
            ILogger<PayRollWhatsAppService> logger,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment environment)
        {
            _context = context;
            _whatsAppService = whatsAppService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
        }

        public async Task<SendSalaryWhatsAppResultDto> SendSalaryWhatsAppAsync(int payRollId)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = GetCorrelationId(),
                ["PayRollId"] = payRollId
            });

            _logger.LogInformation("Starting payroll WhatsApp send for single payroll record");

            var payroll = await _context.PayRolls
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == payRollId);

            if (payroll == null)
            {
                return new SendSalaryWhatsAppResultDto
                {
                    Success = false,
                    Message = "لم يتم العثور على سجل الراتب"
                };
            }

            var phone = payroll.Employee?.WhatsAppNumber;
            if (string.IsNullOrWhiteSpace(phone))
            {
                return new SendSalaryWhatsAppResultDto
                {
                    Success = false,
                    Message = "الموظف لا يملك رقم واتساب مسجل"
                };
            }

            var settings = await _context.AppSettings
                .OrderByDescending(a => a.UpdatedAt)
                .FirstOrDefaultAsync();
            var previousMonthCarryOver = await GetPreviousMonthCarryOverAsync(payroll.Employee_id, payroll.Month, payroll.Year);
            var pdfData = new SalarySlipPdfData
            {
                PayRollId = payroll.Id,
                CompanyName = settings?.CompanyName,
                Month = payroll.Month,
                Year = payroll.Year,
                MonthName = GetArabicMonthName(payroll.Month),
                EmployeeName = payroll.EmployeeName,
                EmployeeCode = payroll.EmployeeCode,
                DepartmentName = payroll.DepartmentName,
                ShiftName = payroll.ShiftName,
                BaseSalary = payroll.BaseSalary,
                SalaryPerHour = payroll.SalaryPerHour,
                SalaryPerDay = payroll.SalaryPerDay,
                SalaryCalculationTypeDisplay = payroll.SalaryCalculationTypeDisplay,
                WorkingDaysInMonth = payroll.WorkingDaysInMonth,
                HolidaysInMonth = payroll.HolidaysInMonth,
                ActualPresentDays = payroll.ActualPresentDays,
                AbsentDays = payroll.AbsentDays,
                ActualWorkedMinutes = payroll.ActualWorkedMinutes,
                ExpectedWorkingHours = payroll.ExpectedWorkingHours,
                OvertimeMinutes = payroll.OvertimeMinutes,
                OvertimeMultiplier = payroll.OvertimeMultiplier,
                LateTimeMinutes = payroll.LateTimeMinutes,
                LateTimeMultiplier = payroll.LateTimeMultiplier,
                EarlyDepartureMinutes = payroll.EarlyDepartureMinutes,
                EarlyDepartureMultiplier = payroll.EarlyDepartureMultiplier,
                PermissionMinutes = payroll.PermissionMinutes,
                WorkedHoursSalary = payroll.WorkedHoursSalary,
                OvertimeAmount = payroll.OvertimeAmount,
                LateTimeDeduction = payroll.LateTimeDeduction,
                EarlyDepartureDeduction = payroll.EarlyDepartureDeduction,
                TotalBonuses = payroll.TotalBonuses,
                TotalDeductions = payroll.TotalDeductions,
                TotalAdvances = payroll.TotalAdvances,
                MonthlyFixedAllowance = payroll.MonthlyFixedAllowance,
                TotalAttendanceAdjustment = payroll.TotalAttendanceAdjustment,
                GrossSalary = payroll.GrossSalary,
                TotalDeductionsAmount = payroll.TotalDeductionsAmount,
                NetSalary = payroll.NetSalary,
                ActualPaidAmount = payroll.ActualPaidAmount,
                PreviousMonthCarryOver = previousMonthCarryOver,
                SalaryCarryOver = payroll.SalaryCarryOver,
                EmployeeNote = payroll.EmployeeNote,
                IsPaid = payroll.IsPaid,
                DateSaved = payroll.DateSaved
            };

            var pdfBytes = SalarySlipPdfBuilder.Build(pdfData, _environment);
            var fileName = $"salary-slip-{payroll.EmployeeCode}-{payroll.Year}{payroll.Month:00}.pdf";
            var caption = BuildSalaryShortMessage(payroll);

            var pdfSizeMb = pdfBytes.Length / (1024d * 1024d);
            _logger.LogInformation("Salary PDF size: {SizeMb:N2} MB", pdfSizeMb);

            if (pdfBytes.Length > 95 * 1024 * 1024)
            {
                return new SendSalaryWhatsAppResultDto
                {
                    Success = false,
                    Message = "حجم ملف الراتب كبير جدًا للإرسال عبر واتساب"
                };
            }

            var sendResult = await _whatsAppService.SendFileAsync(
                phone,
                fileName,
                "application/pdf",
                pdfBytes,
                caption);

            if (sendResult.Success)
            {
                _logger.LogInformation("Payroll WhatsApp send completed successfully for payroll record");
                return new SendSalaryWhatsAppResultDto
                {
                    Success = true,
                    Message = "تم إرسال إيصال الراتب عبر واتساب"
                };
            }

            _logger.LogWarning("Payroll WhatsApp send failed for payroll record");
            if (sendResult.StatusCode == System.Net.HttpStatusCode.RequestEntityTooLarge)
            {
                return new SendSalaryWhatsAppResultDto
                {
                    Success = false,
                    Message = "فشل الإرسال: حجم الملف يتجاوز الحد المسموح من خادم واتساب"
                };
            }

            return new SendSalaryWhatsAppResultDto
            {
                Success = false,
                Message = "فشل إرسال رسالة واتساب"
            };
        }

        private string GetCorrelationId()
        {
            return _httpContextAccessor.HttpContext?.Items[CorrelationIdMiddleware.ItemKey]?.ToString()
                   ?? _httpContextAccessor.HttpContext?.TraceIdentifier
                   ?? Guid.NewGuid().ToString("N");
        }

        private static string BuildSalaryShortMessage(Models.PayRoll payroll)
        {
            var monthName = GetArabicMonthName(payroll.Month);
            return $@"السلام عليكم {payroll.EmployeeName}
تم تجهيز إيصال راتب شهر {monthName} {payroll.Year}.";
        }

        private static string GetArabicMonthName(int month)
        {
            string[] monthNames =
            {
                "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
                "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر"
            };
            return month >= 1 && month <= 12 ? monthNames[month] : month.ToString();
        }

        private async Task<decimal> GetPreviousMonthCarryOverAsync(int employeeId, int month, int year)
        {
            var prevMonth = month == 1 ? 12 : month - 1;
            var prevYear = month == 1 ? year - 1 : year;

            var prevPayroll = await _context.PayRolls
                .Where(p => p.Employee_id == employeeId && p.Month == prevMonth && p.Year == prevYear)
                .OrderByDescending(p => p.DateSaved)
                .Select(p => p.SalaryCarryOver)
                .FirstOrDefaultAsync();

            return prevPayroll;
        }

    }
}
