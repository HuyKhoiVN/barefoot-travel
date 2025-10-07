using barefoot_travel.DTOs.Tour;
using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class TourPriceRepository : ITourPriceRepository
    {
        private readonly SysDbContext _context;

        public TourPriceRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<TourPriceResponseDto?> GetByIdAsync(int id)
        {
            return await (from tp in _context.TourPrices
                         join pt in _context.PriceTypes on tp.PriceTypeId equals pt.Id
                         where tp.Id == id && tp.Active && pt.Active
                         select new TourPriceResponseDto
                         {
                             Id = tp.Id,
                             TourId = tp.TourId,
                             PriceTypeId = tp.PriceTypeId,
                             PriceTypeName = pt.PriceTypeName,
                             Price = tp.Price,
                             CreatedTime = tp.CreatedTime
                         }).FirstOrDefaultAsync();
        }

        public async Task<List<TourPriceResponseDto>> GetByTourIdAsync(int tourId)
        {
            return await (from tp in _context.TourPrices
                         join pt in _context.PriceTypes on tp.PriceTypeId equals pt.Id
                         where tp.TourId == tourId && tp.Active && pt.Active
                         orderby tp.CreatedTime
                         select new TourPriceResponseDto
                         {
                             Id = tp.Id,
                             TourId = tp.TourId,
                             PriceTypeId = tp.PriceTypeId,
                             PriceTypeName = pt.PriceTypeName,
                             Price = tp.Price,
                             CreatedTime = tp.CreatedTime
                         }).ToListAsync();
        }

        public async Task<TourPriceResponseDto> CreateAsync(TourPrice price)
        {
            price.CreatedTime = DateTime.UtcNow;
            price.Active = true;
            await _context.TourPrices.AddAsync(price);
            await _context.SaveChangesAsync();

            // Get price type name for response
            var priceTypeName = await _context.PriceTypes
                .Where(pt => pt.Id == price.PriceTypeId)
                .Select(pt => pt.PriceTypeName)
                .FirstOrDefaultAsync() ?? "";

            return new TourPriceResponseDto
            {
                Id = price.Id,
                TourId = price.TourId,
                PriceTypeId = price.PriceTypeId,
                PriceTypeName = priceTypeName,
                Price = price.Price,
                CreatedTime = price.CreatedTime
            };
        }

        public async Task<TourPriceResponseDto> UpdateAsync(TourPrice price)
        {
            price.UpdatedTime = DateTime.UtcNow;
            _context.TourPrices.Update(price);
            await _context.SaveChangesAsync();

            // Get price type name for response
            var priceTypeName = await _context.PriceTypes
                .Where(pt => pt.Id == price.PriceTypeId)
                .Select(pt => pt.PriceTypeName)
                .FirstOrDefaultAsync() ?? "";

            return new TourPriceResponseDto
            {
                Id = price.Id,
                TourId = price.TourId,
                PriceTypeId = price.PriceTypeId,
                PriceTypeName = priceTypeName,
                Price = price.Price,
                CreatedTime = price.CreatedTime
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var price = await _context.TourPrices
                .Where(tp => tp.Id == id && tp.Active)
                .FirstOrDefaultAsync();

            if (price == null) return false;

            price.Active = false;
            price.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TourPrices
                .Where(tp => tp.Id == id && tp.Active)
                .AnyAsync();
        }

        public async Task<bool> TourHasPricesAsync(int tourId)
        {
            return await _context.TourPrices
                .Where(tp => tp.TourId == tourId && tp.Active)
                .AnyAsync();
        }

        public async Task<decimal> GetMinPriceByTourIdAsync(int tourId)
        {
            return await _context.TourPrices
                .Where(tp => tp.TourId == tourId && tp.Active)
                .MinAsync(tp => tp.Price);
        }

        public async Task<decimal> GetMaxPriceByTourIdAsync(int tourId)
        {
            return await _context.TourPrices
                .Where(tp => tp.TourId == tourId && tp.Active)
                .MaxAsync(tp => tp.Price);
        }
    }
}
