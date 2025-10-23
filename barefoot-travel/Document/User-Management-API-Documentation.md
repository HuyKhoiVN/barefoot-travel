# User Management API Documentation

## Tổng quan

Tài liệu này mô tả các API endpoints cho quản lý người dùng trong hệ thống Barefoot Travel. Tất cả các API đều tuân thủ chuẩn RESTful và SOLID principles theo quy tắc trong `API-Development-Rules.md`.

## Base URL
```
/api/user
```

## Authentication
Tất cả các endpoints đều yêu cầu authentication với JWT token, trừ endpoint `/profile` có thể truy cập bởi cả Admin và User.

## Endpoints

### 1. User CRUD Operations

#### 1.1 Get User by ID
```http
GET /api/user/{id}
Authorization: Bearer {token}
Roles: Admin
```

**Response:**
```json
{
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "userId": 1,
    "username": "admin",
    "name": "Administrator",
    "email": "admin@example.com",
    "phone": "0123456789",
    "roles": ["Admin"]
  }
}
```

#### 1.2 Get All Users
```http
GET /api/user
Authorization: Bearer {token}
Roles: Admin
```

**Response:**
```json
{
  "success": true,
  "message": "Users retrieved successfully",
  "data": [
    {
      "userId": 1,
      "username": "admin",
      "name": "Administrator",
      "email": "admin@example.com",
      "phone": "0123456789",
      "roles": ["Admin"]
    }
  ]
}
```

#### 1.3 Get Users with Pagination
```http
GET /api/user/paged?page=1&pageSize=10&sortBy=username&sortOrder=asc&search=admin&active=true
Authorization: Bearer {token}
Roles: Admin
```

**Query Parameters:**
- `page` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 10)
- `sortBy` (string, optional): Sort field (username, fullName, email, createdTime, roleId)
- `sortOrder` (string, optional): Sort direction (asc, desc, default: asc)
- `search` (string, optional): Search by username, full name, email, or phone
- `active` (bool, optional): Filter by active status

**Response:**
```json
{
  "items": [
    {
      "userId": 1,
      "username": "admin",
      "name": "Administrator",
      "email": "admin@example.com",
      "phone": "0123456789",
      "roles": ["Admin"]
    }
  ],
  "totalItems": 1,
  "totalPages": 1,
  "currentPage": 1,
  "pageSize": 10
}
```

#### 1.4 Create User
```http
POST /api/user
Authorization: Bearer {token}
Roles: Admin
Content-Type: application/json
```

**Request Body:**
```json
{
  "username": "newuser",
  "fullName": "New User",
  "password": "password123",
  "email": "newuser@example.com",
  "phone": "0987654321",
  "roleId": 2,
  "photo": "photo_url"
}
```

**Response:**
```json
{
  "success": true,
  "message": "User created successfully",
  "data": {
    "userId": 2,
    "username": "newuser",
    "name": "New User",
    "email": "newuser@example.com",
    "phone": "0987654321",
    "roles": ["User"]
  }
}
```

#### 1.5 Update User
```http
PUT /api/user/{id}
Authorization: Bearer {token}
Roles: Admin
Content-Type: application/json
```

**Request Body:**
```json
{
  "fullName": "Updated User",
  "email": "updated@example.com",
  "phone": "0987654321",
  "roleId": 2,
  "photo": "new_photo_url",
  "active": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "User updated successfully",
  "data": {
    "userId": 2,
    "username": "newuser",
    "name": "Updated User",
    "email": "updated@example.com",
    "phone": "0987654321",
    "roles": ["User"]
  }
}
```

#### 1.6 Delete User (Soft Delete)
```http
DELETE /api/user/{id}
Authorization: Bearer {token}
Roles: Admin
```

**Response:**
```json
{
  "success": true,
  "message": "User deleted successfully"
}
```

### 2. User Status Operations

#### 2.1 Update User Status
```http
PUT /api/user/{id}/status
Authorization: Bearer {token}
Roles: Admin
Content-Type: application/json
```

**Request Body:**
```json
{
  "active": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "User deactivated successfully"
}
```

### 3. Password Operations

#### 3.1 Change Password
```http
PUT /api/user/{id}/password
Authorization: Bearer {token}
Roles: Admin
Content-Type: application/json
```

**Request Body:**
```json
{
  "currentPassword": "oldpassword",
  "newPassword": "newpassword123",
  "confirmPassword": "newpassword123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Password changed successfully"
}
```

### 4. Profile Operations

#### 4.1 Get Current User Profile
```http
GET /api/user/profile
Authorization: Bearer {token}
Roles: Admin, User
```

**Response:**
```json
{
  "success": true,
  "message": "Profile retrieved successfully",
  "data": {
    "userId": 1,
    "username": "admin",
    "name": "Administrator",
    "email": "admin@example.com",
    "phone": "0123456789",
    "roles": ["Admin"]
  }
}
```

## Data Transfer Objects (DTOs)

### CreateUserDto
```csharp
public class CreateUserDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    [Range(1, 2)]
    public int RoleId { get; set; } = 2;

    public string? Photo { get; set; }
}
```

### UpdateUserDto
```csharp
public class UpdateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; }

    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Required]
    [Range(1, 2)]
    public int RoleId { get; set; }

    public string? Photo { get; set; }

    [Required]
    public bool Active { get; set; }
}
```

### ChangePasswordDto
```csharp
public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; }

    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; }
}
```

### UserStatusDto
```csharp
public class UserStatusDto
{
    [Required]
    public bool Active { get; set; }
}
```

### UserProfileDto
```csharp
public class UserProfileDto
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public List<string> Roles { get; set; }
}
```

## Error Responses

### Validation Error (400)
```json
{
  "success": false,
  "message": "Validation failed",
  "data": [
    "Username is required",
    "Password must be between 6 and 100 characters"
  ]
}
```

### Not Found (404)
```json
{
  "success": false,
  "message": "User not found"
}
```

### Unauthorized (401)
```json
{
  "success": false,
  "message": "Invalid user token"
}
```

### Forbidden (403)
```json
{
  "success": false,
  "message": "Access denied. Admin role required."
}
```

### Server Error (500)
```json
{
  "success": false,
  "message": "An error occurred while processing the request"
}
```

## Implementation Details

### Architecture Compliance
- **SOLID Principles**: Tuân thủ Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **RESTful Design**: Sử dụng HTTP methods phù hợp (GET, POST, PUT, DELETE)
- **Manual Mapping**: Sử dụng mapping thủ công thay vì AutoMapper
- **Validation**: Validation được thực hiện ở Service layer
- **Error Handling**: Sử dụng custom exceptions và ApiResponse pattern
- **Logging**: Structured logging với Serilog

### Security Features
- **Authentication**: JWT token-based authentication
- **Authorization**: Role-based access control (Admin/User)
- **Password Hashing**: BCrypt for password security
- **Soft Delete**: Không xóa thật dữ liệu, chỉ đánh dấu inactive

### Performance Features
- **Pagination**: Hỗ trợ phân trang cho danh sách users
- **Search**: Tìm kiếm theo username, full name, email, phone
- **Sorting**: Sắp xếp theo nhiều trường khác nhau
- **Filtering**: Lọc theo trạng thái active/inactive

## Testing

### Unit Tests
- Test tất cả service methods
- Mock dependencies (repositories, external services)
- Test success và error scenarios
- Test mapping methods riêng biệt

### Integration Tests
- Test API endpoints
- Test database operations
- Test authentication/authorization

## Dependencies

### Required Packages
- `BCrypt.Net-Next` (4.0.3): Password hashing
- `Microsoft.EntityFrameworkCore`: Database operations
- `Microsoft.AspNetCore.Authorization`: Authorization
- `Microsoft.AspNetCore.Mvc`: API controllers

### Service Registration
```csharp
// In Program.cs
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IUserService, UserService>();
```

## Notes

1. **Role Mapping**: RoleId 1 = Admin, RoleId 2 = User
2. **Soft Delete**: User deletion chỉ đánh dấu Active = false
3. **Password Security**: Sử dụng BCrypt để hash password
4. **Audit Trail**: Tracking CreatedBy, UpdatedBy, CreatedTime, UpdatedTime
5. **Validation**: Tất cả validation được thực hiện ở Service layer
6. **Manual Mapping**: Không sử dụng AutoMapper, mapping thủ công để có control tốt hơn
