# HomePage SOLID Architecture Implementation

## Overview
This document describes the architectural refactoring of the HomePage functionality to comply with SOLID principles and proper separation of concerns.

## Problem Statement

### Issues in the Original Implementation
1. **Controller Violations:**
   - Controllers contained business logic
   - Controllers directly accessed repositories
   - Controllers handled DTO-to-Entity conversions
   - Controllers performed data validation

2. **Repository Violations:**
   - Some repository methods returned DTOs instead of entities
   - Business logic mixed with data access logic

3. **Missing Service Layer:**
   - No dedicated service layer for business logic
   - No abstraction between controllers and repositories

## Architecture After Refactoring

### Layers and Responsibilities

#### 1. Controller Layer (`Controllers/Api/HomePageController.cs`)
**Responsibilities:**
- Receive HTTP requests
- Delegate to service layer
- Handle HTTP-specific concerns (status codes, responses)
- Exception handling and error responses
- User authentication/authorization

**What it does NOT do:**
- Business logic
- Data validation (beyond DTO validation)
- Entity-to-DTO conversion
- Direct database access

**Example:**
```csharp
[HttpPut("category/{categoryId}/homepage")]
public async Task<IActionResult> ConfigureCategoryHomepage(int categoryId, [FromBody] ConfigureHomepageDto dto)
{
    try
    {
        var userId = GetUserIdFromClaims.GetUserId(this.User).ToString();
        await _homePageService.ConfigureCategoryHomepageAsync(categoryId, dto, userId);
        return Ok(new ApiResponse(true, "Success"));
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new ApiResponse(false, ex.Message));
    }
}
```

#### 2. Service Layer (`Services/HomePageService.cs`)
**Responsibilities:**
- Business logic implementation
- Data validation
- Entity-to-DTO conversion
- Transaction coordination
- Business rule enforcement

**What it does:**
- Validates business rules (e.g., "Maximum 5 categories in Ways to Travel")
- Enforces constraints (e.g., "Spotlight layout limited to 3 items")
- Coordinates multiple repository calls
- Converts between entities and DTOs
- Handles business exceptions

**Example:**
```csharp
public async Task ConfigureCategoryHomepageAsync(int categoryId, ConfigureHomepageDto dto, string userId)
{
    // Business validation
    var category = await _categoryRepository.GetByIdAsync(categoryId);
    if (category == null)
        throw new InvalidOperationException("Category not found");

    // Business logic
    ValidateLayoutStyle(dto.LayoutStyle);
    EnforceSpotlightConstraint(dto);

    // Entity update
    UpdateCategoryForHomepage(category, dto.HomepageTitle, configJson, dto.DisplayOrder, userId);
    await _categoryRepository.UpdateAsync(category);
}
```

#### 3. Repository Layer (`Repositories/`)
**Responsibilities:**
- Data access operations
- Query execution
- Entity CRUD operations
- Return entities (NOT DTOs)

**What it does NOT do:**
- Business logic
- Data validation beyond data integrity
- Return DTOs (exception: specialized query methods for performance)

**Example:**
```csharp
public async Task<List<Category>> GetCategoriesWithHomepageConfigAsync()
{
    return await _context.Categories
        .Where(c => c.HomepageTitle != null && c.Active)
        .OrderBy(c => c.HomepageOrder ?? c.Priority)
        .ToListAsync();
}
```

## SOLID Principles Applied

### S - Single Responsibility Principle
Each class has one reason to change:
- **Controller:** HTTP concerns only
- **Service:** Business logic only
- **Repository:** Data access only

### O - Open/Closed Principle
- Services can be extended without modifying controllers
- Repository interfaces allow for different implementations

### L - Liskov Substitution Principle
- Service implementations can be swapped via dependency injection
- Repository implementations are interchangeable

### I - Interface Segregation Principle
- Interfaces are specific to their use case (`IHomePageService`)
- Clients only depend on methods they use

### D - Dependency Inversion Principle
- Controllers depend on service interfaces, not implementations
- Services depend on repository interfaces
- Dependency injection provides concrete implementations

## Migration Benefits

### 1. Separation of Concerns
- Clear boundaries between layers
- Each layer has a single responsibility
- Changes in one layer don't affect others

### 2. Testability
- Services can be unit tested independently
- Mock repositories for service tests
- Mock services for controller tests

### 3. Maintainability
- Business logic centralized in services
- Easy to locate and modify business rules
- Changes isolated to specific layers

### 4. Reusability
- Services can be reused by different controllers
- Business logic not tied to HTTP layer

### 5. Scalability
- Easy to add new endpoints
- Services can be split into smaller services as they grow

## Code Examples

### Before (Controller with Business Logic)
```csharp
[HttpPut("category/{categoryId}/homepage")]
public async Task<IActionResult> ConfigureCategoryHomepage(int categoryId, [FromBody] ConfigureHomepageDto dto)
{
    var category = await _categoryRepository.GetByIdAsync(categoryId);
    if (category == null) return NotFound();
    
    // Business logic in controller - BAD!
    var validLayouts = new[] { "grid", "grid-2", "grid-3", "spotlight", "carousel" };
    if (!validLayouts.Contains(dto.LayoutStyle)) return BadRequest();
    
    if (dto.LayoutStyle == "spotlight" && dto.MaxItems > 3) dto.MaxItems = 3;
    
    var configJson = JsonSerializer.Serialize(config);
    category.HomepageTitle = dto.HomepageTitle;
    // ... more business logic in controller
    await _categoryRepository.UpdateAsync(category);
}
```

### After (Controller delegates to Service)
```csharp
[HttpPut("category/{categoryId}/homepage")]
public async Task<IActionResult> ConfigureCategoryHomepage(int categoryId, [FromBody] ConfigureHomepageDto dto)
{
    try
    {
        var userId = GetUserIdFromClaims.GetUserId(this.User).ToString();
        await _homePageService.ConfigureCategoryHomepageAsync(categoryId, dto, userId);
        return Ok(new ApiResponse(true, "Success"));
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new ApiResponse(false, ex.Message));
    }
}
```

## File Structure

```
Controllers/
├── Api/
│   └── HomePageController.cs        # HTTP handling only
Services/
├── IHomePageService.cs               # Service interface
└── HomePageService.cs                # Business logic
Repositories/
├── ICategoryRepository.cs            # Repository interface
├── CategoryRepository.cs             # Data access
├── ITourRepository.cs
└── TourRepository.cs
DTOs/
└── [Various DTOs]                    # Data transfer objects
```

## Dependency Injection Configuration

```csharp
// Program.cs
builder.Services.AddScoped<IHomePageService, HomePageService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITourRepository, TourRepository>();
```

## Testing Strategy

### Unit Tests
- **Service Tests:** Test business logic with mocked repositories
- **Repository Tests:** Test data access with in-memory database

### Integration Tests
- **API Tests:** Test full request/response cycle
- **Database Tests:** Test with real database

## Benefits Summary

✅ **Separation of Concerns:** Clear boundaries between layers
✅ **Testability:** Easy to unit test each layer
✅ **Maintainability:** Business logic centralized and easy to find
✅ **Scalability:** Easy to extend and modify
✅ **SOLID Compliance:** Follows all SOLID principles
✅ **Clean Architecture:** Proper layering and dependencies

## Conclusion

The refactored architecture properly separates concerns and follows SOLID principles, making the codebase more maintainable, testable, and scalable. Each layer has a clear responsibility, and changes can be made with confidence that they won't have unintended side effects.
