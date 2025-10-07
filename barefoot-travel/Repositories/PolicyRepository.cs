using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Policy;
using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class PolicyRepository : IPolicyRepository
    {
        private readonly SysDbContext _context;

        public PolicyRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<Policy?> GetByIdAsync(int id)
        {
            return await _context.Policies
                .Where(p => p.Id == id && p.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Policy>> GetAllAsync()
        {
            return await _context.Policies
                .Where(p => p.Active)
                .OrderBy(p => p.PolicyType)
                .ToListAsync();
        }

        public async Task<PagedResult<Policy>> GetPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", bool? active = null)
        {
            var query = _context.Policies.AsQueryable();

            // Filter by active status
            if (active.HasValue)
            {
                query = query.Where(p => p.Active == active.Value);
            }
            else
            {
                query = query.Where(p => p.Active);
            }

            // Apply sorting
            var sortedQuery = sortBy?.ToLower() switch
            {
                "policytype" => sortOrder == "desc" 
                    ? query.OrderByDescending(p => p.PolicyType) 
                    : query.OrderBy(p => p.PolicyType),
                "createdtime" => sortOrder == "desc" 
                    ? query.OrderByDescending(p => p.CreatedTime) 
                    : query.OrderBy(p => p.CreatedTime),
                _ => query.OrderBy(p => p.PolicyType)
            };

            var totalItems = await sortedQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = await sortedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Policy>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<Policy> CreateAsync(Policy policy)
        {
            policy.CreatedTime = DateTime.UtcNow;
            policy.Active = true;
            await _context.Policies.AddAsync(policy);
            await _context.SaveChangesAsync();
            return policy;
        }

        public async Task<Policy> UpdateAsync(Policy policy)
        {
            policy.UpdatedTime = DateTime.UtcNow;
            _context.Policies.Update(policy);
            await _context.SaveChangesAsync();
            return policy;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var policy = await _context.Policies
                .Where(p => p.Id == id && p.Active)
                .FirstOrDefaultAsync();

            if (policy == null) return false;

            policy.Active = false;
            policy.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Policies
                .Where(p => p.Id == id && p.Active)
                .AnyAsync();
        }

        public async Task<bool> TypeExistsAsync(string policyType, int? excludeId = null)
        {
            var query = _context.Policies.Where(p => p.PolicyType == policyType && p.Active);
            
            if (excludeId.HasValue)
            {
                query = query.Where(p => p.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, bool active, string updatedBy)
        {
            var policy = await _context.Policies
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (policy == null) return false;

            policy.Active = active;
            policy.UpdatedTime = DateTime.UtcNow;
            policy.UpdatedBy = updatedBy;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
