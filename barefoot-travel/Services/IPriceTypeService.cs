using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.PriceType;

namespace barefoot_travel.Services;

public interface IPriceTypeService
{
    Task<ApiResponse> GetPriceTypeByIdAsync(int id);
    Task<ApiResponse> GetAllPriceTypesAsync();
    Task<PagedResult<PriceTypeDto>> GetPriceTypesPagedAsync(int page, int pageSize, string? priceTypeName = null, string? sortBy = "priceTypeName", string? sortOrder = "asc");
    Task<ApiResponse> CreatePriceTypeAsync(CreatePriceTypeDto dto);
    Task<ApiResponse> UpdatePriceTypeAsync(int id, UpdatePriceTypeDto dto);
    Task<ApiResponse> DeletePriceTypeAsync(int id);
}
