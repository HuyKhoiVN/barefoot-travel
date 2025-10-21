using barefoot_travel.Common;
using barefoot_travel.DTOs.BookingStatus;

namespace barefoot_travel.Services
{
    public interface IBookingStatusService
    {
        Task<ApiResponse> GetBookingStatusByIdAsync(int id);
        Task<ApiResponse> GetAllBookingStatusesAsync();
        Task<ApiResponse> CreateBookingStatusAsync(CreateBookingStatusEntityDto dto);
        Task<ApiResponse> UpdateBookingStatusAsync(int id, UpdateBookingStatusEntityDto dto);
        Task<ApiResponse> DeleteBookingStatusAsync(int id);
    }
}
