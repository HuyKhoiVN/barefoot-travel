using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Booking;
using barefoot_travel.Models;
using barefoot_travel.Repositories;

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
                var pagedResult = await _bookingRepository.GetFilteredAsync(filter);
                
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

                // Validate status exists
                var status = await _bookingRepository.GetBookingStatusByIdAsync(dto.StatusTypeId);
                if (status == null)
                {
                    return new ApiResponse(false, "Invalid status ID");
                }

                // Update booking status
                booking.StatusTypeId = dto.StatusTypeId;
                booking.UpdatedTime = DateTime.UtcNow;
                booking.UpdatedBy = updatedBy;

                // Append note if provided
                if (!string.IsNullOrEmpty(dto.Note))
                {
                    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                    var statusChangeNote = $"\n[{timestamp}] Status changed to {status.StatusName} by {updatedBy}: {dto.Note}";
                    booking.Note = (booking.Note ?? "") + statusChangeNote;
                }

                await _bookingRepository.UpdateAsync(booking);

                return new ApiResponse(true, "Booking status updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error updating booking status: {ex.Message}");
            }
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
                var newNote = $"\n[{timestamp}] Note added by {updatedBy}: {dto.Note}";
                booking.Note = (booking.Note ?? "") + newNote;
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
                // Get bookings with details using JOIN
                var bookingsWithDetails = await _bookingRepository.GetBookingsWithDetailsForExportAsync(exportFilter);
                var bookingDtos = bookingsWithDetails.Select(MapToBookingDto).ToList();

                // Generate export file
                var exportService = new ExportService();
                var fileBytes = exportService.GenerateBookingReport(bookingDtos, exportFilter.ExportFormat);

                return new ApiResponse(true, "Export generated successfully", new { 
                    FileBytes = Convert.ToBase64String(fileBytes),
                    FileName = $"Bookings_Export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.{exportFilter.ExportFormat.ToLower()}",
                    ContentType = exportFilter.ExportFormat.ToLower() == "pdf" ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
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
    }
}
