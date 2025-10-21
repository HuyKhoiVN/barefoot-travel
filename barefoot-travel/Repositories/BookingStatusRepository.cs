using Microsoft.EntityFrameworkCore;
using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public class BookingStatusRepository : IBookingStatusRepository
    {
        private readonly SysDbContext _context;

        public BookingStatusRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<BookingStatus?> GetByIdAsync(int id)
        {
            return await _context.BookingStatuses
                .Where(bs => bs.Id == id && bs.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<List<BookingStatus>> GetAllAsync()
        {
            return await _context.BookingStatuses
                .Where(bs => bs.Active)
                .OrderBy(bs => bs.StatusName)
                .ToListAsync();
        }

        public async Task<BookingStatus> CreateAsync(BookingStatus bookingStatus)
        {
            _context.BookingStatuses.Add(bookingStatus);
            await _context.SaveChangesAsync();
            return bookingStatus;
        }

        public async Task<BookingStatus> UpdateAsync(BookingStatus bookingStatus)
        {
            _context.BookingStatuses.Update(bookingStatus);
            await _context.SaveChangesAsync();
            return bookingStatus;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var bookingStatus = await _context.BookingStatuses.FindAsync(id);
            if (bookingStatus == null) return false;

            bookingStatus.Active = false;
            bookingStatus.UpdatedTime = DateTime.UtcNow;
            
            _context.BookingStatuses.Update(bookingStatus);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.BookingStatuses
                .AnyAsync(bs => bs.Id == id && bs.Active);
        }

        public async Task<bool> ExistsByNameAsync(string statusName, int? excludeId = null)
        {
            var query = _context.BookingStatuses
                .Where(bs => bs.StatusName == statusName && bs.Active);

            if (excludeId.HasValue)
            {
                query = query.Where(bs => bs.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
