using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface IBookingStatusRepository
    {
        Task<BookingStatus?> GetByIdAsync(int id);
        Task<List<BookingStatus>> GetAllAsync();
        Task<BookingStatus> CreateAsync(BookingStatus bookingStatus);
        Task<BookingStatus> UpdateAsync(BookingStatus bookingStatus);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> ExistsByNameAsync(string statusName, int? excludeId = null);
    }
}
