using Microsoft.AspNetCore.Mvc;
using barefoot_travel.DTOs.PriceType;
using barefoot_travel.Services;
using barefoot_travel.Common;
using barefoot_travel.DTOs;

namespace barefoot_travel.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class PriceTypeController : ControllerBase
{
    private readonly IPriceTypeService _priceTypeService;

    public PriceTypeController(IPriceTypeService priceTypeService)
    {
        _priceTypeService = priceTypeService;
    }

    /// <summary>
    /// Gets all price types
    /// </summary>
    /// <returns>List of all price types</returns>
    /// <response code="200">Success</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    public async Task<ApiResponse> GetAllPriceTypes()
    {
        return await _priceTypeService.GetAllPriceTypesAsync();
    }

    /// <summary>
    /// Gets a price type by ID
    /// </summary>
    /// <param name="id">Price type ID</param>
    /// <returns>Price type details</returns>
    /// <response code="200">Price type found</response>
    /// <response code="404">Price type not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id}")]
    public async Task<ApiResponse> GetPriceType(int id)
    {
        return await _priceTypeService.GetPriceTypeByIdAsync(id);
    }

    /// <summary>
    /// Gets price types with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="priceTypeName">Search by price type name</param>
    /// <param name="sortBy">Sort field (default: priceTypeName)</param>
    /// <param name="sortOrder">Sort direction: asc or desc (default: asc)</param>
    /// <returns>Paginated list of price types</returns>
    /// <response code="200">Success</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("paged")]
    public async Task<PagedResult<PriceTypeDto>> GetPriceTypesPaged(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? priceTypeName = null,
        [FromQuery] string? sortBy = "priceTypeName",
        [FromQuery] string? sortOrder = "asc")
    {
        return await _priceTypeService.GetPriceTypesPagedAsync(page, pageSize, priceTypeName, sortBy, sortOrder);
    }

    /// <summary>
    /// Creates a new price type
    /// </summary>
    /// <param name="dto">Price type creation data</param>
    /// <returns>Created price type</returns>
    /// <response code="200">Price type created successfully</response>
    /// <response code="400">Validation failed</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    public async Task<ApiResponse> CreatePriceType([FromBody] CreatePriceTypeDto dto)
    {
        return await _priceTypeService.CreatePriceTypeAsync(dto);
    }

    /// <summary>
    /// Updates an existing price type
    /// </summary>
    /// <param name="id">Price type ID</param>
    /// <param name="dto">Price type update data</param>
    /// <returns>Updated price type</returns>
    /// <response code="200">Price type updated successfully</response>
    /// <response code="400">Validation failed</response>
    /// <response code="404">Price type not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPut("{id}")]
    public async Task<ApiResponse> UpdatePriceType(int id, [FromBody] UpdatePriceTypeDto dto)
    {
        return await _priceTypeService.UpdatePriceTypeAsync(id, dto);
    }

    /// <summary>
    /// Deletes a price type (soft delete)
    /// </summary>
    /// <param name="id">Price type ID</param>
    /// <returns>Deletion result</returns>
    /// <response code="200">Price type deleted successfully</response>
    /// <response code="404">Price type not found</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("{id}")]
    public async Task<ApiResponse> DeletePriceType(int id)
    {
        return await _priceTypeService.DeletePriceTypeAsync(id);
    }
}
