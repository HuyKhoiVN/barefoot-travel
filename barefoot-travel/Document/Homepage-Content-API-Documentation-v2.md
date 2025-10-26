# Homepage Content Management - Complete Documentation

## Overview
This document describes a comprehensive system for managing homepage content with minimal database changes. The system allows admins to configure all homepage sections including Featured Tours and Category-based sections dynamically.

## Key Design Principles

1. **Minimal Database Changes**: Only add 3 new fields to existing Category table and 1 new table for Featured Tours
2. **Category-Based**: Use existing Category.Type field to determine display style
3. **Flexible Display**: Same category can be configured differently on homepage
4. **Easy Management**: Simple UI to manage homepage sections without touching tour data
5. **Constraints**: Maximum 2 Featured Tours, maximum 3 tours per spotlight section

## Database Changes

### Change 1: Add Fields to Category Table

```sql
-- Add three optional fields to existing Category table
ALTER TABLE Category
ADD HomepageTitle NVARCHAR(200) NULL,      -- Custom title for homepage display
    HomepageConfig NVARCHAR(MAX) NULL,     -- JSON configuration for homepage display
    HomepageOrder INT NULL;                -- Display order on homepage

-- Add index for homepage configuration
CREATE INDEX IX_Category_HomepageDisplay ON Category(HomepageTitle, Active, HomepageOrder);
```

**Why 3 fields?**
- `HomepageTitle`: Allows different display name on homepage vs admin panel
- `HomepageConfig`: JSON field to store flexible configuration without schema changes
- `HomepageOrder`: Separate ordering for homepage sections (independent from Priority)

### Change 2: Create Featured Tours Table

```sql
-- Create new table for featured tours
CREATE TABLE HomePageFeaturedTours (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TourId INT NOT NULL,
    DisplayOrder INT NOT NULL,
    Title NVARCHAR(200) NULL,              -- Override title for homepage display
    Category NVARCHAR(100) NULL,           -- Category badge text
    Description NVARCHAR(500) NULL,        -- Short description
    ImageUrl NVARCHAR(500) NULL,           -- Override image URL
    CardClass NVARCHAR(100) NULL,          -- Custom CSS class
    Active BIT NOT NULL DEFAULT 1,
    CreatedTime DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedTime DATETIME NULL,
    UpdatedBy NVARCHAR(100) NULL,
    CONSTRAINT FK_FeaturedTour_Tour FOREIGN KEY (TourId) REFERENCES Tour(Id) ON DELETE CASCADE
);

-- Add indexes
CREATE INDEX IX_FeaturedTour_DisplayOrder ON HomePageFeaturedTours(DisplayOrder, Active);
CREATE INDEX IX_FeaturedTour_TourId ON HomePageFeaturedTours(TourId);

-- Add constraint: Maximum 2 active featured tours
-- This will be enforced at application level
```

### Example HomepageConfig JSON:
```json
{
  "isActive": true,
  "displayOrder": 2,
  "layoutStyle": "grid",
  "maxItems": 8,
  "badgeText": "FEATURED",
  "customClass": "special-section",
  "priority": 10
}
```

## How It Works

### Data Flow

```
Category Table (existing)
├── Type field → Determines display style
├── ParentId → Hierarchical structure
└── Priority → Sort order

Homepage Display Logic:
1. Query categories where HomepageTitle IS NOT NULL
2. Read HomepageConfig JSON for display settings
3. Get tours for each category
4. Render based on layoutStyle in config
```

### Category Types for Homepage Display

**Existing Category.Type values:**
- `"featured"` → Display as spotlight/large cards
- `"popular"` → Display as grid with ratings
- `"travel-style"` → Display as small grid (Ways to Travel)
- `"package"` → Display as product cards
- `"destination"` → Display as location cards
- `"daily-tour"` → Display as schedule-based cards

**New logic:**
- If category has `HomepageTitle`, it appears on homepage
- `HomepageConfig.layoutStyle` overrides default for that type
- `Category.Type` provides default display behavior

## API Design

### Featured Tours Management

#### 1. Get All Featured Tours

**GET** `/api/homepage/featured-tours`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "tourId": 10,
      "tourTitle": "Ninh Binh Day Tour",
      "displayOrder": 1,
      "title": "Ninh Binh",
      "category": "IN THE SPOTLIGHTS",
      "description": "Book now for an unforgettable one-day Ninh Binh trip...",
      "imageUrl": "/uploads/tours/ninh-binh.jpg",
      "cardClass": "ninh-binh",
      "active": true
    },
    {
      "id": 2,
      "tourId": 25,
      "tourTitle": "Ha Long Bay Cruise",
      "displayOrder": 2,
      "title": "Ha Long Bay",
      "category": "IN THE SPOTLIGHTS",
      "description": "Experience the wonder of a UNESCO World Heritage Site...",
      "imageUrl": "/uploads/tours/halong.jpg",
      "cardClass": "ha-long",
      "active": true
    }
  ]
}
```

**SQL Query:**
```sql
SELECT 
    ft.Id,
    ft.TourId,
    t.Title AS TourTitle,
    ft.DisplayOrder,
    ft.Title,
    ft.Category,
    ft.Description,
    ft.ImageUrl,
    ft.CardClass,
    ft.Active
FROM HomePageFeaturedTours ft
INNER JOIN Tour t ON ft.TourId = t.Id
WHERE ft.Active = 1
ORDER BY ft.DisplayOrder ASC
```

#### 2. Add Featured Tour

**POST** `/api/homepage/featured-tours`

**Request:**
```json
{
  "tourId": 10,
  "title": "Ninh Binh",
  "category": "IN THE SPOTLIGHTS",
  "description": "Book now for an unforgettable one-day Ninh Binh trip...",
  "imageUrl": "/uploads/tours/ninh-binh.jpg",
  "cardClass": "ninh-binh",
  "displayOrder": 1
}
```

**Business Logic:**
1. Check if there are already 2 active featured tours
2. If yes, return error: "Maximum 2 featured tours allowed"
3. If no, insert new record

**SQL Insert:**
```sql
-- Check current count
DECLARE @CurrentCount INT;
SELECT @CurrentCount = COUNT(*) 
FROM HomePageFeaturedTours 
WHERE Active = 1;

-- If count >= 2, raise error
IF @CurrentCount >= 2
    THROW 50001, 'Maximum 2 featured tours allowed', 1;

-- Insert new featured tour
INSERT INTO HomePageFeaturedTours (
    TourId, DisplayOrder, Title, Category, Description, ImageUrl, CardClass, 
    Active, CreatedTime, UpdatedBy
)
VALUES (
    @TourId, @DisplayOrder, @Title, @Category, @Description, 
    @ImageUrl, @CardClass, 1, GETDATE(), @UserId
);
```

#### 3. Update Featured Tour

**PUT** `/api/homepage/featured-tours/{id}`

**Request:** Same as POST

**SQL Update:**
```sql
UPDATE HomePageFeaturedTours
SET 
    TourId = @TourId,
    Title = @Title,
    Category = @Category,
    Description = @Description,
    ImageUrl = @ImageUrl,
    CardClass = @CardClass,
    DisplayOrder = @DisplayOrder,
    UpdatedTime = GETDATE(),
    UpdatedBy = @UserId
WHERE Id = @Id AND Active = 1
```

#### 4. Delete Featured Tour

**DELETE** `/api/homepage/featured-tours/{id}`

**SQL:**
```sql
UPDATE HomePageFeaturedTours
SET Active = 0, UpdatedTime = GETDATE(), UpdatedBy = @UserId
WHERE Id = @Id
```

#### 5. Reorder Featured Tours

**PUT** `/api/homepage/featured-tours/reorder`

**Request:**
```json
{
  "orders": [
    { "id": 1, "displayOrder": 1 },
    { "id": 2, "displayOrder": 2 }
  ]
}
```

**SQL:**
```sql
-- Use MERGE or individual UPDATE statements
MERGE INTO HomePageFeaturedTours AS target
USING @OrderTable AS source ON target.Id = source.Id
WHEN MATCHED THEN
    UPDATE SET 
        DisplayOrder = source.DisplayOrder,
        UpdatedTime = GETDATE(),
        UpdatedBy = @UserId;
```

### Category-Based Sections Management

#### 6. Configure Category for Homepage

**PUT** `/api/category/{id}/homepage`

**Request:**
```json
{
  "homepageTitle": "Ways to Travel",
  "layoutStyle": "grid",
  "maxItems": 5,
  "displayOrder": 1,
  "isActive": true,
  "badgeText": "POPULAR",
  "customClass": ""
}
```

**Business Logic:**
1. Validate layoutStyle (must be: grid, grid-2, grid-3, spotlight, carousel)
2. Validate maxItems (1-20 for grid layouts, maximum 3 for spotlight)
3. If layoutStyle is "spotlight" and maxItems > 3, set maxItems = 3
4. Update Category table

**SQL Update:**
```sql
DECLARE @HomepageConfig NVARCHAR(MAX) = '{
  "layoutStyle": "' + @LayoutStyle + '",
  "maxItems": ' + CAST(@MaxItems AS VARCHAR) + ',
  "isActive": ' + CAST(@IsActive AS VARCHAR) + ',
  "badgeText": "' + ISNULL(@BadgeText, '') + '",
  "customClass": "' + ISNULL(@CustomClass, '') + '"
}';

UPDATE Category
SET 
    HomepageTitle = @HomepageTitle,
    HomepageConfig = @HomepageConfig,
    HomepageOrder = @DisplayOrder,
    UpdatedTime = GETDATE(),
    UpdatedBy = @UserId
WHERE Id = @CategoryId
```

#### 7. Get Homepage Sections

**GET** `/api/homepage/sections`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "categoryId": 10,
      "categoryName": "Adventure",
      "homepageTitle": "Ways to Travel",
      "type": "travel-style",
      "tourCount": 12,
      "config": {
        "layoutStyle": "grid",
        "maxItems": 5,
        "displayOrder": 1,
        "isActive": true,
        "badgeText": "POPULAR"
      },
      "tours": [
        {
          "id": 1,
          "title": "Mountain Trekking",
          "pricePerPerson": 1500000,
          "duration": "2 days",
          "images": ["/uploads/tour1.jpg"]
        }
      ]
    }
  ]
}
```

**Business Logic:**
1. Query all categories with HomepageTitle configured
2. For each category, parse HomepageConfig JSON
3. If layoutStyle is "spotlight", enforce maxItems <= 3
4. Get tours for each category (limit by maxItems)
5. For spotlight sections, assign spotlightPosition based on displayOrder

**SQL Query:**
```sql
-- Get all configured categories
SELECT 
    c.Id AS CategoryId,
    c.CategoryName,
    c.HomepageTitle,
    c.Type,
    c.HomepageOrder AS DisplayOrder,
    c.HomepageConfig
FROM Category c
WHERE c.HomepageTitle IS NOT NULL 
    AND c.Active = 1
ORDER BY c.HomepageOrder ASC

-- Then for each category, get tours
SELECT TOP (@MaxItems)
    t.Id,
    t.Title,
    t.PricePerPerson,
    t.Duration,
    ti.ImageUrl
FROM Tour t
INNER JOIN TourCategory tc ON t.Id = tc.TourId
LEFT JOIN TourImage ti ON t.Id = ti.TourId AND ti.IsBanner = 1
WHERE tc.CategoryId = @CategoryId
    AND t.Active = 1
    AND tc.Active = 1
ORDER BY t.CreatedTime DESC
```

#### 8. Update Section Order

**PUT** `/api/homepage/sections/reorder`

**Request:**
```json
{
  "sections": [
    { "categoryId": 10, "displayOrder": 1 },
    { "categoryId": 15, "displayOrder": 2 },
    { "categoryId": 20, "displayOrder": 3 }
  ]
}
```

**SQL:**
```sql
-- Update HomepageOrder for each category
UPDATE Category
SET HomepageOrder = @DisplayOrder,
    UpdatedTime = GETDATE(),
    UpdatedBy = @UserId
WHERE Id = @CategoryId
```

#### 9. Remove Section from Homepage

**DELETE** `/api/category/{id}/homepage`

**SQL:**
```sql
UPDATE Category
SET 
    HomepageTitle = NULL,
    HomepageConfig = NULL,
    HomepageOrder = NULL,
    UpdatedTime = GETDATE(),
    UpdatedBy = @UserId
WHERE Id = @CategoryId
```

## Frontend Implementation

### Admin UI Flow

1. **List all categories** → Show which ones are configured for homepage
2. **Click "Configure"** → Set custom title and display options
3. **Save** → Update Category.HomepageTitle and HomepageConfig
4. **Preview** → Show how it will look
5. **Reorder** → Update Category.Priority

### Homepage Rendering

```javascript
// 1. Fetch configured sections
const sections = await fetch('/api/homepage/sections').then(r => r.json());

// 2. For each section, render based on layoutStyle
sections.forEach(section => {
  const layout = section.config.layoutStyle;
  
  switch(layout) {
    case 'spotlight':
      renderSpotlightSection(section);
      break;
    case 'grid':
      renderGridSection(section, 4);
      break;
    case 'grid-3':
      renderGridSection(section, 3);
      break;
    default:
      renderDefaultSection(section);
  }
});
```

## Migration Strategy

### Step 1: Add Columns
```sql
ALTER TABLE Category ADD HomepageTitle NVARCHAR(200) NULL;
ALTER TABLE Category ADD HomepageConfig NVARCHAR(MAX) NULL;
```

### Step 2: Configure Existing Categories
```sql
-- Example: Configure "Ways to Travel" section
UPDATE Category
SET 
    HomepageTitle = 'Ways to Travel',
    HomepageConfig = '{
        "layoutStyle": "grid",
        "maxItems": 5,
        "displayOrder": 2,
        "isActive": true,
        "badgeText": ""
    }'
WHERE CategoryName = 'Adventure' AND Type = 'travel-style';
```

### Step 3: Deploy Updated UI
- Admin can now configure homepage sections
- Frontend reads from Category table
- No changes to Tour or TourCategory tables needed

## Benefits of This Approach

1. **Minimal Changes**: Only 2 new columns added
2. **Backward Compatible**: Existing functionality unchanged
3. **Flexible**: JSON config allows future enhancements without schema changes
4. **Category-Centric**: Uses existing category structure
5. **Easy Migration**: Can roll out incrementally
6. **No Data Duplication**: Reuses existing Tour and TourCategory data

## Comparison with Previous Design

| Aspect | Previous (Full Tables) | New (2 Fields) |
|--------|----------------------|----------------|
| New Tables | 2 tables | 0 tables |
| New Columns | Separate tables | 2 columns |
| Data Storage | Duplicates tour info | References existing |
| Complexity | High | Low |
| Migration | Complex | Simple |
| Performance | Multiple JOINs | Single query |

## Example Use Cases

### Use Case 1: Configure "Ways to Travel"
```
Admin Action:
1. Select "Adventure" category
2. Set homepage title: "Ways to Travel"
3. Choose layout: Grid (3 columns)
4. Max items: 5
5. Save

Database:
- Category.HomepageTitle = "Ways to Travel"
- Category.HomepageConfig = {"layoutStyle": "grid-3", "maxItems": 5, ...}

Frontend:
- Queries tours from Adventure category
- Displays in 3-column grid
- Shows max 5 items
```

### Use Case 2: Add New Section
```
Admin Action:
1. Select "Ha Long Bay" category
2. Set homepage title: "Tours We Love"
3. Choose layout: Spotlight
4. Set badge: "TOP RATED"
5. Save

Database:
- Category.HomepageTitle = "Tours We Love"
- Category.HomepageConfig = {"layoutStyle": "spotlight", "badgeText": "TOP RATED", ...}

Result:
- New section appears on homepage
- Shows Ha Long Bay tours in spotlight layout
```

## Future Enhancements (No Schema Changes)

The JSON config allows adding features without database changes:

```json
{
  "layoutStyle": "grid",
  "maxItems": 8,
  "badgeText": "FEATURED",
  "customClass": "dark-theme",
  // Future additions:
  "showPrice": true,
  "showRating": true,
  "filterByPrice": {"min": 1000000, "max": 5000000},
  "showBadge": "highlight",
  "animation": "fade-in"
}
```

## Admin UI - Business Flow & Features

### Overview
The admin UI provides a comprehensive interface for managing homepage sections with full CRUD operations, preview capabilities, and reordering functionality.

### Main Features

#### 1. **View Configured Sections**
- Display all configured homepage sections as cards
- Show section details: title, category, layout, status, tour count
- Visual indicators for active/inactive status
- Order display based on Priority field

#### 2. **Add New Section**
**Modal:** `#addSectionModal`

**Process:**
1. Click "Configure New Section" button
2. Select a category from dropdown (loaded from Category table)
3. Fill in configuration:
   - **Section Title** (required): Custom title shown on homepage
   - **Select Category** (required): Which category to display
   - **Layout Style**: Grid (2/3/4 cols), Spotlight, or Carousel
   - **Max Items**: Number of tours to display (1-20)
   - **Display Order**: Order on homepage (0 = first)
   - **Status**: Active/Inactive
   - **Badge Text** (optional): Badge to show on section
   - **Custom CSS Class** (optional): Custom styling
4. Click "Save Section"
5. System updates Category.HomepageTitle, HomepageConfig, and Priority

**Data Saved:**
- `Category.HomepageTitle` = Section title
- `Category.HomepageConfig` = JSON config (all other settings)
- `Category.Priority` = Display order

#### 3. **Edit Existing Section**
**Process:**
1. Click Edit button (pencil icon) on section card
2. Modal opens pre-filled with existing data
3. Modify any fields
4. Click "Save Section"
5. System updates the category configuration

**Technical Implementation:**
```javascript
// When edit is clicked
- Find section by ID in allSections array
- Populate modal with section data
- On save, update the section object
- Re-render sections list
```

#### 4. **Preview Section**
**Modal:** `#previewSectionModal`

**Process:**
1. Click Preview button (eye icon) on section card
2. System generates mock tours based on configuration
3. Displays preview with:
   - Section title and badge
   - Tours in selected layout style (grid/spotlight/carousel)
   - Realistic mock data (titles, prices, images)
4. User can see exactly how it will look on homepage

**Preview Layouts:**
- **Spotlight**: 3-column large cards with images (200px height)
- **Grid-3**: 3-column standard cards (150px height)
- **Grid** (default): 4-column compact cards (150px height)

#### 5. **Move Sections Up/Down**
**Process:**
1. Click arrow up/down buttons on section card
2. System swaps display order with adjacent section
3. Updates Category.Priority for both sections
4. UI re-renders in new order

**Validation:**
- Cannot move up if already at order 1
- Cannot move down if already at last position

#### 6. **Delete Section**
**Modal:** `#deleteSectionModal`

**Process:**
1. Click Delete button (trash icon) on section card
2. Confirmation modal shows:
   - Warning icon and message
   - Section details (title, category, order)
   - Note that tours/categories won't be affected
3. Click "Delete Section" to confirm
4. System removes HomepageTitle and HomepageConfig from Category
5. Section disappears from homepage

**Important:** This only removes homepage configuration. Category and tours remain unchanged.

#### 7. **Auto-Generated Mock Data**

**Categories:**
```javascript
const mockCategories = [
  { id: 10, categoryName: 'Adventure', type: 'travel-style' },
  { id: 15, categoryName: 'Full Packages', type: 'package' },
  { id: 20, categoryName: 'Indochina', type: 'package' },
  { id: 25, categoryName: 'Ha Long Bay', type: 'destination' },
  { id: 30, categoryName: 'Sapa Tours', type: 'destination' },
  { id: 35, categoryName: 'Hanoi Day Tours', type: 'daily-tour' },
  { id: 40, categoryName: 'Beach & Relax', type: 'travel-style' },
  { id: 45, categoryName: 'Luxury Tours', type: 'package' },
  { id: 50, categoryName: 'Nature & Wildlife', type: 'travel-style' }
];
```

**Homepage Sections:**
```javascript
const mockSections = [
  {
    id: 1,
    categoryId: 10,
    categoryName: 'Adventure',
    sectionTitle: 'Ways to Travel',
    layoutStyle: 'grid',
    maxItems: 5,
    displayOrder: 1,
    isActive: true,
    badgeText: '',
    tourCount: 12
  },
  // ... more sections
];
```

**Mock Tours (for preview):**
- Random selection from 12 tour titles
- Random prices: 1,000,000 - 10,000,000 VND
- Random duration: 1-7 days
- Placeholder images via placeholder.com

### User Workflow Examples

#### Example 1: Configure "Ways to Travel" Section

**Admin Actions:**
1. Click "Configure New Section"
2. Select "Adventure" category
3. Enter title: "Ways to Travel"
4. Choose layout: "Grid (3 columns)"
5. Set max items: 5
6. Set display order: 1
7. Set status: Active
8. Leave badge empty
9. Click "Save Section"

**What Happens:**
- Category table updated:
  - `HomepageTitle = "Ways to Travel"`
  - `HomepageConfig = {"layoutStyle": "grid-3", "maxItems": 5, "displayOrder": 1, "isActive": true, ...}`
  - `Priority = 1`
- Tours from "Adventure" category shown in 3-column grid
- Maximum 5 tours displayed

#### Example 2: Change Section Order

**Admin Actions:**
1. Section "Ways to Travel" is at order 1
2. Section "Top Vietnam Packages" is at order 2
3. Click "Move Down" on "Ways to Travel"
4. Confirm action

**What Happens:**
- "Ways to Travel" Priority: 1 → 2
- "Top Vietnam Packages" Priority: 2 → 1
- Sections swap positions on homepage

#### Example 3: Preview Before Publishing

**Admin Actions:**
1. Configure a new section for "Beach & Relax"
2. Choose "Spotlight" layout
3. Click "Preview" button
4. See how it will look with 3 large spotlight cards
5. Adjust if needed, save

**Benefits:**
- See exact appearance before publishing
- Validate layout choices
- Ensure content fits well

### Technical Implementation Details

#### State Management
```javascript
let allSections = []; // Global array holding all sections

// Add new section
allSections.push({...sectionData});

// Update section
const index = allSections.findIndex(s => s.id === id);
allSections[index] = {...updatedData};

// Delete section
allSections = allSections.filter(s => s.id !== id);
```

#### Data Structure
```javascript
{
  id: number,              // Section ID
  categoryId: number,      // Reference to Category
  categoryName: string,    // Category name for display
  sectionTitle: string,    // Homepage title
  layoutStyle: string,     // 'grid' | 'grid-3' | 'grid-2' | 'spotlight' | 'carousel'
  maxItems: number,        // Tours to display (1-20)
  displayOrder: number,    // Order on homepage
  isActive: boolean,       // Active/Inactive status
  badgeText: string,       // Optional badge
  customClass: string,     // Optional CSS class
  tourCount: number        // Available tours in category
}
```

#### API Integration (Future)
All mock data will be replaced with actual API calls:
- `GET /api/category/all` → Load categories
- `PUT /api/category/{id}/homepage` → Save section config
- `GET /api/homepage/sections` → Load configured sections
- `PUT /api/homepage/reorder` → Update order
- `DELETE /api/category/{id}/homepage` → Remove section

### UI/UX Considerations

1. **Responsive Design:** Cards use Bootstrap grid (col-md-6 col-lg-4)
2. **Visual Feedback:** Toast notifications for all actions
3. **Confirmation:** Delete requires explicit confirmation
4. **Validation:** Form validation prevents invalid submissions
5. **Loading States:** Can be added when API is integrated
6. **Error Handling:** User-friendly error messages

## Service Layer Implementation

### Featured Tours Service

**File:** `Services/FeaturedToursService.cs`

```csharp
public class FeaturedToursService
{
    private readonly IFeaturedToursRepository _repository;
    private readonly ILogger<FeaturedToursService> _logger;
    
    public FeaturedToursService(
        IFeaturedToursRepository repository,
        ILogger<FeaturedToursService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    // Get all active featured tours
    public async Task<ApiResponse<List<FeaturedTourDto>>> GetAllAsync()
    {
        try
        {
            var tours = await _repository.GetAllActiveAsync();
            return ApiResponse<List<FeaturedTourDto>>.Success(tours);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured tours");
            return ApiResponse<List<FeaturedTourDto>>.Error("Failed to get featured tours");
        }
    }
    
    // Add featured tour with constraint validation
    public async Task<ApiResponse<FeaturedTourDto>> AddAsync(CreateFeaturedTourDto dto, string userId)
    {
        try
        {
            // Check current count
            var currentCount = await _repository.GetActiveCountAsync();
            
            if (currentCount >= 2)
            {
                return ApiResponse<FeaturedTourDto>.Error(
                    "Maximum 2 featured tours allowed. Please remove one before adding another.");
            }
            
            var entity = new HomePageFeaturedTours
            {
                TourId = dto.TourId,
                Title = dto.Title,
                Category = dto.Category,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                CardClass = dto.CardClass,
                DisplayOrder = dto.DisplayOrder,
                Active = true,
                CreatedTime = DateTime.UtcNow,
                UpdatedBy = userId
            };
            
            var result = await _repository.AddAsync(entity);
            return ApiResponse<FeaturedTourDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding featured tour");
            return ApiResponse<FeaturedTourDto>.Error("Failed to add featured tour");
        }
    }
    
    // Update featured tour
    public async Task<ApiResponse<FeaturedTourDto>> UpdateAsync(int id, UpdateFeaturedTourDto dto, string userId)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return ApiResponse<FeaturedTourDto>.Error("Featured tour not found");
            
            entity.TourId = dto.TourId;
            entity.Title = dto.Title;
            entity.Category = dto.Category;
            entity.Description = dto.Description;
            entity.ImageUrl = dto.ImageUrl;
            entity.CardClass = dto.CardClass;
            entity.DisplayOrder = dto.DisplayOrder;
            entity.UpdatedTime = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            
            var result = await _repository.UpdateAsync(entity);
            return ApiResponse<FeaturedTourDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating featured tour");
            return ApiResponse<FeaturedTourDto>.Error("Failed to update featured tour");
        }
    }
    
    // Delete featured tour (soft delete)
    public async Task<ApiResponse<bool>> DeleteAsync(int id, string userId)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return ApiResponse<bool>.Error("Featured tour not found");
            
            entity.Active = false;
            entity.UpdatedTime = DateTime.UtcNow;
            entity.UpdatedBy = userId;
            
            await _repository.UpdateAsync(entity);
            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting featured tour");
            return ApiResponse<bool>.Error("Failed to delete featured tour");
        }
    }
    
    // Reorder featured tours
    public async Task<ApiResponse<bool>> ReorderAsync(List<ReorderDto> orders, string userId)
    {
        try
        {
            foreach (var order in orders)
            {
                var entity = await _repository.GetByIdAsync(order.Id);
                if (entity != null)
                {
                    entity.DisplayOrder = order.DisplayOrder;
                    entity.UpdatedTime = DateTime.UtcNow;
                    entity.UpdatedBy = userId;
                    await _repository.UpdateAsync(entity);
                }
            }
            
            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering featured tours");
            return ApiResponse<bool>.Error("Failed to reorder featured tours");
        }
    }
}
```

### Homepage Sections Service

**File:** `Services/HomepageSectionsService.cs`

```csharp
public class HomepageSectionsService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITourRepository _tourRepository;
    private readonly ILogger<HomepageSectionsService> _logger;
    
    public HomepageSectionsService(
        ICategoryRepository categoryRepository,
        ITourRepository tourRepository,
        ILogger<HomepageSectionsService> logger)
    {
        _categoryRepository = categoryRepository;
        _tourRepository = tourRepository;
        _logger = logger;
    }
    
    // Get all homepage sections with tours
    public async Task<ApiResponse<HomepageDataDto>> GetHomepageDataAsync()
    {
        try
        {
            // Get configured categories
            var categories = await _categoryRepository.GetAllWithHomepageConfigAsync();
            
            var sections = new List<HomepageSectionDto>();
            
            foreach (var category in categories)
            {
                // Parse HomepageConfig JSON
                var config = JsonSerializer.Deserialize<HomepageConfigDto>(
                    category.HomepageConfig, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (!config.IsActive) continue;
                
                // Validate spotlight constraint
                if (config.LayoutStyle == "spotlight" && config.MaxItems > 3)
                {
                    config.MaxItems = 3;
                    _logger.LogWarning(
                        $"Category {category.Id} has spotlight layout but maxItems > 3. Limited to 3.");
                }
                
                // Get tours for this category
                var tours = await _tourRepository.GetToursByCategoryIdAsync(
                    category.Id, 
                    config.MaxItems);
                
                sections.Add(new HomepageSectionDto
                {
                    CategoryId = category.Id,
                    CategoryName = category.CategoryName,
                    HomepageTitle = category.HomepageTitle,
                    Type = category.Type,
                    Config = config,
                    Tours = tours.Select(t => new TourDto
                    {
                        Id = t.Id,
                        Title = t.Title,
                        PricePerPerson = t.PricePerPerson,
                        Duration = t.Duration,
                        Images = t.Images?.Select(i => i.ImageUrl).ToList() ?? new List<string>()
                    }).ToList()
                });
            }
            
            return ApiResponse<HomepageDataDto>.Success(
                new HomepageDataDto { Sections = sections });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting homepage data");
            return ApiResponse<HomepageDataDto>.Error("Failed to get homepage data");
        }
    }
    
    // Configure category for homepage
    public async Task<ApiResponse<bool>> ConfigureCategoryAsync(
        int categoryId, 
        ConfigureHomepageDto dto, 
        string userId)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                return ApiResponse<bool>.Error("Category not found");
            
            // Validate layout style
            var validLayouts = new[] { "grid", "grid-2", "grid-3", "spotlight", "carousel" };
            if (!validLayouts.Contains(dto.LayoutStyle))
                return ApiResponse<bool>.Error("Invalid layout style");
            
            // Enforce spotlight constraint
            if (dto.LayoutStyle == "spotlight" && dto.MaxItems > 3)
            {
                dto.MaxItems = 3;
                _logger.LogWarning(
                    $"Spotlight layout limited to 3 items for category {categoryId}");
            }
            
            // Build HomepageConfig JSON
            var config = new HomepageConfigDto
            {
                LayoutStyle = dto.LayoutStyle,
                MaxItems = dto.MaxItems,
                IsActive = dto.IsActive,
                BadgeText = dto.BadgeText,
                CustomClass = dto.CustomClass
            };
            
            var configJson = JsonSerializer.Serialize(config);
            
            // Update category
            category.HomepageTitle = dto.HomepageTitle;
            category.HomepageConfig = configJson;
            category.HomepageOrder = dto.DisplayOrder;
            category.UpdatedTime = DateTime.UtcNow;
            category.UpdatedBy = userId;
            
            await _categoryRepository.UpdateAsync(category);
            
            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring category for homepage");
            return ApiResponse<bool>.Error("Failed to configure category");
        }
    }
    
    // Remove category from homepage
    public async Task<ApiResponse<bool>> RemoveFromHomepageAsync(int categoryId, string userId)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                return ApiResponse<bool>.Error("Category not found");
            
            category.HomepageTitle = null;
            category.HomepageConfig = null;
            category.HomepageOrder = null;
            category.UpdatedTime = DateTime.UtcNow;
            category.UpdatedBy = userId;
            
            await _categoryRepository.UpdateAsync(category);
            
            return ApiResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing category from homepage");
            return ApiResponse<bool>.Error("Failed to remove category from homepage");
        }
    }
}
```

## User-Facing Homepage Implementation

### Route
- **URL:** `/home` or `/`
- **Controller:** `HomeController.Index()`
- **View:** `Views/Home/Index.cshtml`

### UI Structure
The homepage is built using the user template design from `wwwroot/ui-user-template/index.html` with the following sections:

1. **Header & Navigation**
   - Logo and navigation menu
   - Search box, language selector, currency selector
   - Mobile menu toggle

2. **Hero Section**
   - Hero banner with background
   - Hero badges (250+ trips, 24/7 support, Local Experts, Competitive pricing)
   - Hero text (Book your favorite boat trip)
   - Search form

3. **About Compass Section**
   - Section header with "About Compass Travel"
   - Two-column layout (text left, visual right)
   - Highlight points (Tailor-made itineraries, Trusted local partners, 24/7 customer support)

4. **Contact Us Section**
   - Four contact cards (Head Office, Phone, Email, Working Hours)
   - Responsive grid layout

5. **Featured Tours Section**
   - Dynamic tour cards container (`#tourCards`)
   - Indicators grid (Customer reviews, Whatsapp support, Free tailor-made trips)

6. **Homepage Sections Container** (`#homepageSectionsContainer`)
   - **Main Content Area** - Dynamically populated based on admin configuration
   - Sections are rendered based on:
     - Category.HomepageTitle (section title)
     - Category.HomepageConfig (layout, max items, badge, etc.)
     - Associated tours from the category

7. **Footer**
   - Multiple columns (Follow Us, Destinations, Tour Types, Services, Support)
   - Footer bottom with logo, copyright, payment methods, legal links
   - Back to top button

### JavaScript Implementation

#### Dynamic Section Rendering
```javascript
function loadHomepageSections() {
    // Fetch sections from API
    $.ajax({
        url: '/api/homepage/sections',
        type: 'GET',
        success: function(response) {
            if (response.success && response.data) {
                renderHomepageSections(response.data);
            }
        }
    });
}

function renderHomepageSections(sections) {
    const container = $('#homepageSectionsContainer');
    container.empty();
    
    sections.forEach(function(section) {
        if (!section.config.isActive) return;
        const sectionHtml = renderSection(section);
        container.append(sectionHtml);
    });
}
```

#### Layout Rendering Functions

**1. Spotlight Layout (3-column large cards)**
- Used for premium/featured sections
- Large images (200px height)
- Full product details

**2. Grid Layout (2/3/4 columns)**
- Standard product cards
- Responsive columns based on layout style
- Shows category, title, price, read more button

#### Mock Data Structure
Currently using mock data structure matching the API response:
```javascript
{
    categoryId: number,
    categoryName: string,
    homepageTitle: string,
    type: string,
    tourCount: number,
    config: {
        layoutStyle: string,
        maxItems: number,
        displayOrder: number,
        isActive: boolean,
        badgeText: string,
        customClass: string
    },
    tours: [
        {
            id: number,
            title: string,
            pricePerPerson: number,
            images: string[]
        }
    ]
}
```

### CSS Integration
The homepage uses existing CSS files from `ui-user-template`:
- `style.css` - Main styles
- `responsive.css` - Responsive design
- `sections-theme.css` - Section-specific theming
- Bootstrap grid system for responsive layouts

### Current Implementation Status

**✅ Completed:**
- Homepage route and view created
- UI structure from `index.html` integrated
- JavaScript rendering logic implemented
- Mock data structure matching API design
- Dynamic section container (`#homepageSectionsContainer`)
- Layout rendering functions (spotlight, grid 2/3/4 columns)
- Responsive design with Bootstrap grid

**⏳ Pending:**
- Replace mock data with actual API calls
- Connect to `/api/homepage/sections` endpoint
- Implement tour detail page links
- Add loading states
- Add error handling
- Performance optimization (lazy loading, caching)

### API Integration Points

When API is ready, replace mock data with:

```javascript
// In loadHomepageSections()
$.ajax({
    url: '/api/homepage/sections',
    type: 'GET',
    success: function(response) {
        if (response.success && response.data) {
            renderHomepageSections(response.data);
        }
    },
    error: function(xhr) {
        console.error('Error loading homepage:', xhr);
        // Show error message to user
    }
});
```

## Implementation Checklist

- [x] Admin UI with mock data
- [x] Add/Edit section modal
- [x] Delete confirmation modal
- [x] Preview modal with mock tours
- [x] Move up/down functionality
- [x] User-facing homepage (Views/Home/Index.cshtml)
- [x] Dynamic section rendering with mock data
- [x] Layout rendering functions (spotlight, grid)
- [ ] Add HomepageTitle and HomepageConfig columns to Category table
- [ ] Create migration script
- [ ] Update CategoryRepository to handle new fields
- [ ] Create HomepageService for section management
- [ ] Build API endpoints (4 endpoints)
- [ ] Replace mock data with API calls in homepage
- [ ] Test with existing data
- [ ] Deploy incrementally
