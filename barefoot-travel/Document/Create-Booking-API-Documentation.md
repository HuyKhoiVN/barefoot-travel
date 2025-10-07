# Create Booking API Documentation

## API Endpoint

**POST** `/api/admin/booking`

## Mục đích
Tạo booking mới cho tour du lịch với validation đầy đủ và business logic.

## Request Body

### CreateBookingDto
```json
{
  "tourId": 1,
  "startDate": "2024-12-25",
  "people": 2,
  "phoneNumber": "0123456789",
  "nameCustomer": "Nguyễn Văn A",
  "email": "nguyenvana@example.com",
  "note": "Khách hàng yêu cầu phòng đôi",
  "paymentStatus": "Pending",
  "userId": 5
}
```

### Validation Rules

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `tourId` | int | ✅ | Must exist in Tour table |
| `startDate` | DateTime | ✅ | Cannot be in the past |
| `people` | int | ✅ | Must be >= 1 and <= tour.MaxPeople |
| `phoneNumber` | string | ✅ | Max 20 characters |
| `nameCustomer` | string | ✅ | Max 255 characters |
| `email` | string | ❌ | Valid email format, max 255 characters |
| `note` | string | ❌ | Max 1000 characters |
| `paymentStatus` | string | ✅ | Max 50 characters, default "Pending" |
| `userId` | int? | ❌ | Must exist in Account table if provided |

## Response

### Success Response (201 Created)
```json
{
  "success": true,
  "message": "Booking created successfully",
  "data": {
    "id": 123,
    "tourId": 1,
    "tourTitle": "Tour Đà Nẵng - Hội An",
    "userId": 5,
    "userFullName": "Nguyễn Văn A",
    "startDate": "2024-12-25T00:00:00Z",
    "people": 2,
    "phoneNumber": "0123456789",
    "nameCustomer": "Nguyễn Văn A",
    "email": "nguyenvana@example.com",
    "note": "Khách hàng yêu cầu phòng đôi",
    "totalPrice": 2000000,
    "statusTypeId": 1,
    "statusName": "Pending",
    "paymentStatus": "Pending",
    "createdTime": "2024-12-01T10:30:00Z",
    "updatedTime": null,
    "updatedBy": null,
    "active": true
  }
}
```

### Error Responses

#### 400 Bad Request - Validation Error
```json
{
  "success": false,
  "message": "Tour not found"
}
```

#### 400 Bad Request - Business Logic Error
```json
{
  "success": false,
  "message": "Number of people (5) exceeds tour capacity (4)"
}
```

#### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Error creating booking: Database connection failed"
}
```

## Business Logic

### 1. Validation Flow
1. **Tour Validation**: Kiểm tra tour tồn tại và active
2. **User Validation**: Kiểm tra user tồn tại nếu userId được cung cấp
3. **Date Validation**: Kiểm tra start date không được trong quá khứ
4. **Capacity Validation**: Kiểm tra số người không vượt quá capacity của tour
5. **Data Validation**: Kiểm tra format và length của các field

### 2. Price Calculation
```csharp
var totalPrice = tour.PricePerPerson * dto.People;
```

### 3. Default Values
- `statusTypeId`: 1 (Pending)
- `active`: true
- `createdTime`: DateTime.UtcNow
- `updatedTime`: null
- `updatedBy`: null

### 4. Database Operations
1. Validate related entities (Tour, User)
2. Create Booking entity
3. Save to database
4. Retrieve created booking with details
5. Return mapped DTO

## Implementation Details

### Repository Layer
```csharp
public async Task<Booking> CreateAsync(Booking booking)
{
    _context.Bookings.Add(booking);
    await _context.SaveChangesAsync();
    return booking;
}
```

### Service Layer
```csharp
public async Task<ApiResponse> CreateBookingAsync(CreateBookingDto dto, string createdBy)
{
    // 1. Validate tour exists
    var tour = await _bookingRepository.GetTourByIdAsync(dto.TourId);
    if (tour == null)
        return new ApiResponse(false, "Tour not found");

    // 2. Validate user exists if provided
    if (dto.UserId.HasValue)
    {
        var user = await _bookingRepository.GetAccountByIdAsync(dto.UserId.Value);
        if (user == null)
            return new ApiResponse(false, "User not found");
    }

    // 3. Business logic validations
    if (dto.StartDate.Date < DateTime.Today)
        return new ApiResponse(false, "Start date cannot be in the past");

    if (dto.People > tour.MaxPeople)
        return new ApiResponse(false, $"Number of people ({dto.People}) exceeds tour capacity ({tour.MaxPeople})");

    // 4. Calculate total price
    var totalPrice = tour.PricePerPerson * dto.People;

    // 5. Create and save booking
    var booking = new Booking { /* ... */ };
    var createdBooking = await _bookingRepository.CreateAsync(booking);

    // 6. Return booking with details
    var bookingWithDetails = await _bookingRepository.GetBookingsWithDetailsAsync(new List<int> { createdBooking.Id });
    var bookingDto = MapToBookingDto(bookingWithDetails.First());
    
    return new ApiResponse(true, "Booking created successfully", bookingDto);
}
```

### Controller Layer
```csharp
[HttpPost]
public async Task<ApiResponse> CreateBooking([FromBody] CreateBookingDto dto)
{
    _logger.LogInformation("Creating booking for tour ID: {TourId}, Customer: {CustomerName}", dto.TourId, dto.NameCustomer);

    try
    {
        var createdBy = User.Identity?.Name ?? "System";
        return await _bookingService.CreateBookingAsync(dto, createdBy);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating booking for tour ID: {TourId}", dto.TourId);
        return new ApiResponse(false, $"Error: {ex.Message}");
    }
}
```

## Error Handling

### Validation Errors
- **Tour not found**: TourId không tồn tại hoặc không active
- **User not found**: UserId không tồn tại hoặc không active
- **Start date in past**: Ngày khởi hành trong quá khứ
- **Capacity exceeded**: Số người vượt quá capacity của tour

### Business Logic Errors
- **Invalid data**: Dữ liệu không hợp lệ theo business rules
- **Calculation errors**: Lỗi tính toán giá

### System Errors
- **Database errors**: Lỗi kết nối hoặc thao tác database
- **Mapping errors**: Lỗi mapping dữ liệu
- **Service errors**: Lỗi trong service layer

## Security Considerations

### Authentication
- JWT token validation required
- User identity extraction from claims

### Authorization
- Admin: Full access to create bookings
- User: Can create bookings for themselves or guests

### Data Protection
- Input validation và sanitization
- SQL injection prevention through Entity Framework
- XSS protection through proper encoding

## Performance Considerations

### Database Optimization
- Single query for tour validation
- Single query for user validation
- Efficient entity creation
- Optimized retrieval with JOIN

### Caching Strategy
- Consider caching tour information
- Cache user data for frequent bookings

## Testing Scenarios

### Happy Path
1. Valid tour ID
2. Valid user ID (optional)
3. Future start date
4. People count within capacity
5. All required fields provided

### Error Scenarios
1. Invalid tour ID
2. Invalid user ID
3. Past start date
4. People count exceeds capacity
5. Missing required fields
6. Invalid email format
7. Database connection errors

### Edge Cases
1. Maximum people count
2. Minimum people count (1)
3. Very long note text
4. Special characters in customer name
5. International phone numbers

## API Usage Examples

### cURL Example
```bash
curl -X POST "https://api.barefoot-travel.com/api/admin/booking" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "tourId": 1,
    "startDate": "2024-12-25",
    "people": 2,
    "phoneNumber": "0123456789",
    "nameCustomer": "Nguyễn Văn A",
    "email": "nguyenvana@example.com",
    "note": "Khách hàng yêu cầu phòng đôi",
    "paymentStatus": "Pending"
  }'
```

### JavaScript Example
```javascript
const createBooking = async (bookingData) => {
  try {
    const response = await fetch('/api/admin/booking', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(bookingData)
    });
    
    const result = await response.json();
    return result;
  } catch (error) {
    console.error('Error creating booking:', error);
    throw error;
  }
};

// Usage
const bookingData = {
  tourId: 1,
  startDate: "2024-12-25",
  people: 2,
  phoneNumber: "0123456789",
  nameCustomer: "Nguyễn Văn A",
  email: "nguyenvana@example.com",
  note: "Khách hàng yêu cầu phòng đôi",
  paymentStatus: "Pending"
};

createBooking(bookingData)
  .then(result => {
    if (result.success) {
      console.log('Booking created:', result.data);
    } else {
      console.error('Error:', result.message);
    }
  });
```

## Monitoring và Logging

### Logging Points
- Booking creation attempt
- Validation failures
- Business logic errors
- Database operations
- Success/failure outcomes

### Metrics to Track
- Booking creation success rate
- Validation error frequency
- Average response time
- Database performance
- Error rates by type

## Kết luận

API Create Booking được thiết kế với:
- **Comprehensive validation** cho tất cả input
- **Business logic enforcement** theo yêu cầu nghiệp vụ
- **Proper error handling** với meaningful messages
- **Security considerations** cho authentication/authorization
- **Performance optimization** với efficient database operations
- **Full logging** cho monitoring và debugging

API tuân thủ hoàn toàn API Development Rules và Database Schema, đảm bảo tính nhất quán và chất lượng cao.
