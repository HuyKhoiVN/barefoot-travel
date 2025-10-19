# Barefoot Travel API Documentation

## Overview
This document provides comprehensive API documentation for the Barefoot Travel Management System. The API is built using ASP.NET Core with JWT authentication and follows RESTful principles.

## Base URL
```
https://localhost:7000/api
```

## Authentication
The API uses JWT (JSON Web Token) authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

## Common Response Format
All API responses follow this structure:
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { ... },
  "errors": null
}
```

## Error Responses
- **400 Bad Request**: Invalid input data
- **401 Unauthorized**: Missing or invalid token
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server error

---

## Authentication APIs

### 1. Login
**POST** `/api/auth/login`

Authenticate user and get access token.

**Request Body:**
```json
{
  "username": "admin",
  "password": "password123"
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "password123"
  }'
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "expiresIn": 3600
  }
}
```

**Error Response (401):**
```json
{
  "success": false,
  "message": "Invalid credentials"
}
```

### 2. Register
**POST** `/api/auth/register`

Register a new user account.

**Request Body:**
```json
{
  "username": "newuser",
  "fullName": "New User",
  "password": "password123",
  "email": "user@example.com",
  "phone": "0123456789",
  "roleId": 2
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "newuser",
    "fullName": "New User",
    "password": "password123",
    "email": "user@example.com",
    "phone": "0123456789",
    "roleId": 2
  }'
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Account created successfully",
  "data": {
    "id": 1,
    "username": "newuser",
    "fullName": "New User",
    "email": "user@example.com",
    "phone": "0123456789",
    "roleId": 2,
    "active": true,
    "createdTime": "2024-01-01T00:00:00Z"
  }
}
```

### 3. Refresh Token
**POST** `/api/auth/refresh`

Refresh access token using refresh token.

**Request Body:**
```json
{
  "refreshToken": "refresh_token_here"
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "refresh_token_here"
  }'
```

### 4. Logout
**POST** `/api/auth/logout`

Logout and invalidate refresh token.

**Authorization:** Required (Admin, User)

**Request Body:**
```json
{
  "refreshToken": "refresh_token_here"
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/auth/logout" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "refreshToken": "refresh_token_here"
  }'
```

---

## User APIs

### 1. Get User Profile
**GET** `/api/auth/profile`

Get current user profile information.

**Authorization:** Required (Admin, User)

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/auth/profile" \
  -H "Authorization: Bearer <your-jwt-token>"
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Profile retrieved successfully",
  "data": {
    "id": 1,
    "username": "admin",
    "fullName": "Administrator",
    "email": "admin@example.com",
    "phone": "0123456789",
    "photo": null,
    "active": true,
    "roleId": 1,
    "roleName": "Admin",
    "createdTime": "2024-01-01T00:00:00Z"
  }
}
```

### 2. Get All Users
**GET** `/api/user/all`

Get all users (Admin only).

**Authorization:** Required (Admin)

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/user/all" \
  -H "Authorization: Bearer <your-jwt-token>"
```

---

## Tour Management APIs

### 1. Get All Tours
**GET** `/api/tour`

Get all tours.

**Authorization:** Required

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/tour" \
  -H "Authorization: Bearer <your-jwt-token>"
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Tours retrieved successfully",
  "data": [
    {
      "id": 1,
      "title": "Halong Bay Cruise",
      "description": "Beautiful cruise through Halong Bay",
      "pricePerPerson": 150.00,
      "maxPeople": 20,
      "duration": "2 days 1 night",
      "mapLink": "https://maps.google.com/...",
      "active": true,
      "createdTime": "2024-01-01T00:00:00Z",
      "images": [...],
      "categories": [...],
      "prices": [...],
      "policies": [...]
    }
  ]
}
```

### 2. Get Tour by ID
**GET** `/api/tour/{id}`

Get specific tour by ID.

**Authorization:** Required

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/tour/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

### 3. Get Tours with Pagination
**GET** `/api/tour/paged`

Get paginated list of tours with filtering and sorting.

**Authorization:** Required

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)
- `sortBy` (string): Sort field (title, pricePerPerson, createdTime)
- `sortOrder` (string): Sort direction (asc, desc)
- `categoryId` (int): Filter by category ID
- `active` (bool): Filter by active status

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/tour/paged?page=1&pageSize=10&sortBy=title&sortOrder=asc" \
  -H "Authorization: Bearer <your-jwt-token>"
```

### 4. Create Tour
**POST** `/api/tour`

Create a new tour.

**Authorization:** Required

**Request Body:**
```json
{
  "title": "New Tour",
  "description": "Tour description here",
  "mapLink": "https://maps.google.com/...",
  "pricePerPerson": 200.00,
  "maxPeople": 15,
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

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/tour" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "title": "New Tour",
    "description": "Tour description here",
    "pricePerPerson": 200.00,
    "maxPeople": 15,
    "duration": "3 days 2 nights",
    "categories": [1, 2],
    "prices": [{"priceTypeId": 1, "price": 200.00}],
    "policies": [1, 2]
  }'
```

### 5. Update Tour
**PUT** `/api/tour/{id}`

Update an existing tour.

**Authorization:** Required

**Request Body:**
```json
{
  "title": "Updated Tour Title",
  "description": "Updated description",
  "pricePerPerson": 250.00,
  "maxPeople": 20,
  "duration": "4 days 3 nights"
}
```

**cURL Example:**
```bash
curl -X PUT "https://localhost:7000/api/tour/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "title": "Updated Tour Title",
    "description": "Updated description",
    "pricePerPerson": 250.00,
    "maxPeople": 20,
    "duration": "4 days 3 nights"
  }'
```

### 6. Delete Tour
**DELETE** `/api/tour/{id}`

Delete a tour (soft delete).

**Authorization:** Required

**cURL Example:**
```bash
curl -X DELETE "https://localhost:7000/api/tour/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

### 7. Update Tour Status
**PUT** `/api/tour/{id}/status`

Update tour status (enable/disable).

**Authorization:** Required

**Request Body:**
```json
{
  "active": false
}
```

**cURL Example:**
```bash
curl -X PUT "https://localhost:7000/api/tour/1/status" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{"active": false}'
```

---

## Booking Management APIs

### 1. Get All Bookings
**GET** `/api/admin/booking`

Get all bookings with pagination and sorting.

**Authorization:** Required

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Page size (default: 10)
- `sortBy` (string): Sort field (default: CreatedTime)
- `sortDirection` (string): Sort direction: asc or desc (default: desc)

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/admin/booking?page=1&pageSize=10" \
  -H "Authorization: Bearer <your-jwt-token>"
```

**Success Response (200):**
```json
{
  "items": [
    {
      "id": 1,
      "tourId": 1,
      "tourTitle": "Halong Bay Cruise",
      "nameCustomer": "John Doe",
      "phoneNumber": "0123456789",
      "email": "john@example.com",
      "startDate": "2024-02-01T00:00:00Z",
      "people": 2,
      "totalPrice": 300.00,
      "paymentStatus": "Paid",
      "statusTypeId": 1,
      "statusName": "Confirmed",
      "note": "Customer request",
      "createdTime": "2024-01-15T10:30:00Z"
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

### 2. Get Booking by ID
**GET** `/api/admin/booking/{id}`

Get specific booking by ID.

**Authorization:** Required

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/admin/booking/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

### 3. Create Booking
**POST** `/api/admin/booking`

Create a new booking.

**Authorization:** Required

**Request Body:**
```json
{
  "tourId": 1,
  "startDate": "2024-02-01T00:00:00Z",
  "people": 2,
  "phoneNumber": "0123456789",
  "nameCustomer": "John Doe",
  "email": "john@example.com",
  "note": "Customer request",
  "paymentStatus": "Pending",
  "userId": 1
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/admin/booking" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "tourId": 1,
    "startDate": "2024-02-01T00:00:00Z",
    "people": 2,
    "phoneNumber": "0123456789",
    "nameCustomer": "John Doe",
    "email": "john@example.com",
    "paymentStatus": "Pending"
  }'
```

### 4. Update Booking Status
**PATCH** `/api/admin/booking/{id}/status`

Update booking status.

**Authorization:** Required

**Request Body:**
```json
{
  "statusTypeId": 2
}
```

**cURL Example:**
```bash
curl -X PATCH "https://localhost:7000/api/admin/booking/1/status" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{"statusTypeId": 2}'
```

### 5. Add Booking Note
**POST** `/api/admin/booking/{id}/note`

Add internal note to booking.

**Authorization:** Required

**Request Body:**
```json
{
  "note": "Customer called to confirm booking"
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/admin/booking/1/note" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{"note": "Customer called to confirm booking"}'
```

### 6. Get Booking Statuses
**GET** `/api/admin/booking/statuses`

Get all booking statuses.

**Authorization:** Required

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/admin/booking/statuses" \
  -H "Authorization: Bearer <your-jwt-token>"
```

### 7. Export Bookings
**POST** `/api/admin/booking/export`

Export bookings to Excel or PDF (Admin only).

**Authorization:** Required (Admin)

**Request Body:**
```json
{
  "exportFormat": "Excel",
  "startDate": "2024-01-01T00:00:00Z",
  "endDate": "2024-12-31T23:59:59Z",
  "statusTypeId": null,
  "tourId": null
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/admin/booking/export" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "exportFormat": "Excel",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-12-31T23:59:59Z"
  }'
```

---

## Category Management APIs

### 1. Get All Categories
**GET** `/api/category`

Get all categories.

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/category"
```

### 2. Get Category by ID
**GET** `/api/category/{id}`

Get specific category by ID.

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/category/1"
```

### 3. Get Categories with Pagination
**GET** `/api/category/paged`

Get paginated list of categories.

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)
- `sortBy` (string): Sort field
- `sortOrder` (string): Sort direction (asc, desc)
- `type` (string): Filter by type
- `active` (bool): Filter by active status

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/category/paged?page=1&pageSize=10"
```

### 4. Create Category
**POST** `/api/category`

Create a new category.

**Request Body:**
```json
{
  "categoryName": "Adventure Tours",
  "type": "Tour",
  "parentId": null,
  "enable": true
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/category" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "categoryName": "Adventure Tours",
    "type": "Tour",
    "enable": true
  }'
```

### 5. Update Category
**PUT** `/api/category/{id}`

Update an existing category.

**Request Body:**
```json
{
  "categoryName": "Updated Category Name",
  "type": "Tour",
  "parentId": null,
  "enable": true
}
```

**cURL Example:**
```bash
curl -X PUT "https://localhost:7000/api/category/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "categoryName": "Updated Category Name",
    "type": "Tour",
    "enable": true
  }'
```

### 6. Delete Category
**DELETE** `/api/category/{id}`

Delete a category.

**cURL Example:**
```bash
curl -X DELETE "https://localhost:7000/api/category/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

### 7. Update Category Status
**PATCH** `/api/category/{id}/status`

Update category status.

**Request Body:**
```json
false
```

**cURL Example:**
```bash
curl -X PATCH "https://localhost:7000/api/category/1/status" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d 'false'
```

---

## Policy Management APIs

### 1. Get All Policies
**GET** `/api/policy`

Get all policies.

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/policy"
```

### 2. Get Policy by ID
**GET** `/api/policy/{id}`

Get specific policy by ID.

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/policy/1"
```

### 3. Create Policy
**POST** `/api/policy`

Create a new policy.

**Request Body:**
```json
{
  "policyType": "Cancellation Policy",
  "content": "Free cancellation up to 24 hours before departure",
  "active": true
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/policy" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "policyType": "Cancellation Policy",
    "content": "Free cancellation up to 24 hours before departure",
    "active": true
  }'
```

### 4. Update Policy
**PUT** `/api/policy/{id}`

Update an existing policy.

**Request Body:**
```json
{
  "policyType": "Updated Policy Type",
  "content": "Updated policy content",
  "active": true
}
```

**cURL Example:**
```bash
curl -X PUT "https://localhost:7000/api/policy/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "policyType": "Updated Policy Type",
    "content": "Updated policy content",
    "active": true
  }'
```

### 5. Delete Policy
**DELETE** `/api/policy/{id}`

Delete a policy.

**cURL Example:**
```bash
curl -X DELETE "https://localhost:7000/api/policy/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

---

## Price Type Management APIs

### 1. Get All Price Types
**GET** `/api/pricetype`

Get all price types.

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/pricetype"
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Price types retrieved successfully",
  "data": [
    {
      "id": 1,
      "priceTypeName": "Adult",
      "active": true,
      "createdTime": "2024-01-01T00:00:00Z"
    },
    {
      "id": 2,
      "priceTypeName": "Child",
      "active": true,
      "createdTime": "2024-01-01T00:00:00Z"
    }
  ]
}
```

### 2. Get Price Type by ID
**GET** `/api/pricetype/{id}`

Get specific price type by ID.

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/pricetype/1"
```

### 3. Create Price Type
**POST** `/api/pricetype`

Create a new price type.

**Request Body:**
```json
{
  "priceTypeName": "Senior Citizen"
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/pricetype" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{"priceTypeName": "Senior Citizen"}'
```

### 4. Update Price Type
**PUT** `/api/pricetype/{id}`

Update an existing price type.

**Request Body:**
```json
{
  "priceTypeName": "Updated Price Type Name"
}
```

**cURL Example:**
```bash
curl -X PUT "https://localhost:7000/api/pricetype/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{"priceTypeName": "Updated Price Type Name"}'
```

### 5. Delete Price Type
**DELETE** `/api/pricetype/{id}`

Delete a price type.

**cURL Example:**
```bash
curl -X DELETE "https://localhost:7000/api/pricetype/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

---

## Role Management APIs

### 1. Get All Roles
**GET** `/api/role`

Get all roles (Admin only).

**Authorization:** Required (Admin)

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/role" \
  -H "Authorization: Bearer <your-jwt-token>"
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Roles retrieved successfully",
  "data": [
    {
      "id": 1,
      "roleName": "Admin",
      "description": "Administrator role",
      "active": true,
      "createdTime": "2024-01-01T00:00:00Z"
    },
    {
      "id": 2,
      "roleName": "User",
      "description": "Regular user role",
      "active": true,
      "createdTime": "2024-01-01T00:00:00Z"
    }
  ]
}
```

### 2. Get Role by ID
**GET** `/api/role/{id}`

Get specific role by ID (Admin only).

**Authorization:** Required (Admin)

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/role/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

### 3. Create Role
**POST** `/api/role`

Create a new role (Admin only).

**Authorization:** Required (Admin)

**Request Body:**
```json
{
  "roleName": "Manager",
  "description": "Manager role with limited admin access"
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/role" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "roleName": "Manager",
    "description": "Manager role with limited admin access"
  }'
```

### 4. Update Role
**PUT** `/api/role/{id}`

Update an existing role (Admin only).

**Authorization:** Required (Admin)

**Request Body:**
```json
{
  "roleName": "Updated Role Name",
  "description": "Updated role description"
}
```

**cURL Example:**
```bash
curl -X PUT "https://localhost:7000/api/role/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "roleName": "Updated Role Name",
    "description": "Updated role description"
  }'
```

### 5. Delete Role
**DELETE** `/api/role/{id}`

Delete a role (Admin only).

**Authorization:** Required (Admin)

**cURL Example:**
```bash
curl -X DELETE "https://localhost:7000/api/role/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

---

## Tour Image Management APIs

### 1. Create Tour Image
**POST** `/api/tour/image`

Create a new tour image.

**Authorization:** Required

**Request Body:**
```json
{
  "tourId": 1,
  "imageUrl": "https://example.com/image.jpg",
  "isBanner": true
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/tour/image" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "tourId": 1,
    "imageUrl": "https://example.com/image.jpg",
    "isBanner": true
  }'
```

### 2. Update Tour Image
**PUT** `/api/tour/image/{id}`

Update an existing tour image.

**Authorization:** Required

**Request Body:**
```json
{
  "imageUrl": "https://example.com/updated-image.jpg",
  "isBanner": false
}
```

**cURL Example:**
```bash
curl -X PUT "https://localhost:7000/api/tour/image/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "imageUrl": "https://example.com/updated-image.jpg",
    "isBanner": false
  }'
```

### 3. Delete Tour Image
**DELETE** `/api/tour/image/{id}`

Delete a tour image.

**Authorization:** Required

**cURL Example:**
```bash
curl -X DELETE "https://localhost:7000/api/tour/image/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

### 4. Get Tour Images
**GET** `/api/tour/image/tour/{tourId}`

Get all images for a specific tour.

**Authorization:** Required

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/tour/image/tour/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

---

## Tour Price Management APIs

### 1. Create Tour Price
**POST** `/api/tour/price`

Create a new tour price.

**Authorization:** Required

**Request Body:**
```json
{
  "tourId": 1,
  "priceTypeId": 1,
  "price": 200.00
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/tour/price" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "tourId": 1,
    "priceTypeId": 1,
    "price": 200.00
  }'
```

### 2. Update Tour Price
**PUT** `/api/tour/price/{id}`

Update an existing tour price.

**Authorization:** Required

**Request Body:**
```json
{
  "price": 250.00
}
```

**cURL Example:**
```bash
curl -X PUT "https://localhost:7000/api/tour/price/1" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{"price": 250.00}'
```

### 3. Delete Tour Price
**DELETE** `/api/tour/price/{id}`

Delete a tour price.

**Authorization:** Required

**cURL Example:**
```bash
curl -X DELETE "https://localhost:7000/api/tour/price/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

### 4. Get Tour Prices
**GET** `/api/tour/price/tour/{tourId}`

Get all prices for a specific tour.

**Authorization:** Required

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/tour/price/tour/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

---

## Tour Policy Management APIs

### 1. Create Tour Policy
**POST** `/api/tour/policy`

Create a new tour policy link.

**Authorization:** Required

**Request Body:**
```json
{
  "tourId": 1,
  "policyId": 1
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/tour/policy" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "tourId": 1,
    "policyId": 1
  }'
```

### 2. Delete Tour Policy
**DELETE** `/api/tour/policy/{id}`

Delete a tour policy link.

**Authorization:** Required

**cURL Example:**
```bash
curl -X DELETE "https://localhost:7000/api/tour/policy/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

### 3. Get Tour Policies
**GET** `/api/tour/policy/tour/{tourId}`

Get all policies for a specific tour.

**Authorization:** Required

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/tour/policy/tour/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

---

## Marketing Tag Management APIs

### 1. Add Marketing Tag
**POST** `/api/tour/{id}/marketing-tag`

Add marketing tag to tour.

**Authorization:** Required

**Request Body:**
```json
{
  "categoryId": 1
}
```

**cURL Example:**
```bash
curl -X POST "https://localhost:7000/api/tour/1/marketing-tag" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{"categoryId": 1}'
```

### 2. Remove Marketing Tag
**DELETE** `/api/tour/{id}/marketing-tag/{categoryId}`

Remove marketing tag from tour.

**Authorization:** Required

**cURL Example:**
```bash
curl -X DELETE "https://localhost:7000/api/tour/1/marketing-tag/1" \
  -H "Authorization: Bearer <your-jwt-token>"
```

### 3. Get Marketing Tags
**GET** `/api/tour/{id}/marketing-tags`

Get marketing tags for tour.

**Authorization:** Required

**cURL Example:**
```bash
curl -X GET "https://localhost:7000/api/tour/1/marketing-tags" \
  -H "Authorization: Bearer <your-jwt-token>"
```

---

## Tour Itinerary Management APIs

### 1. Update Tour Itinerary
**PUT** `/api/tour/{id}/itinerary`

Update tour itinerary.

**Authorization:** Required

**Request Body:**
```json
{
  "itineraryJson": "{\"day1\": \"Arrival and city tour\", \"day2\": \"Mountain hiking\", \"day3\": \"Departure\"}"
}
```

**cURL Example:**
```bash
curl -X PUT "https://localhost:7000/api/tour/1/itinerary" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{
    "itineraryJson": "{\"day1\": \"Arrival and city tour\", \"day2\": \"Mountain hiking\", \"day3\": \"Departure\"}"
  }'
```

---

## Error Handling

### Common Error Responses

**400 Bad Request:**
```json
{
  "success": false,
  "message": "Invalid input data",
  "errors": {
    "title": ["Title is required"],
    "pricePerPerson": ["Price must be greater than 0"]
  }
}
```

**401 Unauthorized:**
```json
{
  "success": false,
  "message": "Unauthorized access"
}
```

**403 Forbidden:**
```json
{
  "success": false,
  "message": "Access denied. Admin permission required."
}
```

**404 Not Found:**
```json
{
  "success": false,
  "message": "Resource not found"
}
```

**500 Internal Server Error:**
```json
{
  "success": false,
  "message": "An error occurred while processing your request"
}
```

---

## Rate Limiting

The API implements rate limiting to prevent abuse:
- **Authentication endpoints**: 5 requests per minute per IP
- **General API endpoints**: 100 requests per minute per user
- **Admin endpoints**: 200 requests per minute per admin user

---

## Versioning

The API uses URL versioning:
- Current version: `v1`
- Base URL: `https://localhost:7000/api`

---

## Support

For API support and questions, please contact:
- Email: support@barefoottravel.com
- Documentation: https://docs.barefoottravel.com
- Status Page: https://status.barefoottravel.com