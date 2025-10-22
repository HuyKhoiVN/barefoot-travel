using barefoot_travel.DTOs.Booking;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace barefoot_travel.Services
{
    public class ExportService
    {
        public byte[] GenerateBookingReport(List<BookingDto> bookings, string format, DateTime? startDateFrom = null, DateTime? startDateTo = null)
        {
            if (format.ToLower() == "excel")
            {
                return GenerateExcelReport(bookings, startDateFrom, startDateTo);
            }
            else if (format.ToLower() == "pdf")
            {
                return GeneratePdfReport(bookings);
            }
            else
            {
                throw new ArgumentException("Unsupported export format");
            }
        }

        private byte[] GenerateExcelReport(List<BookingDto> bookings, DateTime? startDateFrom = null, DateTime? startDateTo = null)
        {
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Get template file path
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "template", "barefootReportTemplate.xlsx");
            
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Template file not found: {templatePath}");
            }

            using var package = new ExcelPackage(new FileInfo(templatePath));
            var worksheet = package.Workbook.Worksheets[0];

            // Fill date range in template
            if (startDateFrom.HasValue)
            {
                worksheet.Cells["D6"].Value = startDateFrom.Value.ToLocalTime().ToString("dd/MM/yyyy");
            }
            if (startDateTo.HasValue)
            {
                worksheet.Cells["I6"].Value = startDateTo.Value.ToLocalTime().ToString("dd/MM/yyyy");
            }

            // Fill data starting from row 9
            int currentRow = 9;
            int rowNumber = 1;

            foreach (var booking in bookings)
            {
                // No.
                worksheet.Cells[currentRow, 1].Value = rowNumber;

                // Tour Title
                worksheet.Cells[currentRow, 2].Value = booking.TourTitle ?? "";

                // Customer Name
                worksheet.Cells[currentRow, 3].Value = booking.NameCustomer ?? "";

                // Phone - add ' prefix if starts with 0
                var phoneNumber = booking.PhoneNumber ?? "";
                if (phoneNumber.StartsWith("0"))
                {
                    phoneNumber = "'" + phoneNumber;
                }
                worksheet.Cells[currentRow, 4].Value = phoneNumber;

                // Email
                worksheet.Cells[currentRow, 5].Value = booking.Email ?? "";

                // Start Date - convert UTC to local time and show only date
                worksheet.Cells[currentRow, 6].Value = booking.StartDate?.ToLocalTime().ToString("dd/MM/yyyy");

                // People
                worksheet.Cells[currentRow, 7].Value = booking.People;

                // Total Price - format as currency without symbol
                worksheet.Cells[currentRow, 8].Value = booking.TotalPrice;
                worksheet.Cells[currentRow, 8].Style.Numberformat.Format = "#,##0";

                // Status
                worksheet.Cells[currentRow, 9].Value = booking.StatusName ?? "";

                // Payment Status with color coding
                var paymentStatusCell = worksheet.Cells[currentRow, 10];
                paymentStatusCell.Value = booking.PaymentStatus ?? "";
                
                // Apply color based on payment status
                switch (booking.PaymentStatus?.ToUpper())
                {
                    case "PENDING":
                        paymentStatusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        paymentStatusCell.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                        break;
                    case "PAID":
                        paymentStatusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        paymentStatusCell.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                        break;
                    case "CANCELLED":
                    case "REFUNDED":
                        paymentStatusCell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        paymentStatusCell.Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                        break;
                }

                // Created Time - convert UTC to local time and show date + time
                worksheet.Cells[currentRow, 11].Value = booking.CreatedTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

                // Note
                worksheet.Cells[currentRow, 12].Value = booking.Note ?? "";

                currentRow++;
                rowNumber++;
            }

            // Auto-fit columns for better readability
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return package.GetAsByteArray();
        }

        private byte[] GeneratePdfReport(List<BookingDto> bookings)
        {
            var html = new StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine("<title>Booking Report</title>");
            html.AppendLine("<style>");
            html.AppendLine("table { border-collapse: collapse; width: 100%; font-family: Arial, sans-serif; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f2f2f2; font-weight: bold; }");
            html.AppendLine("tr:nth-child(even) { background-color: #f9f9f9; }");
            html.AppendLine("h1 { color: #333; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<h1>Booking Report</h1>");
            html.AppendLine($"<p><strong>Generated on:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}</p>");
            html.AppendLine($"<p><strong>Total Records:</strong> {bookings.Count}</p>");
            
            html.AppendLine("<table>");
            html.AppendLine("<tr>");
            html.AppendLine("<th>ID</th>");
            html.AppendLine("<th>Tour Title</th>");
            html.AppendLine("<th>Customer Name</th>");
            html.AppendLine("<th>Phone</th>");
            html.AppendLine("<th>Email</th>");
            html.AppendLine("<th>Start Date</th>");
            html.AppendLine("<th>People</th>");
            html.AppendLine("<th>Total Price</th>");
            html.AppendLine("<th>Status</th>");
            html.AppendLine("<th>Payment Status</th>");
            html.AppendLine("<th>Created Time</th>");
            html.AppendLine("<th>Note</th>");
            html.AppendLine("</tr>");

            foreach (var booking in bookings)
            {
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{booking.Id}</td>");
                html.AppendLine($"<td>{EscapeHtml(booking.TourTitle ?? "")}</td>");
                html.AppendLine($"<td>{EscapeHtml(booking.NameCustomer ?? "")}</td>");
                html.AppendLine($"<td>{EscapeHtml(booking.PhoneNumber ?? "")}</td>");
                html.AppendLine($"<td>{EscapeHtml(booking.Email ?? "")}</td>");
                html.AppendLine($"<td>{booking.StartDate:yyyy-MM-dd}</td>");
                html.AppendLine($"<td>{booking.People}</td>");
                html.AppendLine($"<td>${booking.TotalPrice:F2}</td>");
                html.AppendLine($"<td>{EscapeHtml(booking.StatusName ?? "")}</td>");
                html.AppendLine($"<td>{EscapeHtml(booking.PaymentStatus ?? "")}</td>");
                html.AppendLine($"<td>{booking.CreatedTime:yyyy-MM-dd HH:mm:ss}</td>");
                html.AppendLine($"<td>{EscapeHtml(booking.Note ?? "")}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</table>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            // Use UTF-8 with BOM for better compatibility
            var utf8WithBom = new UTF8Encoding(true);
            return utf8WithBom.GetBytes(html.ToString());
        }

        private string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            return text.Replace("&", "&amp;")
                      .Replace("<", "&lt;")
                      .Replace(">", "&gt;")
                      .Replace("\"", "&quot;")
                      .Replace("'", "&#39;")
                      .Replace("\n", "<br>")
                      .Replace("\r", "");
        }
    }
}
