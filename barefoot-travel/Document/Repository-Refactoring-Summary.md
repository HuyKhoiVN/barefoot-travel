# Repository Refactoring Summary

## Tổng quan refactoring

Đã thực hiện refactoring toàn diện để tách logic theo nguyên tắc Single Responsibility Principle và implement transaction cho các operations phức tạp.

## Các thay đổi chính

### 1. Tách Repository theo Entity

#### Trước khi refactor:
- `TourRepository` chứa tất cả logic cho Tour, TourImage, TourCategory, TourPrice, TourPolicy
- Vi phạm Single Responsibility Principle
- Khó maintain và test

#### Sau khi refactor:
- **TourRepository**: Chỉ chứa logic liên quan đến Tour entity
- **TourImageRepository**: Chuyên xử lý TourImage operations
- **TourCategoryRepository**: Chuyên xử lý TourCategory operations  
- **TourPriceRepository**: Chuyên xử lý TourPrice operations
- **TourPolicyRepository**: Chuyên xử lý TourPolicy operations

### 2. Cấu trúc Repository mới

#### TourImageRepository
```csharp
public interface ITourImageRepository
{
    Task<TourImageResponseDto?> GetByIdAsync(int id);
    Task<List<TourImageResponseDto>> GetByTourIdAsync(int tourId);
    Task<TourImageResponseDto> CreateAsync(TourImage image);
    Task<TourImageResponseDto> UpdateAsync(TourImage image);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> TourHasImagesAsync(int tourId);
    Task<int> GetImageCountByTourIdAsync(int tourId);
}
```

#### TourCategoryRepository
```csharp
public interface ITourCategoryRepository
{
    Task<TourCategoryResponseDto?> GetByIdAsync(int id);
    Task<List<TourCategoryResponseDto>> GetByTourIdAsync(int tourId);
    Task<TourCategoryResponseDto> CreateAsync(TourCategory category);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> LinkExistsAsync(int tourId, int categoryId);
    Task<List<MarketingTagDto>> GetMarketingTagsByTourIdAsync(int tourId);
    Task<MarketingTagDto> CreateMarketingTagAsync(TourCategory category);
    Task<bool> DeleteMarketingTagAsync(int tourId, int categoryId);
}
```

#### TourPriceRepository
```csharp
public interface ITourPriceRepository
{
    Task<TourPriceResponseDto?> GetByIdAsync(int id);
    Task<List<TourPriceResponseDto>> GetByTourIdAsync(int tourId);
    Task<TourPriceResponseDto> CreateAsync(TourPrice price);
    Task<TourPriceResponseDto> UpdateAsync(TourPrice price);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> TourHasPricesAsync(int tourId);
    Task<decimal> GetMinPriceByTourIdAsync(int tourId);
    Task<decimal> GetMaxPriceByTourIdAsync(int tourId);
}
```

#### TourPolicyRepository
```csharp
public interface ITourPolicyRepository
{
    Task<TourPolicyResponseDto?> GetByIdAsync(int id);
    Task<List<TourPolicyResponseDto>> GetByTourIdAsync(int tourId);
    Task<TourPolicyResponseDto> CreateAsync(TourPolicy policy);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> LinkExistsAsync(int tourId, int policyId);
    Task<bool> TourHasPoliciesAsync(int tourId);
    Task<int> GetPolicyCountByTourIdAsync(int tourId);
}
```

### 3. Refactor TourRepository

#### Loại bỏ logic không liên quan:
- Xóa các method CRUD cho TourImage, TourCategory, TourPrice, TourPolicy
- Chỉ giữ lại các method cơ bản liên quan đến Tour entity
- Thêm các method helper để check relationships

#### TourRepository sau refactor:
```csharp
// Tour CRUD Operations
Task<Tour?> GetByIdAsync(int id);
Task<List<Tour>> GetAllAsync();
Task<PagedResult<Tour>> GetPagedAsync(...);
Task<Tour> CreateAsync(Tour tour);
Task<Tour> UpdateAsync(Tour tour);
Task<bool> DeleteAsync(int id);

// Tour with Related Data - DTO methods with joins
Task<TourDetailDto?> GetTourDetailByIdAsync(int id);
Task<List<TourDto>> GetToursWithBasicInfoAsync();
Task<PagedResult<TourDto>> GetToursPagedWithBasicInfoAsync(...);
Task<List<TourDto>> GetToursByCategoryAsync(int categoryId);

// Optimized bulk operations
Task<List<TourDetailDto>> GetToursWithRelatedDataAsync(List<int> tourIds);
Task<List<TourDto>> GetToursWithBannerImageAsync(int? categoryId = null, int? limit = null);

// Basic relationship checks
Task<bool> TourHasImagesAsync(int tourId);
Task<bool> TourHasCategoriesAsync(int tourId);
Task<bool> TourHasPricesAsync(int tourId);
Task<bool> TourHasPoliciesAsync(int tourId);
Task<bool> TourHasMarketingTagsAsync(int tourId);
```

### 4. Implement Transaction trong TourService

#### CreateTourAsync với Transaction:
```csharp
public async Task<ApiResponse> CreateTourAsync(CreateTourDto dto, string adminUsername)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
        // Validate title uniqueness
        if (await _tourRepository.TitleExistsAsync(dto.Title))
        {
            return new ApiResponse(false, "Tour title already exists");
        }

        // Create tour
        var tour = MapToTour(dto, adminUsername);
        var createdTour = await _tourRepository.CreateAsync(tour);

        // Create related data within transaction
        await CreateTourRelatedDataAsync(createdTour.Id, dto, adminUsername);

        await transaction.CommitAsync();

        var tourDto = MapToTourDto(createdTour);
        return new ApiResponse(true, "Tour created successfully", tourDto);
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        return new ApiResponse(false, $"Error creating tour: {ex.Message}");
    }
}
```

### 5. Cập nhật TourService

#### Dependency Injection mới:
```csharp
public class TourService : ITourService
{
    private readonly ITourRepository _tourRepository;
    private readonly ITourImageRepository _tourImageRepository;
    private readonly ITourCategoryRepository _tourCategoryRepository;
    private readonly ITourPriceRepository _tourPriceRepository;
    private readonly ITourPolicyRepository _tourPolicyRepository;
    private readonly SysDbContext _context;

    public TourService(
        ITourRepository tourRepository,
        ITourImageRepository tourImageRepository,
        ITourCategoryRepository tourCategoryRepository,
        ITourPriceRepository tourPriceRepository,
        ITourPolicyRepository tourPolicyRepository,
        SysDbContext context)
    {
        // ... constructor implementation
    }
}
```

#### Cập nhật các method để sử dụng repository chuyên biệt:
- `CreateTourImageAsync` → sử dụng `_tourImageRepository`
- `CreateTourCategoryAsync` → sử dụng `_tourCategoryRepository`
- `CreateTourPriceAsync` → sử dụng `_tourPriceRepository`
- `CreateTourPolicyAsync` → sử dụng `_tourPolicyRepository`

### 6. Cập nhật Program.cs

#### Đăng ký các repository mới:
```csharp
// Repository Registration
builder.Services.AddScoped<barefoot_travel.Repositories.IAccountRepository, barefoot_travel.Repositories.AccountRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.IRoleRepository, barefoot_travel.Repositories.RoleRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.ITourRepository, barefoot_travel.Repositories.TourRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.ITourImageRepository, barefoot_travel.Repositories.TourImageRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.ITourCategoryRepository, barefoot_travel.Repositories.TourCategoryRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.ITourPriceRepository, barefoot_travel.Repositories.TourPriceRepository>();
builder.Services.AddScoped<barefoot_travel.Repositories.ITourPolicyRepository, barefoot_travel.Repositories.TourPolicyRepository>();
```

## Lợi ích của refactoring

### 1. Single Responsibility Principle
- Mỗi repository chỉ chịu trách nhiệm cho một entity
- Dễ maintain và test
- Code rõ ràng và dễ hiểu

### 2. Transaction Management
- Đảm bảo data consistency cho các operations phức tạp
- Rollback tự động khi có lỗi
- Tránh partial updates

### 3. Better Separation of Concerns
- Service layer chỉ chứa business logic
- Repository layer chỉ chứa data access logic
- Dễ dàng thay đổi implementation

### 4. Improved Testability
- Có thể mock từng repository riêng biệt
- Unit test dễ viết và maintain
- Integration test chính xác hơn

### 5. Scalability
- Dễ dàng thêm repository mới
- Có thể optimize từng repository riêng biệt
- Support cho microservices architecture

## Khuyến nghị tiếp theo

### 1. Unit Testing
- Viết unit test cho từng repository
- Test transaction scenarios
- Mock dependencies properly

### 2. Integration Testing
- Test end-to-end scenarios
- Test transaction rollback
- Test performance với large datasets

### 3. Monitoring
- Monitor transaction performance
- Track repository usage
- Alert on transaction failures

### 4. Documentation
- Update API documentation
- Document transaction boundaries
- Create migration guide

## Kết luận

Refactoring đã cải thiện đáng kể:
- **Code organization**: Rõ ràng và dễ maintain
- **Data consistency**: Transaction đảm bảo ACID properties
- **Testability**: Dễ test và debug
- **Scalability**: Dễ mở rộng và optimize

Cấu trúc mới tuân thủ các nguyên tắc SOLID và best practices của .NET development.
