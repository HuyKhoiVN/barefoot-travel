# Featured Tours & Daily Tours Management - Implementation Summary

## Overview
Implemented a complete admin management system for Featured Tours (max 2) and Daily Tours (max 3) with RESTful APIs, SOLID architecture, and responsive admin UI.

## Database Changes

### 1. Category Model Extensions
Added Daily Tour fields to `Category` model:
- `DailyTourBadge` (NVARCHAR(100)) - Badge text for daily-card-category
- `DailyTourDescription` (NVARCHAR(500)) - Description for p tag
- `DailyTourImageUrl` (NVARCHAR(500)) - Image URL
- `DailyTourOrder` (INT) - Display order
- `ShowInDailyTours` (BIT) - Active flag
- `DailyTourCardClass` (NVARCHAR(100)) - CSS class for styling

### 2. HomePageFeaturedTour Model
Uses existing table with fields:
- `TourId` - Links to Tour table
- `Title` - Maps to CategoryName (h3 title)
- `Category` - Maps to Badge (card-category)
- `Description` - p tag description
- `ImageUrl` - Background image
- `CardClass` - CSS class for styling

### 3. Database Indexes
Added indexes in SysDbContext:
- `IX_Category_DailyTours` on `ShowInDailyTours` and `DailyTourOrder`
- `IX_FeaturedTour_DisplayOrder` on `DisplayOrder` and `Active` (existing)

## Architecture (SOLID Principles)

### Repository Layer
1. **IHomePageFeaturedTourRepository** - Interface for Featured Tours data access
2. **HomePageFeaturedTourRepository** - Implementation with CRUD operations
3. **ICategoryRepository** - Extended with Daily Tours methods
   - `GetDailyTourCategoriesAsync()` - Get all active daily tours
   - `GetDailyTourCategoryByIdAsync(id)` - Get single daily tour
   - `GetMaxDailyTourOrderAsync()` - Get max display order

### Service Layer
**IFeaturedDailyToursService** & **FeaturedDailyToursService**
- Business logic for Featured Tours and Daily Tours
- DTO conversions (Entity ↔ DTO)
- Validation (max limits, required fields)
- Dependency injection with repositories

### Controller Layer
**FeaturedDailyToursController** (`/api/FeaturedDailyTours`)
- RESTful API endpoints
- HTTP concerns only (status codes, responses)
- Delegates to service layer
- Authorization with `[Authorize]` and `[AllowAnonymous]` for public endpoints

## API Endpoints

### Featured Tours
- `GET /api/FeaturedDailyTours/featured-tours` - Get all featured tours (AllowAnonymous)
- `PUT /api/FeaturedDailyTours/featured-tours/{tourId}` - Configure featured tour
- `DELETE /api/FeaturedDailyTours/featured-tours/{id}` - Remove featured tour
- `PUT /api/FeaturedDailyTours/featured-tours/reorder` - Reorder featured tours

### Daily Tours
- `GET /api/FeaturedDailyTours/daily-tours` - Get all daily tours (AllowAnonymous)
- `PUT /api/FeaturedDailyTours/daily-tours/{categoryId}` - Configure daily tour
- `DELETE /api/FeaturedDailyTours/daily-tours/{categoryId}` - Remove daily tour
- `PUT /api/FeaturedDailyTours/daily-tours/reorder` - Reorder daily tours

## DTOs

### Request DTOs
- `ConfigureFeaturedTourDto` - For featured tour configuration
- `ConfigureDailyTourDto` - For daily tour configuration
- `ReorderTourDto` - For reordering tours

### Response DTOs
- `FeaturedTourDto` - Response with id, badge, categoryName, description, imageUrl, etc.
- `DailyTourDto` - Response with id, categoryId, badge, categoryName, description, imageUrl, etc.
- `FeaturedToursConfigDto` - Wrapper with list of tours
- `DailyToursConfigDto` - Wrapper with list of tours

## Admin UI

### Page: `/featured-daily-management` (Route: `featured-daily-management`)

**Features:**
1. **Featured Tours Section**
   - List all configured featured tours (max 2)
   - Add/Edit/Delete operations
   - Image gallery integration
   - Reorder functionality (Up/Down buttons)
   - Configure: badge, title (h3), description (p), image URL, display order, card CSS class

2. **Daily Tours Section**
   - List all configured daily tours (max 3)
   - Add/Edit/Delete operations
   - Image gallery integration
   - Reorder functionality (Up/Down buttons)
   - Configure: badge, title (h3), description (p), image URL, display order, card CSS class

3. **Image Management**
   - Select from uploaded images
   - Upload new images (validation: type, 10MB max)
   - Image preview
   - Responsive gallery modal

## Homepage Integration

### User-Facing Homepage (`Views/Home/Index.cshtml`)
- Fetches Featured Tours from `/api/FeaturedDailyTours/featured-tours`
- Fetches Daily Tours from `/api/FeaturedDailyTours/daily-tours`
- Renders with proper structure:
  - **Featured Tours**: `tour-card` with `card-category`, `h3`, `p`, background image
  - **Daily Tours**: `daily-tour-card` with `daily-card-category`, `h3`, `p`, background image

## File Structure

### New Files Created
```
DTOs/
  Category/
    FeaturedToursDto.cs (NEW)
    WaysToTravelDto.cs (existing, moved from root)
Models/
  Category.cs (modified)
  SysDbContext.cs (modified)
Repositories/
  IHomePageFeaturedTourRepository.cs (NEW)
  HomePageFeaturedTourRepository.cs (NEW)
Services/
  IFeaturedDailyToursService.cs (NEW)
  FeaturedDailyToursService.cs (NEW)
Controllers/
  Api/
    FeaturedDailyToursController.cs (NEW)
  FeaturedDailyToursController.cs (NEW)
Views/
  FeaturedDailyTours/
    Index.cshtml (NEW)
Program.cs (modified - registration)
```

## API Response Structure

### Featured Tours Response
```json
{
  "success": true,
  "message": "Featured tours retrieved successfully",
  "data": {
    "tours": [
      {
        "id": 1,
        "badge": "IN THE SPOTLIGHTS",
        "categoryName": "Ha Long Bay",
        "description": "Experience the wonder...",
        "imageUrl": "/uploads/...",
        "displayOrder": 1,
        "tourId": 5,
        "cardClass": "ha-long"
      }
    ]
  }
}
```

### Daily Tours Response
```json
{
  "success": true,
  "message": "Daily tours retrieved successfully",
  "data": {
    "tours": [
      {
        "id": 1,
        "categoryId": 10,
        "badge": "DAILY TOURS",
        "categoryName": "North Vietnam",
        "description": "Adventurous experiences",
        "imageUrl": "/uploads/...",
        "displayOrder": 1,
        "cardClass": "north-vietnam"
      }
    ]
  }
}
```

## Key Features

1. **Max Limits Enforcement**
   - Featured Tours: Max 2 (validated in `ConfigureFeaturedTourAsync`)
   - Daily Tours: Max 3 (validated in `ConfigureDailyTourAsync`)
   - Validation occurs when creating a new tour (not when updating existing)
   - Throws `InvalidOperationException` with descriptive message

2. **Image Management**
   - Upload to `wwwroot/uploads/`
   - Store paths in database
   - Image validation (type, size)
   - Responsive gallery

3. **Reorder Functionality**
   - Up/Down buttons
   - Batch API call for multiple tours
   - Real-time UI update

4. **Clean Architecture**
   - SOLID principles
   - Separation of concerns
   - Repository pattern
   - Dependency injection

## Migration Notes

To apply database changes:

```sql
-- Add Daily Tour columns to Category table
ALTER TABLE [Category] ADD [DailyTourBadge] NVARCHAR(100) NULL;
ALTER TABLE [Category] ADD [DailyTourDescription] NVARCHAR(500) NULL;
ALTER TABLE [Category] ADD [DailyTourImageUrl] NVARCHAR(500) NULL;
ALTER TABLE [Category] ADD [DailyTourOrder] INT NULL;
ALTER TABLE [Category] ADD [ShowInDailyTours] BIT NULL DEFAULT 0;
ALTER TABLE [Category] ADD [DailyTourCardClass] NVARCHAR(100) NULL;

-- Create index for Daily Tours
CREATE INDEX [IX_Category_DailyTours] ON [Category]([ShowInDailyTours], [DailyTourOrder]);
```

## Next Steps

1. Run database migration
2. Test API endpoints via Swagger
3. Configure Featured Tours and Daily Tours via admin UI
4. Verify rendering on homepage
