# Ways to Travel - Database Design & Business Flow

## Overview
Ways to Travel is a special homepage section that displays categories with travel styles. This document describes how to leverage existing database structure with minimal changes.

## Database Strategy

### Recommended Approach: Extend Category Table ⭐

Add new fields to existing `Category` table:
```sql
ALTER TABLE Category
ADD WaysToTravelImage1 NVARCHAR(500) NULL,
    WaysToTravelImage2 NVARCHAR(500) NULL,
    WaysToTravelOrder INT NULL,
    ShowInWaysToTravel BIT DEFAULT 0;

CREATE INDEX IX_Category_WaysToTravel ON Category(ShowInWaysToTravel, WaysToTravelOrder);
```

**Benefits:**
- No new table needed
- Clean data model (one source of truth)
- Easy to maintain
- Reuses Category.Type = "travel-style"
- Simple queries (no JOIN needed)

## Business Rules

1. **Maximum 5 categories** in Ways to Travel
2. **Image1 is required** (at least one image)
3. **Image2 is optional**
4. **Categories must be active**
5. **Category.Type = "travel-style"** recommended

## Data Flow

### Admin Workflow
1. Admin opens Homepage Content Management
2. Click "Configure Ways to Travel"
3. Select category and upload images (1-2 images)
4. Set display order
5. Save configuration
6. System validates (max 5, image required)
7. Display on homepage

### Frontend Display
1. Query categories where ShowInWaysToTravel = 1
2. Order by WaysToTravelOrder
3. Display with CategoryName, TotalTours, Images

## API Endpoints

```
GET /api/WaysToTravel - Get all Ways to Travel categories
PUT /api/Category/{id}/ways-to-travel - Configure category
DELETE /api/Category/{id}/ways-to-travel - Remove from section
PUT /api/WaysToTravel/reorder - Reorder categories
```
