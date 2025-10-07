using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.PriceType;
using barefoot_travel.Models;
using barefoot_travel.Repositories;

namespace barefoot_travel.Services;

public class PriceTypeService : IPriceTypeService
{
    private readonly IPriceTypeRepository _priceTypeRepository;

    public PriceTypeService(IPriceTypeRepository priceTypeRepository)
    {
        _priceTypeRepository = priceTypeRepository;
    }

    public async Task<ApiResponse> GetPriceTypeByIdAsync(int id)
    {
        try
        {
            var priceType = await _priceTypeRepository.GetByIdAsync(id);
            if (priceType == null)
            {
                return new ApiResponse(false, "Price type not found");
            }

            var priceTypeDto = MapToPriceTypeDto(priceType);
            return new ApiResponse(true, "Success", priceTypeDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse(false, $"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetAllPriceTypesAsync()
    {
        try
        {
            var priceTypes = await _priceTypeRepository.GetAllAsync();
            var priceTypeDtos = MapToPriceTypeDtoList(priceTypes);
            return new ApiResponse(true, "Success", priceTypeDtos);
        }
        catch (Exception ex)
        {
            return new ApiResponse(false, $"Error: {ex.Message}");
        }
    }

    public async Task<PagedResult<PriceTypeDto>> GetPriceTypesPagedAsync(int page, int pageSize)
    {
        try
        {
            var pagedResult = await _priceTypeRepository.GetPagedAsync(page, pageSize);
            var priceTypeDtos = MapToPriceTypeDtoList(pagedResult.Items);

            return new PagedResult<PriceTypeDto>
            {
                Items = priceTypeDtos,
                TotalItems = pagedResult.TotalItems,
                TotalPages = pagedResult.TotalPages,
                CurrentPage = pagedResult.CurrentPage,
                PageSize = pagedResult.PageSize
            };
        }
        catch (Exception ex)
        {
            return new PagedResult<PriceTypeDto>
            {
                Items = new List<PriceTypeDto>(),
                TotalItems = 0,
                TotalPages = 0,
                CurrentPage = page,
                PageSize = pageSize
            };
        }
    }

    public async Task<ApiResponse> CreatePriceTypeAsync(CreatePriceTypeDto dto)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(dto.PriceTypeName))
            {
                return new ApiResponse(false, "Price type name is required");
            }

            // Check if name already exists
            var exists = await _priceTypeRepository.ExistsByNameAsync(dto.PriceTypeName);
            if (exists)
            {
                return new ApiResponse(false, "Price type name already exists");
            }

            var priceType = MapToPriceType(dto);
            var createdPriceType = await _priceTypeRepository.CreateAsync(priceType);
            var priceTypeDto = MapToPriceTypeDto(createdPriceType);

            return new ApiResponse(true, "Price type created successfully", priceTypeDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse(false, $"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse> UpdatePriceTypeAsync(int id, UpdatePriceTypeDto dto)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(dto.PriceTypeName))
            {
                return new ApiResponse(false, "Price type name is required");
            }

            var existingPriceType = await _priceTypeRepository.GetByIdAsync(id);
            if (existingPriceType == null)
            {
                return new ApiResponse(false, "Price type not found");
            }

            // Check if name already exists (excluding current record)
            var exists = await _priceTypeRepository.ExistsByNameAsync(dto.PriceTypeName, id);
            if (exists)
            {
                return new ApiResponse(false, "Price type name already exists");
            }

            MapToPriceTypeForUpdate(existingPriceType, dto);
            var updatedPriceType = await _priceTypeRepository.UpdateAsync(existingPriceType);
            var priceTypeDto = MapToPriceTypeDto(updatedPriceType);

            return new ApiResponse(true, "Price type updated successfully", priceTypeDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse(false, $"Error: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeletePriceTypeAsync(int id)
    {
        try
        {
            var exists = await _priceTypeRepository.ExistsAsync(id);
            if (!exists)
            {
                return new ApiResponse(false, "Price type not found");
            }

            var success = await _priceTypeRepository.DeleteAsync(id);
            if (success)
            {
                return new ApiResponse(true, "Price type deleted successfully");
            }
            else
            {
                return new ApiResponse(false, "Failed to delete price type");
            }
        }
        catch (Exception ex)
        {
            return new ApiResponse(false, $"Error: {ex.Message}");
        }
    }

    // Manual mapping methods
    private PriceTypeDto MapToPriceTypeDto(PriceType priceType)
    {
        if (priceType == null) return null;

        return new PriceTypeDto
        {
            Id = priceType.Id,
            PriceTypeName = priceType.PriceTypeName,
            CreatedTime = priceType.CreatedTime,
            UpdatedTime = priceType.UpdatedTime,
            UpdatedBy = priceType.UpdatedBy,
            Active = priceType.Active
        };
    }

    private List<PriceTypeDto> MapToPriceTypeDtoList(List<PriceType> priceTypes)
    {
        if (priceTypes == null || !priceTypes.Any()) return new List<PriceTypeDto>();

        return priceTypes.Select(MapToPriceTypeDto).Where(dto => dto != null).ToList();
    }

    private PriceType MapToPriceType(CreatePriceTypeDto dto)
    {
        if (dto == null) return null;

        return new PriceType
        {
            PriceTypeName = dto.PriceTypeName,
            Active = true,
            CreatedTime = DateTime.UtcNow
        };
    }

    private void MapToPriceTypeForUpdate(PriceType existingPriceType, UpdatePriceTypeDto dto)
    {
        if (existingPriceType == null || dto == null) return;

        existingPriceType.PriceTypeName = dto.PriceTypeName;
        existingPriceType.UpdatedTime = DateTime.UtcNow;
        existingPriceType.UpdatedBy = dto.UpdatedBy;
    }
}
