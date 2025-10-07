# Category & Policy API Documentation

## Tổng quan

Đã xây dựng hoàn chỉnh CategoryController và PolicyController với CRUD cơ bản, tuân thủ các nguyên tắc SOLID và best practices.

## Category API

### Endpoints

#### 1. Get Category by ID
```
GET /api/category/{id}
```
**Response:**
```json
{
  "success": true,
  "message": "Category retrieved successfully",
  "data": {
    "id": 1,
    "parentId": null,
    "categoryName": "Adventure Tours",
    "enable": true,
    "type": "Tour",
    "priority": 1,
    "createdTime": "2024-01-01T00:00:00Z",
    "updatedTime": null,
    "updatedBy": null,
    "active": true
  }
}
```

#### 2. Get All Categories
```
GET /api/category
```
**Response:**
```json
{
  "success": true,
  "message": "Categories retrieved successfully",
  "data": [
    {
      "id": 1,
      "categoryName": "Adventure Tours",
      "type": "Tour",
      "active": true
    }
  ]
}
```

#### 3. Get Categories with Pagination
```
GET /api/category/paged?page=1&pageSize=10&sortBy=categoryName&sortOrder=asc&type=Tour&active=true
```
**Query Parameters:**
- `page`: Page number (default: 1)
- `pageSize`: Items per page (default: 10)
- `sortBy`: Sort field (categoryName, type, priority, createdTime)
- `sortOrder`: Sort direction (asc, desc)
- `type`: Filter by type
- `active`: Filter by active status

#### 4. Get Categories by Type
```
GET /api/category/type/{type}
```
**Example:** `/api/category/type/Tour`

#### 5. Get Categories by Parent ID
```
GET /api/category/parent/{parentId?}
```
**Example:** `/api/category/parent/1` or `/api/category/parent/` for root categories

#### 6. Create Category
```
POST /api/category
```
**Request Body:**
```json
{
  "categoryName": "Adventure Tours",
  "parentId": null,
  "enable": true,
  "type": "Tour",
  "priority": 1
}
```

#### 7. Update Category
```
PUT /api/category/{id}
```
**Request Body:**
```json
{
  "categoryName": "Updated Adventure Tours",
  "parentId": null,
  "enable": true,
  "type": "Tour",
  "priority": 2
}
```

#### 8. Delete Category
```
DELETE /api/category/{id}
```
**Note:** Cannot delete categories with child categories

#### 9. Update Category Status
```
PATCH /api/category/{id}/status
```
**Request Body:** `true` or `false`

## Policy API

### Endpoints

#### 1. Get Policy by ID
```
GET /api/policy/{id}
```
**Response:**
```json
{
  "success": true,
  "message": "Policy retrieved successfully",
  "data": {
    "id": 1,
    "policyType": "Cancellation Policy",
    "createdTime": "2024-01-01T00:00:00Z",
    "updatedTime": null,
    "updatedBy": null,
    "active": true
  }
}
```

#### 2. Get All Policies
```
GET /api/policy
```

#### 3. Get Policies with Pagination
```
GET /api/policy/paged?page=1&pageSize=10&sortBy=policyType&sortOrder=asc&active=true
```

#### 4. Create Policy
```
POST /api/policy
```
**Request Body:**
```json
{
  "policyType": "Cancellation Policy"
}
```

#### 5. Update Policy
```
PUT /api/policy/{id}
```
**Request Body:**
```json
{
  "policyType": "Updated Cancellation Policy"
}
```

#### 6. Delete Policy
```
DELETE /api/policy/{id}
```

#### 7. Update Policy Status
```
PATCH /api/policy/{id}/status
```
**Request Body:** `true` or `false`

## DTOs

### Category DTOs

#### CategoryDto
```csharp
public class CategoryDto
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string CategoryName { get; set; }
    public bool Enable { get; set; }
    public string Type { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }
    public string? UpdatedBy { get; set; }
    public bool Active { get; set; }
}
```

#### CreateCategoryDto
```csharp
public class CreateCategoryDto
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; }
    
    public int? ParentId { get; set; }
    public bool Enable { get; set; } = true;
    
    [Required]
    [StringLength(50)]
    public string Type { get; set; }
    
    public int Priority { get; set; } = 0;
}
```

### Policy DTOs

#### PolicyDto
```csharp
public class PolicyDto
{
    public int Id { get; set; }
    public string PolicyType { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }
    public string? UpdatedBy { get; set; }
    public bool Active { get; set; }
}
```

#### CreatePolicyDto
```csharp
public class CreatePolicyDto
{
    [Required]
    [StringLength(100)]
    public string PolicyType { get; set; }
}
```

## Business Logic

### Category Business Rules

1. **Hierarchical Structure**: Categories can have parent-child relationships
2. **Name Uniqueness**: Category names must be unique
3. **Type Validation**: Categories must have a valid type
4. **Deletion Rules**: Cannot delete categories with child categories
5. **Self-Reference Prevention**: Category cannot be its own parent

### Policy Business Rules

1. **Type Uniqueness**: Policy types must be unique
2. **Simple Structure**: Policies are flat (no hierarchy)
3. **Soft Delete**: Policies are soft deleted (Active = false)

## Error Handling

### Common Error Responses

#### 400 Bad Request
```json
{
  "success": false,
  "message": "Invalid model state",
  "data": {
    "categoryName": ["Category name is required"]
  }
}
```

#### 404 Not Found
```json
{
  "success": false,
  "message": "Category not found"
}
```

#### 400 Business Logic Error
```json
{
  "success": false,
  "message": "Category name already exists"
}
```

## Authentication

All endpoints require authentication via JWT token in the Authorization header:
```
Authorization: Bearer {token}
```

## Validation

### Category Validation
- `CategoryName`: Required, max 100 characters
- `Type`: Required, max 50 characters
- `ParentId`: Must exist if provided
- `Priority`: Integer value

### Policy Validation
- `PolicyType`: Required, max 100 characters

## Performance Considerations

1. **Pagination**: All list endpoints support pagination
2. **Sorting**: Multiple sort options available
3. **Filtering**: Filter by type, active status
4. **Indexing**: Ensure database indexes on frequently queried fields

## Database Indexes Recommended

```sql
-- Category indexes
CREATE INDEX IX_Categories_Active ON Categories(Active);
CREATE INDEX IX_Categories_Type ON Categories(Type);
CREATE INDEX IX_Categories_ParentId ON Categories(ParentId);
CREATE INDEX IX_Categories_CategoryName ON Categories(CategoryName);

-- Policy indexes
CREATE INDEX IX_Policies_Active ON Policies(Active);
CREATE INDEX IX_Policies_PolicyType ON Policies(PolicyType);
```

## Testing

### Unit Tests
- Repository layer tests
- Service layer tests
- Controller tests

### Integration Tests
- End-to-end API tests
- Database integration tests
- Authentication tests

## Security

1. **Authentication**: JWT token required
2. **Authorization**: Role-based access control
3. **Input Validation**: All inputs validated
4. **SQL Injection**: Protected by Entity Framework
5. **XSS Protection**: Input sanitization

## Monitoring

1. **Logging**: Structured logging for all operations
2. **Metrics**: Performance metrics for endpoints
3. **Health Checks**: Database connectivity checks
4. **Error Tracking**: Centralized error logging
