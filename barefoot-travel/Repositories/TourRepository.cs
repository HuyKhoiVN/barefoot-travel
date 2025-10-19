using barefoot_travel.Models;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Tour;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class TourRepository : ITourRepository
    {
        private readonly SysDbContext _context;

        public TourRepository(SysDbContext context)
        {
            _context = context;
        }

        #region Tour CRUD Operations

        public async Task<Tour?> GetByIdAsync(int id)
        {
            return await _context.Tours
                .Where(t => t.Id == id && t.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Tour>> GetAllAsync()
        {
            return await _context.Tours
                .Where(t => t.Active)
                .OrderBy(t => t.Title)
                .ToListAsync();
        }

        public async Task<PagedResult<Tour>> GetPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", int? categoryId = null, bool? active = null)
        {
            var query = _context.Tours.AsQueryable();

            // Filter by active status
            if (active.HasValue)
            {
                query = query.Where(t => t.Active == active.Value);
            }
            else
            {
                query = query.Where(t => t.Active);
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                query = from t in query
                        where (from tc in _context.TourCategories
                               where tc.TourId == t.Id && tc.CategoryId == categoryId && tc.Active
                               select tc).Any()
                        select t;
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "title" => sortOrder == "desc" ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                "priceperperson" => sortOrder == "desc" ? query.OrderByDescending(t => t.PricePerPerson) : query.OrderBy(t => t.PricePerPerson),
                "createdtime" => sortOrder == "desc" ? query.OrderByDescending(t => t.CreatedTime) : query.OrderBy(t => t.CreatedTime),
                _ => query.OrderBy(t => t.Title)
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Tour>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<Tour> CreateAsync(Tour tour)
        {
            tour.CreatedTime = DateTime.UtcNow;
            tour.Active = true;
            await _context.Tours.AddAsync(tour);
            await _context.SaveChangesAsync();
            return tour;
        }

        public async Task<Tour> UpdateAsync(Tour tour)
        {
            tour.UpdatedTime = DateTime.UtcNow;
            _context.Tours.Update(tour);
            await _context.SaveChangesAsync();
            return tour;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var tour = await _context.Tours
                .Where(t => t.Id == id && t.Active)
                .FirstOrDefaultAsync();

            if (tour == null) return false;

            tour.Active = false;
            tour.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Tours
                .Where(t => t.Id == id && t.Active)
                .AnyAsync();
        }

        public async Task<bool> TitleExistsAsync(string title, int? excludeId = null)
        {
            var query = _context.Tours.Where(t => t.Title == title && t.Active);
            
            if (excludeId.HasValue)
            {
                query = query.Where(t => t.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        #endregion

        #region Tour with Related Data - DTO methods with joins

        public async Task<TourDetailDto?> GetTourDetailByIdAsync(int id)
        {
            // Single optimized query with all joins performed on server
            var tourData = await (from t in _context.Tours
                                 where t.Id == id && t.Active
                                 select new
                                 {
                                     Tour = t,
                                     Images = (from ti in _context.TourImages
                                              where ti.TourId == t.Id && ti.Active
                                              select new { ti.Id, ti.TourId, ti.ImageUrl, ti.IsBanner, ti.CreatedTime }).ToList(),
                                     Categories = (from tc in _context.TourCategories
                                                  join c in _context.Categories on tc.CategoryId equals c.Id
                                                  where tc.TourId == t.Id && tc.Active && c.Active
                                                  select new { c.Id, c.CategoryName, c.Type }).ToList(),
                                     Prices = (from tp in _context.TourPrices
                                              join pt in _context.PriceTypes on tp.PriceTypeId equals pt.Id
                                              where tp.TourId == t.Id && tp.Active && pt.Active
                                              select new { tp.Id, tp.TourId, tp.PriceTypeId, pt.PriceTypeName, tp.Price, tp.CreatedTime }).ToList(),
                                     Policies = (from tpol in _context.TourPolicies
                                                join p in _context.Policies on tpol.PolicyId equals p.Id
                                                where tpol.TourId == t.Id && tpol.Active && p.Active
                                                select new { p.Id, p.PolicyType }).ToList()
                                 }).FirstOrDefaultAsync();

            if (tourData == null) return null;

            // Map to DTO after server-side data retrieval
            return new TourDetailDto
            {
                Id = tourData.Tour.Id,
                Title = tourData.Tour.Title,
                Description = tourData.Tour.Description,
                MapLink = tourData.Tour.MapLink,
                PricePerPerson = tourData.Tour.PricePerPerson,
                MaxPeople = tourData.Tour.MaxPeople,
                Duration = tourData.Tour.Duration,
                StartTime = tourData.Tour.StartTime,
                ReturnTime = tourData.Tour.ReturnTime,
                CreatedTime = tourData.Tour.CreatedTime,
                UpdatedTime = tourData.Tour.UpdatedTime,
                UpdatedBy = tourData.Tour.UpdatedBy,
                Active = tourData.Tour.Active,
                Images = tourData.Images.Select(img => new TourImageDto
                {
                    Id = img.Id,
                    TourId = img.TourId,
                    ImageUrl = img.ImageUrl,
                    IsBanner = img.IsBanner,
                    CreatedTime = img.CreatedTime
                }).ToList(),
                Categories = tourData.Categories.Select(cat => new CategoryDto
                {
                    Id = cat.Id,
                    CategoryName = cat.CategoryName,
                    Type = cat.Type
                }).ToList(),
                Prices = tourData.Prices.Select(price => new TourPriceDto
                {
                    Id = price.Id,
                    TourId = price.TourId,
                    PriceTypeId = price.PriceTypeId,
                    PriceTypeName = price.PriceTypeName,
                    Price = price.Price,
                    CreatedTime = price.CreatedTime
                }).ToList(),
                Policies = tourData.Policies.Select(policy => new PolicyDto
                {
                    Id = policy.Id,
                    PolicyType = policy.PolicyType
                }).ToList()
            };
        }

        public async Task<List<TourDto>> GetToursWithBasicInfoAsync()
        {
            return await (from t in _context.Tours
                         where t.Active
                         orderby t.Title
                         select new TourDto
                         {
                             Id = t.Id,
                             Title = t.Title,
                             Description = t.Description,
                             MapLink = t.MapLink,
                             PricePerPerson = t.PricePerPerson,
                             MaxPeople = t.MaxPeople,
                             Duration = t.Duration,
                             StartTime = t.StartTime,
                             ReturnTime = t.ReturnTime,
                             CreatedTime = t.CreatedTime,
                             UpdatedTime = t.UpdatedTime,
                             UpdatedBy = t.UpdatedBy,
                             Active = t.Active
                         }).ToListAsync();
        }

        public async Task<PagedResult<TourDto>> GetToursPagedWithBasicInfoAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", List<int>? categoryIds = null, string? search = null, bool? active = null)
        {
            // Build base query with optimized filtering
            var baseQuery = _context.Tours.AsQueryable();

            // Apply active filter first for better performance
            if (active.HasValue)
            {
                baseQuery = baseQuery.Where(t => t.Active == active.Value);
            }
            else
            {
                baseQuery = baseQuery.Where(t => t.Active);
            }

            // Apply search filter by title
            if (!string.IsNullOrEmpty(search))
            {
                baseQuery = baseQuery.Where(t => t.Title.Contains(search));
            }

            // Apply category filter with optimized join
            if (categoryIds != null && categoryIds.Any())
            {
                baseQuery = baseQuery.Where(t => _context.TourCategories
                    .Any(tc => tc.TourId == t.Id && categoryIds.Contains(tc.CategoryId) && tc.Active));
            }

            // Apply sorting with server-side processing
            var sortedQuery = sortBy?.ToLower() switch
            {
                "title" => sortOrder == "desc" 
                    ? baseQuery.OrderByDescending(t => t.Title) 
                    : baseQuery.OrderBy(t => t.Title),
                "priceperperson" => sortOrder == "desc" 
                    ? baseQuery.OrderByDescending(t => t.PricePerPerson) 
                    : baseQuery.OrderBy(t => t.PricePerPerson),
                "createdtime" => sortOrder == "desc" 
                    ? baseQuery.OrderByDescending(t => t.CreatedTime) 
                    : baseQuery.OrderBy(t => t.CreatedTime),
                _ => baseQuery.OrderBy(t => t.Title)
            };

            // Get total count efficiently
            var totalItems = await sortedQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // Execute paginated query with projection to DTO including images
            var items = await sortedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TourDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    MapLink = t.MapLink,
                    PricePerPerson = t.PricePerPerson,
                    MaxPeople = t.MaxPeople,
                    Duration = t.Duration,
                    StartTime = t.StartTime,
                    ReturnTime = t.ReturnTime,
                    CreatedTime = t.CreatedTime,
                    UpdatedTime = t.UpdatedTime,
                    UpdatedBy = t.UpdatedBy,
                    Active = t.Active,
                    Images = _context.TourImages
                        .Where(ti => ti.TourId == t.Id && ti.Active)
                        .Select(ti => new TourImageDto
                        {
                            Id = ti.Id,
                            TourId = ti.TourId,
                            ImageUrl = ti.ImageUrl,
                            IsBanner = ti.IsBanner,
                            CreatedTime = ti.CreatedTime,
                            UpdatedTime = ti.UpdatedTime,
                            UpdatedBy = ti.UpdatedBy,
                            Active = ti.Active
                        })
                        .ToList()
                })
                .ToListAsync();

            return new PagedResult<TourDto>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<List<TourDto>> GetToursByCategoryAsync(int categoryId)
        {
            // Optimized query with proper join and filtering
            return await (from t in _context.Tours
                         join tc in _context.TourCategories on t.Id equals tc.TourId
                         where tc.CategoryId == categoryId && t.Active && tc.Active
                         orderby t.Title
                         select new TourDto
                         {
                             Id = t.Id,
                             Title = t.Title,
                             Description = t.Description,
                             MapLink = t.MapLink,
                             PricePerPerson = t.PricePerPerson,
                             MaxPeople = t.MaxPeople,
                             Duration = t.Duration,
                             StartTime = t.StartTime,
                             ReturnTime = t.ReturnTime,
                             CreatedTime = t.CreatedTime,
                             UpdatedTime = t.UpdatedTime,
                             UpdatedBy = t.UpdatedBy,
                             Active = t.Active
                         }).ToListAsync();
        }

        #endregion

        #region Optimized Bulk Operations

        /// <summary>
        /// Gets multiple tours with their related data in a single optimized query
        /// </summary>
        public async Task<List<TourDetailDto>> GetToursWithRelatedDataAsync(List<int> tourIds)
        {
            if (!tourIds.Any()) return new List<TourDetailDto>();

            // Single query to get all tours with related data
            var toursData = await (from t in _context.Tours
                                  where tourIds.Contains(t.Id) && t.Active
                                  select new
                                  {
                                      Tour = t,
                                      Images = (from ti in _context.TourImages
                                               where ti.TourId == t.Id && ti.Active
                                               select new { ti.Id, ti.TourId, ti.ImageUrl, ti.IsBanner, ti.CreatedTime }).ToList(),
                                      Categories = (from tc in _context.TourCategories
                                                   join c in _context.Categories on tc.CategoryId equals c.Id
                                                   where tc.TourId == t.Id && tc.Active && c.Active
                                                   select new { c.Id, c.CategoryName, c.Type }).ToList(),
                                      Prices = (from tp in _context.TourPrices
                                               join pt in _context.PriceTypes on tp.PriceTypeId equals pt.Id
                                               where tp.TourId == t.Id && tp.Active && pt.Active
                                               select new { tp.Id, tp.TourId, tp.PriceTypeId, pt.PriceTypeName, tp.Price, tp.CreatedTime }).ToList(),
                                      Policies = (from tpol in _context.TourPolicies
                                                 join p in _context.Policies on tpol.PolicyId equals p.Id
                                                 where tpol.TourId == t.Id && tpol.Active && p.Active
                                                 select new { p.Id, p.PolicyType }).ToList()
                                  }).ToListAsync();

            // Map to DTOs after server-side data retrieval
            return toursData.Select(tourData => new TourDetailDto
            {
                Id = tourData.Tour.Id,
                Title = tourData.Tour.Title,
                Description = tourData.Tour.Description,
                MapLink = tourData.Tour.MapLink,
                PricePerPerson = tourData.Tour.PricePerPerson,
                MaxPeople = tourData.Tour.MaxPeople,
                Duration = tourData.Tour.Duration,
                StartTime = tourData.Tour.StartTime,
                ReturnTime = tourData.Tour.ReturnTime,
                CreatedTime = tourData.Tour.CreatedTime,
                UpdatedTime = tourData.Tour.UpdatedTime,
                UpdatedBy = tourData.Tour.UpdatedBy,
                Active = tourData.Tour.Active,
                Images = tourData.Images.Select(img => new TourImageDto
                {
                    Id = img.Id,
                    TourId = img.TourId,
                    ImageUrl = img.ImageUrl,
                    IsBanner = img.IsBanner,
                    CreatedTime = img.CreatedTime
                }).ToList(),
                Categories = tourData.Categories.Select(cat => new CategoryDto
                {
                    Id = cat.Id,
                    CategoryName = cat.CategoryName,
                    Type = cat.Type
                }).ToList(),
                Prices = tourData.Prices.Select(price => new TourPriceDto
                {
                    Id = price.Id,
                    TourId = price.TourId,
                    PriceTypeId = price.PriceTypeId,
                    PriceTypeName = price.PriceTypeName,
                    Price = price.Price,
                    CreatedTime = price.CreatedTime
                }).ToList(),
                Policies = tourData.Policies.Select(policy => new PolicyDto
                {
                    Id = policy.Id,
                    PolicyType = policy.PolicyType
                }).ToList()
            }).ToList();
        }

        /// <summary>
        /// Gets tours with basic info and first banner image in a single query
        /// </summary>
        public async Task<List<TourDto>> GetToursWithBannerImageAsync(int? categoryId = null, int? limit = null)
        {
            var query = from t in _context.Tours
                        where t.Active
                        select t;

            // Apply category filter if specified
            if (categoryId.HasValue)
            {
                query = query.Where(t => _context.TourCategories
                    .Any(tc => tc.TourId == t.Id && tc.CategoryId == categoryId && tc.Active));
            }

            // Apply limit if specified
            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            return await query
                .OrderBy(t => t.Title)
                .Select(t => new TourDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    MapLink = t.MapLink,
                    PricePerPerson = t.PricePerPerson,
                    MaxPeople = t.MaxPeople,
                    Duration = t.Duration,
                    StartTime = t.StartTime,
                    ReturnTime = t.ReturnTime,
                    CreatedTime = t.CreatedTime,
                    UpdatedTime = t.UpdatedTime,
                    UpdatedBy = t.UpdatedBy,
                    Active = t.Active
                })
                .ToListAsync();
        }

        #endregion

        #region Tour Status Operations

        public async Task<bool> UpdateStatusAsync(int id, bool active, string updatedBy)
        {
            var tour = await _context.Tours
                .Where(t => t.Id == id)
                .FirstOrDefaultAsync();

            if (tour == null) return false;

            tour.Active = active;
            tour.UpdatedTime = DateTime.UtcNow;
            tour.UpdatedBy = updatedBy;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasActiveBookingsAsync(int tourId)
        {
            return await _context.Bookings
                .Where(b => b.TourId == tourId && b.Active && b.StatusTypeId != 3) // StatusTypeId 3 = Cancelled
                .AnyAsync();
        }

        #endregion

        #region Tour Image Operations (Basic)

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

        #endregion

        #region Tour Category Operations (Basic)

        public async Task<bool> TourHasCategoriesAsync(int tourId)
        {
            return await _context.TourCategories
                .Where(tc => tc.TourId == tourId && tc.Active)
                .AnyAsync();
        }

        public async Task<bool> CategoryLinkExistsAsync(int tourId, int categoryId)
        {
            return await _context.TourCategories
                .Where(tc => tc.TourId == tourId && tc.CategoryId == categoryId && tc.Active)
                .AnyAsync();
        }

        #endregion

        #region Tour Price Operations (Basic)

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

        #endregion

        #region Tour Policy Operations (Basic)

        public async Task<bool> TourHasPoliciesAsync(int tourId)
        {
            return await _context.TourPolicies
                .Where(tp => tp.TourId == tourId && tp.Active)
                .AnyAsync();
        }

        public async Task<bool> PolicyLinkExistsAsync(int tourId, int policyId)
        {
            return await _context.TourPolicies
                .Where(tp => tp.TourId == tourId && tp.PolicyId == policyId && tp.Active)
                .AnyAsync();
        }

        #endregion

        #region Marketing Tag Operations (Basic)

        public async Task<bool> TourHasMarketingTagsAsync(int tourId)
        {
            return await (from tc in _context.TourCategories
                         join c in _context.Categories on tc.CategoryId equals c.Id
                         where tc.TourId == tourId && tc.Active && c.Active && c.Type == "Marketing"
                         select tc).AnyAsync();
        }

        #endregion

        #region Validation Methods

        public async Task<bool> TourExistsAsync(int id)
        {
            return await _context.Tours
                .Where(t => t.Id == id && t.Active)
                .AnyAsync();
        }

        public async Task<bool> CategoryExistsAsync(int id)
        {
            return await _context.Categories
                .Where(c => c.Id == id && c.Active)
                .AnyAsync();
        }

        public async Task<bool> PriceTypeExistsAsync(int id)
        {
            return await _context.PriceTypes
                .Where(pt => pt.Id == id && pt.Active)
                .AnyAsync();
        }

        public async Task<bool> PolicyExistsAsync(int id)
        {
            return await _context.Policies
                .Where(p => p.Id == id && p.Active)
                .AnyAsync();
        }

        public async Task<bool> IsMarketingCategoryAsync(int categoryId)
        {
            return await _context.Categories
                .Where(c => c.Id == categoryId && c.Active && c.Type == "Marketing")
                .AnyAsync();
        }

        #endregion
    }
}