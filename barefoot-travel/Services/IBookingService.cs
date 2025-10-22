using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Booking;

namespace barefoot_travel.Services
{
    public interface IBookingService
    {
        Task<ApiResponse> GetBookingByIdAsync(int id);
        Task<PagedResult<BookingDto>> GetBookingsPagedAsync(int page, int pageSize, string sortBy, string sortDirection);
        Task<PagedResult<BookingDto>> GetBookingsFilteredAsync(BookingFilterDto filter);
        Task<ApiResponse> UpdateBookingStatusAsync(int id, UpdateBookingStatusDto dto, string updatedBy);
        Task<ApiResponse> UpdatePaymentStatusAsync(int id, UpdatePaymentStatusDto dto, string updatedBy);
        Task<ApiResponse> AddBookingNoteAsync(int id, AddBookingNoteDto dto, string updatedBy);
        Task<ApiResponse> GetBookingStatusesAsync();
        Task<ApiResponse> ExportBookingsAsync(ExportBookingDto exportFilter);
        Task<ApiResponse> CreateBookingAsync(CreateBookingDto dto, string createdBy);
    }
}
