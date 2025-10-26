# Homepage Content Management API Documentation

## Overview
This document describes the API design for managing homepage content. The homepage displays various tour sections that need to be dynamically managed through the admin panel.

## Database Design

### New Tables

#### 1. HomePageSection
Manages different sections of the homepage.
```sql
CREATE TABLE HomePageSection (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SectionKey VARCHAR(100) NOT NULL UNIQUE, -- e.g., 'featured_tours', 'ways_to_travel', 'top_packages'
    SectionName NVARCHAR(200) NOT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedTime DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedTime DATETIME NULL,
    UpdatedBy NVARCHAR(100) NULL
)
```

#### 2. HomePageContent
Stores content items for each section.
```sql
CREATE TABLE HomePageContent (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SectionId INT NOT NULL,
    TourId INT NULL, -- FK to Tour table (optional - some content may not be linked to tours)
    DisplayOrder INT NOT NULL DEFAULT 0,
    ContentType VARCHAR(50) NOT NULL, -- 'tour', 'category', 'custom'
    Title NVARCHAR(500),
    Description NVARCHAR(MAX),
    ImageUrl NVARCHAR(500),
    LinkUrl NVARCHAR(500),
    BadgeText NVARCHAR(100), -- e.g., 'IN THE SPOTLIGHTS'
    Metadata NVARCHAR(MAX), -- JSON for additional data
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedTime DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedTime DATETIME NULL,
    UpdatedBy NVARCHAR(100) NULL,
    FOREIGN KEY (SectionId) REFERENCES HomePageSection(Id) ON DELETE CASCADE,
    FOREIGN KEY (TourId) REFERENCES Tour(Id) ON DELETE SET NULL
)
```

### Usage Example

#### Inserting Featured Tours
```sql
-- Section already exists for featured_tours
INSERT INTO HomePageContent (SectionId, TourId, DisplayOrder, ContentType, Title, Description, BadgeText, IsActive)
SELECT 
    s.Id,
    t.Id,
    t.Id, -- Using tour ID as display order
    'tour',
    t.Title,
    LEFT(t.Description, 200) + '...',
    'IN THE SPOTLIGHTS',
    1
FROM HomePageSection s
CROSS JOIN Tour t
WHERE s.SectionKey = 'featured_tours' AND t.Active = 1
```

## API Endpoints

### 1. Get Homepage Content by Section

**GET** `/api/homepage-content/section/{sectionKey}`

**Parameters:**
- `sectionKey` (string, required): The key of the section (e.g., 'featured_tours', 'ways_to_travel')

**Response:**
```json
{
  "success": true,
  "data": {
    "sectionId": 1,
    "sectionKey": "featured_tours",
    "sectionName": "Featured Tours",
    "items": [
      {
        "id": 1,
        "tourId": 5,
        "displayOrder": 1,
        "title": "Ninh Binh Adventure",
        "description": "Book now for an unforgettable one-day Ninh Binh trip",
        "imageUrl": "/images/ninhbinh.jpg",
        "badgeText": "IN THE SPOTLIGHTS",
        "tour": {
          "id": 5,
          "title": "Ninh Binh Adventure",
          "pricePerPerson": 1500000,
          "duration": "1 day"
        }
      }
    ]
  }
}
```

**Database Query:**
```sql
SELECT 
    hpc.Id,
    hpc.TourId,
    hpc.DisplayOrder,
    hpc.Title,
    hpc.Description,
    hpc.ImageUrl,
    hpc.BadgeText,
    hpc.Metadata,
    t.Id as Tour_Id,
    t.Title as Tour_Title,
    t.PricePerPerson as Tour_PricePerPerson,
    t.Duration as Tour_Duration
FROM HomePageContent hpc
LEFT JOIN Tour t ON hpc.TourId = t.Id
INNER JOIN HomePageSection hps ON hpc.SectionId = hps.Id
WHERE hps.SectionKey = @sectionKey 
    AND hpc.IsActive = 1 
    AND hps.IsActive = 1
ORDER BY hpc.DisplayOrder ASC
```

### 2. Get All Sections

**GET** `/api/homepage-content/sections`

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "sectionKey": "featured_tours",
      "sectionName": "Featured Tours",
      "displayOrder": 1,
      "isActive": true
    },
    {
      "id": 2,
      "sectionKey": "ways_to_travel",
      "sectionName": "Ways to Travel",
      "displayOrder": 2,
      "isActive": true
    }
  ]
}
```

### 3. Add Content to Section

**POST** `/api/homepage-content`

**Request Body:**
```json
{
  "sectionKey": "featured_tours",
  "tourId": 5,
  "displayOrder": 1,
  "contentType": "tour",
  "title": "Ninh Binh Adventure",
  "description": "Book now for an unforgettable one-day Ninh Binh trip",
  "badgeText": "IN THE SPOTLIGHTS",
  "isActive": true
}
```

**Database Operations:**
```sql
DECLARE @SectionId INT = (SELECT Id FROM HomePageSection WHERE SectionKey = @sectionKey);
DECLARE @MaxOrder INT = ISNULL((SELECT MAX(DisplayOrder) FROM HomePageContent WHERE SectionId = @SectionId), 0);

INSERT INTO HomePageContent (
    SectionId, TourId, DisplayOrder, ContentType, 
    Title, Description, BadgeText, IsActive
)
VALUES (
    @SectionId, 
    @TourId, 
    ISNULL(@DisplayOrder, @MaxOrder + 1),
    @ContentType,
    @Title,
    @Description,
    @BadgeText,
    @IsActive
);

SELECT @@IDENTITY as Id; -- Return the newly created ID
```

### 4. Update Content

**PUT** `/api/homepage-content/{id}`

**Request Body:**
```json
{
  "displayOrder": 2,
  "title": "Updated Tour Title",
  "description": "Updated description",
  "badgeText": "FEATURED",
  "isActive": true
}
```

**Database Query:**
```sql
UPDATE HomePageContent
SET 
    DisplayOrder = @DisplayOrder,
    Title = @Title,
    Description = @Description,
    BadgeText = @BadgeText,
    IsActive = @IsActive,
    UpdatedTime = GETDATE(),
    UpdatedBy = @UpdatedBy
WHERE Id = @Id
```

### 5. Delete Content

**DELETE** `/api/homepage-content/{id}`

**Database Query:**
```sql
-- Soft delete
UPDATE HomePageContent 
SET IsActive = 0, UpdatedTime = GETDATE(), UpdatedBy = @UpdatedBy
WHERE Id = @Id

-- Or hard delete
DELETE FROM HomePageContent WHERE Id = @Id
```

### 6. Reorder Content Items

**PUT** `/api/homepage-content/reorder`

**Request Body:**
```json
{
  "sectionKey": "featured_tours",
  "itemIds": [3, 1, 2]  // New order of IDs
}
```

**Database Operations:**
```sql
DECLARE @SectionId INT = (SELECT Id FROM HomePageSection WHERE SectionKey = @sectionKey);

-- Update all items in the section
UPDATE HomePageContent
SET DisplayOrder = t.NewOrder
FROM HomePageContent hpc
INNER JOIN (
    SELECT Id, ROW_NUMBER() OVER (ORDER BY value) as NewOrder
    FROM STRING_SPLIT(@itemIds, ',')
) t ON hpc.Id = t.Id
WHERE hpc.SectionId = @SectionId;
```

## Implementation Notes

### Service Layer Operations

```csharp
// HomePageContentService.cs

public class HomePageContentService
{
    private readonly IRepository<HomePageContent> _contentRepository;
    private readonly IRepository<HomePageSection> _sectionRepository;
    private readonly ITourRepository _tourRepository;

    // 1. Get content by section
    public async Task<List<HomePageContentDto>> GetContentBySection(string sectionKey)
    {
        var section = await _sectionRepository.GetByKey(sectionKey);
        if (section == null || !section.IsActive)
            return new List<HomePageContentDto>();

        var contents = await _contentRepository.GetManyAsync(
            c => c.SectionId == section.Id && c.IsActive,
            orderBy: c => c.DisplayOrder
        );

        return contents.Select(c => MapToDto(c)).ToList();
    }

    // 2. Add content to section
    public async Task<int> AddContent(CreateHomePageContentDto dto)
    {
        var section = await _sectionRepository.GetByKey(dto.SectionKey);
        if (section == null)
            throw new ArgumentException("Invalid section key");

        // Get max display order
        var maxOrder = await _contentRepository.GetMaxAsync(c => c.DisplayOrder, 
            c => c.SectionId == section.Id);

        var content = new HomePageContent
        {
            SectionId = section.Id,
            TourId = dto.TourId,
            DisplayOrder = dto.DisplayOrder ?? maxOrder + 1,
            ContentType = dto.ContentType,
            Title = dto.Title,
            Description = dto.Description,
            BadgeText = dto.BadgeText,
            IsActive = true,
            CreatedTime = DateTime.Now
        };

        return await _contentRepository.CreateAsync(content);
    }

    // 3. Update display order
    public async Task ReorderContent(string sectionKey, List<int> itemIds)
    {
        var section = await _sectionRepository.GetByKey(sectionKey);
        
        // Update each item's display order
        for (int i = 0; i < itemIds.Count; i++)
        {
            await _contentRepository.UpdateAsync(
                c => c.Id == itemIds[i] && c.SectionId == section.Id,
                c => {
                    c.DisplayOrder = i + 1;
                    c.UpdatedTime = DateTime.Now;
                }
            );
        }
    }
}
```

### Data Flow

1. **Admin adds tour to homepage:**
   - Admin selects tour from tour list
   - Chooses which section to add it to
   - System creates HomePageContent record linked to Tour
   - Display order is auto-assigned

2. **Frontend displays content:**
   - User visits homepage
   - Frontend calls GET `/api/homepage-content/section/{sectionKey}`
   - Backend queries HomePageContent with Tour join
   - Returns content with tour details
   - Frontend renders the section

3. **Admin reorders items:**
   - Admin drags items to reorder
   - Frontend sends PUT `/api/homepage-content/reorder` with new order
   - Backend updates DisplayOrder for all items
   - Next page load shows new order

## Benefits of This Design

1. **Flexibility:** Can add any content (tour-based or custom) to any section
2. **Performance:** Single query with JOINs instead of multiple queries
3. **Maintainability:** Changes to tour data automatically reflect on homepage
4. **Scalability:** Easy to add new sections or content types
5. **Reusability:** Same tour can appear in multiple sections with different presentations

## Future Enhancements

1. **Category-based content:** Add support for category-filtered tours
2. **Scheduled content:** Display different content based on date/time
3. **A/B testing:** Support multiple variants of homepage content
4. **Analytics:** Track which content performs best
5. **Multi-language:** Support for different language versions
