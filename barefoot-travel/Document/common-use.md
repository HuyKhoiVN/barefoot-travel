# Phân tích kiến trúc dự án Barefoot Travel

## Tổng quan dự án
Dự án **Barefoot Travel** là một ứng dụng web ASP.NET MVC (.NET 8.0) được thiết kế để quản lý tour du lịch. Dự án sử dụng SQL Server làm cơ sở dữ liệu và có hệ thống xác thực JWT.

## Cấu hình Database

### Connection String
```json
"ConnectionStrings": {
  "DbConnection": "Data Source=HuyKhoiTuf\\SQLEXPRESS;Initial Catalog=barefoot;Integrated Security=True;Trust Server Certificate=True"
}
```

### Entity Framework DbContext
- **File**: `Models/SysDbContext.cs`
- **Chức năng**: Quản lý kết nối và mapping với SQL Server
- **Các bảng chính**:
  - `Account`: Quản lý tài khoản người dùng
  - `Tour`: Thông tin tour du lịch
  - `Booking`: Đặt tour
  - `Category`: Danh mục tour
  - `Role`, `Permission`, `RolePermission`: Hệ thống phân quyền
  - `CompanyInfo`: Thông tin công ty
  - `Policy`: Chính sách tour

## Cấu hình Authentication & Authorization

### JWT Configuration
```json
"Jwt": {
  "Key": "w(7bc!3NeaxUhPxCV#u4HdCP",
  "Issuer": "https://localhost:7244/",
  "ExpireTime": 1,
  "AdminExpireMinutes": 360
}
```

### CORS Configuration
```json
"CorsOrigins": [ "*" ]
```

**⚠️ Lưu ý bảo mật**: CORS được cấu hình cho phép tất cả origins (`"*"`), điều này không an toàn cho production.

## Phân tích thư mục Common/

Thư mục `Common/` chứa các file tiện ích và helper được sử dụng chung trong toàn bộ dự án:

### 1. ApiResponse.cs
**Chức năng**: Class chuẩn hóa response API
```csharp
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public object? Data { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
}
```
- **Mục đích**: Tạo cấu trúc response thống nhất cho tất cả API endpoints
- **Sử dụng**: Trả về kết quả API với format chuẩn

### 2. Constant.cs
**Chức năng**: Định nghĩa các hằng số role trong hệ thống
```csharp
public static class RoleConstant
{
    public const int ADMIN = 1;
    public const int USER = 2;
}
```
- **Mục đích**: Quản lý các role ID cố định
- **Sử dụng**: So sánh quyền hạn người dùng

### 3. FileUploadOperationFilter.cs
**Chức năng**: Custom filter cho Swagger UI để hỗ trợ upload file
```csharp
public class FileUploadOperationFilter : IOperationFilter
```
- **Mục đích**: Cấu hình Swagger để hiển thị đúng interface upload file
- **Sử dụng**: Khi API có tham số `IFormFile` hoặc `List<IFormFile>`
- **Tính năng**: Tự động detect và cấu hình multipart/form-data cho file upload

### 4. GetUserIdFromClaims.cs
**Chức năng**: Helper để lấy User ID từ JWT claims
```csharp
public static class GetUserIdFromClaims
{
    public static Guid GetUserId(ClaimsPrincipal user)
}
```
- **Mục đích**: Trích xuất User ID từ JWT token
- **Sử dụng**: Trong controllers để lấy thông tin user hiện tại
- **Xử lý lỗi**: Throw `UnauthorizedAccessException` nếu không tìm thấy User ID

### 5. HelpersAuth.cs
**Chức năng**: Các helper cho xác thực và OTP
```csharp
public static class HelpersAuth
{
    // Tạo OTP số ngẫu nhiên
    public static string GenerateNumericOtp(int length = 6)
    
    // Hash OTP bằng BCrypt
    public static string HashOtp(string otp, int workFactor = 10)
    
    // Verify OTP
    public static bool VerifyOtp(string inputOtp, string otpHash)
    
    // Tạo và hash OTP trong một lần
    public static string GenerateAndHashOtp(out string otp, int length = 6, int workFactor = 10)
    
    // Tạo thời gian hết hạn
    public static DateTime CreateExpiryUtc(int minutes = 10)
    
    // Kiểm tra OTP đã hết hạn
    public static bool IsExpired(DateTime? expiryUtc)
}
```
- **Mục đích**: Xử lý OTP (One-Time Password) cho xác thực 2FA
- **Bảo mật**: Sử dụng BCrypt để hash OTP
- **Sử dụng**: Đăng ký, đăng nhập, reset password

### 6. JwtMiddleware.cs
**Chức năng**: Middleware xử lý JWT authentication
```csharp
public class JwtMiddleware
{
    public async Task Invoke(HttpContext context)
    private bool ValidateToken(string token, out int userId)
}
```
- **Mục đích**: Tự động validate JWT token trong mỗi request
- **Hoạt động**: 
  - Lấy token từ header `Authorization`
  - Validate token và extract User ID
  - Lưu User ID vào `context.Items["UserId"]`
- **Sử dụng**: Middleware pipeline để bảo vệ API endpoints

### 7. GhnOptions.cs
**Chức năng**: Cấu hình cho dịch vụ Giao Hàng Nhanh (GHN)
```csharp
public class GhnOptions
{
    public string BaseUrl { get; set; } = "";
    public string Token { get; set; } = "";
    public string ShopId { get; set; } = "";
    public int FromDistrictId { get; set; }
    public string FromWardCode { get; set; } = "";
}
```
- **Mục đích**: Cấu hình tích hợp với API GHN
- **Sử dụng**: Tính phí vận chuyển, tạo đơn hàng giao hàng

## Phân tích Program.cs

### ✅ Cấu hình đã được hoàn thiện

```csharp
// Database Configuration
builder.Services.AddDbContext<SysDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnection")));

// JWT Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        if (corsOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(corsOrigins);
        }
        policy.AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger/OpenAPI Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Barefoot Travel API", 
        Version = "v1",
        Description = "API for Barefoot Travel Management System"
    });
    
    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    // File Upload Support
    c.OperationFilter<FileUploadOperationFilter>();
});

// AutoMapper & FluentValidation
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
```

### ✅ Pipeline đã được cấu hình đúng thứ tự

```csharp
// CORS must be before UseRouting
app.UseCors("AllowSpecificOrigins");

app.UseRouting();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Custom JWT Middleware
app.UseMiddleware<JwtMiddleware>();
```

### ✅ Các vấn đề đã được khắc phục

1. **✅ Database Context** - Đã đăng ký `SysDbContext` với SQL Server
2. **✅ JWT Authentication** - Đã cấu hình JWT Bearer authentication đầy đủ
3. **✅ CORS Policy** - Đã cấu hình CORS với policy linh hoạt
4. **✅ JWT Middleware** - Đã đăng ký `JwtMiddleware` vào pipeline
5. **✅ Swagger/OpenAPI** - Đã cấu hình Swagger với JWT support và file upload
6. **✅ AutoMapper & FluentValidation** - Đã cấu hình các thư viện hỗ trợ

## Dependencies (NuGet Packages)

### Packages đã cài đặt
- **Entity Framework**: `Microsoft.EntityFrameworkCore.SqlServer` (8.0.13)
- **JWT Authentication**: `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.13)
- **Password Hashing**: `BCrypt.Net-Next` (4.0.3)
- **API Documentation**: `Swashbuckle.AspNetCore` (7.3.0)
- **Object Mapping**: `AutoMapper.Extensions.Microsoft.DependencyInjection` (12.0.0)
- **Validation**: `FluentValidation.AspNetCore` (11.3.0)
- **Cloud Storage**: `Azure.Storage.Blobs` (12.24.0)

## Khuyến nghị cải thiện tiếp theo

### ✅ Đã hoàn thành
1. **✅ Cấu hình Program.cs** - Đã hoàn thiện tất cả cấu hình cần thiết
2. **✅ JWT Authentication** - Đã cấu hình đầy đủ JWT Bearer authentication
3. **✅ CORS Policy** - Đã cấu hình CORS linh hoạt
4. **✅ Swagger Integration** - Đã tích hợp Swagger với JWT support
5. **✅ AutoMapper & FluentValidation** - Đã cấu hình các thư viện hỗ trợ

### 🔄 Cần cải thiện tiếp theo

1. **Bảo mật nâng cao**:
   - Sử dụng User Secrets cho JWT key trong development
   - Implement refresh token mechanism
   - Thêm rate limiting cho API endpoints
   - Cấu hình CORS cụ thể cho production (thay vì `"*"`)

2. **Logging & Monitoring**:
   - Thêm Serilog cho structured logging
   - Cấu hình health checks
   - Implement application insights

3. **Kiến trúc nâng cao**:
   - Implement Repository pattern
   - Thêm Service layer
   - Implement proper error handling middleware
   - Thêm caching (Redis/Memory Cache)

4. **Database**:
   - Tạo migration scripts
   - Cấu hình connection string cho các environment khác nhau
   - Implement database seeding
   - Thêm database backup strategy

5. **API Documentation**:
   - Thêm XML comments cho controllers
   - Cấu hình Swagger examples
   - Thêm API versioning

### 🚀 Cách sử dụng sau khi cấu hình

1. **Chạy ứng dụng**: `dotnet run`
2. **Truy cập Swagger**: `https://localhost:7244/swagger`
3. **Test JWT Authentication**: Sử dụng Swagger UI để test API với JWT
4. **Database**: Entity Framework sẽ tự động tạo database khi chạy lần đầu
