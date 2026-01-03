using ClosedXML.Excel;
using HR_system.Data;
using HR_system.DTOs.Employee;
using HR_system.Models;
using HR_system.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HR_system.Services
{
    public class EmployeeExcelService : IEmployeeExcelService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeeExcelService> _logger;

        // Column headers in Arabic
        private static readonly string[] RequiredColumns = new[]
        {
            "الكود",
            "الاسم",
            "الراتب",
            "النوع",
            "العمر",
            "القسم",
            "الوردية",
            "الحالة",
            "معدل الوقت الإضافي",
            "معدل وقت التأخير"
        };

        public EmployeeExcelService(ApplicationDbContext context, ILogger<EmployeeExcelService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public byte[] ExportToExcel(IEnumerable<EmployeeDto> employees)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("الموظفين");

            // Set RTL direction for the worksheet
            worksheet.RightToLeft = true;

            // Create header row
            var headers = new[]
            {
                "الكود",
                "الاسم",
                "الراتب",
                "النوع",
                "العمر",
                "القسم",
                "الوردية",
                "الحالة",
                "معدل الوقت الإضافي",
                "معدل وقت التأخير"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Add data rows
            int row = 2;
            foreach (var emp in employees)
            {
                worksheet.Cell(row, 1).Value = emp.Code;
                worksheet.Cell(row, 2).Value = emp.Emp_name;
                worksheet.Cell(row, 3).Value = emp.Salary;
                worksheet.Cell(row, 4).Value = emp.Gender ?? "";
                worksheet.Cell(row, 5).Value = emp.Age ?? 0;
                worksheet.Cell(row, 6).Value = emp.Department_name ?? "";
                worksheet.Cell(row, 7).Value = emp.Shift_name;
                worksheet.Cell(row, 8).Value = emp.Status ?? "Active";
                worksheet.Cell(row, 9).Value = emp.Rate_overtime_multiplier;
                worksheet.Cell(row, 10).Value = emp.Rate_latetime_multiplier;
                row++;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Add filter
            worksheet.RangeUsed()?.SetAutoFilter();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GetImportTemplate()
        {
            using var workbook = new XLWorkbook();
            
            // Main data worksheet
            var worksheet = workbook.Worksheets.Add("الموظفين");
            worksheet.RightToLeft = true;

            // Create header row
            var headers = new[]
            {
                "الكود",
                "الاسم",
                "الراتب",
                "النوع",
                "العمر",
                "القسم",
                "الوردية",
                "الحالة",
                "معدل الوقت الإضافي",
                "معدل وقت التأخير"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                var cell = worksheet.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.DarkBlue;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Add sample data row
            worksheet.Cell(2, 1).Value = "EMP001";
            worksheet.Cell(2, 2).Value = "أحمد محمد";
            worksheet.Cell(2, 3).Value = 5000;
            worksheet.Cell(2, 4).Value = "ذكر";
            worksheet.Cell(2, 5).Value = 30;
            worksheet.Cell(2, 6).Value = "الإدارة";
            worksheet.Cell(2, 7).Value = "الصباحية";
            worksheet.Cell(2, 8).Value = "Active";
            worksheet.Cell(2, 9).Value = 1.5;
            worksheet.Cell(2, 10).Value = 1;

            // Style sample row
            for (int i = 1; i <= 10; i++)
            {
                worksheet.Cell(2, i).Style.Font.Italic = true;
                worksheet.Cell(2, i).Style.Font.FontColor = XLColor.Gray;
            }

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Add instructions worksheet
            var instructionsSheet = workbook.Worksheets.Add("تعليمات");
            instructionsSheet.RightToLeft = true;
            instructionsSheet.Cell(1, 1).Value = "تعليمات استيراد الموظفين";
            instructionsSheet.Cell(1, 1).Style.Font.Bold = true;
            instructionsSheet.Cell(1, 1).Style.Font.FontSize = 16;

            var instructions = new[]
            {
                "",
                "الحقول المطلوبة:",
                "• الكود - كود الموظف الفريد (مطلوب)",
                "• الاسم - اسم الموظف (مطلوب)",
                "• الراتب - الراتب الأساسي (مطلوب، رقم موجب)",
                "• الوردية - اسم الوردية (مطلوب، يجب أن يكون موجود في النظام)",
                "",
                "الحقول الاختيارية:",
                "• النوع - ذكر أو أنثى",
                "• العمر - عدد صحيح بين 18 و 100",
                "• القسم - اسم القسم (يجب أن يكون موجود في النظام)",
                "• الحالة - Active, Inactive, On Leave, أو Terminated (الافتراضي: Active)",
                "• معدل الوقت الإضافي - معدل حساب الساعات الإضافية (الافتراضي: 1.5)",
                "• معدل وقت التأخير - معدل خصم التأخير (الافتراضي: 1)",
                "",
                "ملاحظات:",
                "• الصف الأول هو صف العناوين ولن يتم استيراده",
                "• السطر الثاني في ورقة الموظفين هو مثال ويمكن حذفه",
                "• إذا كان الكود موجوداً مسبقاً، يمكنك اختيار تحديث البيانات أو تخطي السجل"
            };

            for (int i = 0; i < instructions.Length; i++)
            {
                instructionsSheet.Cell(i + 2, 1).Value = instructions[i];
            }

            instructionsSheet.Column(1).Width = 80;

            // Add reference data worksheet
            var refSheet = workbook.Worksheets.Add("البيانات المرجعية");
            refSheet.RightToLeft = true;

            // Load departments and shifts
            var departments = _context.Departments.ToList();
            var shifts = _context.Shifts.ToList();

            refSheet.Cell(1, 1).Value = "الأقسام المتاحة";
            refSheet.Cell(1, 1).Style.Font.Bold = true;
            refSheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            for (int i = 0; i < departments.Count; i++)
            {
                refSheet.Cell(i + 2, 1).Value = departments[i].Department_name;
            }

            refSheet.Cell(1, 3).Value = "الورديات المتاحة";
            refSheet.Cell(1, 3).Style.Font.Bold = true;
            refSheet.Cell(1, 3).Style.Fill.BackgroundColor = XLColor.LightGreen;

            for (int i = 0; i < shifts.Count; i++)
            {
                refSheet.Cell(i + 2, 3).Value = shifts[i].Shift_name;
            }

            refSheet.Cell(1, 5).Value = "حالات الموظف";
            refSheet.Cell(1, 5).Style.Font.Bold = true;
            refSheet.Cell(1, 5).Style.Fill.BackgroundColor = XLColor.LightYellow;

            var statuses = new[] { "Active", "Inactive", "On Leave", "Terminated" };
            for (int i = 0; i < statuses.Length; i++)
            {
                refSheet.Cell(i + 2, 5).Value = statuses[i];
            }

            refSheet.Cell(1, 7).Value = "النوع";
            refSheet.Cell(1, 7).Style.Font.Bold = true;
            refSheet.Cell(1, 7).Style.Fill.BackgroundColor = XLColor.LightPink;

            refSheet.Cell(2, 7).Value = "ذكر";
            refSheet.Cell(3, 7).Value = "أنثى";

            refSheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public List<string> ValidateExcelStructure(Stream fileStream)
        {
            var errors = new List<string>();

            try
            {
                fileStream.Position = 0;
                using var workbook = new XLWorkbook(fileStream);
                
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    errors.Add("الملف لا يحتوي على أي أوراق عمل");
                    return errors;
                }

                // Check if first row has headers
                var firstRow = worksheet.Row(1);
                if (firstRow.IsEmpty())
                {
                    errors.Add("الصف الأول (العناوين) فارغ");
                    return errors;
                }

                // Check required columns exist
                var headerRow = worksheet.Row(1);
                var existingHeaders = new List<string>();
                for (int col = 1; col <= 10; col++)
                {
                    var cellValue = headerRow.Cell(col).GetString().Trim();
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        existingHeaders.Add(cellValue);
                    }
                }

                // Check for minimum required columns (Code, Name, Salary, Shift)
                var requiredHeaders = new[] { "الكود", "الاسم", "الراتب", "الوردية" };
                foreach (var required in requiredHeaders)
                {
                    if (!existingHeaders.Any(h => h.Contains(required)))
                    {
                        errors.Add($"العمود المطلوب '{required}' غير موجود");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"خطأ في قراءة الملف: {ex.Message}");
            }

            return errors;
        }

        public async Task<EmployeeImportResult> ImportFromExcelAsync(Stream fileStream, bool updateExisting = false)
        {
            var result = new EmployeeImportResult();

            try
            {
                fileStream.Position = 0;
                using var workbook = new XLWorkbook(fileStream);
                var worksheet = workbook.Worksheets.First();

                // Get column indices from header row
                var headerRow = worksheet.Row(1);
                var columnMap = new Dictionary<string, int>();

                for (int col = 1; col <= 20; col++)
                {
                    var cellValue = headerRow.Cell(col).GetString().Trim();
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        columnMap[cellValue] = col;
                    }
                }

                // Load reference data
                var departments = await _context.Departments.ToListAsync();
                var shifts = await _context.Shifts.ToListAsync();
                var existingCodes = await _context.Employees.Select(e => e.Code).ToListAsync();

                // Process data rows
                var lastRowUsed = worksheet.LastRowUsed()?.RowNumber() ?? 1;

                for (int row = 2; row <= lastRowUsed; row++)
                {
                    var currentRow = worksheet.Row(row);
                    if (currentRow.IsEmpty()) continue;

                    try
                    {
                        var importDto = new EmployeeImportDto
                        {
                            Code = GetCellValue(currentRow, columnMap, "الكود"),
                            Emp_name = GetCellValue(currentRow, columnMap, "الاسم"),
                            Gender = GetCellValue(currentRow, columnMap, "النوع"),
                            Department_name = GetCellValue(currentRow, columnMap, "القسم"),
                            Shift_name = GetCellValue(currentRow, columnMap, "الوردية"),
                            Status = GetCellValue(currentRow, columnMap, "الحالة")
                        };

                        // Parse numeric values
                        if (decimal.TryParse(GetCellValue(currentRow, columnMap, "الراتب"), out decimal salary))
                        {
                            importDto.Salary = salary;
                        }

                        if (int.TryParse(GetCellValue(currentRow, columnMap, "العمر"), out int age))
                        {
                            importDto.Age = age;
                        }

                        if (decimal.TryParse(GetCellValue(currentRow, columnMap, "معدل الوقت الإضافي"), out decimal overtimeRate))
                        {
                            importDto.Rate_overtime_multiplier = overtimeRate;
                        }
                        else
                        {
                            importDto.Rate_overtime_multiplier = 1.5m;
                        }

                        if (decimal.TryParse(GetCellValue(currentRow, columnMap, "معدل وقت التأخير"), out decimal lateRate))
                        {
                            importDto.Rate_latetime_multiplier = lateRate;
                        }
                        else
                        {
                            importDto.Rate_latetime_multiplier = 1m;
                        }

                        // Validate required fields
                        var validationErrors = ValidateImportRow(importDto, row, shifts, departments);
                        if (validationErrors.Any())
                        {
                            foreach (var error in validationErrors)
                            {
                                result.Errors.Add(new EmployeeImportError
                                {
                                    RowNumber = row,
                                    EmployeeName = importDto.Emp_name,
                                    EmployeeCode = importDto.Code,
                                    ErrorMessage = error
                                });
                            }
                            result.FailedCount++;
                            continue;
                        }

                        // Find shift and department
                        var shift = shifts.FirstOrDefault(s => 
                            s.Shift_name.Equals(importDto.Shift_name, StringComparison.OrdinalIgnoreCase));
                        
                        Department? department = null;
                        if (!string.IsNullOrEmpty(importDto.Department_name))
                        {
                            department = departments.FirstOrDefault(d => 
                                d.Department_name.Equals(importDto.Department_name, StringComparison.OrdinalIgnoreCase));
                        }

                        // Check if employee exists
                        var existingEmployee = await _context.Employees
                            .FirstOrDefaultAsync(e => e.Code == importDto.Code);

                        if (existingEmployee != null)
                        {
                            if (updateExisting)
                            {
                                // Update existing employee
                                existingEmployee.Emp_name = importDto.Emp_name;
                                existingEmployee.Salary = importDto.Salary;
                                existingEmployee.Gender = importDto.Gender;
                                existingEmployee.Age = importDto.Age;
                                existingEmployee.Department_id = department?.Id;
                                existingEmployee.Shift_id = shift!.Id;
                                existingEmployee.Status = string.IsNullOrEmpty(importDto.Status) ? "Active" : importDto.Status;
                                existingEmployee.Rate_overtime_multiplier = importDto.Rate_overtime_multiplier;
                                existingEmployee.Rate_latetime_multiplier = importDto.Rate_latetime_multiplier;

                                result.UpdatedCount++;
                            }
                            else
                            {
                                result.Errors.Add(new EmployeeImportError
                                {
                                    RowNumber = row,
                                    EmployeeName = importDto.Emp_name,
                                    EmployeeCode = importDto.Code,
                                    ErrorMessage = "الكود موجود مسبقاً. اختر 'تحديث الموجودين' لتحديث البيانات"
                                });
                                result.FailedCount++;
                            }
                        }
                        else
                        {
                            // Create new employee
                            var employee = new Employee
                            {
                                Emp_name = importDto.Emp_name,
                                Code = importDto.Code,
                                Salary = importDto.Salary,
                                Gender = importDto.Gender,
                                Age = importDto.Age,
                                Department_id = department?.Id,
                                Shift_id = shift!.Id,
                                Status = string.IsNullOrEmpty(importDto.Status) ? "Active" : importDto.Status,
                                Rate_overtime_multiplier = importDto.Rate_overtime_multiplier,
                                Rate_latetime_multiplier = importDto.Rate_latetime_multiplier
                            };

                            _context.Employees.Add(employee);
                            result.SuccessCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new EmployeeImportError
                        {
                            RowNumber = row,
                            EmployeeName = "",
                            EmployeeCode = "",
                            ErrorMessage = $"خطأ في معالجة الصف: {ex.Message}"
                        });
                        result.FailedCount++;
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing employees from Excel");
                result.Errors.Add(new EmployeeImportError
                {
                    RowNumber = 0,
                    EmployeeName = "",
                    EmployeeCode = "",
                    ErrorMessage = $"خطأ عام في الاستيراد: {ex.Message}"
                });
            }

            return result;
        }

        private static string GetCellValue(IXLRow row, Dictionary<string, int> columnMap, string columnName)
        {
            if (columnMap.TryGetValue(columnName, out int colIndex))
            {
                return row.Cell(colIndex).GetString().Trim();
            }
            return string.Empty;
        }

        private static List<string> ValidateImportRow(EmployeeImportDto dto, int row, List<Shift> shifts, List<Department> departments)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Code))
            {
                errors.Add("الكود مطلوب");
            }

            if (string.IsNullOrWhiteSpace(dto.Emp_name))
            {
                errors.Add("الاسم مطلوب");
            }

            if (dto.Salary <= 0)
            {
                errors.Add("الراتب يجب أن يكون أكبر من صفر");
            }

            if (string.IsNullOrWhiteSpace(dto.Shift_name))
            {
                errors.Add("الوردية مطلوبة");
            }
            else
            {
                var shift = shifts.FirstOrDefault(s => 
                    s.Shift_name.Equals(dto.Shift_name, StringComparison.OrdinalIgnoreCase));
                if (shift == null)
                {
                    errors.Add($"الوردية '{dto.Shift_name}' غير موجودة في النظام");
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Department_name))
            {
                var department = departments.FirstOrDefault(d => 
                    d.Department_name.Equals(dto.Department_name, StringComparison.OrdinalIgnoreCase));
                if (department == null)
                {
                    errors.Add($"القسم '{dto.Department_name}' غير موجود في النظام");
                }
            }

            if (dto.Age.HasValue && (dto.Age < 18 || dto.Age > 100))
            {
                errors.Add("العمر يجب أن يكون بين 18 و 100");
            }

            var validStatuses = new[] { "Active", "Inactive", "On Leave", "Terminated" };
            if (!string.IsNullOrWhiteSpace(dto.Status) && !validStatuses.Contains(dto.Status, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"الحالة '{dto.Status}' غير صالحة. القيم المسموحة: Active, Inactive, On Leave, Terminated");
            }

            return errors;
        }
    }
}
