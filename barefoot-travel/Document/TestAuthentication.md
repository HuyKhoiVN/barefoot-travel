# Authentication System Test Guide

## Prerequisites

1. Ensure the application is running
2. Database is seeded with initial data (automatic on startup)
3. Have access to Swagger UI or a REST client (Postman, cURL, etc.)

## Important Notes

- This system follows the updated API Development Rules
- All validation is performed in Service layer
- Models are kept simple without Data Annotations
- Refresh token functionality is not implemented (will return "Not Implemented" error)

## Test Credentials

The system creates two default users during seeding:

### Admin User
- **Username**: `admin`
- **Password**: `admin123`
- **Roles**: Admin
- **Access**: All endpoints

### Regular User
- **Username**: `user`
- **Password**: `user123`
- **Roles**: User
- **Access**: Limited endpoints

## Test Scenarios

### 1. Login Test

#### Test Admin Login
```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

**Expected Response**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890..."
  }
}
```

#### Test User Login
```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "user",
    "password": "user123"
  }'
```

### 2. Profile Access Test

#### Get Admin Profile
```bash
curl -X GET "https://localhost:7000/api/user/profile" \
  -H "Authorization: Bearer YOUR_ADMIN_ACCESS_TOKEN"
```

**Expected Response**:
```json
{
  "success": true,
  "message": "Profile retrieved successfully",
  "data": {
    "userId": 1,
    "username": "admin",
    "name": "System Administrator",
    "email": "admin@barefoottravel.com",
    "phone": "+1234567890",
    "roles": ["Admin"]
  }
}
```

#### Get User Profile
```bash
curl -X GET "https://localhost:7000/api/user/profile" \
  -H "Authorization: Bearer YOUR_USER_ACCESS_TOKEN"
```

### 3. Admin-Only Endpoint Test

#### Get All Users (Admin Only)
```bash
curl -X GET "https://localhost:7000/api/user/all" \
  -H "Authorization: Bearer YOUR_ADMIN_ACCESS_TOKEN"
```

**Expected Response**:
```json
{
  "success": true,
  "message": "Users retrieved successfully",
  "data": [
    {
      "userId": 1,
      "username": "admin",
      "name": "System Administrator",
      "email": "admin@barefoottravel.com",
      "phone": "+1234567890",
      "roles": ["Admin"]
    },
    {
      "userId": 2,
      "username": "user",
      "name": "John Doe",
      "email": "user@example.com",
      "phone": "+0987654321",
      "roles": ["User"]
    }
  ]
}
```

#### Test User Access to Admin Endpoint
```bash
curl -X GET "https://localhost:7000/api/user/all" \
  -H "Authorization: Bearer YOUR_USER_ACCESS_TOKEN"
```

**Expected Response**: `403 Forbidden` (User role cannot access Admin-only endpoints)

### 4. Token Refresh Test

#### Refresh Token (Not Implemented)
```bash
curl -X POST "https://localhost:7000/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

**Expected Response**: `400 Bad Request` with message "Refresh token functionality is not implemented"

### 5. Logout Test

#### Logout
```bash
curl -X POST "https://localhost:7000/api/auth/logout" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

**Expected Response**:
```json
{
  "success": true,
  "message": "Logout successful"
}
```

### 6. Error Handling Tests

#### Invalid Credentials
```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "wrongpassword"
  }'
```

**Expected Response**: `401 Unauthorized`

#### Invalid Token
```bash
curl -X GET "https://localhost:7000/api/user/profile" \
  -H "Authorization: Bearer invalid_token"
```

**Expected Response**: `401 Unauthorized`

#### Missing Token
```bash
curl -X GET "https://localhost:7000/api/user/profile"
```

**Expected Response**: `401 Unauthorized`

#### Expired Token
Wait for 15 minutes after login, then try to access a protected endpoint.

**Expected Response**: `401 Unauthorized`

## Using Swagger UI

1. Navigate to `https://localhost:7000/swagger`
2. Click "Authorize" button
3. Enter `Bearer YOUR_ACCESS_TOKEN` in the format: `Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`
4. Click "Authorize"
5. Test the endpoints using the Swagger interface

## Automated Testing Script

Create a PowerShell script for automated testing:

```powershell
# Test Authentication System
$baseUrl = "https://localhost:7000"

# Test 1: Admin Login
Write-Host "Testing Admin Login..."
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -ContentType "application/json" -Body '{"username":"admin","password":"admin123"}'
$adminToken = $loginResponse.data.accessToken
Write-Host "Admin Login: $($loginResponse.success)"

# Test 2: User Login
Write-Host "Testing User Login..."
$userLoginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -ContentType "application/json" -Body '{"username":"user","password":"user123"}'
$userToken = $userLoginResponse.data.accessToken
Write-Host "User Login: $($userLoginResponse.success)"

# Test 3: Get Admin Profile
Write-Host "Testing Admin Profile..."
$headers = @{ "Authorization" = "Bearer $adminToken" }
$profileResponse = Invoke-RestMethod -Uri "$baseUrl/api/user/profile" -Method GET -Headers $headers
Write-Host "Admin Profile: $($profileResponse.success)"

# Test 4: Get All Users (Admin Only)
Write-Host "Testing Get All Users (Admin)..."
$allUsersResponse = Invoke-RestMethod -Uri "$baseUrl/api/user/all" -Method GET -Headers $headers
Write-Host "Get All Users: $($allUsersResponse.success)"

# Test 5: Test User Access to Admin Endpoint
Write-Host "Testing User Access to Admin Endpoint..."
$userHeaders = @{ "Authorization" = "Bearer $userToken" }
try {
    $userAllUsersResponse = Invoke-RestMethod -Uri "$baseUrl/api/user/all" -Method GET -Headers $userHeaders
    Write-Host "User Access to Admin Endpoint: Should have failed but didn't"
} catch {
    Write-Host "User Access to Admin Endpoint: Correctly blocked (403 Forbidden)"
}

Write-Host "All tests completed!"
```

## Troubleshooting

### Common Issues

1. **Connection Refused**
   - Ensure the application is running
   - Check the port number (default: 7000 for HTTPS)

2. **Database Connection Error**
   - Verify connection string in appsettings.json
   - Ensure SQL Server is running
   - Check database exists

3. **JWT Configuration Error**
   - Verify JwtSettings in appsettings.json
   - Ensure SecretKey is at least 32 characters

4. **Token Validation Error**
   - Check token format (should start with "Bearer ")
   - Verify token hasn't expired
   - Ensure correct JWT settings

5. **Role Authorization Error**
   - Verify user has assigned roles in database
   - Check role names match exactly
   - Ensure user account is active

### Database Verification

Check the database tables to ensure data is seeded correctly:

```sql
-- Check roles
SELECT * FROM Role WHERE Active = 1;

-- Check accounts
SELECT Id, Username, FullName, Email, RoleId, Active FROM Account WHERE Active = 1;

-- Check user roles
SELECT ur.Id, ur.UserId, ur.RoleId, a.Username, r.RoleName 
FROM UserRole ur
JOIN Account a ON ur.UserId = a.Id
JOIN Role r ON ur.RoleId = r.Id
WHERE ur.Active = 1 AND a.Active = 1 AND r.Active = 1;
```

## Performance Testing

For load testing, you can use tools like:
- **Apache Bench (ab)**
- **JMeter**
- **Artillery**
- **k6**

Example with Apache Bench:
```bash
# Test login endpoint with 100 requests, 10 concurrent
ab -n 100 -c 10 -H "Content-Type: application/json" -p login.json https://localhost:7000/api/auth/login
```

Where `login.json` contains:
```json
{"username":"admin","password":"admin123"}
```
