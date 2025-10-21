using Microsoft.AspNetCore.Mvc;
using barefoot_travel.Common;
using barefoot_travel.DTOs.BookingStatus;
using barefoot_travel.Services;
using Microsoft.AspNetCore.Authorization;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingStatusController : ControllerBase
    {
        private readonly IBookingStatusService _bookingStatusService;
        private readonly ILogger<BookingStatusController> _logger;

        public BookingStatusController(IBookingStatusService bookingStatusService, ILogger<BookingStatusController> logger)
        {
            _bookingStatusService = bookingStatusService;
            _logger = logger;
        }

        /// <summary>
        /// Gets all booking statuses
        /// </summary>
        /// <returns>List of all booking statuses</returns>
        /// <response code="200">Success</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        public async Task<ApiResponse> GetAllBookingStatuses()
        {
            _logger.LogInformation("Getting all booking statuses");

            try
            {
                var result = await _bookingStatusService.GetAllBookingStatusesAsync();
                _logger.LogInformation("All booking statuses retrieved successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all booking statuses");
                return new ApiResponse(false, "An error occurred while retrieving booking statuses");
            }
        }

        /// <summary>
        /// Gets a booking status by ID
        /// </summary>
        /// <param name="id">Booking status ID</param>
        /// <returns>Booking status details</returns>
        /// <response code="200">Booking status found</response>
        /// <response code="404">Booking status not found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        public async Task<ApiResponse> GetBookingStatus(int id)
        {
            _logger.LogInformation("Getting booking status with ID: {BookingStatusId}", id);

            try
            {
                var result = await _bookingStatusService.GetBookingStatusByIdAsync(id);
                _logger.LogInformation("Booking status retrieved successfully with ID: {BookingStatusId}", id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking status with ID: {BookingStatusId}", id);
                return new ApiResponse(false, "An error occurred while retrieving booking status");
            }
        }

        /// <summary>
        /// Creates a new booking status
        /// </summary>
        /// <param name="dto">Booking status creation data</param>
        /// <returns>Created booking status</returns>
        /// <response code="200">Booking status created successfully</response>
        /// <response code="400">Validation failed</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        public async Task<ApiResponse> CreateBookingStatus([FromBody] CreateBookingStatusEntityDto dto)
        {
            _logger.LogInformation("Creating new booking status: {StatusName}", dto.StatusName);

            try
            {
                var result = await _bookingStatusService.CreateBookingStatusAsync(dto);
                _logger.LogInformation("Booking status created successfully: {StatusName}", dto.StatusName);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking status: {StatusName}", dto.StatusName);
                return new ApiResponse(false, "An error occurred while creating booking status");
            }
        }

        /// <summary>
        /// Updates an existing booking status
        /// </summary>
        /// <param name="id">Booking status ID</param>
        /// <param name="dto">Booking status update data</param>
        /// <returns>Updated booking status</returns>
        /// <response code="200">Booking status updated successfully</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Booking status not found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpPut("{id}")]
        public async Task<ApiResponse> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusEntityDto dto)
        {
            _logger.LogInformation("Updating booking status with ID: {BookingStatusId}", id);

            try
            {
                var result = await _bookingStatusService.UpdateBookingStatusAsync(id, dto);
                _logger.LogInformation("Booking status updated successfully with ID: {BookingStatusId}", id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status with ID: {BookingStatusId}", id);
                return new ApiResponse(false, "An error occurred while updating booking status");
            }
        }

        /// <summary>
        /// Deletes a booking status (soft delete)
        /// </summary>
        /// <param name="id">Booking status ID</param>
        /// <returns>Deletion result</returns>
        /// <response code="200">Booking status deleted successfully</response>
        /// <response code="404">Booking status not found</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="500">Internal server error</response>
        [HttpDelete("{id}")]
        public async Task<ApiResponse> DeleteBookingStatus(int id)
        {
            _logger.LogInformation("Deleting booking status with ID: {BookingStatusId}", id);

            try
            {
                var result = await _bookingStatusService.DeleteBookingStatusAsync(id);
                _logger.LogInformation("Booking status deleted successfully with ID: {BookingStatusId}", id);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking status with ID: {BookingStatusId}", id);
                return new ApiResponse(false, "An error occurred while deleting booking status");
            }
        }
    }
}
