using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Booking;
using barefoot_travel.Models;
using barefoot_travel.Repositories;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<ApiResponse> GetBookingByIdAsync(int id)
        {
            try
            {
                var booking = await _bookingRepository.GetByIdAsync(id);
                if (booking == null)
                {
                    return new ApiResponse(false, "Booking not found");
                }

                // Get related data using JOIN
                var bookingWithDetails = await _bookingRepository.GetBookingsWithDetailsAsync(new List<int> { id });
                if (!bookingWithDetails.Any())
                {
                    return new ApiResponse(false, "Booking not found");
                }

                var bookingDto = MapToBookingDto(bookingWithDetails.First());
                return new ApiResponse(true, "Success", bookingDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        public async Task<PagedResult<BookingDto>> GetBookingsPagedAsync(int page, int pageSize, string sortBy, string sortDirection)
        {
            try
            {
                var pagedResult = await _bookingRepository.GetPagedAsync(page, pageSize, sortBy, sortDirection);
                
                // Get related data using JOIN
                var bookingIds = pagedResult.Items.Select(b => b.Id).ToList();
                var bookingsWithDetails = await _bookingRepository.GetBookingsWithDetailsAsync(bookingIds);

                var bookingDtos = bookingsWithDetails.Select(MapToBookingDto).ToList();

                return new PagedResult<BookingDto>
                {
                    Items = bookingDtos,
                    TotalItems = pagedResult.TotalItems,
                    TotalPages = pagedResult.TotalPages,
                    CurrentPage = pagedResult.CurrentPage,
                    PageSize = pagedResult.PageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting bookings: {ex.Message}");
            }
        }

        public async Task<PagedResult<BookingDto>> GetBookingsFilteredAsync(BookingFilterDto filter)
        {
            try
            {
                // Use the new optimized method that combines filtering, sorting, and joining in one query
                var pagedResult = await _bookingRepository.GetFilteredWithDetailsAsync(filter);
                
                var bookingDtos = pagedResult.Items.Select(MapToBookingDto).ToList();

                return new PagedResult<BookingDto>
                {
                    Items = bookingDtos,
                    TotalItems = pagedResult.TotalItems,
                    TotalPages = pagedResult.TotalPages,
                    CurrentPage = pagedResult.CurrentPage,
                    PageSize = pagedResult.PageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error filtering bookings: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateBookingStatusAsync(int id, UpdateBookingStatusDto dto, string updatedBy)
        {
            try
            {
                // Validate booking exists
                var booking = await _bookingRepository.GetByIdAsync(id);
                if (booking == null)
                {
                    return new ApiResponse(false, "Booking not found");
                }

                // Validate status transition rules
                var validationResult = ValidateStatusTransition(booking.StatusTypeId, dto.StatusTypeId);
                if (!validationResult.IsValid)
                {
                    return new ApiResponse(false, validationResult.ErrorMessage);
                }

                // Use transaction for status update with automatic payment status update
                using var transaction = await _bookingRepository.BeginTransactionAsync();
                try
                {
                    // Update booking status
                    booking.StatusTypeId = dto.StatusTypeId;
                    booking.UpdatedTime = DateTime.UtcNow;
                    booking.UpdatedBy = updatedBy;

                    // Add note if provided
                    if (!string.IsNullOrEmpty(dto.Note))
                    {
                        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                        var statusName = GetStatusNameById(dto.StatusTypeId);
                        var newNote = $"[{timestamp}] Status updated to {statusName}: {dto.Note}";
                        booking.Note = string.IsNullOrEmpty(booking.Note) ? newNote : booking.Note + "\n" + newNote;
                    }

                    // Auto-update payment status based on booking status change
                    var paymentStatusResult = UpdatePaymentStatusForBookingStatus(booking, dto.StatusTypeId);
                    if (!paymentStatusResult.IsValid)
                    {
                        await transaction.RollbackAsync();
                        return new ApiResponse(false, paymentStatusResult.ErrorMessage);
                    }

                    await _bookingRepository.UpdateAsync(booking);
                    await transaction.CommitAsync();

                    return new ApiResponse(true, "Booking status updated successfully");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error updating booking status: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdatePaymentStatusAsync(int id, UpdatePaymentStatusDto dto, string updatedBy)
        {
            try
            {
                // Validate booking exists
                var booking = await _bookingRepository.GetByIdAsync(id);
                if (booking == null)
                {
                    return new ApiResponse(false, "Booking not found");
                }

                // Check if booking status allows payment status update
                if (booking.StatusTypeId == BookingStatusConstant.Cancel || booking.StatusTypeId == BookingStatusConstant.Complete)
                {
                    return new ApiResponse(false, "Cannot update payment status for cancelled or completed bookings");
                }

                // Validate payment status transition rules
                var validationResult = ValidatePaymentStatusTransition(booking.PaymentStatus, dto.PaymentStatus);
                if (!validationResult.IsValid)
                {
                    return new ApiResponse(false, validationResult.ErrorMessage);
                }

                // Use transaction for payment status update
                using var transaction = await _bookingRepository.BeginTransactionAsync();
                try
                {
                    // Update payment status
                    booking.PaymentStatus = dto.PaymentStatus;
                    booking.UpdatedTime = DateTime.UtcNow;
                    booking.UpdatedBy = updatedBy;
                    if (!string.IsNullOrEmpty(dto.Note))
                    {
                        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                        var newNote = $"[{timestamp}] Note added for payment status {dto.PaymentStatus}: {dto.Note}";
                        booking.Note = string.IsNullOrEmpty(booking.Note) ? newNote : booking.Note + "\n" + newNote;
                    }

                    await _bookingRepository.UpdateAsync(booking);
                    await transaction.CommitAsync();

                    return new ApiResponse(true, "Payment status updated successfully");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error updating payment status: {ex.Message}");
            }
        }

        private (bool IsValid, string ErrorMessage) ValidateStatusTransition(int currentStatusId, int newStatusId)
        {
            // Cannot update if current status is Cancel or Complete
            if (currentStatusId == BookingStatusConstant.Cancel || currentStatusId == BookingStatusConstant.Complete)
            {
                return (false, "Cannot update status for cancelled or completed bookings");
            }

            // If current status is InProgress, can only change to Complete
            if (currentStatusId == BookingStatusConstant.InProgress)
            {
                if (newStatusId != BookingStatusConstant.Complete)
                {
                    return (false, "In-progress bookings can only be marked as Complete");
                }
            }

            // Cannot change to Cancel or Complete from Pending or Confirmed (business rule)
            if (newStatusId == BookingStatusConstant.Cancel || newStatusId == BookingStatusConstant.Complete)
            {
                if (currentStatusId == BookingStatusConstant.Pending || currentStatusId == BookingStatusConstant.Confirmed)
                {
                    return (false, "Cannot directly change to Cancel or Complete from Pending or Confirmed status");
                }
            }

            return (true, string.Empty);
        }

        private (bool IsValid, string ErrorMessage) ValidatePaymentStatusTransition(string currentPaymentStatus, string newPaymentStatus)
        {
            // Only allow specific transitions
            if (currentPaymentStatus == PaymentStatusConstant.PENDING)
            {
                if (newPaymentStatus != PaymentStatusConstant.PAID && newPaymentStatus != PaymentStatusConstant.CANCELLED)
                {
                    return (false, "Pending payments can only be changed to Paid or Cancelled");
                }
            }
            else if (currentPaymentStatus == PaymentStatusConstant.PAID)
            {
                if (newPaymentStatus != PaymentStatusConstant.CANCELLED) // Using CANCELLED as REFUNDED
                {
                    return (false, "Paid payments can only be changed to Refunded");
                }
            }
            else
            {
                return (false, "Cannot update payment status from current status");
            }

            return (true, string.Empty);
        }

        private (bool IsValid, string ErrorMessage) UpdatePaymentStatusForBookingStatus(Booking booking, int newStatusId)
        {
            // Only update payment status when booking status changes to Cancel
            if (newStatusId == BookingStatusConstant.Cancel)
            {
                if (booking.PaymentStatus == PaymentStatusConstant.PENDING)
                {
                    booking.PaymentStatus = PaymentStatusConstant.CANCELLED;
                }
                else if (booking.PaymentStatus == PaymentStatusConstant.PAID)
                {
                    booking.PaymentStatus = PaymentStatusConstant.REFUNDED;
                }
            }

            return (true, string.Empty);
        }

        public async Task<ApiResponse> AddBookingNoteAsync(int id, AddBookingNoteDto dto, string updatedBy)
        {
            try
            {
                // Validate booking exists
                var booking = await _bookingRepository.GetByIdAsync(id);
                if (booking == null)
                {
                    return new ApiResponse(false, "Booking not found");
                }

                // Validate note is not empty
                if (string.IsNullOrWhiteSpace(dto.Note))
                {
                    return new ApiResponse(false, "Note cannot be empty");
                }

                // Append note with timestamp
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                var newNote = $"[{timestamp}] Note added: {dto.Note}";
                booking.Note = string.IsNullOrEmpty(booking.Note) ? newNote : booking.Note + "\n" + newNote;
                booking.UpdatedTime = DateTime.UtcNow;
                booking.UpdatedBy = updatedBy;

                await _bookingRepository.UpdateAsync(booking);

                return new ApiResponse(true, "Note added successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error adding note: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetBookingStatusesAsync()
        {
            try
            {
                var statuses = await _bookingRepository.GetBookingStatusesAsync();
                var statusDtos = statuses.Select(s => new { s.Id, s.StatusName }).ToList();
                return new ApiResponse(true, "Success", statusDtos);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error getting booking statuses: {ex.Message}");
            }
        }

        public async Task<ApiResponse> ExportBookingsAsync(ExportBookingDto exportFilter)
        {
            try
            {
                // Validate date range - maximum 3 months
                if (exportFilter.StartDateFrom.HasValue && exportFilter.StartDateTo.HasValue)
                {
                    var startDate = exportFilter.StartDateFrom.Value;
                    var endDate = exportFilter.StartDateTo.Value;
                    var threeMonthsLater = startDate.AddMonths(3);
                    
                    if (endDate > threeMonthsLater)
                    {
                        return new ApiResponse(false, "Export date range cannot exceed 3 months");
                    }
                }
                else if (exportFilter.CreatedTimeFrom.HasValue && exportFilter.CreatedTimeTo.HasValue)
                {
                    var startDate = exportFilter.CreatedTimeFrom.Value;
                    var endDate = exportFilter.CreatedTimeTo.Value;
                    var threeMonthsLater = startDate.AddMonths(3);
                    
                    if (endDate > threeMonthsLater)
                    {
                        return new ApiResponse(false, "Export date range cannot exceed 3 months");
                    }
                }

                // Get bookings with details using JOIN
                var bookingsWithDetails = await _bookingRepository.GetBookingsWithDetailsForExportAsync(exportFilter);
                var bookingDtos = bookingsWithDetails.Select(MapToBookingDto).ToList();

                // Generate export file
                var exportService = new ExportService();
                var fileBytes = exportService.GenerateBookingReport(bookingDtos, exportFilter.ExportFormat, exportFilter.StartDateFrom, exportFilter.StartDateTo);

                // Convert to base64 with proper encoding
                var base64String = Convert.ToBase64String(fileBytes);
                
                // Determine file extension and content type
                var fileExtension = exportFilter.ExportFormat.ToLower() == "pdf" ? "html" : "xlsx"; // PDF is actually HTML for now
                var contentType = exportFilter.ExportFormat.ToLower() == "pdf" ? "text/html" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return new ApiResponse(true, "Export generated successfully", new { 
                    FileBytes = base64String,
                    FileName = $"Bookings_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{fileExtension}",
                    ContentType = contentType,
                    FileSize = fileBytes.Length
                });
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error exporting bookings: {ex.Message}");
            }
        }

        public async Task<ApiResponse> CreateBookingAsync(CreateBookingDto dto, string createdBy)
        {
            try
            {
                // Validate tour exists
                var tour = await _bookingRepository.GetTourByIdAsync(dto.TourId);
                if (tour == null)
                {
                    return new ApiResponse(false, "Tour not found");
                }

                // Validate user exists if provided
                if (dto.UserId.HasValue)
                {
                    var user = await _bookingRepository.GetAccountByIdAsync(dto.UserId.Value);
                    if (user == null)
                    {
                        return new ApiResponse(false, "User not found");
                    }
                }

                // Validate number of people does not exceed tour capacity
                if (dto.People > tour.MaxPeople)
                {
                    return new ApiResponse(false, $"Number of people ({dto.People}) exceeds tour capacity ({tour.MaxPeople})");
                }

                // Calculate total price
                var totalPrice = tour.PricePerPerson * dto.People;

                // Create booking entity
                var booking = new Booking
                {
                    TourId = dto.TourId,
                    UserId = dto.UserId,
                    StartDate = dto.StartDate,
                    People = dto.People,
                    PhoneNumber = dto.PhoneNumber,
                    NameCustomer = dto.NameCustomer,
                    Email = dto.Email,
                    Note = dto.Note,
                    TotalPrice = totalPrice,
                    StatusTypeId = 1, // Default to Pending status
                    PaymentStatus = dto.PaymentStatus,
                    Active = true,
                    CreatedTime = DateTime.UtcNow,
                    UpdatedTime = null,
                    UpdatedBy = null
                };

                // Save booking
                var createdBooking = await _bookingRepository.CreateAsync(booking);

                // Get booking with details for response
                var bookingWithDetails = await _bookingRepository.GetBookingsWithDetailsAsync(new List<int> { createdBooking.Id });
                if (!bookingWithDetails.Any())
                {
                    return new ApiResponse(false, "Error retrieving created booking");
                }

                var bookingDto = MapToBookingDto(bookingWithDetails.First());

                return new ApiResponse(true, "Booking created successfully", bookingDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error creating booking: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetBookingsForCalendarAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Debug logging for date parameters
                Console.WriteLine($"GetBookingsForCalendarAsync called with startDate: {startDate:yyyy-MM-dd HH:mm:ss}, endDate: {endDate:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine($"startDate.Date: {startDate.Date:yyyy-MM-dd}, endDate.Date: {endDate.Date:yyyy-MM-dd}");
                
                // Validate date range (max 3 months)
                //var maxEndDate = startDate.AddMonths(3);
                //if (endDate > maxEndDate)
                //{
                //    return new ApiResponse(false, "Date range cannot exceed 3 months");
                //}

                // Get bookings in date range
                var bookings = await _bookingRepository.GetBookingsByDateRangeAsync(startDate, endDate);
                
                // Debug logging
                Console.WriteLine($"Found {bookings.Count} bookings in date range {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");
                foreach (var booking in bookings.Take(5)) // Log first 5 bookings
                {
                    Console.WriteLine($"Booking {booking.Id}: {booking.TourTitle} on {booking.StartDate:yyyy-MM-dd}");
                }
                
                // Group bookings by date and month
                var calendarView = CreateCalendarView(startDate, endDate, bookings);
                
                return new ApiResponse(true, "Calendar data retrieved successfully", calendarView);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error getting calendar data: {ex.Message}");
            }
        }

        private CalendarViewDto CreateCalendarView(DateTime startDate, DateTime endDate, List<BookingWithDetailsDto> bookings)
        {
            var calendarView = new CalendarViewDto
            {
                StartDate = startDate,
                EndDate = endDate,
                Months = new List<CalendarMonthDto>()
            };

            // Group bookings by date
            var bookingsByDate = bookings.GroupBy(b => b.StartDate?.Date ?? DateTime.MinValue)
                .ToDictionary(g => g.Key, g => g.ToList());

            Console.WriteLine($"Grouped bookings by date. Keys: {string.Join(", ", bookingsByDate.Keys.Select(d => d.ToString("yyyy-MM-dd")))}");

            // Create months - ensure we start from the beginning of the start month
            var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
            var endMonth = new DateTime(endDate.Year, endDate.Month, 1);

            Console.WriteLine($"Creating months from {currentDate:yyyy-MM-dd} to {endMonth:yyyy-MM-dd}");

            while (currentDate <= endMonth)
            {
                var monthDto = new CalendarMonthDto
                {
                    Year = currentDate.Year,
                    Month = currentDate.Month,
                    MonthName = currentDate.ToString("MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture),
                    Days = new List<CalendarDayDto>()
                };

                Console.WriteLine($"Creating month: {monthDto.MonthName} (Year: {monthDto.Year}, Month: {monthDto.Month})");

                // Get days in month
                var daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(currentDate.Year, currentDate.Month, day);
                    
                    // Skip if date is outside range
                    if (date < startDate.Date || date > endDate.Date)
                        continue;

                    // Only create day if there are bookings for this date
                    if (bookingsByDate.ContainsKey(date.Date))
                    {
                        var dayBookings = bookingsByDate[date.Date];
                        var dayDto = new CalendarDayDto
                        {
                            Date = date,
                            Bookings = dayBookings.Select(MapToCalendarBookingDto).ToList(),
                            TotalBookings = dayBookings.Count,
                            TotalPeople = dayBookings.Sum(b => b.People),
                            TotalRevenue = dayBookings.Sum(b => b.TotalPrice)
                        };
                        
                        Console.WriteLine($"Added {dayBookings.Count} bookings for {date:yyyy-MM-dd}");
                        monthDto.Days.Add(dayDto);
                    }
                }

                // Calculate month totals
                monthDto.TotalBookings = monthDto.Days.Sum(d => d.TotalBookings);
                monthDto.TotalPeople = monthDto.Days.Sum(d => d.TotalPeople);
                monthDto.TotalRevenue = monthDto.Days.Sum(d => d.TotalRevenue);

                Console.WriteLine($"Month {monthDto.MonthName}: {monthDto.TotalBookings} bookings, {monthDto.TotalPeople} people, ${monthDto.TotalRevenue:F2} revenue");
                calendarView.Months.Add(monthDto);
                currentDate = currentDate.AddMonths(1);
            }

            // Calculate overall totals
            calendarView.TotalBookings = calendarView.Months.Sum(m => m.TotalBookings);
            calendarView.TotalPeople = calendarView.Months.Sum(m => m.TotalPeople);
            calendarView.TotalRevenue = calendarView.Months.Sum(m => m.TotalRevenue);

            Console.WriteLine($"Calendar view created with {calendarView.Months.Count} months");
            return calendarView;
        }

        private CalendarBookingDto MapToCalendarBookingDto(BookingWithDetailsDto booking)
        {
            return new CalendarBookingDto
            {
                Id = booking.Id,
                TourTitle = booking.TourTitle,
                StartDate = booking.StartDate ?? DateTime.MinValue,
                People = booking.People,
                StatusName = booking.StatusName,
                PaymentStatus = booking.PaymentStatus,
                NameCustomer = booking.NameCustomer,
                PhoneNumber = booking.PhoneNumber,
                Email = booking.Email,
                TotalPrice = booking.TotalPrice,
                Note = booking.Note
            };
        }

        // Manual mapping methods
        private BookingDto MapToBookingDto(BookingWithDetailsDto booking)
        {
            if (booking == null) return null;

            return new BookingDto
            {
                Id = booking.Id,
                TourId = booking.TourId,
                TourTitle = booking.TourTitle,
                UserId = booking.UserId,
                UserFullName = booking.UserFullName ?? "Guest",
                StartDate = booking.StartDate,
                People = booking.People,
                PhoneNumber = booking.PhoneNumber,
                NameCustomer = booking.NameCustomer,
                Email = booking.Email,
                Note = booking.Note,
                TotalPrice = booking.TotalPrice,
                StatusTypeId = booking.StatusTypeId,
                StatusName = booking.StatusName,
                PaymentStatus = booking.PaymentStatus,
                CreatedTime = booking.CreatedTime,
                UpdatedTime = booking.UpdatedTime,
                UpdatedBy = booking.UpdatedBy,
                Active = booking.Active
            };
        }

        private string GetStatusNameById(int statusId)
        {
            return statusId switch
            {
                BookingStatusConstant.Pending => "Pending",
                BookingStatusConstant.Confirmed => "Confirmed",
                BookingStatusConstant.InProgress => "In Progress",
                BookingStatusConstant.Cancel => "Cancel",
                BookingStatusConstant.Complete => "Complete",
                _ => "Unknown"
            };
        }
    }
}
