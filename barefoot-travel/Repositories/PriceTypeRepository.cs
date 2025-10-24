using Microsoft.EntityFrameworkCore;
using barefoot_travel.Models;
using barefoot_travel.DTOs;

namespace barefoot_travel.Repositories;

public class PriceTypeRepository : IPriceTypeRepository
{
    private readonly SysDbContext _context;

    public PriceTypeRepository(SysDbContext context)
    {
        _context = context;
    }

    public async Task<PriceType?> GetByIdAsync(int id)
    {
        return await _context.PriceTypes
            .Where(pt => pt.Id == id && pt.Active)
            .FirstOrDefaultAsync();
    }

    public async Task<List<PriceType>> GetAllAsync()
    {
        return await _context.PriceTypes
            .Where(pt => pt.Active)
            .OrderBy(pt => pt.PriceTypeName)
            .ToListAsync();
    }

    public async Task<PagedResult<PriceType>> GetPagedAsync(int page, int pageSize, string? priceTypeName = null, string? sortBy = "priceTypeName", string? sortOrder = "asc")
    {
        var query = _context.PriceTypes
            .Where(pt => pt.Active);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(priceTypeName))
        {
            query = query.Where(pt => pt.PriceTypeName.Contains(priceTypeName));
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "pricetypename" => sortOrder?.ToLower() == "desc" 
                ? query.OrderByDescending(pt => pt.PriceTypeName)
                : query.OrderBy(pt => pt.PriceTypeName),
            "createdtime" => sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(pt => pt.CreatedTime)
                : query.OrderBy(pt => pt.CreatedTime),
            _ => query.OrderBy(pt => pt.PriceTypeName)
        };

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<PriceType>
        {
            Items = items,
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<PriceType> CreateAsync(PriceType priceType)
    {
        _context.PriceTypes.Add(priceType);
        await _context.SaveChangesAsync();
        return priceType;
    }

    public async Task<PriceType> UpdateAsync(PriceType priceType)
    {
        _context.PriceTypes.Update(priceType);
        await _context.SaveChangesAsync();
        return priceType;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var priceType = await _context.PriceTypes.FindAsync(id);
        if (priceType == null) return false;

        priceType.Active = false;
        priceType.UpdatedTime = DateTime.UtcNow;
        
        _context.PriceTypes.Update(priceType);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.PriceTypes
            .AnyAsync(pt => pt.Id == id && pt.Active);
    }

    public async Task<bool> ExistsByNameAsync(string priceTypeName, int? excludeId = null)
    {
        var query = _context.PriceTypes
            .Where(pt => pt.PriceTypeName == priceTypeName && pt.Active);

        if (excludeId.HasValue)
        {
            query = query.Where(pt => pt.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}
