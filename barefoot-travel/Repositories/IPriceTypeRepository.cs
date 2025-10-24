using barefoot_travel.DTOs;
using barefoot_travel.Models;

namespace barefoot_travel.Repositories;

public interface IPriceTypeRepository
{
    Task<PriceType?> GetByIdAsync(int id);
    Task<List<PriceType>> GetAllAsync();
    Task<PagedResult<PriceType>> GetPagedAsync(int page, int pageSize, string? priceTypeName = null, string? sortBy = "priceTypeName", string? sortOrder = "asc");
    Task<PriceType> CreateAsync(PriceType priceType);
    Task<PriceType> UpdateAsync(PriceType priceType);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ExistsByNameAsync(string priceTypeName, int? excludeId = null);
}
