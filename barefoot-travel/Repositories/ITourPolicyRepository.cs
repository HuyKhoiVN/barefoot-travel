using barefoot_travel.DTOs.Tour;
using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface ITourPolicyRepository
    {
        // TourPolicy CRUD operations
        Task<TourPolicyResponseDto?> GetByIdAsync(int id);
        Task<List<TourPolicyResponseDto>> GetByTourIdAsync(int tourId);
        Task<TourPolicyResponseDto> CreateAsync(TourPolicy policy);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByTourIdAsync(int tourId);
        Task<bool> ExistsAsync(int id);
        Task<bool> LinkExistsAsync(int tourId, int policyId);
        Task<bool> TourHasPoliciesAsync(int tourId);
        Task<int> GetPolicyCountByTourIdAsync(int tourId);
    }
}
