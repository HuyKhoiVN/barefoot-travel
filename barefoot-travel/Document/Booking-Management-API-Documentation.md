# Booking Management API Documentation

## Overview

This document describes the detailed logic for the Booking Management module APIs in the Barefoot Travel System. The module supports viewing, filtering, updating booking status, adding notes, and exporting booking reports.

## API Endpoints

All endpoints are prefixed with `/api/admin/booking` and require JWT authentication with Admin or User role.

### 1. Get Bookings (US12 - View Booking List)

**Endpoint**: `GET /api/admin/booking`

**Purpose**: Retrieve paginated list of bookings with sorting

**Parameters**:
- `page` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Page size (default: 10)
- `sortBy` (string, optional): Sort field - "StartDate", "CreatedTime", "TotalPrice", "NameCustomer" (default: "CreatedTime")
- `sortDirection` (string, optional): Sort direction - "asc" or "desc" (default: "desc")

**Logic Flow**:
1. **Authentication**: Validate JWT token and extract user role
2. **Authorization**: Check if user has Admin or User role
3. **Validation**: Validate pagination parameters (page > 0, pageSize 1-100)
4. **Database Query**: 
   ```sql
   SELECT * FROM Booking 
   WHERE Active = 1 
   ORDER BY {sortBy} {sortDirection}
   OFFSET {(page-1) * pageSize} ROWS 
   FETCH NEXT {pageSize} ROWS ONLY
   ```
5. **Related Data Loading**: Include Tour, Account, and BookingStatus data
6. **Mapping**: Convert to BookingDto with related information
7. **Response**: Return PagedResult<BookingDto>

**Optimization**: Uses existing indexes on Booking.Active, Booking.CreatedTime, Booking.StartDate

### 2. Filter Bookings (US13 - Filter Bookings)

**Endpoint**: `POST /api/admin/booking/filter`

**Purpose**: Retrieve filtered bookings with advanced search criteria

**Request Body**: BookingFilterDto
```json
{
  "statusTypeId": 1,
  "tourId": 5,
  "userId": 10,
  "startDateFrom": "2024-01-01",
  "startDateTo": "2024-12-31",
  "createdTimeFrom": "2024-01-01T00:00:00Z",
  "createdTimeTo": "2024-12-31T23:59:59Z",
  "phoneNumber": "0123456789",
  "nameCustomer": "John Doe",
  "email": "john@example.com",
  "paymentStatus": "Paid",
  "page": 1,
  "pageSize": 10,
  "sortBy": "CreatedTime",
  "sortDirection": "desc"
}
```

**Logic Flow**:
1. **Authentication & Authorization**: Same as Get Bookings
2. **Validation**: Validate all filter parameters
3. **Dynamic Query Building**:
   ```sql
   SELECT * FROM Booking 
   WHERE Active = 1
   AND (@StatusTypeId IS NULL OR StatusTypeId = @StatusTypeId)
   AND (@TourId IS NULL OR TourId = @TourId)
   AND (@UserId IS NULL OR UserId = @UserId)
   AND (@StartDateFrom IS NULL OR StartDate >= @StartDateFrom)
   AND (@StartDateTo IS NULL OR StartDate <= @StartDateTo)
   AND (@CreatedTimeFrom IS NULL OR CreatedTime >= @CreatedTimeFrom)
   AND (@CreatedTimeTo IS NULL OR CreatedTime <= @CreatedTimeTo)
   AND (@PhoneNumber IS NULL OR PhoneNumber LIKE '%' + @PhoneNumber + '%')
   AND (@NameCustomer IS NULL OR NameCustomer LIKE '%' + @NameCustomer + '%')
   AND (@Email IS NULL OR Email LIKE '%' + @Email + '%')
   AND (@PaymentStatus IS NULL OR PaymentStatus LIKE '%' + @PaymentStatus + '%')
   ORDER BY {sortBy} {sortDirection}
   OFFSET {(page-1) * pageSize} ROWS 
   FETCH NEXT {pageSize} ROWS ONLY
   ```
4. **Related Data Loading**: Include Tour, Account, and BookingStatus
5. **Mapping & Response**: Same as Get Bookings

**Optimization**: Uses indexes on StatusTypeId, TourId, UserId, StartDate, CreatedTime

### 3. Get Booking by ID

**Endpoint**: `GET /api/admin/booking/{id}`

**Purpose**: Retrieve specific booking details

**Logic Flow**:
1. **Authentication & Authorization**: Same as above
2. **Validation**: Validate booking ID is positive integer
3. **Database Query**:
   ```sql
   SELECT b.*, t.Title, a.FullName, bs.StatusName
   FROM Booking b
   LEFT JOIN Tour t ON b.TourId = t.Id AND t.Active = 1
   LEFT JOIN Account a ON b.UserId = a.Id AND a.Active = 1
   LEFT JOIN BookingStatus bs ON b.StatusTypeId = bs.Id AND bs.Active = 1
   WHERE b.Id = @id AND b.Active = 1
   ```
4. **Response**: Return ApiResponse with BookingDto or error message

### 4. Update Booking Status (US14 - Update Booking Status)

**Endpoint**: `PATCH /api/admin/booking/{id}/status`

**Purpose**: Update booking status with optional note

**Request Body**: UpdateBookingStatusDto
```json
{
  "statusTypeId": 2,
  "note": "Status updated after customer confirmation"
}
```

**Logic Flow**:
1. **Authentication & Authorization**: Same as above
2. **Transaction Start**: Begin database transaction
3. **Validation**:
   - Validate booking exists and is active
   - Validate status exists in BookingStatus table
   - Validate status transition is allowed (business rules)
4. **Status Update**:
   ```sql
   UPDATE Booking 
   SET StatusTypeId = @StatusTypeId,
       UpdatedTime = GETDATE(),
       UpdatedBy = @UpdatedBy
   WHERE Id = @Id AND Active = 1
   ```
5. **Note Append** (if provided):
   ```sql
   UPDATE Booking 
   SET Note = CONCAT(ISNULL(Note, ''), @NewNote)
   WHERE Id = @Id
   ```
6. **Transaction Commit**: Commit changes
7. **Response**: Return success/error message

**Business Rules**:
- Only Admin can change status to "Cancelled"
- Status transitions must follow business logic
- All status changes are logged with timestamp and user

### 5. Add Booking Note (US15 - Add Internal Notes)

**Endpoint**: `POST /api/admin/booking/{id}/note`

**Purpose**: Add internal note to booking

**Request Body**: AddBookingNoteDto
```json
{
  "note": "Customer requested early check-in"
}
```

**Logic Flow**:
1. **Authentication & Authorization**: Same as above
2. **Transaction Start**: Begin database transaction
3. **Validation**:
   - Validate booking exists and is active
   - Validate note is not empty or whitespace
4. **Note Append**:
   ```sql
   UPDATE Booking 
   SET Note = CONCAT(ISNULL(Note, ''), @NewNote),
       UpdatedTime = GETDATE(),
       UpdatedBy = @UpdatedBy
   WHERE Id = @Id AND Active = 1
   ```
5. **Transaction Commit**: Commit changes
6. **Response**: Return success/error message

**Note Format**: `\n[YYYY-MM-DD HH:mm:ss] Note added by {username}: {note}`

### 6. Get Booking Statuses

**Endpoint**: `GET /api/admin/booking/statuses`

**Purpose**: Retrieve all available booking statuses

**Logic Flow**:
1. **Authentication & Authorization**: Same as above
2. **Database Query**:
   ```sql
   SELECT Id, StatusName 
   FROM BookingStatus 
   WHERE Active = 1 
   ORDER BY Id
   ```
3. **Response**: Return list of statuses

### 7. Export Bookings (US16 - Export Booking Report)

**Endpoint**: `POST /api/admin/booking/export`

**Purpose**: Export filtered bookings to Excel or PDF

**Request Body**: ExportBookingDto
```json
{
  "startDateFrom": "2024-01-01",
  "startDateTo": "2024-12-31",
  "createdTimeFrom": "2024-01-01T00:00:00Z",
  "createdTimeTo": "2024-12-31T23:59:59Z",
  "statusTypeId": 1,
  "tourId": 5,
  "exportFormat": "Excel"
}
```

**Logic Flow**:
1. **Authentication & Authorization**: 
   - Validate JWT token
   - Check user role is Admin (export permission required)
2. **Validation**: Validate export parameters and format
3. **Data Retrieval**:
   ```sql
   SELECT b.*, t.Title, a.FullName, bs.StatusName
   FROM Booking b
   LEFT JOIN Tour t ON b.TourId = t.Id AND t.Active = 1
   LEFT JOIN Account a ON b.UserId = a.Id AND a.Active = 1
   LEFT JOIN BookingStatus bs ON b.StatusTypeId = bs.Id AND bs.Active = 1
   WHERE b.Active = 1
   AND (@StatusTypeId IS NULL OR b.StatusTypeId = @StatusTypeId)
   AND (@TourId IS NULL OR b.TourId = @TourId)
   AND (@StartDateFrom IS NULL OR b.StartDate >= @StartDateFrom)
   AND (@StartDateTo IS NULL OR b.StartDate <= @StartDateTo)
   AND (@CreatedTimeFrom IS NULL OR b.CreatedTime >= @CreatedTimeFrom)
   AND (@CreatedTimeTo IS NULL OR b.CreatedTime <= @CreatedTimeTo)
   ORDER BY b.CreatedTime DESC
   ```
4. **File Generation**:
   - **Excel**: Generate CSV format with headers and data
   - **PDF**: Generate HTML table format (can be converted to PDF)
5. **Response**: Return base64-encoded file with metadata

**Export Formats**:
- **Excel**: CSV format with UTF-8 encoding
- **PDF**: HTML table format (requires PDF library for production)

## Database Schema Compliance

### Tables Used
- **Booking**: Main booking records
- **Tour**: Tour information (LEFT JOIN)
- **Account**: User information (LEFT JOIN for registered users)
- **BookingStatus**: Status definitions

### Indexes Utilized
- `IX_Booking_Active`: For filtering active bookings
- `IX_Booking_StatusTypeId`: For status filtering
- `IX_Booking_TourId`: For tour filtering
- `IX_Booking_UserId`: For user filtering
- `IX_Booking_StartDate`: For date range filtering
- `IX_Booking_CreatedTime`: For creation date filtering

### Relationships
- Booking.TourId → Tour.Id (no FK constraint)
- Booking.UserId → Account.Id (no FK constraint, nullable for guests)
- Booking.StatusTypeId → BookingStatus.Id (no FK constraint)

## Error Handling

### Validation Errors
- **400 Bad Request**: Invalid parameters, missing required fields
- **404 Not Found**: Booking not found, status not found
- **403 Forbidden**: Insufficient permissions (export requires Admin)

### Business Logic Errors
- Invalid status transitions
- Empty note validation
- Export permission checks

### Database Errors
- Transaction rollback on errors
- Connection timeout handling
- Constraint violation handling

## Security Considerations

### Authentication
- JWT token validation on all endpoints
- Token expiration checking
- Role-based access control

### Authorization
- Admin: Full access to all operations
- User: View and update access, no export permission
- Guest: No access to admin endpoints

### Data Protection
- Soft delete (Active = 0) instead of hard delete
- Audit trail with UpdatedBy and UpdatedTime
- Input validation and sanitization

## Performance Optimizations

### Query Optimization
- Use existing indexes for filtering
- Avoid N+1 queries with Include() for related data
- Pagination to limit result sets
- Projection to select only needed fields

### Caching Strategy
- Consider caching booking statuses (rarely change)
- Cache tour information for display
- Implement response caching for static data

### Large Dataset Handling
- Pagination for list endpoints
- Streaming for large exports
- Background processing for heavy operations

## API Response Examples

### Success Response
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "items": [...],
    "totalItems": 100,
    "totalPages": 10,
    "currentPage": 1,
    "pageSize": 10
  }
}
```

### Error Response
```json
{
  "success": false,
  "message": "Booking not found"
}
```

### Export Response
```json
{
  "success": true,
  "message": "Export generated successfully",
  "data": {
    "fileBytes": "base64-encoded-file-content",
    "fileName": "Bookings_Export_20241201_143022.xlsx",
    "contentType": "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
  }
}
```

## Testing Guidelines

### Unit Tests
- Test all service methods with mock repositories
- Test validation logic
- Test mapping methods
- Test error scenarios

### Integration Tests
- Test API endpoints with test database
- Test authentication and authorization
- Test transaction rollback scenarios
- Test export functionality

### Performance Tests
- Test with large datasets
- Test pagination performance
- Test export performance
- Test concurrent access

## Deployment Considerations

### Configuration
- Database connection strings
- JWT secret keys
- CORS origins
- Export file size limits

### Monitoring
- API response times
- Database query performance
- Error rates
- Export generation times

### Scaling
- Database connection pooling
- Caching strategies
- Background job processing
- File storage for exports

This documentation provides a comprehensive guide for implementing and maintaining the Booking Management APIs in the Barefoot Travel System.
