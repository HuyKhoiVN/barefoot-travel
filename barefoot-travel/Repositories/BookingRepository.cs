using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Booking;
using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace barefoot_travel.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly SysDbContext _context;

        public BookingRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .Where(b => b.Id == id && b.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<Booking>> GetPagedAsync(int page, int pageSize, string sortBy, string sortDirection)
        {
            var query = from b in _context.Bookings
                        where b.Active
                        select b;

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "startdate" => sortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.StartDate) 
                    : query.OrderByDescending(b => b.StartDate),
                "createdtime" => sortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.CreatedTime) 
                    : query.OrderByDescending(b => b.CreatedTime),
                "totalprice" => sortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.TotalPrice) 
                    : query.OrderByDescending(b => b.TotalPrice),
                "namecustomer" => sortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.NameCustomer) 
                    : query.OrderByDescending(b => b.NameCustomer),
                "numberofpeople" => sortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.People) 
                    : query.OrderByDescending(b => b.People),
                "paymentstatus" => sortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.PaymentStatus) 
                    : query.OrderByDescending(b => b.PaymentStatus),
                "statustypename" => sortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.StatusTypeId) 
                    : query.OrderByDescending(b => b.StatusTypeId),
                "tourtitle" => sortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.TourId) 
                    : query.OrderByDescending(b => b.TourId),
                _ => query.OrderByDescending(b => b.CreatedTime)
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Booking>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<Booking>> GetFilteredAsync(BookingFilterDto filter)
        {
            var query = from b in _context.Bookings
                        where b.Active
                        select b;

            // Apply filters
            if (filter.StatusTypeId.HasValue)
                query = from b in query where b.StatusTypeId == filter.StatusTypeId.Value select b;

            if (filter.TourId.HasValue)
                query = from b in query where b.TourId == filter.TourId.Value select b;

            if (filter.UserId.HasValue)
                query = from b in query where b.UserId == filter.UserId.Value select b;

            if (filter.StartDateFrom.HasValue)
                query = from b in query where b.StartDate >= filter.StartDateFrom.Value select b;

            if (filter.StartDateTo.HasValue)
                query = from b in query where b.StartDate <= filter.StartDateTo.Value select b;

            if (filter.CreatedTimeFrom.HasValue)
                query = from b in query where b.CreatedTime >= filter.CreatedTimeFrom.Value select b;

            if (filter.CreatedTimeTo.HasValue)
                query = from b in query where b.CreatedTime <= filter.CreatedTimeTo.Value select b;

            if (!string.IsNullOrEmpty(filter.SearchAll))
            {
                string searchAll = filter.SearchAll.Trim().ToLower();
                query = query.Where(b =>
                    EF.Functions.Collate(b.Id.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.TourId.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.UserId.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.People.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.PhoneNumber.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.NameCustomer.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.Email.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.Note.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.TotalPrice.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.StatusTypeId.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.PaymentStatus.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.Active.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.UpdatedBy.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General))
                );
            }

            if (!string.IsNullOrEmpty(filter.PaymentStatus))
                query = from b in query where b.PaymentStatus.Contains(filter.PaymentStatus) select b;

            // Apply sorting
            query = filter.SortBy.ToLower() switch
            {
                "startdate" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.StartDate) 
                    : query.OrderByDescending(b => b.StartDate),
                "createdtime" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.CreatedTime) 
                    : query.OrderByDescending(b => b.CreatedTime),
                "totalprice" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.TotalPrice) 
                    : query.OrderByDescending(b => b.TotalPrice),
                "namecustomer" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.NameCustomer) 
                    : query.OrderByDescending(b => b.NameCustomer),
                "numberofpeople" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.People) 
                    : query.OrderByDescending(b => b.People),
                "paymentstatus" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.PaymentStatus) 
                    : query.OrderByDescending(b => b.PaymentStatus),
                "statustypename" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.StatusTypeId) 
                    : query.OrderByDescending(b => b.StatusTypeId),
                "tourtitle" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.TourId) 
                    : query.OrderByDescending(b => b.TourId),
                _ => query.OrderByDescending(b => b.CreatedTime)
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Booking>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<List<Booking>> GetForExportAsync(ExportBookingDto exportFilter)
        {
            var query = from b in _context.Bookings
                        where b.Active
                        select b;

            // Apply export filters
            if (exportFilter.StatusTypeId.HasValue)
                query = from b in query where b.StatusTypeId == exportFilter.StatusTypeId.Value select b;

            if (exportFilter.TourId.HasValue)
                query = from b in query where b.TourId == exportFilter.TourId.Value select b;

            if (exportFilter.StartDateFrom.HasValue)
                query = from b in query where b.StartDate >= exportFilter.StartDateFrom.Value select b;

            if (exportFilter.StartDateTo.HasValue)
                query = from b in query where b.StartDate <= exportFilter.StartDateTo.Value select b;

            if (exportFilter.CreatedTimeFrom.HasValue)
                query = from b in query where b.CreatedTime >= exportFilter.CreatedTimeFrom.Value select b;

            if (exportFilter.CreatedTimeTo.HasValue)
                query = from b in query where b.CreatedTime <= exportFilter.CreatedTimeTo.Value select b;

            return await query
                .OrderByDescending(b => b.CreatedTime)
                .ToListAsync();
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking> UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return false;

            booking.Active = false;
            booking.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Bookings
                .AnyAsync(b => b.Id == id && b.Active);
        }

        public async Task<List<BookingStatus>> GetBookingStatusesAsync()
        {
            return await _context.BookingStatuses
                .Where(bs => bs.Active)
                .OrderBy(bs => bs.Id)
                .ToListAsync();
        }

        public async Task<BookingStatus?> GetBookingStatusByIdAsync(int id)
        {
            return await _context.BookingStatuses
                .Where(bs => bs.Id == id && bs.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<Tour?> GetTourByIdAsync(int id)
        {
            return await _context.Tours
                .Where(t => t.Id == id && t.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<Account?> GetAccountByIdAsync(int id)
        {
            return await _context.Accounts
                .Where(a => a.Id == id && a.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<BookingWithDetailsDto>> GetFilteredWithDetailsAsync(BookingFilterDto filter)
        {
            var query = from b in _context.Bookings
                        join t in _context.Tours on b.TourId equals t.Id into tourGroup
                        from t in tourGroup.DefaultIfEmpty()
                        join a in _context.Accounts on b.UserId equals a.Id into accountGroup
                        from a in accountGroup.DefaultIfEmpty()
                        join bs in _context.BookingStatuses on b.StatusTypeId equals bs.Id into statusGroup
                        from bs in statusGroup.DefaultIfEmpty()
                        where b.Active && (t == null || t.Active) && (a == null || a.Active) && (bs == null || bs.Active)
                        select new BookingWithDetailsDto
                        {
                            Id = b.Id,
                            TourId = b.TourId,
                            TourTitle = t != null ? t.Title : "N/A",
                            UserId = b.UserId,
                            UserFullName = a != null ? a.FullName : "Guest",
                            StartDate = b.StartDate,
                            People = b.People,
                            PhoneNumber = b.PhoneNumber,
                            NameCustomer = b.NameCustomer ?? "N/A",
                            Email = b.Email,
                            Note = b.Note,
                            TotalPrice = b.TotalPrice,
                            StatusTypeId = b.StatusTypeId,
                            StatusName = bs != null ? bs.StatusName : "Unknown",
                            PaymentStatus = b.PaymentStatus,
                            CreatedTime = b.CreatedTime,
                            UpdatedTime = b.UpdatedTime,
                            UpdatedBy = b.UpdatedBy,
                            Active = b.Active
                        };

            // Apply filters
            if (filter.StatusTypeId.HasValue)
                query = query.Where(b => b.StatusTypeId == filter.StatusTypeId.Value);

            if (filter.TourId.HasValue)
                query = query.Where(b => b.TourId == filter.TourId.Value);

            if (filter.UserId.HasValue)
                query = query.Where(b => b.UserId == filter.UserId.Value);

            if (filter.StartDateFrom.HasValue)
                query = query.Where(b => b.StartDate >= filter.StartDateFrom.Value);

            if (filter.StartDateTo.HasValue)
                query = query.Where(b => b.StartDate <= filter.StartDateTo.Value);

            if (filter.CreatedTimeFrom.HasValue)
                query = query.Where(b => b.CreatedTime >= filter.CreatedTimeFrom.Value);

            if (filter.CreatedTimeTo.HasValue)
                query = query.Where(b => b.CreatedTime <= filter.CreatedTimeTo.Value);

            if (!string.IsNullOrEmpty(filter.SearchAll))
            {
                string searchAll = filter.SearchAll.Trim().ToLower();
                query = query.Where(b =>
                    EF.Functions.Collate(b.Id.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.TourId.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.UserId.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.People.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.PhoneNumber.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.NameCustomer.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.Email.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.Note.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.TotalPrice.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.StatusTypeId.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.PaymentStatus.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.Active.ToString().ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.UpdatedBy.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.TourTitle.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General)) ||
                    EF.Functions.Collate(b.StatusName.ToLower(), SQLParams.Latin_General).Contains(EF.Functions.Collate(searchAll, SQLParams.Latin_General))
                );
            }

            if (!string.IsNullOrEmpty(filter.PaymentStatus))
                query = query.Where(b => b.PaymentStatus.Contains(filter.PaymentStatus));

            // Apply sorting - this is the key fix!
            query = filter.SortBy.ToLower() switch
            {
                "startdate" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.StartDate) 
                    : query.OrderByDescending(b => b.StartDate),
                "createdtime" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.CreatedTime) 
                    : query.OrderByDescending(b => b.CreatedTime),
                "totalprice" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.TotalPrice) 
                    : query.OrderByDescending(b => b.TotalPrice),
                "namecustomer" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.NameCustomer) 
                    : query.OrderByDescending(b => b.NameCustomer),
                "numberofpeople" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.People) 
                    : query.OrderByDescending(b => b.People),
                "paymentstatus" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.PaymentStatus) 
                    : query.OrderByDescending(b => b.PaymentStatus),
                "statustypename" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.StatusName) 
                    : query.OrderByDescending(b => b.StatusName),
                "tourtitle" => filter.SortDirection.ToLower() == "asc" 
                    ? query.OrderBy(b => b.TourTitle) 
                    : query.OrderByDescending(b => b.TourTitle),
                _ => query.OrderByDescending(b => b.CreatedTime)
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<BookingWithDetailsDto>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = filter.Page,
                PageSize = filter.PageSize
            };
        }

        public async Task<List<BookingWithDetailsDto>> GetBookingsWithDetailsAsync(List<int> bookingIds)
        {
            if (!bookingIds.Any())
                return new List<BookingWithDetailsDto>();

            var query = from b in _context.Bookings
                        join t in _context.Tours on b.TourId equals t.Id into tourGroup
                        from t in tourGroup.DefaultIfEmpty()
                        join a in _context.Accounts on b.UserId equals a.Id into accountGroup
                        from a in accountGroup.DefaultIfEmpty()
                        join bs in _context.BookingStatuses on b.StatusTypeId equals bs.Id into statusGroup
                        from bs in statusGroup.DefaultIfEmpty()
                        where b.Active && bookingIds.Contains(b.Id) && (t == null || t.Active) && (a == null || a.Active) && (bs == null || bs.Active)
                        orderby b.CreatedTime descending
                        select new BookingWithDetailsDto
                        {
                            Id = b.Id,
                            TourId = b.TourId,
                            TourTitle = t != null ? t.Title : "N/A",
                            UserId = b.UserId,
                            UserFullName = a != null ? a.FullName : "Guest",
                            StartDate = b.StartDate,
                            People = b.People,
                            PhoneNumber = b.PhoneNumber,
                            NameCustomer = b.NameCustomer,
                            Email = b.Email,
                            Note = b.Note,
                            TotalPrice = b.TotalPrice,
                            StatusTypeId = b.StatusTypeId,
                            StatusName = bs != null ? bs.StatusName : "Unknown",
                            PaymentStatus = b.PaymentStatus,
                            CreatedTime = b.CreatedTime,
                            UpdatedTime = b.UpdatedTime,
                            UpdatedBy = b.UpdatedBy,
                            Active = b.Active
                        };

            return await query.ToListAsync();
        }

        public async Task<List<BookingWithDetailsDto>> GetBookingsWithDetailsForExportAsync(ExportBookingDto exportFilter)
        {
            var query = from b in _context.Bookings
                        join t in _context.Tours on b.TourId equals t.Id into tourGroup
                        from t in tourGroup.DefaultIfEmpty()
                        join a in _context.Accounts on b.UserId equals a.Id into accountGroup
                        from a in accountGroup.DefaultIfEmpty()
                        join bs in _context.BookingStatuses on b.StatusTypeId equals bs.Id into statusGroup
                        from bs in statusGroup.DefaultIfEmpty()
                        where b.Active && (t == null || t.Active) && (a == null || a.Active) && (bs == null || bs.Active)
                        select new { b, t, a, bs };

            // Apply export filters
            if (exportFilter.StatusTypeId.HasValue)
                query = from item in query where item.b.StatusTypeId == exportFilter.StatusTypeId.Value select item;

            if (exportFilter.TourId.HasValue)
                query = from item in query where item.b.TourId == exportFilter.TourId.Value select item;

            if (exportFilter.StartDateFrom.HasValue)
                query = from item in query where item.b.StartDate >= exportFilter.StartDateFrom.Value select item;

            if (exportFilter.StartDateTo.HasValue)
                query = from item in query where item.b.StartDate <= exportFilter.StartDateTo.Value select item;

            if (exportFilter.CreatedTimeFrom.HasValue)
                query = from item in query where item.b.CreatedTime >= exportFilter.CreatedTimeFrom.Value select item;

            if (exportFilter.CreatedTimeTo.HasValue)
                query = from item in query where item.b.CreatedTime <= exportFilter.CreatedTimeTo.Value select item;

            var result = from item in query
                         orderby item.b.CreatedTime descending
                         select new BookingWithDetailsDto
                         {
                             Id = item.b.Id,
                             TourId = item.b.TourId,
                             TourTitle = item.t != null ? item.t.Title : "N/A",
                             UserId = item.b.UserId,
                             UserFullName = item.a != null ? item.a.FullName : "Guest",
                             StartDate = item.b.StartDate,
                             People = item.b.People,
                             PhoneNumber = item.b.PhoneNumber,
                             NameCustomer = item.b.NameCustomer,
                             Email = item.b.Email,
                             Note = item.b.Note,
                             TotalPrice = item.b.TotalPrice,
                             StatusTypeId = item.b.StatusTypeId,
                             StatusName = item.bs != null ? item.bs.StatusName : "Unknown",
                             PaymentStatus = item.b.PaymentStatus,
                             CreatedTime = item.b.CreatedTime,
                             UpdatedTime = item.b.UpdatedTime,
                             UpdatedBy = item.b.UpdatedBy,
                             Active = item.b.Active
                         };

            return await result.ToListAsync();
        }

        public async Task<List<BookingWithDetailsDto>> GetBookingsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var query = from b in _context.Bookings
                        join t in _context.Tours on b.TourId equals t.Id into tourGroup
                        from t in tourGroup.DefaultIfEmpty()
                        join a in _context.Accounts on b.UserId equals a.Id into accountGroup
                        from a in accountGroup.DefaultIfEmpty()
                        join bs in _context.BookingStatuses on b.StatusTypeId equals bs.Id into statusGroup
                        from bs in statusGroup.DefaultIfEmpty()
                        where b.Active && 
                              (b.StartDate ?? DateTime.MinValue).Date >= startDate.Date && 
                              (b.StartDate ?? DateTime.MinValue).Date <= endDate.Date &&
                              (t == null || t.Active) && 
                              (a == null || a.Active) && 
                              (bs == null || bs.Active)
                        orderby b.StartDate, b.CreatedTime
                        select new BookingWithDetailsDto
                        {
                            Id = b.Id,
                            TourId = b.TourId,
                            TourTitle = t != null ? t.Title : "N/A",
                            UserId = b.UserId,
                            UserFullName = a != null ? a.FullName : "Guest",
                            StartDate = b.StartDate,
                            People = b.People,
                            PhoneNumber = b.PhoneNumber,
                            NameCustomer = b.NameCustomer,
                            Email = b.Email,
                            Note = b.Note,
                            TotalPrice = b.TotalPrice,
                            StatusTypeId = b.StatusTypeId,
                            StatusName = bs != null ? bs.StatusName : "Unknown",
                            PaymentStatus = b.PaymentStatus,
                            CreatedTime = b.CreatedTime,
                            UpdatedTime = b.UpdatedTime,
                            UpdatedBy = b.UpdatedBy,
                            Active = b.Active
                        };

            return await query.ToListAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}
