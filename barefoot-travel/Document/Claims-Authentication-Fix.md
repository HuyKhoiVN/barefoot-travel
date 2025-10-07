# Claims Authentication Fix

## Vấn đề phát hiện

Trong quá trình kiểm tra logic authentication, phát hiện các vấn đề sau:

### 1. **Thiếu Username Claim trong JWT Token**
- `AuthService.cs` chỉ tạo các claims: `NameIdentifier`, `Name`, `sub`, `Role`
- **Thiếu claim `username`** cần thiết cho các operations

### 2. **Logic GetAdminUsername() không nhất quán**
- `TourController.GetAdminUsername()` tìm claim `"username"` nhưng token không có
- Các controller khác sử dụng `GetUserIdFromClaims.GetUserId()` thay vì username

### 3. **Inconsistent Claims Usage**
- Một số nơi cần `userId`, một số nơi cần `username`
- Logic không thống nhất giữa các controller

## Giải pháp đã thực hiện

### 1. **Cập nhật AuthService.cs**

#### Trước:
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
    new Claim(ClaimTypes.Name, userId.ToString()),
    new Claim("sub", userId.ToString())
};
```

#### Sau:
```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
    new Claim(ClaimTypes.Name, userId.ToString()),
    new Claim("sub", userId.ToString()),
    new Claim("username", user.Username)  // ✅ Thêm username claim
};
```

### 2. **Mở rộng GetUserIdFromClaims.cs**

#### Thêm method mới:
```csharp
public static string GetUsername(ClaimsPrincipal user)
{
    var usernameClaim = user.FindFirst("username")?.Value;

    if (string.IsNullOrEmpty(usernameClaim))
    {
        throw new UnauthorizedAccessException("Không thể xác định Username.");
    }

    return usernameClaim;
}
```

### 3. **Cập nhật TourController.cs**

#### Trước:
```csharp
private string GetAdminUsername()
{
    var usernameClaim = User.FindFirst("username")?.Value;
    if (string.IsNullOrEmpty(usernameClaim))
    {
        throw new UnauthorizedAccessException("Username not found in token");
    }
    return usernameClaim;
}
```

#### Sau:
```csharp
private string GetAdminUsername()
{
    return GetUserIdFromClaims.GetUsername(User);
}
```

### 4. **Cập nhật CategoryController và PolicyController**

#### Trước:
```csharp
var adminUsername = GetUserIdFromClaims.GetUserId(HttpContext);
```

#### Sau:
```csharp
var adminUsername = GetUserIdFromClaims.GetUsername(HttpContext.User);
```

## JWT Token Claims Structure

### Sau khi fix, JWT token sẽ chứa:

```json
{
  "sub": "123",
  "name": "123", 
  "nameid": "123",
  "username": "admin_user",  // ✅ Mới thêm
  "role": "Admin"
}
```

### Claims mapping:
- `sub`: User ID (string)
- `name`: User ID (string) 
- `nameid`: User ID (string)
- `username`: Username (string) ✅ **Mới thêm**
- `role`: Role name (string)

## Lợi ích của việc fix

### 1. **Consistency**
- Tất cả controllers sử dụng cùng một logic để lấy username
- Centralized claims handling trong `GetUserIdFromClaims`

### 2. **Maintainability**
- Dễ dàng thay đổi logic claims trong một nơi
- Consistent error handling

### 3. **Security**
- Proper validation cho tất cả claims
- Clear error messages khi claims không tồn tại

### 4. **Flexibility**
- Có thể lấy cả `userId` và `username` từ token
- Support cho cả user operations và admin operations

## Testing

### Test Cases cần kiểm tra:

1. **Login với username claim**
   ```csharp
   // Verify JWT token contains username claim
   var token = await authService.LoginAsync("admin", "password");
   var claims = jwtHandler.ReadJwtToken(token.AccessToken);
   Assert.Contains(claims.Claims, c => c.Type == "username" && c.Value == "admin");
   ```

2. **GetUsername từ claims**
   ```csharp
   // Verify GetUsername method works correctly
   var username = GetUserIdFromClaims.GetUsername(claimsPrincipal);
   Assert.Equal("admin", username);
   ```

3. **Controller operations**
   ```csharp
   // Verify controllers can get username for operations
   var result = await tourController.CreateTour(createDto);
   // Should not throw UnauthorizedAccessException
   ```

## Migration Notes

### Cho existing users:
- Existing JWT tokens sẽ không có `username` claim
- Cần re-login để có token mới với `username` claim
- Hoặc implement backward compatibility

### Cho new users:
- Tất cả JWT tokens mới sẽ có đầy đủ claims
- Controllers sẽ hoạt động bình thường

## Best Practices

### 1. **Claims Naming Convention**
- Sử dụng lowercase cho custom claims: `"username"`
- Sử dụng standard claims khi có thể: `ClaimTypes.NameIdentifier`

### 2. **Error Handling**
- Luôn validate claims trước khi sử dụng
- Provide clear error messages

### 3. **Security**
- Không expose sensitive information trong claims
- Validate claims trên server side

## Kết luận

Việc fix này đảm bảo:
- ✅ JWT tokens chứa đầy đủ thông tin cần thiết
- ✅ Controllers có thể lấy username một cách nhất quán
- ✅ Code dễ maintain và extend
- ✅ Proper error handling và validation
