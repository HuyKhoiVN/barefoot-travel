using barefoot_travel.DTOs.Tour;
using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class TourImageRepository : ITourImageRepository
    {
        private readonly SysDbContext _context;

        public TourImageRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<TourImageResponseDto?> GetByIdAsync(int id)
        {
            return await _context.TourImages
                .Where(ti => ti.Id == id && ti.Active)
                .Select(ti => new TourImageResponseDto
                {
                    Id = ti.Id,
                    TourId = ti.TourId,
                    ImageUrl = ti.ImageUrl,
                    IsBanner = ti.IsBanner,
                    CreatedTime = ti.CreatedTime
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<TourImageResponseDto>> GetByTourIdAsync(int tourId)
        {
            return await _context.TourImages
                .Where(ti => ti.TourId == tourId && ti.Active)
                .OrderBy(ti => ti.CreatedTime)
                .Select(ti => new TourImageResponseDto
                {
                    Id = ti.Id,
                    TourId = ti.TourId,
                    ImageUrl = ti.ImageUrl,
                    IsBanner = ti.IsBanner,
                    CreatedTime = ti.CreatedTime
                })
                .ToListAsync();
        }

        public async Task<TourImageResponseDto> CreateAsync(TourImage image)
        {
            image.CreatedTime = DateTime.UtcNow;
            image.Active = true;
            await _context.TourImages.AddAsync(image);
            await _context.SaveChangesAsync();
            
            return new TourImageResponseDto
            {
                Id = image.Id,
                TourId = image.TourId,
                ImageUrl = image.ImageUrl,
                IsBanner = image.IsBanner,
                CreatedTime = image.CreatedTime
            };
        }

        public async Task<TourImageResponseDto> UpdateAsync(TourImage image)
        {
            image.UpdatedTime = DateTime.UtcNow;
            _context.TourImages.Update(image);
            await _context.SaveChangesAsync();
            
            return new TourImageResponseDto
            {
                Id = image.Id,
                TourId = image.TourId,
                ImageUrl = image.ImageUrl,
                IsBanner = image.IsBanner,
                CreatedTime = image.CreatedTime
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var image = await _context.TourImages
                .Where(ti => ti.Id == id && ti.Active)
                .FirstOrDefaultAsync();

            if (image == null) return false;

            image.Active = false;
            image.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TourImages
                .Where(ti => ti.Id == id && ti.Active)
                .AnyAsync();
        }

        public async Task<bool> TourHasImagesAsync(int tourId)
        {
            return await _context.TourImages
                .Where(ti => ti.TourId == tourId && ti.Active)
                .AnyAsync();
        }

        public async Task<int> GetImageCountByTourIdAsync(int tourId)
        {
            return await _context.TourImages
                .Where(ti => ti.TourId == tourId && ti.Active)
                .CountAsync();
        }
    }
}
