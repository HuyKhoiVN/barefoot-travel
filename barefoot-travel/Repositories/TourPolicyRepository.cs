using barefoot_travel.DTOs.Tour;
using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class TourPolicyRepository : ITourPolicyRepository
    {
        private readonly SysDbContext _context;

        public TourPolicyRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<TourPolicyResponseDto?> GetByIdAsync(int id)
        {
            return await (from tp in _context.TourPolicies
                         join p in _context.Policies on tp.PolicyId equals p.Id
                         where tp.Id == id && tp.Active && p.Active
                         select new TourPolicyResponseDto
                         {
                             Id = tp.Id,
                             TourId = tp.TourId,
                             PolicyId = tp.PolicyId,
                             PolicyType = p.PolicyType,
                             CreatedTime = tp.CreatedTime
                         }).FirstOrDefaultAsync();
        }

        public async Task<List<TourPolicyResponseDto>> GetByTourIdAsync(int tourId)
        {
            return await (from tp in _context.TourPolicies
                         join p in _context.Policies on tp.PolicyId equals p.Id
                         where tp.TourId == tourId && tp.Active && p.Active
                         orderby tp.CreatedTime
                         select new TourPolicyResponseDto
                         {
                             Id = tp.Id,
                             TourId = tp.TourId,
                             PolicyId = tp.PolicyId,
                             PolicyType = p.PolicyType,
                             CreatedTime = tp.CreatedTime
                         }).ToListAsync();
        }

        public async Task<TourPolicyResponseDto> CreateAsync(TourPolicy policy)
        {
            policy.CreatedTime = DateTime.UtcNow;
            policy.Active = true;
            await _context.TourPolicies.AddAsync(policy);
            await _context.SaveChangesAsync();

            // Get policy type for response
            var policyType = await _context.Policies
                .Where(p => p.Id == policy.PolicyId)
                .Select(p => p.PolicyType)
                .FirstOrDefaultAsync() ?? "";

            return new TourPolicyResponseDto
            {
                Id = policy.Id,
                TourId = policy.TourId,
                PolicyId = policy.PolicyId,
                PolicyType = policyType,
                CreatedTime = policy.CreatedTime
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var policy = await _context.TourPolicies
                .Where(tp => tp.Id == id && tp.Active)
                .FirstOrDefaultAsync();

            if (policy == null) return false;

            policy.Active = false;
            policy.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByTourIdAsync(int tourId)
        {
            var policies = await _context.TourPolicies
                .Where(tp => tp.TourId == tourId && tp.Active)
                .ToListAsync();

            if (!policies.Any()) return false;

            foreach (var policy in policies)
            {
                policy.Active = false;
                policy.UpdatedTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TourPolicies
                .Where(tp => tp.Id == id && tp.Active)
                .AnyAsync();
        }

        public async Task<bool> LinkExistsAsync(int tourId, int policyId)
        {
            return await _context.TourPolicies
                .Where(tp => tp.TourId == tourId && tp.PolicyId == policyId && tp.Active)
                .AnyAsync();
        }

        public async Task<bool> TourHasPoliciesAsync(int tourId)
        {
            return await _context.TourPolicies
                .Where(tp => tp.TourId == tourId && tp.Active)
                .AnyAsync();
        }

        public async Task<int> GetPolicyCountByTourIdAsync(int tourId)
        {
            return await _context.TourPolicies
                .Where(tp => tp.TourId == tourId && tp.Active)
                .CountAsync();
        }
    }
}
