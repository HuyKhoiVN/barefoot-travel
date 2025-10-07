# PriceType API Documentation - Barefoot Travel System

## Tổng quan

API CRUD cơ bản cho quản lý loại giá tour (PriceType) trong hệ thống Barefoot Travel. API này tuân thủ các quy tắc phát triển đã định nghĩa trong `API-Development-Rules.md` và cấu trúc database từ `Database-schema.md`.

## Base URL

```
/api/pricetype
```

## Authentication

Tất cả endpoints đều yêu cầu JWT authentication (trừ khi có ghi chú khác).

## Endpoints

### 1. Lấy danh sách tất cả loại giá

**GET** `/api/pricetype`

**Mô tả**: Lấy danh sách tất cả loại giá đang hoạt động

**Response**:
```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "id": 1,
      "priceTypeName": "Adult",
      "createdTime": "2024-01-01T00:00:00Z",
      "updatedTime": null,
      "updatedBy": null,
      "active": true
    },
    {
      "id": 2,
      "priceTypeName": "Child",
      "createdTime": "2024-01-01T00:00:00Z",
      "updatedTime": null,
      "updatedBy": null,
      "active": true
    }
  ]
}
```

### 2. Lấy loại giá theo ID

**GET** `/api/pricetype/{id}`

**Mô tả**: Lấy thông tin chi tiết của một loại giá

**Parameters**:
- `id` (int, required): ID của loại giá

**Response Success**:
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": 1,
    "priceTypeName": "Adult",
    "createdTime": "2024-01-01T00:00:00Z",
    "updatedTime": null,
    "updatedBy": null,
    "active": true
  }
}
```

**Response Error**:
```json
{
  "success": false,
  "message": "Price type not found"
}
```

### 3. Lấy danh sách loại giá có phân trang

**GET** `/api/pricetype/paged`

**Mô tả**: Lấy danh sách loại giá với phân trang

**Query Parameters**:
- `page` (int, optional): Số trang (default: 1)
- `pageSize` (int, optional): Kích thước trang (default: 10)

**Response**:
```json
{
  "items": [
    {
      "id": 1,
      "priceTypeName": "Adult",
      "createdTime": "2024-01-01T00:00:00Z",
      "updatedTime": null,
      "updatedBy": null,
      "active": true
    }
  ],
  "totalItems": 3,
  "totalPages": 1,
  "currentPage": 1,
  "pageSize": 10
}
```

### 4. Tạo loại giá mới

**POST** `/api/pricetype`

**Mô tả**: Tạo một loại giá mới

**Request Body**:
```json
{
  "priceTypeName": "Senior"
}
```

**Validation Rules**:
- `priceTypeName`: Required, max 255 characters, must be unique

**Response Success**:
```json
{
  "success": true,
  "message": "Price type created successfully",
  "data": {
    "id": 4,
    "priceTypeName": "Senior",
    "createdTime": "2024-01-15T10:30:00Z",
    "updatedTime": null,
    "updatedBy": null,
    "active": true
  }
}
```

**Response Error**:
```json
{
  "success": false,
  "message": "Price type name already exists"
}
```

### 5. Cập nhật loại giá

**PUT** `/api/pricetype/{id}`

**Mô tả**: Cập nhật thông tin loại giá

**Parameters**:
- `id` (int, required): ID của loại giá cần cập nhật

**Request Body**:
```json
{
  "priceTypeName": "Adult Updated",
  "updatedBy": "admin"
}
```

**Validation Rules**:
- `priceTypeName`: Required, max 255 characters, must be unique (excluding current record)
- `updatedBy`: Optional, max 100 characters

**Response Success**:
```json
{
  "success": true,
  "message": "Price type updated successfully",
  "data": {
    "id": 1,
    "priceTypeName": "Adult Updated",
    "createdTime": "2024-01-01T00:00:00Z",
    "updatedTime": "2024-01-15T10:30:00Z",
    "updatedBy": "admin",
    "active": true
  }
}
```

**Response Error**:
```json
{
  "success": false,
  "message": "Price type not found"
}
```

### 6. Xóa loại giá

**DELETE** `/api/pricetype/{id}`

**Mô tả**: Xóa loại giá (soft delete)

**Parameters**:
- `id` (int, required): ID của loại giá cần xóa

**Response Success**:
```json
{
  "success": true,
  "message": "Price type deleted successfully"
}
```

**Response Error**:
```json
{
  "success": false,
  "message": "Price type not found"
}
```

## Data Models

### CreatePriceTypeDto

```csharp
public class CreatePriceTypeDto
{
    [Required(ErrorMessage = "Price type name is required")]
    [StringLength(255, ErrorMessage = "Price type name cannot exceed 255 characters")]
    public string PriceTypeName { get; set; } = string.Empty;
}
```

### UpdatePriceTypeDto

```csharp
public class UpdatePriceTypeDto
{
    [Required(ErrorMessage = "Price type name is required")]
    [StringLength(255, ErrorMessage = "Price type name cannot exceed 255 characters")]
    public string PriceTypeName { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Updated by cannot exceed 100 characters")]
    public string? UpdatedBy { get; set; }
}
```

### PriceTypeDto

```csharp
public class PriceTypeDto
{
    public int Id { get; set; }
    public string PriceTypeName { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }
    public string? UpdatedBy { get; set; }
    public bool Active { get; set; }
}
```

## Error Handling

### Common Error Responses

#### 400 Bad Request
```json
{
  "success": false,
  "message": "Validation failed",
  "data": [
    "Price type name is required",
    "Price type name cannot exceed 255 characters"
  ]
}
```

#### 404 Not Found
```json
{
  "success": false,
  "message": "Price type not found"
}
```

#### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Error: [Error details]"
}
```

## Business Rules

### 1. Naming Convention
- Tên loại giá phải duy nhất trong hệ thống
- Không được trùng với tên loại giá khác (kể cả đã bị xóa)

### 2. Soft Delete
- Khi xóa loại giá, chỉ đánh dấu `Active = false`
- Không thực sự xóa dữ liệu khỏi database

### 3. Validation
- Tên loại giá không được để trống
- Tên loại giá không được vượt quá 255 ký tự
- Tên loại giá phải duy nhất

### 4. Audit Trail
- Tự động ghi nhận thời gian tạo (`CreatedTime`)
- Ghi nhận thời gian cập nhật (`UpdatedTime`) khi có thay đổi
- Ghi nhận người cập nhật (`UpdatedBy`) nếu được cung cấp

## Implementation Details

### Architecture
- **Controller**: `PriceTypeController` - Xử lý HTTP requests
- **Service**: `PriceTypeService` - Business logic và validation
- **Repository**: `PriceTypeRepository` - Data access layer
- **DTOs**: Data transfer objects cho input/output

### Manual Mapping
- Sử dụng mapping thủ công thay vì AutoMapper
- Mapping methods: `MapToPriceTypeDto`, `MapToPriceType`, `MapToPriceTypeForUpdate`
- Null-safe mapping với kiểm tra null values

### Database Operations
- Sử dụng Entity Framework Core
- Soft delete pattern với `Active` field
- Pagination support cho danh sách lớn

## Testing

### Unit Tests
- Test tất cả service methods
- Test mapping methods riêng biệt
- Mock dependencies (repository)

### Integration Tests
- Test API endpoints
- Test database operations
- Test validation rules

## Security Considerations

1. **Authentication**: Tất cả endpoints yêu cầu JWT token
2. **Authorization**: Cần kiểm tra quyền truy cập phù hợp
3. **Input Validation**: Validate tất cả input data
4. **XSS Protection**: Sanitize input data trước khi lưu database

## Performance Considerations

1. **Pagination**: Sử dụng pagination cho danh sách lớn
2. **Indexing**: Đảm bảo có index trên các trường thường query
3. **Caching**: Có thể cache danh sách loại giá (ít thay đổi)
4. **Async Operations**: Tất cả operations đều async

## Dependencies

### Required Services
- `IPriceTypeRepository` - Data access
- `IPriceTypeService` - Business logic

### Required Models
- `PriceType` - Entity model
- `SysDbContext` - Database context

### Required DTOs
- `CreatePriceTypeDto` - Input for creation
- `UpdatePriceTypeDto` - Input for updates
- `PriceTypeDto` - Output format

## Example Usage

### Create a new price type
```bash
curl -X POST "https://api.barefoot-travel.com/api/pricetype" \
  -H "Authorization: Bearer <jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "priceTypeName": "Senior Citizen"
  }'
```

### Get all price types
```bash
curl -X GET "https://api.barefoot-travel.com/api/pricetype" \
  -H "Authorization: Bearer <jwt-token>"
```

### Update a price type
```bash
curl -X PUT "https://api.barefoot-travel.com/api/pricetype/1" \
  -H "Authorization: Bearer <jwt-token>" \
  -H "Content-Type: application/json" \
  -d '{
    "priceTypeName": "Adult Updated",
    "updatedBy": "admin"
  }'
```

### Delete a price type
```bash
curl -X DELETE "https://api.barefoot-travel.com/api/pricetype/1" \
  -H "Authorization: Bearer <jwt-token>"
```

## Changelog

### Version 1.0.0
- Initial implementation
- CRUD operations for PriceType
- Manual mapping implementation
- Soft delete support
- Pagination support
- Comprehensive validation
