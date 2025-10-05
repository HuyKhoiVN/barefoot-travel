# Authentication Guide - Barefoot Travel System

## Overview

This guide explains the authentication and authorization system implemented in the Barefoot Travel .NET MVC 8.0 application. The system uses JWT (JSON Web Tokens) for authentication with role-based authorization supporting Admin and User roles.

**Important Notes:**
- This system follows the updated API Development Rules
- All validation is performed in the Service layer, not in Models
- Models are kept simple without Data Annotations
- The system uses the existing database schema without creating new tables
- Role-based access is implemented using RoleId directly from Account table

## Authentication Flow

### 1. Login Process
```
User → POST /api/auth/login → AuthService → JWT + RefreshToken → Client
```

1. User sends credentials (username, password) to `/api/auth/login`
2. System validates credentials using BCrypt password hashing
3. System retrieves user roles from database
4. System generates JWT access token (15 minutes expiry) and refresh token (7 days expiry)
5. Refresh token is stored in database
6. Both tokens are returned to client

### 2. Token Refresh Process
```
Client → POST /api/auth/refresh → AuthService → New JWT + New RefreshToken → Client
```

**Note**: Refresh token functionality is currently not implemented and will return a "Not Implemented" error. This can be implemented later based on specific requirements.

### 3. Logout Process
```
Client → POST /api/auth/logout → AuthService → Success
```

**Note**: Logout functionality is simplified and does not require database operations for token invalidation.

## API Endpoints

### Authentication Endpoints

#### POST /api/auth/login
**Purpose**: Authenticate user and get tokens

**Request Body**:
```json
{
  "username": "admin",
  "password": "password123"
}
```

**Response (200 OK)**:
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

**Error Responses**:
- `400 Bad Request`: Invalid input or validation errors
- `401 Unauthorized`: Invalid credentials

#### POST /api/auth/refresh
**Purpose**: Refresh access token using refresh token

**Request Body**:
```json
{
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890..."
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Token refresh successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "b2c3d4e5-f6g7-8901-bcde-f23456789012..."
  }
}
```

**Error Responses**:
- `400 Bad Request`: Invalid input
- `401 Unauthorized`: Invalid or expired refresh token

#### POST /api/auth/logout
**Purpose**: Logout and invalidate refresh token

**Headers**: `Authorization: Bearer <access_token>`

**Request Body**:
```json
{
  "refreshToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890..."
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Logout successful"
}
```

**Error Responses**:
- `400 Bad Request`: Invalid input
- `401 Unauthorized`: Invalid token

### User Management Endpoints

#### GET /api/user/profile
**Purpose**: Get current user profile

**Headers**: `Authorization: Bearer <access_token>`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Profile retrieved successfully",
  "data": {
    "userId": 1,
    "username": "admin",
    "name": "Administrator",
    "email": "admin@barefoottravel.com",
    "phone": "+1234567890",
    "roles": ["Admin"]
  }
}
```

**Error Responses**:
- `401 Unauthorized`: Invalid or missing token
- `404 Not Found`: User not found

#### GET /api/user/all
**Purpose**: Get all users (Admin only)

**Headers**: `Authorization: Bearer <access_token>`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Users retrieved successfully",
  "data": [
    {
      "userId": 1,
      "username": "admin",
      "name": "Administrator",
      "email": "admin@barefoottravel.com",
      "phone": "+1234567890",
      "roles": ["Admin"]
    },
    {
      "userId": 2,
      "username": "user1",
      "name": "John Doe",
      "email": "john@example.com",
      "phone": "+0987654321",
      "roles": ["User"]
    }
  ]
}
```

**Error Responses**:
- `401 Unauthorized`: Invalid or missing token
- `403 Forbidden`: Admin access required

## Setup Instructions

### 1. Database Configuration

Ensure your `appsettings.json` contains the database connection string:

```json
{
  "ConnectionStrings": {
    "DbConnection": "Server=localhost;Database=barefoot;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### 2. JWT Configuration

Add JWT settings to your `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "BarefootTravel",
    "Audience": "BarefootTravelUsers"
  }
}
```

**Important**: 
- Replace `SecretKey` with a strong, random key (at least 32 characters)
- Use different keys for different environments (Development, Production)
- Store production keys securely (Azure Key Vault, environment variables, etc.)

### 3. Database Migration

Run the following commands to create/update the database:

```bash
# Add migration for new entities
dotnet ef migrations add AddAuthenticationEntities

# Update database
dotnet ef database update
```

### 4. Seed Data

The system automatically seeds initial data when the application starts. The `DatabaseSeeder` class creates:

1. **Roles**: Admin (Id=1) and User (Id=2)
2. **Admin User**: username="admin", password="admin123", RoleId=1
3. **Sample User**: username="user", password="user123", RoleId=2

No additional setup is required as the seeding is handled automatically in `Program.cs`.

## Security Features

### 1. Password Security
- Passwords are hashed using BCrypt with salt
- No plain text passwords are stored
- Password verification uses BCrypt.Verify()

### 2. Token Security
- Access tokens expire in 15 minutes
- Refresh token functionality is not implemented (simplified approach)
- JWT tokens contain user ID and role information
- Tokens are validated on each request

### 3. Role-Based Authorization
- Two roles: Admin and User
- Admin can access all endpoints
- User has limited access
- Authorization is enforced at controller level using `[Authorize(Roles = "Admin,User")]`

### 4. Input Validation
- All validation is performed in Service layer
- Custom validation logic for business rules
- Custom exception handling for different error types
- No Data Annotations in Models (following API Development Rules)

## Error Handling

The system uses custom exceptions for different error scenarios:

- `BadRequestException`: Invalid input or validation errors (400)
- `UnauthorizedException`: Authentication failures (401)
- `NotFoundException`: Resource not found (404)
- `ForbiddenException`: Authorization failures (403)

All errors are wrapped in `ApiResponse` format for consistency.

## Testing the API

### Using Swagger UI
1. Run the application
2. Navigate to `/swagger`
3. Use the "Authorize" button to set Bearer token
4. Test the endpoints

### Using cURL

#### Login
```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

#### Get Profile
```bash
curl -X GET "https://localhost:7000/api/user/profile" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

#### Refresh Token
```bash
curl -X POST "https://localhost:7000/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "YOUR_REFRESH_TOKEN"
  }'
```

## Troubleshooting

### Common Issues

1. **JWT SecretKey not configured**
   - Ensure `JwtSettings:SecretKey` is set in appsettings.json
   - Key must be at least 32 characters long

2. **Database connection issues**
   - Check connection string in appsettings.json
   - Ensure SQL Server is running
   - Run database migrations

3. **Token validation errors**
   - Check JWT settings (Issuer, Audience)
   - Ensure system clock is synchronized
   - Verify token format and expiration

4. **Role authorization failures**
   - Ensure user has assigned roles in UserRole table
   - Check role names match exactly ("Admin", "User")
   - Verify user account is active

### Logging

The system includes comprehensive logging. Check application logs for:
- Authentication attempts
- Token generation and validation
- Authorization failures
- Database operations

## Best Practices

1. **Token Management**
   - Store tokens securely on client side
   - Implement automatic token refresh
   - Clear tokens on logout

2. **Security**
   - Use HTTPS in production
   - Rotate JWT secret keys regularly
   - Implement rate limiting for login endpoints
   - Monitor for suspicious activity

3. **Database**
   - Regular backups
   - Monitor refresh token table size
   - Clean up expired tokens periodically

4. **Development**
   - Use different JWT keys for different environments
   - Never commit secrets to source control
   - Test with different user roles
   - Implement proper error handling in client applications
