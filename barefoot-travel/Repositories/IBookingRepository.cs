using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Booking;
using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace barefoot_travel.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(int id);
        Task<PagedResult<Booking>> GetPagedAsync(int page, int pageSize, string sortBy, string sortDirection);
        Task<PagedResult<Booking>> GetFilteredAsync(BookingFilterDto filter);
        Task<PagedResult<BookingWithDetailsDto>> GetFilteredWithDetailsAsync(BookingFilterDto filter);
        Task<List<Booking>> GetForExportAsync(ExportBookingDto exportFilter);
        Task<Booking> CreateAsync(Booking booking);
        Task<Booking> UpdateAsync(Booking booking);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<List<BookingStatus>> GetBookingStatusesAsync();
        Task<BookingStatus?> GetBookingStatusByIdAsync(int id);
        Task<Tour?> GetTourByIdAsync(int id);
        Task<Account?> GetAccountByIdAsync(int id);
        Task<List<BookingWithDetailsDto>> GetBookingsWithDetailsAsync(List<int> bookingIds);
        Task<List<BookingWithDetailsDto>> GetBookingsWithDetailsForExportAsync(ExportBookingDto exportFilter);
        Task<List<BookingWithDetailsDto>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
