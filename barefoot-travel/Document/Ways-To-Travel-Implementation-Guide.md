# Ways to Travel - Implementation Guide

## Step-by-Step Implementation

### Step 1: Update Category Model
Add these properties to `Models/Category.cs`:
```csharp
public string? WaysToTravelImage1 { get; set; }
public string? WaysToTravelImage2 { get; set; }
public int? WaysToTravelOrder { get; set; }
public bool ShowInWaysToTravel { get; set; }
```

### Step 2: Update SysDbContext
In `Models/SysDbContext.cs`, OnModelCreating method, add:
```csharp
entity.Property(e => e.WaysToTravelImage1).HasMaxLength(500);
entity.Property(e => e.WaysToTravelImage2).HasMaxLength(500);
entity.Property(e => e.ShowInWaysToTravel).HasDefaultValue(false);
```

### Step 3: Create DTO
File: `DTOs/WaysToTravelDto.cs`
```csharp
public class WaysToTravelCategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalTours { get; set; }
    public string ImageUrl1 { get; set; } = string.Empty;
    public string? ImageUrl2 { get; set; }
    public int DisplayOrder { get; set; }
}
```

### Step 4: Add API Endpoints to HomePageController
Add methods to `Controllers/Api/HomePageController.cs`:
- GetWaysToTravelCategories()
- ConfigureCategoryForWaysToTravel(id, dto)
- RemoveFromWaysToTravel(id)
- ReorderWaysToTravelCategories(orders)

### Step 5: UI Integration
Add "Configure Ways to Travel" section in `Views/HomePageContent/Index.cshtml`

## Key Features to Implement

1. **Image Gallery Modal**
   - Browse uploaded images from /uploads
   - Upload new images
   - Image preview and selection
   - Responsive design for mobile/desktop

2. **Category Selection**
   - Dropdown with available categories
   - Maximum 5 categories
   - Display order management

3. **Validation**
   - Max 5 categories
   - At least 1 image required per category
   - Image file type validation (.jpg, .png, .gif, .webp)
   - Max file size 5MB

4. **Display on Homepage**
   - Update `Views/Home/Index.cshtml` to fetch from API
   - Render travel grid layout
   - Show CategoryName + TotalTours
