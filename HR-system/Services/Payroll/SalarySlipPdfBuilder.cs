using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HR_system.Services.Payroll
{
    public static class SalarySlipPdfBuilder
    {
        private const string Currency = "ج.م";
        private const string ArabicFontFamily = "Cairo";
        private const string AccentColor = "#6C80F3";
        private const string SoftBackground = "#F8F9FD";
        private const string TextMuted = "#5A5A5A";
        private static readonly object FontLock = new();
        private static bool _fontRegistered;

        public static byte[] Build(SalarySlipPdfData data, IWebHostEnvironment environment)
        {
            var companyName = string.IsNullOrWhiteSpace(data.CompanyName) ? "الشركة" : data.CompanyName!;
            var logoPath = ResolveLogoPath("/assets/company-logo.png", environment);
            var hasArabicFont = EnsureArabicFontRegistered(environment);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(24);
                    page.ContentFromRightToLeft();
                    page.DefaultTextStyle(x =>
                    {
                        var style = x.FontSize(10.5f).DirectionFromRightToLeft();
                        return hasArabicFont ? style.FontFamily(ArabicFontFamily) : style;
                    });

                    page.Content().Column(column =>
                    {
                        column.Spacing(12);
                        column.Item().Element(element => BuildHeader(element, companyName, logoPath, data));
                        column.Item().Element(element => BuildEmployeeSummary(element, data));
                        column.Item().Element(element => BuildPayrollTable(element, "الاستحقاقات", BuildEarningsRows(data), false));
                        column.Item().Element(element => BuildPayrollTable(element, "الخصومات", BuildDeductionRows(data), true));
                        column.Item().Element(element => BuildPreviousCarryOverSection(element, data));
                        column.Item().Element(element => BuildNetPayBar(element, data));
                        column.Item().Element(element => BuildFooterNote(element, data));
                    });
                });
            });

            var settings = new DocumentSettings
            {
                CompressDocument = true,
                ImageRasterDpi = 96,
                ImageCompressionQuality = ImageCompressionQuality.Low
            };

            return document.WithSettings(settings).GeneratePdf();
        }

        private static void BuildHeader(IContainer container, string companyName, string? logoPath, SalarySlipPdfData data)
        {
            container.Column(column =>
            {
                column.Spacing(8);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Spacing(1);
                        col.Item().Text(companyName).SemiBold().FontSize(17);
                        col.Item().Text($"{data.DepartmentName ?? "الموارد البشرية"}").FontSize(10).FontColor(TextMuted);
                    });

                    if (!string.IsNullOrWhiteSpace(logoPath))
                    {
                        row.ConstantItem(170).Height(75).AlignLeft().Image(logoPath).FitArea();
                    }
                });

                column.Item().LineHorizontal(1).LineColor(AccentColor);

                column.Item().Text($" مرتب شهر {data.MonthName} {data.Year}")
                    .SemiBold()
                    .FontSize(14);
            });
        }

        private static void BuildEmployeeSummary(IContainer container, SalarySlipPdfData data)
        {
            container.Row(row =>
            {
                row.Spacing(10);

                row.RelativeItem(3).Background(SoftBackground).Padding(10).Column(col =>
                {
                    col.Spacing(5);
                    col.Item().Text("بيانات الموظف").SemiBold().FontColor(AccentColor).FontSize(10.5f);
                    col.Item().Element(c => BuildLabelValue(c, "اسم الموظف:", data.EmployeeName));
                    col.Item().Element(c => BuildLabelValue(c, "كود الموظف:", data.EmployeeCode));
                    col.Item().Element(c => BuildLabelValue(c, "طريقة الحساب:", data.SalaryCalculationTypeDisplay));
                    col.Item().Element(c => BuildLabelValue(c, "فترة الراتب:", $"{data.MonthName} {data.Year}"));
                    col.Item().Element(c => BuildLabelValue(c, "تاريخ الصرف:", data.DateSaved.ToString("dd/MM/yyyy")));
                });

                row.RelativeItem(2).Background(SoftBackground).Padding(10).Column(col =>
                {
                    col.Spacing(5);
                    col.Item().AlignCenter().Text("الراتب المدفوع").FontSize(11);
                    col.Item().AlignCenter().Text(FormatCurrency(data.ActualPaidAmount)).Bold().FontSize(27);
                    col.Item().AlignCenter().Text($"أيام الحضور: {data.ActualPresentDays} | أيام الغياب: {data.AbsentDays}")
                        .FontColor(TextMuted)
                        .FontSize(10);
                });
            });
        }

        private static IEnumerable<(string Item, decimal Amount, bool IsTotal)> BuildEarningsRows(SalarySlipPdfData data)
        {
            return new[]
            {
                ("الراتب الأساسي", data.BaseSalary, false),
                ("راتب أيام العمل", data.WorkedHoursSalary, false),
                ("مقابل الوقت الإضافي", data.OvertimeAmount, false),
                ("البدلات الشهري", data.MonthlyFixedAllowance, false),
                ("بونص المبيعات + بونص العمل", data.TotalBonuses, false),
                ("إجمالي الاستحقاقات", data.GrossSalary, true)
            };
        }

        private static IEnumerable<(string Item, decimal Amount, bool IsTotal)> BuildDeductionRows(SalarySlipPdfData data)
        {
            return new[]
            {
                ("خصم التأخير", data.LateTimeDeduction, false),
                ("خصم الانصراف المبكر", data.EarlyDepartureDeduction, false),
                ("الخصومات", data.TotalDeductions, false),
                ("السحوبات", data.TotalAdvances, false),
                ("إجمالي الخصومات", data.TotalDeductionsAmount, true)
            };
        }

        private static void BuildPayrollTable(
            IContainer container,
            string title,
            IEnumerable<(string Item, decimal Amount, bool IsTotal)> rows,
            bool isDeduction)
        {
            var amountHeader = isDeduction ? "المبلغ (-)" : "المبلغ";

            container.Border(1)
                .Border(1)
                .BorderColor("#D9DDED")
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(7);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(TableHeaderCell).Text(title).SemiBold().FontColor(AccentColor);
                        header.Cell().Element(TableHeaderCell).AlignRight().Text(amountHeader).SemiBold().FontColor(AccentColor);
                    });

                    foreach (var row in rows)
                    {
                        var itemCell = table.Cell().Element(c => TableBodyCell(c, row.IsTotal)).Text(row.Item);
                        if (row.IsTotal)
                        {
                            itemCell.SemiBold();
                        }

                        var amountCell = table.Cell().Element(c => TableBodyCell(c, row.IsTotal)).AlignRight().Text(FormatCurrency(row.Amount));
                        if (row.IsTotal)
                        {
                            amountCell.SemiBold();
                        }
                    }
                });
        }

        private static IContainer TableHeaderCell(IContainer container)
        {
            return container
                .Background(SoftBackground)
                .PaddingVertical(7)
                .PaddingHorizontal(10)
                .BorderBottom(1)
                .BorderColor("#D9DDED");
        }

        private static IContainer TableBodyCell(IContainer container, bool isTotal)
        {
            return container
                .Background(isTotal ? "#F3F5FD" : "#FFFFFF")
                .PaddingVertical(7)
                .PaddingHorizontal(10)
                .BorderBottom(1)
                .BorderColor("#ECEFF9");
        }

        private static void BuildNetPayBar(IContainer container, SalarySlipPdfData data)
        {
            container.Background("#EEF1FF")
                .PaddingVertical(10)
                .PaddingHorizontal(12)
                .Row(row =>
                {
                    row.RelativeItem().Text("صافي الراتب (إجمالي الاستحقاقات - إجمالي الخصومات)").SemiBold().FontColor("#24324F");
                    row.RelativeItem().AlignRight().Text(FormatCurrency(data.NetSalary)).SemiBold().FontColor("#24324F");
                });
        }

        private static void BuildPreviousCarryOverSection(IContainer container, SalarySlipPdfData data)
        {
            var carry = data.PreviousMonthCarryOver;
            var statusText = carry switch
            {
                > 0m => "موجب",
                < 0m => "سالب",
                _ => "صفر"
            };

            var statusColor = carry switch
            {
                > 0m => "#1E8E3E",
                < 0m => "#D93025",
                _ => TextMuted
            };

            container.Background("#FAFBFF")
                .Border(1)
                .BorderColor("#D9DDED")
                .Padding(10)
                .Row(row =>
                {
                    row.RelativeItem().Text("ترحيل من الشهر السابق").SemiBold().FontColor("#24324F");
                    row.RelativeItem().AlignRight().Text($"{FormatCurrency(carry)} ({statusText})").SemiBold().FontColor(statusColor);
                });
        }

        private static void BuildFooterNote(IContainer container, SalarySlipPdfData data)
        {
            container.Column(column =>
            {
                column.Spacing(6);
                column.Item().AlignCenter().Text($"الراتب المدفوع: {FormatCurrency(data.ActualPaidAmount)}")
                    .FontSize(13)
                    .SemiBold()
                    .FontColor("#24324F");

                column.Item().AlignCenter().Text(
                    $"رقم القسيمة: {data.PayRollId} | تاريخ الحفظ: {data.DateSaved:yyyy-MM-dd HH:mm} | الترحيل: {FormatCurrency(data.SalaryCarryOver)}")
                    .FontSize(9.5f)
                    .FontColor(TextMuted);

                if (!string.IsNullOrWhiteSpace(data.EmployeeNote))
                {
                    column.Item().Background("#FFF8E6").Border(1).BorderColor("#E8D9A8").Padding(8)
                        .Text($"ملاحظة: {data.EmployeeNote}")
                        .FontSize(10)
                        .FontColor("#3F3A2A");
                }

                column.Item().LineHorizontal(1).LineColor(AccentColor);
            });
        }

        private static void BuildLabelValue(IContainer container, string label, string value)
        {
            container.Row(row =>
            {
                row.RelativeItem(2).Text(label).FontColor(TextMuted);
                row.RelativeItem(3).Text(value).SemiBold();
            });
        }

        private static string FormatCurrency(decimal value)
        {
            return string.Format(CultureInfo.GetCultureInfo("ar-EG"), "{0:F2} {1}", value, Currency);
        }

        private static string? ResolveLogoPath(string? relativePath, IWebHostEnvironment environment)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return null;
            }

            var trimmed = relativePath.TrimStart('~');
            if (!trimmed.StartsWith('/'))
            {
                trimmed = "/" + trimmed;
            }

            var absolutePath = Path.Combine(environment.WebRootPath, trimmed.TrimStart('/'));
            return File.Exists(absolutePath) ? absolutePath : null;
        }

        private static bool EnsureArabicFontRegistered(IWebHostEnvironment environment)
        {
            if (_fontRegistered)
            {
                return true;
            }

            var fontCandidates = new[]
            {
                Path.Combine(environment.WebRootPath, "fonts", "Cairo-Medium.ttf"),
                Path.Combine(environment.WebRootPath, "fonts", "Cairo-Regular.ttf"),
                Path.Combine(environment.WebRootPath, "fonts", "NotoSansArabic-Medium.ttf")
            };

            var fontPath = fontCandidates.FirstOrDefault(File.Exists);
            if (string.IsNullOrWhiteSpace(fontPath) || !File.Exists(fontPath))
            {
                return false;
            }

            lock (FontLock)
            {
                if (_fontRegistered)
                {
                    return true;
                }

                using var stream = File.OpenRead(fontPath);
                FontManager.RegisterFont(stream);
                _fontRegistered = true;
                return true;
            }
        }
    }
}
