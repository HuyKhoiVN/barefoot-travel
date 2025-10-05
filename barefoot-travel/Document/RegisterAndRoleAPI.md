# Register and Role Management API Guide

## Overview

This guide covers the new Register functionality and Role CRUD operations added to the Barefoot Travel authentication system.

## New API Endpoints

### Authentication Endpoints

#### POST /api/auth/register
**Purpose**: Register a new user account

**Request Body**:
```json
{
  "username": "newuser",
  "fullName": "New User",
  "password": "password123",
  "email": "newuser@example.com",
  "phone": "+1234567890",
  "roleId": 2
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Account created successfully",
  "data": {
    "userId": 3,
    "username": "newuser",
    "name": "New User",
    "email": "newuser@example.com",
    "phone": "+1234567890",
    "roles": ["User"]
  }
}
```

**Error Responses**:
- `400 Bad Request`: Invalid input, username already exists, or validation errors

### Role Management Endpoints (Admin Only)

#### GET /api/role
**Purpose**: Get all roles

**Headers**: `Authorization: Bearer <admin_token>`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Roles retrieved successfully",
  "data": [
    {
      "id": 1,
      "roleName": "Admin",
      "description": "Administrator role with full access",
      "createdTime": "2024-01-01T00:00:00Z",
      "updatedTime": null,
      "updatedBy": null,
      "active": true
    },
    {
      "id": 2,
      "roleName": "User",
      "description": "Regular user role with limited access",
      "createdTime": "2024-01-01T00:00:00Z",
      "updatedTime": null,
      "updatedBy": null,
      "active": true
    }
  ]
}
```

#### GET /api/role/{id}
**Purpose**: Get role by ID

**Headers**: `Authorization: Bearer <admin_token>`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Role retrieved successfully",
  "data": {
    "id": 1,
    "roleName": "Admin",
    "description": "Administrator role with full access",
    "createdTime": "2024-01-01T00:00:00Z",
    "updatedTime": null,
    "updatedBy": null,
    "active": true
  }
}
```

#### POST /api/role
**Purpose**: Create new role

**Headers**: `Authorization: Bearer <admin_token>`

**Request Body**:
```json
{
  "roleName": "Manager",
  "description": "Manager role with moderate access"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Role created successfully",
  "data": {
    "id": 3,
    "roleName": "Manager",
    "description": "Manager role with moderate access",
    "createdTime": "2024-01-15T10:30:00Z",
    "updatedTime": null,
    "updatedBy": null,
    "active": true
  }
}
```

#### PUT /api/role/{id}
**Purpose**: Update existing role

**Headers**: `Authorization: Bearer <admin_token>`

**Request Body**:
```json
{
  "roleName": "Updated Manager",
  "description": "Updated manager role description",
  "active": true
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Role updated successfully",
  "data": {
    "id": 3,
    "roleName": "Updated Manager",
    "description": "Updated manager role description",
    "createdTime": "2024-01-15T10:30:00Z",
    "updatedTime": "2024-01-15T11:00:00Z",
    "updatedBy": null,
    "active": true
  }
}
```

#### DELETE /api/role/{id}
**Purpose**: Delete role (soft delete)

**Headers**: `Authorization: Bearer <admin_token>`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Role deleted successfully"
}
```

## Validation Rules

### Register Validation
- **Username**: Required, max 50 characters, must be unique
- **FullName**: Required, max 255 characters
- **Password**: Required, max 255 characters, will be hashed with BCrypt
- **Email**: Optional, max 255 characters
- **Phone**: Optional, max 20 characters
- **RoleId**: Required, must be valid role ID (defaults to 2 for User)

### Role Validation
- **RoleName**: Required, max 100 characters, must be unique
- **Description**: Optional, max 255 characters
- **Active**: Boolean, defaults to true

## Testing Examples

### Register New User
```bash
curl -X POST "https://localhost:7000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "fullName": "Test User",
    "password": "testpass123",
    "email": "test@example.com",
    "phone": "+1234567890",
    "roleId": 2
  }'
```

### Create New Role (Admin Only)
```bash
curl -X POST "https://localhost:7000/api/role" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "roleName": "Moderator",
    "description": "Moderator role for content management"
  }'
```

### Get All Roles (Admin Only)
```bash
curl -X GET "https://localhost:7000/api/role" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

### Update Role (Admin Only)
```bash
curl -X PUT "https://localhost:7000/api/role/3" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "roleName": "Updated Moderator",
    "description": "Updated moderator role",
    "active": true
  }'
```

### Delete Role (Admin Only)
```bash
curl -X DELETE "https://localhost:7000/api/role/3" \
  -H "Authorization: Bearer YOUR_ADMIN_TOKEN"
```

## Error Handling

### Common Error Responses

#### 400 Bad Request
```json
{
  "success": false,
  "message": "Username already exists"
}
```

#### 401 Unauthorized
```json
{
  "success": false,
  "message": "Unauthorized access"
}
```

#### 403 Forbidden
```json
{
  "success": false,
  "message": "Admin access required"
}
```

#### 404 Not Found
```json
{
  "success": false,
  "message": "Role not found"
}
```

## Security Features

### Register Security
- Passwords are automatically hashed using BCrypt
- Username uniqueness is enforced
- Input validation prevents injection attacks
- No email verification required (simplified approach)

### Role Management Security
- Only Admin users can manage roles
- Soft delete prevents data loss
- Role name uniqueness is enforced
- All operations are logged

## Database Impact

### New Tables
- No new tables created (uses existing Account and Role tables)

### Modified Operations
- Account table: New records via register endpoint
- Role table: Full CRUD operations for role management

### Data Integrity
- Foreign key relationships maintained
- Soft delete preserves data integrity
- Unique constraints enforced

## Best Practices

### For Registration
1. Always validate input on client side before sending
2. Use strong passwords (enforce on client side)
3. Store sensitive data securely
4. Implement rate limiting for registration attempts

### For Role Management
1. Only grant Admin role to trusted users
2. Document role purposes clearly
3. Regular audit of role assignments
4. Test role changes in development first

## Troubleshooting

### Common Issues

1. **Username Already Exists**
   - Check if username is already in use
   - Try a different username

2. **Role Name Already Exists**
   - Check existing roles
   - Use a different role name

3. **Invalid Role ID**
   - Ensure role ID exists and is active
   - Check role table for valid IDs

4. **Admin Access Required**
   - Ensure user has Admin role
   - Check JWT token validity

### Database Queries for Debugging

```sql
-- Check all accounts
SELECT Id, Username, FullName, Email, RoleId, Active, CreatedTime 
FROM Account 
WHERE Active = 1;

-- Check all roles
SELECT Id, RoleName, Description, Active, CreatedTime 
FROM Role 
WHERE Active = 1;

-- Check role assignments
SELECT a.Username, a.FullName, r.RoleName 
FROM Account a
JOIN Role r ON a.RoleId = r.Id
WHERE a.Active = 1 AND r.Active = 1;
```
