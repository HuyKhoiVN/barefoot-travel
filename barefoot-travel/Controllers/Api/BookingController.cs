using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Booking;
using barefoot_travel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Get all bookings with pagination and filtering
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="sortBy">Sort field (default: CreatedTime)</param>
        /// <param name="sortDirection">Sort direction: asc or desc (default: desc)</param>
        /// <param name="statusTypeId">Filter by status ID</param>
        /// <param name="paymentStatus">Filter by payment status</param>
        /// <param name="startDateFrom">Filter by start date from</param>
        /// <param name="startDateTo">Filter by start date to</param>
        /// <param name="searchAll">Search across all booking properties</param>
        /// <returns>Paged list of bookings</returns>
        [HttpGet("paged")]
        public async Task<PagedResult<BookingDto>> GetBookingsPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "CreatedTime",
            [FromQuery] string sortDirection = "desc",
            [FromQuery] int? statusTypeId = null,
            [FromQuery] string? paymentStatus = null,
            [FromQuery] DateTime? startDateFrom = null,
            [FromQuery] DateTime? startDateTo = null,
            [FromQuery] string? searchAll = null)
        {
            _logger.LogInformation("Getting bookings with filters - Page: {Page}, PageSize: {PageSize}, SortBy: {SortBy}, SortDirection: {SortDirection}, SearchAll: {SearchAll}", 
                page, pageSize, sortBy, sortDirection, searchAll);

            try
            {
                var filter = new BookingFilterDto
                {
                    Page = page,
                    PageSize = pageSize,
                    SortBy = sortBy,
                    SortDirection = sortDirection,
                    StatusTypeId = statusTypeId,
                    PaymentStatus = paymentStatus,
                    StartDateFrom = startDateFrom,
                    StartDateTo = startDateTo,
                    SearchAll = searchAll
                };

                return await _bookingService.GetBookingsFilteredAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings with filters");
                throw;
            }
        }

        /// <summary>
        /// Get filtered bookings with pagination and sorting
        /// </summary>
        /// <param name="filter">Filter parameters</param>
        /// <returns>Paged list of filtered bookings</returns>
        [HttpPost("filter")]
        public async Task<PagedResult<BookingDto>> GetBookingsFiltered([FromBody] BookingFilterDto filter)
        {
            _logger.LogInformation("Filtering bookings with filter: {@Filter}", filter);

            try
            {
                return await _bookingService.GetBookingsFilteredAsync(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering bookings");
                throw;
            }
        }

        /// <summary>
        /// Get a specific booking by ID
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <returns>Booking details</returns>
        [HttpGet("{id}")]
        public async Task<ApiResponse> GetBooking(int id)
        {
            _logger.LogInformation("Getting booking with ID: {BookingId}", id);

            try
            {
                return await _bookingService.GetBookingByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking with ID: {BookingId}", id);
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update booking status
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="dto">Status update data</param>
        /// <returns>Update result</returns>
        [HttpPatch("{id}/status")]
        public async Task<ApiResponse> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto dto)
        {
            _logger.LogInformation("Updating booking status for ID: {BookingId}, Status: {StatusId}", id, dto.StatusTypeId);

            try
            {
                // Get current user from claims
                var updatedBy = GetUserIdFromClaims.GetUsername(User) ?? "Guest";
                
                return await _bookingService.UpdateBookingStatusAsync(id, dto, updatedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status for ID: {BookingId}", id);
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update payment status
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="dto">Payment status update data</param>
        /// <returns>Update result</returns>
        [HttpPatch("{id}/payment")]
        public async Task<ApiResponse> UpdatePaymentStatus(int id, [FromBody] UpdatePaymentStatusDto dto)
        {
            _logger.LogInformation("Updating payment status for ID: {BookingId}, PaymentStatus: {PaymentStatus}", id, dto.PaymentStatus);

            try
            {
                // Get current user from claims
                var updatedBy = GetUserIdFromClaims.GetUsername(User) ?? "Guest";
                
                return await _bookingService.UpdatePaymentStatusAsync(id, dto, updatedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for ID: {BookingId}", id);
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Add internal note to booking
        /// </summary>
        /// <param name="id">Booking ID</param>
        /// <param name="dto">Note data</param>
        /// <returns>Add note result</returns>
        [HttpPost("{id}/note")]
        public async Task<ApiResponse> AddBookingNote(int id, [FromBody] AddBookingNoteDto dto)
        {
            _logger.LogInformation("Adding note to booking ID: {BookingId}", id);

            try
            {
                var updatedBy = GetUserIdFromClaims.GetUsername(User) ?? "Guest";
                
                return await _bookingService.AddBookingNoteAsync(id, dto, updatedBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding note to booking ID: {BookingId}", id);
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get all booking statuses
        /// </summary>
        /// <returns>List of booking statuses</returns>
        [HttpGet("statuses")]
        public async Task<ApiResponse> GetBookingStatuses()
        {
            _logger.LogInformation("Getting booking statuses");

            try
            {
                return await _bookingService.GetBookingStatusesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking statuses");
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Create a new booking
        /// </summary>
        /// <param name="dto">Booking creation data</param>
        /// <returns>Created booking details</returns>
        [HttpPost]
        public async Task<ApiResponse> CreateBooking([FromBody] CreateBookingDto dto)
        {
            _logger.LogInformation("Creating booking for tour ID: {TourId}, Customer: {CustomerName}", dto.TourId, dto.NameCustomer);

            try
            {
                // Get current user from claims
                var createdBy = GetUserIdFromClaims.GetUsername(User) ?? "Guest";

                return await _bookingService.CreateBookingAsync(dto, createdBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking for tour ID: {TourId}", dto.TourId);
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Export bookings to Excel or PDF
        /// </summary>
        /// <param name="exportFilter">Export filter parameters</param>
        /// <returns>Export file data</returns>
        [HttpPost("export")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse> ExportBookings([FromBody] ExportBookingDto exportFilter)
        {
            _logger.LogInformation("Exporting bookings with filter: {@ExportFilter}", exportFilter);

            try
            {
                // Check if user has export permission (Admin only)
                //var userRole = User.FindFirst("role")?.Value;
                //if (userRole != "Admin")
                //{
                //    return new ApiResponse(false, "Access denied. Export permission required.");
                //}

                return await _bookingService.ExportBookingsAsync(exportFilter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting bookings");
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get bookings for calendar view by date range
        /// </summary>
        /// <param name="startDate">Start date for the range</param>
        /// <param name="endDate">End date for the range</param>
        /// <returns>List of bookings grouped by date</returns>
        [HttpGet("calendar")]
        public async Task<ApiResponse> GetBookingsForCalendar(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            _logger.LogInformation("Getting bookings for calendar from {StartDate} to {EndDate}", startDate, endDate);

            try
            {
                return await _bookingService.GetBookingsForCalendarAsync(startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings for calendar");
                return new ApiResponse(false, $"Error: {ex.Message}");
            }
        }
    }
}
