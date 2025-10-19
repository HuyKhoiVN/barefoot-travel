# Tour Management API Specification

## Overview

This document defines the RESTful API specifications for the Tour Management module in the Barefoot Travel System. The APIs are designed to manage tours and their associated data (images, categories, prices, policies) following the established development rules and database schema.

## API Design Principles

- **RESTful Conventions**: Follow REST principles with proper HTTP methods and status codes
- **Authentication**: JWT-based authentication with Admin role validation
- **Response Format**: Standardized ApiResponse format for all endpoints
- **Database Operations**: Use transactions for multi-table operations
- **Soft Deletes**: Set Active = 0 instead of physical deletion
- **Audit Fields**: Consistent CreatedTime, UpdatedTime, UpdatedBy tracking

## Development Order

The APIs are developed in the following order to ensure data dependencies are met:

1. **Auxiliary Table APIs** (TourImage, TourCategory, TourPrice, TourPolicy)
2. **Tour Management APIs** (US06, US07, US08, US09)

---

## 1. Auxiliary Table APIs

### 1.1 TourImage APIs

#### 1.1.1 Create Tour Image

**Endpoint**: `POST /api/tour-image`

**Description**: Create a new tour image record

**Authorization**: Admin role required

**Request Body**:
```json
{
  "tourId": 1,
  "imageUrl": "https://example.com/image.jpg",
  "isBanner": true
}
```

**Response**:
```json
{
  "success": true,
  "message": "Tour image created successfully",
  "data": {
    "id": 1,
    "tourId": 1,
    "imageUrl": "https://example.com/image.jpg",
    "isBanner": true,
    "createdTime": "2024-01-01T00:00:00Z"
  }
}
```

**Error Responses**:
- `400 Bad Request`: Invalid input data
- `401 Unauthorized`: Missing or invalid JWT token
- `403 Forbidden`: User is not an Admin
- `404 Not Found`: Tour not found
- `500 Internal Server Error`: Server error

**Related Tables**: TourImage, Tour

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var tourExists = (from t in dbContext.Tours
                      where t.Id == request.TourId && t.Active == true
                      select t).Any();
    if (!tourExists) throw new Exception("Tour not found");
    
    var tourImage = new TourImage
    {
        TourId = request.TourId,
        ImageUrl = request.ImageUrl,
        IsBanner = request.IsBanner,
        CreatedTime = DateTime.UtcNow,
        Active = true
    };
    
    dbContext.TourImages.Add(tourImage);
    dbContext.SaveChanges();
    transaction.Commit();
}
```

#### 1.1.2 Update Tour Image

**Endpoint**: `PUT /api/tour-image/{id}`

**Description**: Update an existing tour image

**Authorization**: Admin role required

**Request Body**:
```json
{
  "imageUrl": "https://example.com/updated-image.jpg",
  "isBanner": false
}
```

**Response**:
```json
{
  "success": true,
  "message": "Tour image updated successfully",
  "data": {
    "id": 1,
    "tourId": 1,
    "imageUrl": "https://example.com/updated-image.jpg",
    "isBanner": false,
    "updatedTime": "2024-01-01T00:00:00Z"
  }
}
```

**Related Tables**: TourImage

**LINQ Query**:
```csharp
var image = (from ti in dbContext.TourImages
             where ti.Id == imageId && ti.Active == true
             select ti).SingleOrDefault();

if (image != null)
{
    image.ImageUrl = request.ImageUrl;
    image.IsBanner = request.IsBanner;
    image.UpdatedTime = DateTime.UtcNow;
    image.UpdatedBy = adminUsername;
    dbContext.SaveChanges();
}
```

#### 1.1.3 Delete Tour Image

**Endpoint**: `DELETE /api/tour-image/{id}`

**Description**: Soft delete a tour image

**Authorization**: Admin role required

**Response**:
```json
{
  "success": true,
  "message": "Tour image deleted successfully"
}
```

**Related Tables**: TourImage

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var image = (from ti in dbContext.TourImages
                 where ti.Id == imageId && ti.Active == true
                 select ti).SingleOrDefault();
    
    if (image != null)
    {
        image.Active = false;
        image.UpdatedTime = DateTime.UtcNow;
        image.UpdatedBy = adminUsername;
        dbContext.SaveChanges();
    }
    transaction.Commit();
}
```

#### 1.1.4 List Tour Images

**Endpoint**: `GET /api/tour-image/tour/{tourId}`

**Description**: Get all images for a specific tour

**Authorization**: Admin role required

**Response**:
```json
{
  "success": true,
  "message": "Tour images retrieved successfully",
  "data": [
    {
      "id": 1,
      "tourId": 1,
      "imageUrl": "https://example.com/image1.jpg",
      "isBanner": true,
      "createdTime": "2024-01-01T00:00:00Z"
    }
  ]
}
```

**Related Tables**: TourImage

**LINQ Query**:
```csharp
var images = (from ti in dbContext.TourImages
              where ti.TourId == tourId && ti.Active == true
              select new { 
                  ti.Id, 
                  ti.ImageUrl, 
                  ti.IsBanner, 
                  ti.CreatedTime 
              }).ToList();
```

### 1.2 TourCategory APIs

#### 1.2.1 Create Tour Category

**Endpoint**: `POST /api/tour-category`

**Description**: Link a tour to a category

**Authorization**: Admin role required

**Request Body**:
```json
{
  "tourId": 1,
  "categoryId": 2
}
```

**Response**:
```json
{
  "success": true,
  "message": "Tour category created successfully",
  "data": {
    "id": 1,
    "tourId": 1,
    "categoryId": 2,
    "createdTime": "2024-01-01T00:00:00Z"
  }
}
```

**Related Tables**: TourCategory, Tour, Category

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var tourExists = (from t in dbContext.Tours
                      where t.Id == request.TourId && t.Active == true
                      select t).Any();
    var categoryExists = (from c in dbContext.Categories
                         where c.Id == request.CategoryId && c.Active == true
                         select c).Any();
    
    if (!tourExists || !categoryExists) 
        throw new Exception("Invalid TourId or CategoryId");
    
    var tourCategory = new TourCategory
    {
        TourId = request.TourId,
        CategoryId = request.CategoryId,
        CreatedTime = DateTime.UtcNow,
        Active = true
    };
    
    dbContext.TourCategories.Add(tourCategory);
    dbContext.SaveChanges();
    transaction.Commit();
}
```

#### 1.2.2 Delete Tour Category

**Endpoint**: `DELETE /api/tour-category/{id}`

**Description**: Soft delete a tour category link

**Authorization**: Admin role required

**Response**:
```json
{
  "success": true,
  "message": "Tour category deleted successfully"
}
```

**Related Tables**: TourCategory

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var tourCategory = (from tc in dbContext.TourCategories
                        where tc.Id == id && tc.Active == true
                        select tc).SingleOrDefault();
    
    if (tourCategory != null)
    {
        tourCategory.Active = false;
        tourCategory.UpdatedTime = DateTime.UtcNow;
        tourCategory.UpdatedBy = adminUsername;
        dbContext.SaveChanges();
    }
    transaction.Commit();
}
```

#### 1.2.3 List Tour Categories

**Endpoint**: `GET /api/tour-category/tour/{tourId}`

**Description**: Get all categories for a specific tour

**Authorization**: Admin role required

**Response**:
```json
{
  "success": true,
  "message": "Tour categories retrieved successfully",
  "data": [
    {
      "id": 1,
      "tourId": 1,
      "categoryId": 2,
      "categoryName": "Adventure",
      "createdTime": "2024-01-01T00:00:00Z"
    }
  ]
}
```

**Related Tables**: TourCategory, Category

**LINQ Query**:
```csharp
var categories = (from tc in dbContext.TourCategories
                  join c in dbContext.Categories on tc.CategoryId equals c.Id
                  where tc.TourId == tourId && tc.Active == true && c.Active == true
                  select new { 
                      tc.Id, 
                      tc.TourId, 
                      tc.CategoryId, 
                      CategoryName = c.CategoryName,
                      tc.CreatedTime 
                  }).ToList();
```

### 1.3 TourPrice APIs

#### 1.3.1 Create Tour Price

**Endpoint**: `POST /api/tour-price`

**Description**: Create a new tour price record

**Authorization**: Admin role required

**Request Body**:
```json
{
  "tourId": 1,
  "priceTypeId": 1,
  "price": 150.00
}
```

**Response**:
```json
{
  "success": true,
  "message": "Tour price created successfully",
  "data": {
    "id": 1,
    "tourId": 1,
    "priceTypeId": 1,
    "price": 150.00,
    "createdTime": "2024-01-01T00:00:00Z"
  }
}
```

**Related Tables**: TourPrice, Tour, PriceType

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var tourExists = (from t in dbContext.Tours
                      where t.Id == request.TourId && t.Active == true
                      select t).Any();
    var priceTypeExists = (from pt in dbContext.PriceTypes
                          where pt.Id == request.PriceTypeId && pt.Active == true
                          select pt).Any();
    
    if (!tourExists || !priceTypeExists) 
        throw new Exception("Invalid TourId or PriceTypeId");
    
    var tourPrice = new TourPrice
    {
        TourId = request.TourId,
        PriceTypeId = request.PriceTypeId,
        Price = request.Price,
        CreatedTime = DateTime.UtcNow,
        Active = true
    };
    
    dbContext.TourPrices.Add(tourPrice);
    dbContext.SaveChanges();
    transaction.Commit();
}
```

#### 1.3.2 Update Tour Price

**Endpoint**: `PUT /api/tour-price/{id}`

**Description**: Update an existing tour price

**Authorization**: Admin role required

**Request Body**:
```json
{
  "price": 175.00
}
```

**Response**:
```json
{
  "success": true,
  "message": "Tour price updated successfully",
  "data": {
    "id": 1,
    "tourId": 1,
    "priceTypeId": 1,
    "price": 175.00,
    "updatedTime": "2024-01-01T00:00:00Z"
  }
}
```

**Related Tables**: TourPrice

**LINQ Query**:
```csharp
var tourPrice = (from tp in dbContext.TourPrices
                 where tp.Id == id && tp.Active == true
                 select tp).SingleOrDefault();

if (tourPrice != null)
{
    tourPrice.Price = request.Price;
    tourPrice.UpdatedTime = DateTime.UtcNow;
    tourPrice.UpdatedBy = adminUsername;
    dbContext.SaveChanges();
}
```

#### 1.3.3 Delete Tour Price

**Endpoint**: `DELETE /api/tour-price/{id}`

**Description**: Soft delete a tour price

**Authorization**: Admin role required

**Response**:
```json
{
  "success": true,
  "message": "Tour price deleted successfully"
}
```

**Related Tables**: TourPrice

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var tourPrice = (from tp in dbContext.TourPrices
                     where tp.Id == id && tp.Active == true
                     select tp).SingleOrDefault();
    
    if (tourPrice != null)
    {
        tourPrice.Active = false;
        tourPrice.UpdatedTime = DateTime.UtcNow;
        tourPrice.UpdatedBy = adminUsername;
        dbContext.SaveChanges();
    }
    transaction.Commit();
}
```

#### 1.3.4 List Tour Prices

**Endpoint**: `GET /api/tour-price/tour/{tourId}`

**Description**: Get all prices for a specific tour

**Authorization**: Admin role required

**Response**:
```json
{
  "success": true,
  "message": "Tour prices retrieved successfully",
  "data": [
    {
      "id": 1,
      "tourId": 1,
      "priceTypeId": 1,
      "priceTypeName": "Adult",
      "price": 150.00,
      "createdTime": "2024-01-01T00:00:00Z"
    }
  ]
}
```

**Related Tables**: TourPrice, PriceType

**LINQ Query**:
```csharp
var prices = (from tp in dbContext.TourPrices
             join pt in dbContext.PriceTypes on tp.PriceTypeId equals pt.Id
             where tp.TourId == tourId && tp.Active == true && pt.Active == true
             select new { 
                 tp.Id, 
                 tp.TourId, 
                 tp.PriceTypeId, 
                 PriceTypeName = pt.PriceTypeName,
                 tp.Price, 
                 tp.CreatedTime 
             }).ToList();
```

### 1.4 TourPolicy APIs

#### 1.4.1 Create Tour Policy

**Endpoint**: `POST /api/tour-policy`

**Description**: Link a tour to a policy

**Authorization**: Admin role required

**Request Body**:
```json
{
  "tourId": 1,
  "policyId": 2
}
```

**Response**:
```json
{
  "success": true,
  "message": "Tour policy created successfully",
  "data": {
    "id": 1,
    "tourId": 1,
    "policyId": 2,
    "createdTime": "2024-01-01T00:00:00Z"
  }
}
```

**Related Tables**: TourPolicy, Tour, Policy

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var tourExists = (from t in dbContext.Tours
                      where t.Id == request.TourId && t.Active == true
                      select t).Any();
    var policyExists = (from p in dbContext.Policies
                        where p.Id == request.PolicyId && p.Active == true
                        select p).Any();
    
    if (!tourExists || !policyExists) 
        throw new Exception("Invalid TourId or PolicyId");
    
    var tourPolicy = new TourPolicy
    {
        TourId = request.TourId,
        PolicyId = request.PolicyId,
        CreatedTime = DateTime.UtcNow,
        Active = true
    };
    
    dbContext.TourPolicies.Add(tourPolicy);
    dbContext.SaveChanges();
    transaction.Commit();
}
```

#### 1.4.2 Delete Tour Policy

**Endpoint**: `DELETE /api/tour-policy/{id}`

**Description**: Soft delete a tour policy link

**Authorization**: Admin role required

**Response**:
```json
{
  "success": true,
  "message": "Tour policy deleted successfully"
}
```

**Related Tables**: TourPolicy

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var tourPolicy = (from tp in dbContext.TourPolicies
                      where tp.Id == id && tp.Active == true
                      select tp).SingleOrDefault();
    
    if (tourPolicy != null)
    {
        tourPolicy.Active = false;
        tourPolicy.UpdatedTime = DateTime.UtcNow;
        tourPolicy.UpdatedBy = adminUsername;
        dbContext.SaveChanges();
    }
    transaction.Commit();
}
```

#### 1.4.3 List Tour Policies

**Endpoint**: `GET /api/tour-policy/tour/{tourId}`

**Description**: Get all policies for a specific tour

**Authorization**: Admin role required

**Response**:
```json
{
  "success": true,
  "message": "Tour policies retrieved successfully",
  "data": [
    {
      "id": 1,
      "tourId": 1,
      "policyId": 2,
      "policyType": "Cancellation",
      "createdTime": "2024-01-01T00:00:00Z"
    }
  ]
}
```

**Related Tables**: TourPolicy, Policy

**LINQ Query**:
```csharp
var policies = (from tp in dbContext.TourPolicies
               join p in dbContext.Policies on tp.PolicyId equals p.Id
               where tp.TourId == tourId && tp.Active == true && p.Active == true
               select new { 
                   tp.Id, 
                   tp.TourId, 
                   tp.PolicyId, 
                   PolicyType = p.PolicyType,
                   tp.CreatedTime 
               }).ToList();
```

---

## 2. Tour Management APIs

### 2.1 US06 - Tour Management (CRUD)

#### 2.1.1 Create Tour

**Endpoint**: `POST /api/tour`

**Description**: Create a new tour with all associated data (images, categories, prices, policies)

**Authorization**: Admin role required

**Request Body**:
```json
{
  "title": "Adventure Tour",
  "description": "An exciting adventure tour",
  "mapLink": "https://maps.google.com/...",
  "pricePerPerson": 200.00,
  "maxPeople": 20,
  "duration": "3 days 2 nights",
  "startTime": "08:00:00",
  "returnTime": "18:00:00",
  "images": [
    {
      "imageUrl": "https://example.com/image1.jpg",
      "isBanner": true
    }
  ],
  "categories": [1, 2],
  "prices": [
    {
      "priceTypeId": 1,
      "price": 200.00
    }
  ],
  "policies": [1, 2]
}
```

**Response**:
```json
{
  "success": true,
  "message": "Tour created successfully",
  "data": {
    "id": 1,
    "title": "Adventure Tour",
    "description": "An exciting adventure tour",
    "mapLink": "https://maps.google.com/...",
    "pricePerPerson": 200.00,
    "maxPeople": 20,
    "duration": "3 days 2 nights",
    "startTime": "08:00:00",
    "returnTime": "18:00:00",
    "createdTime": "2024-01-01T00:00:00Z",
    "active": true
  }
}
```

**Related Tables**: Tour, TourImage, TourCategory, TourPrice, TourPolicy

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var titleExists = (from t in dbContext.Tours
                       where t.Title == request.Title && t.Active == true
                       select t).Any();
    if (titleExists) throw new Exception("Tour title already exists");
    
    var tour = new Tour
    {
        Title = request.Title,
        Description = request.Description,
        MapLink = request.MapLink,
        PricePerPerson = request.PricePerPerson,
        MaxPeople = request.MaxPeople,
        Duration = request.Duration,
        StartTime = request.StartTime,
        ReturnTime = request.ReturnTime,
        CreatedTime = DateTime.UtcNow,
        UpdatedBy = adminUsername,
        Active = true
    };
    
    dbContext.Tours.Add(tour);
    dbContext.SaveChanges();
    
    // Add images
    var tourImages = request.Images.Select(i => new TourImage
    {
        TourId = tour.Id,
        ImageUrl = i.ImageUrl,
        IsBanner = i.IsBanner,
        CreatedTime = DateTime.UtcNow,
        Active = true
    }).ToList();
    dbContext.TourImages.AddRange(tourImages);
    
    // Add categories
    var tourCategories = request.Categories.Select(c => new TourCategory
    {
        TourId = tour.Id,
        CategoryId = c,
        CreatedTime = DateTime.UtcNow,
        Active = true
    }).ToList();
    dbContext.TourCategories.AddRange(tourCategories);
    
    // Add prices
    var tourPrices = request.Prices.Select(p => new TourPrice
    {
        TourId = tour.Id,
        PriceTypeId = p.PriceTypeId,
        Price = p.Price,
        CreatedTime = DateTime.UtcNow,
        Active = true
    }).ToList();
    dbContext.TourPrices.AddRange(tourPrices);
    
    // Add policies
    var tourPolicies = request.Policies.Select(p => new TourPolicy
    {
        TourId = tour.Id,
        PolicyId = p,
        CreatedTime = DateTime.UtcNow,
        Active = true
    }).ToList();
    dbContext.TourPolicies.AddRange(tourPolicies);
    
    dbContext.SaveChanges();
    transaction.Commit();
}
```

#### 2.1.2 Update Tour

**Endpoint**: `PUT /api/tour/{id}`

**Description**: Update an existing tour and its associated data

**Authorization**: Admin role required

**Request Body**:
```json
{
  "title": "Updated Adventure Tour",
  "description": "Updated description",
  "mapLink": "https://maps.google.com/updated",
  "pricePerPerson": 250.00,
  "maxPeople": 25,
  "duration": "4 days 3 nights",
  "startTime": "09:00:00",
  "returnTime": "19:00:00"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Tour updated successfully",
  "data": {
    "id": 1,
    "title": "Updated Adventure Tour",
    "description": "Updated description",
    "mapLink": "https://maps.google.com/updated",
    "pricePerPerson": 250.00,
    "maxPeople": 25,
    "duration": "4 days 3 nights",
    "startTime": "09:00:00",
    "returnTime": "19:00:00",
    "updatedTime": "2024-01-01T00:00:00Z"
  }
}
```

**Related Tables**: Tour

**LINQ Query**:
```csharp
var tour = (from t in dbContext.Tours
            where t.Id == tourId && t.Active == true
            select t).SingleOrDefault();

if (tour != null)
{
    tour.Title = request.Title;
    tour.Description = request.Description;
    tour.MapLink = request.MapLink;
    tour.PricePerPerson = request.PricePerPerson;
    tour.MaxPeople = request.MaxPeople;
    tour.Duration = request.Duration;
    tour.StartTime = request.StartTime;
    tour.ReturnTime = request.ReturnTime;
    tour.UpdatedTime = DateTime.UtcNow;
    tour.UpdatedBy = adminUsername;
    dbContext.SaveChanges();
}
```

#### 2.1.3 Delete Tour

**Endpoint**: `DELETE /api/tour/{id}`

**Description**: Soft delete a tour (only if no active bookings)

**Authorization**: Admin role required

**Response**:
```json
{
  "success": true,
  "message": "Tour deleted successfully"
}
```

**Error Responses**:
- `400 Bad Request`: Cannot delete tour with active bookings

**Related Tables**: Tour, Booking

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var hasActiveBookings = (from b in dbContext.Bookings
                            where b.TourId == tourId && b.Active == true && b.StatusTypeId != 3
                            select b).Any();
    if (hasActiveBookings) throw new Exception("Cannot delete tour with active bookings");
    
    var tour = (from t in dbContext.Tours
                where t.Id == tourId && t.Active == true
                select t).SingleOrDefault();
    
    if (tour != null)
    {
        tour.Active = false;
        tour.UpdatedTime = DateTime.UtcNow;
        tour.UpdatedBy = adminUsername;
        dbContext.SaveChanges();
    }
    transaction.Commit();
}
```

#### 2.1.4 Get Tour

**Endpoint**: `GET /api/tour/{id}`

**Description**: Get a specific tour with all related data

**Authorization**: Admin role required

**Response**:
```json
{
  "success": true,
  "message": "Tour retrieved successfully",
  "data": {
    "id": 1,
    "title": "Adventure Tour",
    "description": "An exciting adventure tour",
    "mapLink": "https://maps.google.com/...",
    "pricePerPerson": 200.00,
    "maxPeople": 20,
    "duration": "3 days 2 nights",
    "startTime": "08:00:00",
    "returnTime": "18:00:00",
    "images": [
      {
        "id": 1,
        "imageUrl": "https://example.com/image1.jpg",
        "isBanner": true
      }
    ],
    "categories": [
      {
        "id": 1,
        "categoryName": "Adventure"
      }
    ],
    "prices": [
      {
        "id": 1,
        "priceTypeName": "Adult",
        "price": 200.00
      }
    ],
    "policies": [
      {
        "id": 1,
        "policyType": "Cancellation"
      }
    ],
    "createdTime": "2024-01-01T00:00:00Z",
    "active": true
  }
}
```

**Related Tables**: Tour, TourImage, TourCategory, TourPrice, TourPolicy, Category, PriceType, Policy

**LINQ Query**:
```csharp
var tour = (from t in dbContext.Tours
            where t.Id == tourId && t.Active == true
            select new
            {
                t.Id,
                t.Title,
                t.Description,
                t.MapLink,
                t.PricePerPerson,
                t.MaxPeople,
                t.Duration,
                t.StartTime,
                t.ReturnTime,
                Images = (from ti in dbContext.TourImages 
                         where ti.TourId == t.Id && ti.Active == true 
                         select new { ti.Id, ti.ImageUrl, ti.IsBanner }),
                Categories = (from tc in dbContext.TourCategories
                              join c in dbContext.Categories on tc.CategoryId equals c.Id
                              where tc.TourId == t.Id && tc.Active == true && c.Active == true
                              select new { c.Id, c.CategoryName }),
                Prices = (from tp in dbContext.TourPrices
                          join pt in dbContext.PriceTypes on tp.PriceTypeId equals pt.Id
                          where tp.TourId == t.Id && tp.Active == true && pt.Active == true
                          select new { tp.Id, pt.PriceTypeName, tp.Price }),
                Policies = (from tpol in dbContext.TourPolicies
                           join p in dbContext.Policies on tpol.PolicyId equals p.Id
                           where tpol.TourId == t.Id && tpol.Active == true && p.Active == true
                           select new { p.Id, p.PolicyType }),
                t.CreatedTime,
                t.Active
            }).SingleOrDefault();
```

#### 2.1.5 List Tours

**Endpoint**: `GET /api/tour`

**Description**: Get paginated list of tours with filtering and sorting

**Authorization**: Admin role required

**Query Parameters**:
- `page` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 10)
- `sortBy` (string, optional): Sort field (title, pricePerPerson, createdTime)
- `sortOrder` (string, optional): Sort direction (asc, desc)
- `categoryId` (int, optional): Filter by category
- `active` (bool, optional): Filter by active status

**Response**:
```json
{
  "success": true,
  "message": "Tours retrieved successfully",
  "data": {
    "items": [
      {
        "id": 1,
        "title": "Adventure Tour",
        "pricePerPerson": 200.00,
        "duration": "3 days 2 nights",
        "maxPeople": 20,
        "active": true,
        "createdTime": "2024-01-01T00:00:00Z"
      }
    ],
    "totalItems": 1,
    "totalPages": 1,
    "currentPage": 1,
    "pageSize": 10
  }
}
```

**Related Tables**: Tour, TourCategory, Category

**LINQ Query**:
```csharp
var query = from t in dbContext.Tours
            where t.Active == true
            select t;

if (categoryId.HasValue)
{
    query = from t in query
            where (from tc in dbContext.TourCategories
                   where tc.TourId == t.Id && tc.CategoryId == categoryId && tc.Active == true
                   select tc).Any()
            select t;
}

if (active.HasValue)
{
    query = from t in query
            where t.Active == active
            select t;
}

var totalItems = query.Count();

var tours = query
    .OrderBy(t => sortBy == "title" ? t.Title : 
              sortBy == "pricePerPerson" ? t.PricePerPerson.ToString() : 
              t.CreatedTime.ToString())
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .Select(t => new { 
        t.Id, 
        t.Title, 
        t.PricePerPerson, 
        t.Duration, 
        t.MaxPeople, 
        t.Active, 
        t.CreatedTime 
    })
    .ToList();
```

### 2.2 US07 - Enable/Disable Tour

#### 2.2.1 Toggle Tour Status

**Endpoint**: `PUT /api/tour/{id}/status`

**Description**: Enable or disable a tour

**Authorization**: Admin role required

**Request Body**:
```json
{
  "active": false
}
```

**Response**:
```json
{
  "success": true,
  "message": "Tour status updated successfully",
  "data": {
    "id": 1,
    "active": false,
    "updatedTime": "2024-01-01T00:00:00Z"
  }
}
```

**Error Responses**:
- `400 Bad Request`: Cannot disable tour with active bookings

**Related Tables**: Tour, Booking

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var hasActiveBookings = (from b in dbContext.Bookings
                            where b.TourId == tourId && b.Active == true && b.StatusTypeId != 3
                            select b).Any();
    if (hasActiveBookings && !request.Active) 
        throw new Exception("Cannot disable tour with active bookings");
    
    var tour = (from t in dbContext.Tours
                where t.Id == tourId
                select t).SingleOrDefault();
    
    if (tour != null)
    {
        tour.Active = request.Active;
        tour.UpdatedTime = DateTime.UtcNow;
        tour.UpdatedBy = adminUsername;
        dbContext.SaveChanges();
    }
    transaction.Commit();
}
```

### 2.3 US08 - Tour Itinerary Management

#### 2.3.1 Update Itinerary

**Endpoint**: `PUT /api/tour/{id}/itinerary`

**Description**: Update tour itinerary (stored in Description field)

**Authorization**: Admin role required

**Request Body**:
```json
{
  "itineraryJson": "{\"days\":[{\"day\":1,\"activities\":[\"Arrival\",\"City Tour\"]}]}"
}
```

**Response**:
```json
{
  "success": true,
  "message": "Tour itinerary updated successfully",
  "data": {
    "id": 1,
    "itineraryJson": "{\"days\":[{\"day\":1,\"activities\":[\"Arrival\",\"City Tour\"]}]}",
    "updatedTime": "2024-01-01T00:00:00Z"
  }
}
```

**Related Tables**: Tour

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var tour = (from t in dbContext.Tours
                where t.Id == tourId && t.Active == true
                select t).SingleOrDefault();
    
    if (tour == null) throw new Exception("Tour not found");
    
    tour.Description = request.ItineraryJson;
    tour.UpdatedTime = DateTime.UtcNow;
    tour.UpdatedBy = adminUsername;
    dbContext.SaveChanges();
    transaction.Commit();
}
```

### 2.4 US09 - Attach Marketing Tags

#### 2.4.1 Add Marketing Tag

**Endpoint**: `POST /api/tour/{id}/marketing-tag`

**Description**: Add a marketing tag to a tour

**Authorization**: Admin role required

**Request Body**:
```json
{
  "categoryId": 5
}
```

**Response**:
```json
{
  "success": true,
  "message": "Marketing tag added successfully",
  "data": {
    "id": 1,
    "tourId": 1,
    "categoryId": 5,
    "categoryName": "Best Seller",
    "createdTime": "2024-01-01T00:00:00Z"
  }
}
```

**Related Tables**: TourCategory, Tour, Category

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var categoryExists = (from c in dbContext.Categories
                         where c.Id == request.CategoryId && c.Active == true && c.Type == "Marketing"
                         select c).Any();
    if (!categoryExists) throw new Exception("Invalid marketing category");
    
    var tourExists = (from t in dbContext.Tours
                      where t.Id == tourId && t.Active == true
                      select t).Any();
    if (!tourExists) throw new Exception("Tour not found");
    
    var tourCategory = new TourCategory
    {
        TourId = tourId,
        CategoryId = request.CategoryId,
        CreatedTime = DateTime.UtcNow,
        Active = true
    };
    
    dbContext.TourCategories.Add(tourCategory);
    dbContext.SaveChanges();
    transaction.Commit();
}
```

#### 2.4.2 Remove Marketing Tag

**Endpoint**: `DELETE /api/tour/{id}/marketing-tag/{categoryId}`

**Description**: Remove a marketing tag from a tour

**Authorization**: Admin role required

**Response**:
```json
{
  "success": true,
  "message": "Marketing tag removed successfully"
}
```

**Related Tables**: TourCategory

**LINQ Query**:
```csharp
using (var transaction = dbContext.Database.BeginTransaction())
{
    var tourCategory = (from tc in dbContext.TourCategories
                        where tc.TourId == tourId && tc.CategoryId == categoryId && tc.Active == true
                        select tc).SingleOrDefault();
    
    if (tourCategory != null)
    {
        tourCategory.Active = false;
        tourCategory.UpdatedTime = DateTime.UtcNow;
        tourCategory.UpdatedBy = adminUsername;
        dbContext.SaveChanges();
    }
    transaction.Commit();
}
```

#### 2.4.3 List Marketing Tags

**Endpoint**: `GET /api/tour/{id}/marketing-tags`

**Description**: Get all marketing tags for a tour

**Authorization**: Admin role required

**Response**:
```json
{
  "success": true,
  "message": "Marketing tags retrieved successfully",
  "data": [
    {
      "id": 1,
      "tourId": 1,
      "categoryId": 5,
      "categoryName": "Best Seller",
      "createdTime": "2024-01-01T00:00:00Z"
    }
  ]
}
```

**Related Tables**: TourCategory, Category

**LINQ Query**:
```csharp
var marketingTags = (from tc in dbContext.TourCategories
                    join c in dbContext.Categories on tc.CategoryId equals c.Id
                    where tc.TourId == tourId && tc.Active == true && c.Active == true && c.Type == "Marketing"
                    select new { 
                        tc.Id, 
                        tc.TourId, 
                        tc.CategoryId, 
                        CategoryName = c.CategoryName,
                        tc.CreatedTime 
                    }).ToList();
```

---

## Error Handling

### Standard Error Responses

All endpoints return standardized error responses:

```json
{
  "success": false,
  "message": "Error description",
  "data": null
}
```

### HTTP Status Codes

- `200 OK`: Successful GET, PUT, DELETE operations
- `201 Created`: Successful POST operations
- `400 Bad Request`: Invalid input data or business rule violations
- `401 Unauthorized`: Missing or invalid JWT token
- `403 Forbidden`: User is not an Admin
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server error

### Common Error Messages

- "Tour not found"
- "Tour title already exists"
- "Cannot delete tour with active bookings"
- "Cannot disable tour with active bookings"
- "Invalid TourId or CategoryId"
- "Invalid marketing category"
- "Validation failed"

---

## Security Considerations

### Authentication
- All endpoints require JWT-based authentication
- JWT token must contain valid role claims
- Token expiration is handled automatically

### Authorization
- Only Admin role can access Tour Management APIs
- Role validation via JWT claims
- No additional permission checks required

### Data Validation
- Input validation for all request bodies
- Business rule validation (e.g., unique titles, active bookings)
- SQL injection prevention through parameterized queries

### Audit Trail
- All operations track CreatedTime, UpdatedTime, UpdatedBy
- Soft deletes maintain data integrity
- Transaction rollback on failures

---

## Performance Considerations

### Database Optimization
- Use existing indexes for query optimization
- Avoid N+1 queries with proper joins
- Use projections for list endpoints
- Implement pagination for large datasets

### Caching Strategy
- Consider caching for frequently accessed tour data
- Cache category and price type lookups
- Implement cache invalidation on updates

### Query Optimization
- Use LINQ projections to minimize data transfer
- Implement proper sorting and filtering
- Use transactions only when necessary

---

## Testing Guidelines

### Unit Testing
- Test all service methods with mocked dependencies
- Test mapping methods independently
- Test error scenarios and edge cases

### Integration Testing
- Test API endpoints with real database
- Test authentication and authorization
- Test transaction rollback scenarios

### Performance Testing
- Test pagination with large datasets
- Test concurrent operations
- Monitor query execution times

---

## Conclusion

This API specification provides a comprehensive set of RESTful endpoints for Tour Management in the Barefoot Travel System. The APIs follow established conventions, implement proper security measures, and ensure data consistency through transaction management. The development order ensures that all dependencies are met and each API can be used independently without violating database constraints.
