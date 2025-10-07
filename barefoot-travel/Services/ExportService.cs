using barefoot_travel.DTOs.Booking;
using System.Text;

namespace barefoot_travel.Services
{
    public class ExportService
    {
        public byte[] GenerateBookingReport(List<BookingDto> bookings, string format)
        {
            if (format.ToLower() == "excel")
            {
                return GenerateExcelReport(bookings);
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

        private byte[] GenerateExcelReport(List<BookingDto> bookings)
        {
            var csv = new StringBuilder();
            
            // Add headers
            csv.AppendLine("ID,Tour Title,Customer Name,Phone,Email,Start Date,People,Total Price,Status,Payment Status,Created Time,Note");

            // Add data rows
            foreach (var booking in bookings)
            {
                csv.AppendLine($"{booking.Id}," +
                              $"\"{booking.TourTitle}\"," +
                              $"\"{booking.NameCustomer}\"," +
                              $"\"{booking.PhoneNumber}\"," +
                              $"\"{booking.Email ?? ""}\"," +
                              $"{booking.StartDate:yyyy-MM-dd}," +
                              $"{booking.People}," +
                              $"{booking.TotalPrice}," +
                              $"\"{booking.StatusName}\"," +
                              $"\"{booking.PaymentStatus}\"," +
                              $"{booking.CreatedTime:yyyy-MM-dd HH:mm:ss}," +
                              $"\"{booking.Note?.Replace("\"", "\"\"") ?? ""}\"");
            }

            return Encoding.UTF8.GetBytes(csv.ToString());
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
            html.AppendLine("table { border-collapse: collapse; width: 100%; }");
            html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #f2f2f2; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<h1>Booking Report</h1>");
            html.AppendLine($"<p>Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}</p>");
            html.AppendLine($"<p>Total Records: {bookings.Count}</p>");
            
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
                html.AppendLine($"<td>{booking.TourTitle}</td>");
                html.AppendLine($"<td>{booking.NameCustomer}</td>");
                html.AppendLine($"<td>{booking.PhoneNumber}</td>");
                html.AppendLine($"<td>{booking.Email ?? ""}</td>");
                html.AppendLine($"<td>{booking.StartDate:yyyy-MM-dd}</td>");
                html.AppendLine($"<td>{booking.People}</td>");
                html.AppendLine($"<td>{booking.TotalPrice:C}</td>");
                html.AppendLine($"<td>{booking.StatusName}</td>");
                html.AppendLine($"<td>{booking.PaymentStatus}</td>");
                html.AppendLine($"<td>{booking.CreatedTime:yyyy-MM-dd HH:mm:ss}</td>");
                html.AppendLine($"<td>{booking.Note ?? ""}</td>");
                html.AppendLine("</tr>");
            }

            html.AppendLine("</table>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            // For simplicity, return HTML as bytes
            // In a real implementation, you would use a PDF library like iTextSharp or PdfSharp
            return Encoding.UTF8.GetBytes(html.ToString());
        }
    }
}
