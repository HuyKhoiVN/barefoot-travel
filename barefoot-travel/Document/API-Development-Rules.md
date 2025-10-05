# API Development Rules - Barefoot Travel System

## Tổng quan

Tài liệu này định nghĩa các quy tắc phát triển cho Services, Repositories và Controllers trong hệ thống Barefoot Travel, đảm bảo tính nhất quán và chuẩn hóa trong việc xử lý dữ liệu và trả về response.

## 1. Quy tắc chung về Models và Database

### 1.1 Models và Database Schema
**Quy tắc**:
- **KHÔNG BAO GIỜ** thay đổi Models gốc và SysDbContext.cs
- **KHÔNG TỰ Ý** tạo table mới hoặc thay đổi cấu trúc table gốc
- **TUÂN THỦ NGHIÊM NGẶT** Database-schema.md đã định nghĩa
- **GIỮ NGUYÊN** tất cả Models hiện có trong project

### 1.2 Validation Rules
**Quy tắc**:
- **TẤT CẢ VALIDATION** diễn ra ở Service layer
- **KHÔNG SỬ DỤNG** Data Annotations trong Models
- **SỬ DỤNG** FluentValidation hoặc custom validation trong Service
- **Models chỉ chứa** properties và navigation properties

### 1.3 Database Operations
**Quy tắc**:
- **SỬ DỤNG** Models gốc cho tất cả database operations
- **KHÔNG TẠO** Models mới cho cùng một entity
- **SỬ DỤNG** existing DbContext configuration
- **TUÂN THỦ** naming convention trong Database-schema.md

---

## 2. Kiểu trả về chuẩn

### 1.1 ApiResponse Class
**File**: `Common/ApiResponse.cs`

```csharp
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public object? Data { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
}
```

**Sử dụng cho**:
- Tất cả API endpoints
- Response có dữ liệu đơn lẻ
- Response có authentication token
- Response có thông báo lỗi/thành công

### 1.2 PagedResult Class
**File**: `DTOs/PagedResult.cs`

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
}
```

**Sử dụng cho**:
- Danh sách có phân trang
- Search results
- List data với metadata

---

## 2. Controller Types

### 2.1 ApiController
**Mục đích**: Xử lý API requests, trả về JSON data

**Quy tắc**:
- **Namespace**: `barefoot_travel.Controllers.Api`
- **Base Class**: `ControllerBase`
- **Route**: `[Route("api/[controller]")]`
- **Return Type**: Luôn trả về `ApiResponse` hoặc `PagedResult<T>`
- **Content-Type**: `application/json`

**Ví dụ**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class TourController : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse> GetAllTours()
    {
        // Logic here
        return new ApiResponse(true, "Success", data);
    }
    
    [HttpGet("paged")]
    public async Task<PagedResult<TourDto>> GetToursPaged(int page = 1, int pageSize = 10)
    {
        // Logic here
        return pagedResult;
    }
}
```

### 2.2 ClientController
**Mục đích**: Xử lý web requests, trả về Views

**Quy tắc**:
- **Namespace**: `barefoot_travel.Controllers`
- **Base Class**: `Controller`
- **Route**: `[Route("[controller]")]`
- **Return Type**: Luôn trả về `View()`, `RedirectToAction()`, hoặc `IActionResult`
- **Content-Type**: `text/html`

**Ví dụ**:
```csharp
public class TourController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult Details(int id)
    {
        return View();
    }
}
```

---

## 3. Repository Layer Rules

### 3.1 Interface Definition
**Quy tắc**:
- **Namespace**: `barefoot_travel.Repositories.Interfaces`
- **Naming**: `I{EntityName}Repository`
- **Methods**: Chỉ trả về raw data hoặc primitive types

```csharp
public interface ITourRepository
{
    Task<Tour?> GetByIdAsync(int id);
    Task<List<Tour>> GetAllAsync();
    Task<List<Tour>> GetByCategoryAsync(int categoryId);
    Task<PagedResult<Tour>> GetPagedAsync(int page, int pageSize);
    Task<Tour> CreateAsync(Tour tour);
    Task<Tour> UpdateAsync(Tour tour);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
```

### 3.2 Implementation
**Quy tắc**:
- **Namespace**: `barefoot_travel.Repositories`
- **Naming**: `{EntityName}Repository`
- **Base Class**: Implement interface
- **Dependency**: Inject `SysDbContext`

```csharp
public class TourRepository : ITourRepository
{
    private readonly SysDbContext _context;
    
    public TourRepository(SysDbContext context)
    {
        _context = context;
    }
    
    public async Task<Tour?> GetByIdAsync(int id)
    {
        return await _context.Tours
            .Where(t => t.Id == id && t.Active)
            .FirstOrDefaultAsync();
    }
    
    // Other methods...
}
```

---

## 4. Service Layer Rules

### 4.1 Interface Definition
**Quy tắc**:
- **Namespace**: `barefoot_travel.Services.Interfaces`
- **Naming**: `I{EntityName}Service`
- **Methods**: Chứa business logic, validation, mapping thủ công

```csharp
public interface ITourService
{
    Task<ApiResponse> GetTourByIdAsync(int id);
    Task<ApiResponse> GetAllToursAsync();
    Task<PagedResult<TourDto>> GetToursPagedAsync(int page, int pageSize);
    Task<ApiResponse> CreateTourAsync(CreateTourDto dto);
    Task<ApiResponse> UpdateTourAsync(int id, UpdateTourDto dto);
    Task<ApiResponse> DeleteTourAsync(int id);
}
```

### 4.2 Implementation
**Quy tắc**:
- **Namespace**: `barefoot_travel.Services`
- **Naming**: `{EntityName}Service`
- **Dependencies**: Inject repositories, validation services
- **Mapping**: Sử dụng mapping thủ công giữa Models và DTOs

```csharp
public class TourService : ITourService
{
    private readonly ITourRepository _tourRepository;
    private readonly IValidator<CreateTourDto> _validator;
    
    public TourService(
        ITourRepository tourRepository,
        IValidator<CreateTourDto> validator)
    {
        _tourRepository = tourRepository;
        _validator = validator;
    }
    
    public async Task<ApiResponse> GetTourByIdAsync(int id)
    {
        try
        {
            var tour = await _tourRepository.GetByIdAsync(id);
            if (tour == null)
            {
                return new ApiResponse(false, "Tour not found");
            }
            
            var tourDto = MapToTourDto(tour);
            return new ApiResponse(true, "Success", tourDto);
        }
        catch (Exception ex)
        {
            return new ApiResponse(false, $"Error: {ex.Message}");
        }
    }
    
    public async Task<PagedResult<TourDto>> GetToursPagedAsync(int page, int pageSize)
    {
        var pagedResult = await _tourRepository.GetPagedAsync(page, pageSize);
        var tourDtos = pagedResult.Items.Select(MapToTourDto).ToList();
        
        return new PagedResult<TourDto>
        {
            Items = tourDtos,
            TotalItems = pagedResult.TotalItems,
            TotalPages = pagedResult.TotalPages,
            CurrentPage = pagedResult.CurrentPage,
            PageSize = pagedResult.PageSize
        };
    }
    
    // Manual mapping methods
    private TourDto MapToTourDto(Tour tour)
    {
        return new TourDto
        {
            Id = tour.Id,
            Title = tour.Title,
            Description = tour.Description,
            MapLink = tour.MapLink,
            PricePerPerson = tour.PricePerPerson,
            MaxPeople = tour.MaxPeople,
            Duration = tour.Duration,
            StartTime = tour.StartTime,
            ReturnTime = tour.ReturnTime,
            CreatedTime = tour.CreatedTime,
            UpdatedTime = tour.UpdatedTime,
            Active = tour.Active
        };
    }
    
    private Tour MapToTour(CreateTourDto dto)
    {
        return new Tour
        {
            Title = dto.Title,
            Description = dto.Description,
            MapLink = dto.MapLink,
            PricePerPerson = dto.PricePerPerson,
            MaxPeople = dto.MaxPeople,
            Duration = dto.Duration,
            StartTime = dto.StartTime,
            ReturnTime = dto.ReturnTime,
            Active = true,
            CreatedTime = DateTime.UtcNow
        };
    }
}
```

---

## 5. Manual Mapping Guidelines

### 5.1 Mapping Methods Naming Convention
**Quy tắc**:
- **Model to DTO**: `MapTo{EntityName}Dto`
- **DTO to Model**: `MapTo{EntityName}`
- **Update Model**: `MapTo{EntityName}ForUpdate`

```csharp
// Examples
private TourDto MapToTourDto(Tour tour)
private Tour MapToTour(CreateTourDto dto)
private void MapToTourForUpdate(Tour existingTour, UpdateTourDto dto)
```

### 5.2 Mapping Method Structure
**Quy tắc**:
- Đặt tất cả mapping methods ở cuối class
- Sử dụng `private` access modifier
- Luôn kiểm tra null values
- Sử dụng null-conditional operators khi cần thiết

```csharp
private TourDto MapToTourDto(Tour tour)
{
    if (tour == null) return null;
    
    return new TourDto
    {
        Id = tour.Id,
        Title = tour.Title,
        Description = tour.Description,
        MapLink = tour.MapLink,
        PricePerPerson = tour.PricePerPerson,
        MaxPeople = tour.MaxPeople,
        Duration = tour.Duration,
        StartTime = tour.StartTime,
        ReturnTime = tour.ReturnTime,
        CreatedTime = tour.CreatedTime,
        UpdatedTime = tour.UpdatedTime,
        Active = tour.Active
    };
}

private Tour MapToTour(CreateTourDto dto)
{
    if (dto == null) return null;
    
    return new Tour
    {
        Title = dto.Title,
        Description = dto.Description,
        MapLink = dto.MapLink,
        PricePerPerson = dto.PricePerPerson,
        MaxPeople = dto.MaxPeople,
        Duration = dto.Duration,
        StartTime = dto.StartTime,
        ReturnTime = dto.ReturnTime,
        Active = true,
        CreatedTime = DateTime.UtcNow
    };
}

private void MapToTourForUpdate(Tour existingTour, UpdateTourDto dto)
{
    if (existingTour == null || dto == null) return;
    
    existingTour.Title = dto.Title;
    existingTour.Description = dto.Description;
    existingTour.MapLink = dto.MapLink;
    existingTour.PricePerPerson = dto.PricePerPerson;
    existingTour.MaxPeople = dto.MaxPeople;
    existingTour.Duration = dto.Duration;
    existingTour.StartTime = dto.StartTime;
    existingTour.ReturnTime = dto.ReturnTime;
    existingTour.UpdatedTime = DateTime.UtcNow;
    existingTour.UpdatedBy = dto.UpdatedBy;
}
```

### 5.3 Complex Mapping Examples

#### Mapping với Related Entities
```csharp
private TourDetailDto MapToTourDetailDto(Tour tour)
{
    if (tour == null) return null;
    
    return new TourDetailDto
    {
        Id = tour.Id,
        Title = tour.Title,
        Description = tour.Description,
        PricePerPerson = tour.PricePerPerson,
        MaxPeople = tour.MaxPeople,
        Duration = tour.Duration,
        StartTime = tour.StartTime,
        ReturnTime = tour.ReturnTime,
        Images = tour.TourImages?.Select(MapToTourImageDto).ToList() ?? new List<TourImageDto>(),
        Categories = tour.TourCategories?.Select(tc => MapToCategoryDto(tc.Category)).ToList() ?? new List<CategoryDto>(),
        Policies = tour.TourPolicies?.Select(tp => MapToPolicyDto(tp.Policy)).ToList() ?? new List<PolicyDto>()
    };
}

private TourImageDto MapToTourImageDto(TourImage image)
{
    if (image == null) return null;
    
    return new TourImageDto
    {
        Id = image.Id,
        ImageUrl = image.ImageUrl,
        IsBanner = image.IsBanner
    };
}
```

#### Mapping cho List Operations
```csharp
private List<TourDto> MapToTourDtoList(List<Tour> tours)
{
    if (tours == null || !tours.Any()) return new List<TourDto>();
    
    return tours.Select(MapToTourDto).Where(dto => dto != null).ToList();
}
```

### 5.4 Mapping Best Practices

1. **Null Safety**: Luôn kiểm tra null trước khi mapping
2. **Performance**: Sử dụng LINQ Select cho list mapping
3. **Consistency**: Giữ naming convention nhất quán
4. **Maintainability**: Tách mapping methods riêng biệt
5. **Testing**: Dễ dàng unit test mapping logic

```csharp
// Good: Null-safe mapping
private AccountDto MapToAccountDto(Account account)
{
    if (account == null) return null;
    
    return new AccountDto
    {
        Id = account.Id,
        Username = account.Username,
        FullName = account.FullName,
        Email = account.Email,
        Phone = account.Phone,
        Photo = account.Photo,
        RoleId = account.RoleId,
        CreatedTime = account.CreatedTime,
        Active = account.Active
    };
}

// Bad: Không kiểm tra null
private AccountDto MapToAccountDto(Account account)
{
    return new AccountDto
    {
        Id = account.Id, // Có thể throw NullReferenceException
        Username = account.Username,
        // ...
    };
}
```

---

## 6. API Response Patterns

### 6.1 Success Responses

#### Single Data
```csharp
// Success with data
return new ApiResponse(true, "Operation successful", data);

// Success without data
return new ApiResponse(true, "Operation successful");
```

#### Paged Data
```csharp
// Paged results
return new PagedResult<TourDto>
{
    Items = tourDtos,
    TotalItems = totalCount,
    TotalPages = totalPages,
    CurrentPage = page,
    PageSize = pageSize
};
```

#### Authentication
```csharp
// Login success
return new ApiResponse(true, "Login successful", userData, token, refreshToken);
```

### 6.2 Error Responses

#### Validation Error
```csharp
return new ApiResponse(false, "Validation failed", validationErrors);
```

#### Not Found
```csharp
return new ApiResponse(false, "Resource not found");
```

#### Unauthorized
```csharp
return new ApiResponse(false, "Unauthorized access");
```

#### Server Error
```csharp
return new ApiResponse(false, "Internal server error");
```

---

## 7. Controller Implementation Examples

### 7.1 ApiController Example

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Nếu cần authentication
public class TourController : ControllerBase
{
    private readonly ITourService _tourService;
    
    public TourController(ITourService tourService)
    {
        _tourService = tourService;
    }
    
    [HttpGet]
    public async Task<ApiResponse> GetAllTours()
    {
        return await _tourService.GetAllToursAsync();
    }
    
    [HttpGet("{id}")]
    public async Task<ApiResponse> GetTour(int id)
    {
        return await _tourService.GetTourByIdAsync(id);
    }
    
    [HttpGet("paged")]
    public async Task<PagedResult<TourDto>> GetToursPaged(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        return await _tourService.GetToursPagedAsync(page, pageSize);
    }
    
    [HttpPost]
    public async Task<ApiResponse> CreateTour([FromBody] CreateTourDto dto)
    {
        return await _tourService.CreateTourAsync(dto);
    }
    
    [HttpPut("{id}")]
    public async Task<ApiResponse> UpdateTour(int id, [FromBody] UpdateTourDto dto)
    {
        return await _tourService.UpdateTourAsync(id, dto);
    }
    
    [HttpDelete("{id}")]
    public async Task<ApiResponse> DeleteTour(int id)
    {
        return await _tourService.DeleteTourAsync(id);
    }
}
```

### 7.2 ClientController Example

```csharp
public class TourController : Controller
{
    private readonly ITourService _tourService;
    
    public TourController(ITourService tourService)
    {
        _tourService = tourService;
    }
    
    public async Task<IActionResult> Index()
    {
        // Logic được viết trong Razor script
        return View();
    }
    
    public async Task<IActionResult> Details(int id)
    {
        // Logic được viết trong Razor script
        return View();
    }
    
    public async Task<IActionResult> Create()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateTourDto dto)
    {
        // Logic được viết trong Razor script
        return View();
    }
}
```

---

## 8. Dependency Injection Configuration

### 8.1 Program.cs Registration

```csharp
// Repositories
builder.Services.AddScoped<ITourRepository, TourRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// Services
builder.Services.AddScoped<ITourService, TourService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Note: AutoMapper đã được loại bỏ, sử dụng mapping thủ công
```

---

## 9. Validation Rules

### 9.1 DTO Validation
```csharp
public class CreateTourDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
    public string Title { get; set; }
    
    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal PricePerPerson { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Max people must be at least 1")]
    public int MaxPeople { get; set; }
}
```

### 9.2 Service Validation
```csharp
public async Task<ApiResponse> CreateTourAsync(CreateTourDto dto)
{
    var validationResult = await _validator.ValidateAsync(dto);
    if (!validationResult.IsValid)
    {
        var errors = validationResult.Errors.Select(e => e.ErrorMessage);
        return new ApiResponse(false, "Validation failed", errors);
    }
    
    // Continue with business logic...
}
```

---

## 10. Error Handling

### 10.1 Global Exception Handling
```csharp
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var response = new ApiResponse(false, "An error occurred");
        
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

---

## 11. Best Practices

### 11.1 Naming Conventions
- **Controllers**: `{EntityName}Controller`
- **Services**: `{EntityName}Service`
- **Repositories**: `{EntityName}Repository`
- **DTOs**: `{Action}{EntityName}Dto` (CreateTourDto, UpdateTourDto)

### 11.2 Method Naming
- **Repository**: `GetByIdAsync`, `GetAllAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`
- **Service**: `Get{Entity}ByIdAsync`, `GetAll{Entity}Async`, `Create{Entity}Async`
- **Controller**: `Get{Entity}`, `GetAll{Entity}`, `Create{Entity}`, `Update{Entity}`, `Delete{Entity}`

### 11.3 Response Consistency
- Luôn sử dụng `ApiResponse` cho API endpoints
- Luôn sử dụng `PagedResult<T>` cho danh sách có phân trang
- Luôn trả về `View()` cho ClientController
- Luôn xử lý exception và trả về response phù hợp

### 11.4 Logging
```csharp
public async Task<ApiResponse> GetTourByIdAsync(int id)
{
    _logger.LogInformation("Getting tour with ID: {TourId}", id);
    
    try
    {
        var tour = await _tourRepository.GetByIdAsync(id);
        if (tour == null)
        {
            _logger.LogWarning("Tour with ID {TourId} not found", id);
            return new ApiResponse(false, "Tour not found");
        }
        
        _logger.LogInformation("Successfully retrieved tour with ID: {TourId}", id);
        return new ApiResponse(true, "Success", tour);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting tour with ID: {TourId}", id);
        return new ApiResponse(false, "An error occurred");
    }
}
```

---

## 12. Testing Guidelines

### 12.1 Unit Testing
- Test tất cả service methods
- Mock dependencies (repositories, external services)
- Test success và error scenarios
- Test mapping methods riêng biệt

#### Mapping Method Testing
```csharp
[Test]
public void MapToTourDto_WithValidTour_ReturnsCorrectDto()
{
    // Arrange
    var tour = new Tour
    {
        Id = 1,
        Title = "Test Tour",
        Description = "Test Description",
        PricePerPerson = 100.00m,
        MaxPeople = 10,
        Duration = "3 days",
        Active = true,
        CreatedTime = DateTime.UtcNow
    };
    
    var service = new TourService(mockRepository, mockValidator);
    
    // Act
    var result = service.MapToTourDto(tour);
    
    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result.Id, Is.EqualTo(1));
    Assert.That(result.Title, Is.EqualTo("Test Tour"));
    Assert.That(result.PricePerPerson, Is.EqualTo(100.00m));
}

[Test]
public void MapToTourDto_WithNullTour_ReturnsNull()
{
    // Arrange
    var service = new TourService(mockRepository, mockValidator);
    
    // Act
    var result = service.MapToTourDto(null);
    
    // Assert
    Assert.That(result, Is.Null);
}
```

### 12.2 Integration Testing
- Test API endpoints
- Test database operations
- Test authentication/authorization

### 12.3 Test Data
- Sử dụng test data builders
- Clean up test data sau mỗi test
- Sử dụng in-memory database cho testing

---

## 13. Documentation

### 13.1 XML Comments
```csharp
/// <summary>
/// Gets a tour by its unique identifier
/// </summary>
/// <param name="id">The tour identifier</param>
/// <returns>ApiResponse containing tour data or error message</returns>
/// <response code="200">Tour found successfully</response>
/// <response code="404">Tour not found</response>
/// <response code="500">Internal server error</response>
[HttpGet("{id}")]
public async Task<ApiResponse> GetTour(int id)
{
    return await _tourService.GetTourByIdAsync(id);
}
```

### 13.2 Swagger Documentation
- Sử dụng XML comments để generate Swagger docs
- Thêm examples cho request/response
- Document authentication requirements

---

## Kết luận

Tài liệu này cung cấp framework hoàn chỉnh cho việc phát triển API trong hệ thống Barefoot Travel. Việc tuân thủ các quy tắc này sẽ đảm bảo:

1. **Tính nhất quán** trong codebase
2. **Dễ bảo trì** và mở rộng
3. **Hiệu suất cao** và ổn định
4. **Dễ test** và debug
5. **Documentation** đầy đủ và rõ ràng
6. **Mapping thủ công** rõ ràng và dễ hiểu (không phụ thuộc AutoMapper)

### Đặc điểm chính của framework:

- **Manual Mapping**: Sử dụng mapping thủ công thay vì AutoMapper để có control tốt hơn
- **Null Safety**: Tất cả mapping methods đều kiểm tra null
- **Performance**: Mapping trực tiếp không có overhead của reflection
- **Maintainability**: Code mapping rõ ràng, dễ đọc và maintain
- **Testing**: Dễ dàng unit test mapping logic

### Lưu ý quan trọng:

- **KHÔNG sử dụng AutoMapper** trong bất kỳ trường hợp nào
- **Luôn sử dụng mapping thủ công** giữa Models và DTOs
- **Tuân thủ naming convention** cho mapping methods
- **Test mapping methods** riêng biệt để đảm bảo tính chính xác

Tất cả developers trong team cần tuân thủ nghiêm ngặt các quy tắc này để đảm bảo chất lượng code và tính nhất quán của hệ thống.
