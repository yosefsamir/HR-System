using ClosedXML.Excel;
using HR_system.DTOs.Attendence;
using HR_system.Services.Interfaces;

namespace HR_system.Services
{
    /// <summary>
    /// Service for handling attendance Excel operations (import/export)
    /// </summary>
    public class AttendanceExcelService : IAttendanceExcelService
    {
        private readonly IEmployeeService _employeeService;
        private readonly IShiftService _shiftService;
        private readonly IAttendenceService _attendenceService;

        public AttendanceExcelService(
            IEmployeeService employeeService,
            IShiftService shiftService,
            IAttendenceService attendenceService)
        {
            _employeeService = employeeService;
            _shiftService = shiftService;
            _attendenceService = attendenceService;
        }

        /// <summary>
        /// Generate an Excel template with all active employees and their shift data
        /// </summary>
        public async Task<byte[]> GenerateTemplateAsync(DateTime date)
        {
            var employees = await _employeeService.GetAllAsync();
            var activeEmployees = employees.Where(e => e.Status == "Active").ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("الحضور والانصراف");

            // Set RTL direction for the worksheet
            worksheet.RightToLeft = true;

            // Header row
            var headers = new[] 
            { 
                "كود الموظف", 
                "اسم الموظف", 
                "الوردية", 
                "وقت الحضور المجدول", 
                "وقت الانصراف المجدول", 
                "التاريخ", 
                "وقت الحضور الفعلي", 
                "وقت الانصراف الفعلي", 
                "غائب", 
                "إذن (دقائق)" 
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            // Style header
            var headerRange = worksheet.Range(1, 1, 1, headers.Length);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0d6efd");
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Add employee data
            int row = 2;
            foreach (var emp in activeEmployees)
            {
                // Get shift info
                var shift = await _shiftService.GetByIdAsync(emp.Shift_id);
                var startTime = shift?.Start_time.ToString(@"hh\:mm") ?? "08:00";
                var endTime = shift?.End_time.ToString(@"hh\:mm") ?? "17:00";

                worksheet.Cell(row, 1).Value = emp.Code;
                worksheet.Cell(row, 2).Value = emp.Emp_name;
                worksheet.Cell(row, 3).Value = emp.Shift_name;
                worksheet.Cell(row, 4).Value = startTime;
                worksheet.Cell(row, 5).Value = endTime;
                worksheet.Cell(row, 6).Value = date.ToString("yyyy-MM-dd");
                worksheet.Cell(row, 7).Value = startTime; // Default actual check-in to shift start
                worksheet.Cell(row, 8).Value = endTime;   // Default actual check-out to shift end
                worksheet.Cell(row, 9).Value = "لا";      // Not absent by default
                worksheet.Cell(row, 10).Value = 0;        // No permission by default

                // Mark read-only columns (employee info and shift times)
                for (int col = 1; col <= 5; col++)
                {
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Gray;
                }

                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Add validation notes sheet
            AddNotesSheet(workbook);

            // Generate file
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// Import attendance records from an Excel file
        /// </summary>
        public async Task<AttendanceImportResultDto> ImportFromExcelAsync(Stream fileStream)
        {
            var result = new AttendanceImportResultDto();

            // Get all employees for lookup
            var employees = await _employeeService.GetAllAsync();
            var employeeDict = employees.ToDictionary(e => e.Code.ToLower(), e => e);

            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1); // First sheet

            var rows = worksheet.RangeUsed()?.RowsUsed().Skip(1) ?? Enumerable.Empty<IXLRangeRow>(); // Skip header row

            foreach (var row in rows)
            {
                result.TotalRows++;
                var rowNumber = row.RowNumber();

                try
                {
                    var importResult = await ProcessRowAsync(row, rowNumber, employeeDict, result);
                    if (importResult != null)
                    {
                        result.ImportedRecords.Add(importResult);
                        result.SuccessCount++;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    // Duplicate record or business rule violation
                    result.FailedCount++;
                    result.Errors.Add($"الصف {rowNumber}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add($"الصف {rowNumber}: خطأ - {ex.Message}");
                }
            }

            return result;
        }

        #region Private Helper Methods

        /// <summary>
        /// Process a single row from the Excel file
        /// </summary>
        private async Task<AttendenceDto?> ProcessRowAsync(
            IXLRangeRow row, 
            int rowNumber, 
            Dictionary<string, DTOs.Employee.EmployeeDto> employeeDict,
            AttendanceImportResultDto result)
        {
            var employeeCode = row.Cell(1).GetString()?.Trim();
            if (string.IsNullOrEmpty(employeeCode))
            {
                result.SkippedCount++;
                result.Warnings.Add($"الصف {rowNumber}: كود الموظف فارغ");
                return null;
            }

            if (!employeeDict.TryGetValue(employeeCode.ToLower(), out var employee))
            {
                result.FailedCount++;
                result.Errors.Add($"الصف {rowNumber}: الموظف بالكود '{employeeCode}' غير موجود");
                return null;
            }

            // Parse work date
            var workDate = ParseDateFromCell(row.Cell(6));
            if (!workDate.HasValue)
            {
                result.FailedCount++;
                result.Errors.Add($"الصف {rowNumber}: تاريخ غير صحيح");
                return null;
            }

            // Parse is absent
            var absentStr = row.Cell(9).GetString()?.Trim().ToLower();
            var isAbsent = absentStr == "نعم" || absentStr == "yes" || absentStr == "1" || absentStr == "true";

            // Parse times
            TimeSpan? checkIn = null;
            TimeSpan? checkOut = null;

            if (!isAbsent)
            {
                checkIn = ParseTimeFromCell(row.Cell(7));
                checkOut = ParseTimeFromCell(row.Cell(8));

                if (!checkIn.HasValue && !checkOut.HasValue)
                {
                    result.SkippedCount++;
                    result.Warnings.Add($"الصف {rowNumber}: لا يوجد وقت حضور أو انصراف للموظف {employeeCode}");
                    return null;
                }
            }

            // Parse permission minutes
            var permissionMinutes = ParseIntFromCell(row.Cell(10));

            // Create attendance record
            var createDto = new CreateAttendenceDto
            {
                Employee_id = employee.Id,
                Work_date = workDate.Value,
                Is_absent = isAbsent,
                Check_In_time = checkIn,
                Check_out_time = checkOut,
                Permission_minutes = permissionMinutes
            };

            // Create attendance record using the attendance service
            return await _attendenceService.CreateAsync(createDto);
        }

        /// <summary>
        /// Add notes/instructions sheet to the workbook
        /// </summary>
        private void AddNotesSheet(XLWorkbook workbook)
        {
            var notesSheet = workbook.Worksheets.Add("ملاحظات");
            notesSheet.RightToLeft = true;

            notesSheet.Cell(1, 1).Value = "ملاحظات الاستيراد:";
            notesSheet.Cell(1, 1).Style.Font.Bold = true;
            notesSheet.Cell(2, 1).Value = "1. لا تقم بتعديل أعمدة: كود الموظف، اسم الموظف، الوردية، أوقات الوردية";
            notesSheet.Cell(3, 1).Value = "2. قم بتعديل أعمدة: وقت الحضور الفعلي، وقت الانصراف الفعلي، غائب، إذن (دقائق)";
            notesSheet.Cell(4, 1).Value = "3. صيغة الوقت: HH:mm (مثال: 08:30)";
            notesSheet.Cell(5, 1).Value = "4. عمود غائب: نعم أو لا";
            notesSheet.Cell(6, 1).Value = "5. إذا كان الموظف غائب، لن يتم قراءة أوقات الحضور والانصراف";

            notesSheet.Columns().AdjustToContents();
        }

        /// <summary>
        /// Parse date from Excel cell
        /// </summary>
        private DateTime? ParseDateFromCell(IXLCell cell)
        {
            if (cell == null) return null;

            try
            {
                if (cell.DataType == XLDataType.DateTime)
                {
                    return cell.GetDateTime();
                }

                var dateStr = cell.GetString()?.Trim();
                if (DateTime.TryParse(dateStr, out var date))
                {
                    return date;
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return null;
        }

        /// <summary>
        /// Parse time from Excel cell
        /// </summary>
        private TimeSpan? ParseTimeFromCell(IXLCell cell)
        {
            if (cell == null) return null;

            try
            {
                if (cell.DataType == XLDataType.DateTime)
                {
                    var dateTime = cell.GetDateTime();
                    return dateTime.TimeOfDay;
                }
                else if (cell.DataType == XLDataType.Number)
                {
                    // Excel stores times as decimal fractions of a day
                    var decimalTime = cell.GetDouble();
                    return TimeSpan.FromDays(decimalTime);
                }
                else
                {
                    var timeStr = cell.GetString()?.Trim();
                    if (string.IsNullOrEmpty(timeStr)) return null;

                    if (TimeSpan.TryParse(timeStr, out var time))
                    {
                        return time;
                    }

                    // Try parsing as DateTime and extract time
                    if (DateTime.TryParse(timeStr, out var dateTime))
                    {
                        return dateTime.TimeOfDay;
                    }
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return null;
        }

        /// <summary>
        /// Parse integer from Excel cell
        /// </summary>
        private int ParseIntFromCell(IXLCell cell)
        {
            if (cell == null) return 0;

            try
            {
                if (cell.DataType == XLDataType.Number)
                {
                    return (int)cell.GetDouble();
                }

                var str = cell.GetString()?.Trim();
                if (int.TryParse(str, out var value))
                {
                    return value;
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return 0;
        }

        #endregion
    }
}
