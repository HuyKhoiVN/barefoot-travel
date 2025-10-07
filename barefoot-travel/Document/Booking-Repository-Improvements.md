# Booking Repository Improvements

## Tóm tắt các cải thiện đã thực hiện

### 1. Sử dụng SQL Syntax thay vì LINQ Include

**Trước đây:**
```csharp
var query = _context.Bookings
    .Where(b => b.Active)
    .Include(b => b.Tour)
    .Include(b => b.Account)
    .Include(b => b.BookingStatus);
```

**Sau khi cải thiện:**
```csharp
var sql = @"
    SELECT 
        b.Id, b.TourId, ISNULL(t.Title, 'N/A') as TourTitle,
        b.UserId, ISNULL(a.FullName, 'Guest') as UserFullName,
        b.StartDate, b.People, b.PhoneNumber, b.NameCustomer,
        b.Email, b.Note, b.TotalPrice, b.StatusTypeId,
        ISNULL(bs.StatusName, 'Unknown') as StatusName,
        b.PaymentStatus, b.CreatedTime, b.UpdatedTime,
        b.UpdatedBy, b.Active
    FROM Booking b
    LEFT JOIN Tour t ON b.TourId = t.Id AND t.Active = 1
    LEFT JOIN Account a ON b.UserId = a.Id AND a.Active = 1
    LEFT JOIN BookingStatus bs ON b.StatusTypeId = bs.Id AND bs.Active = 1
    WHERE b.Active = 1
    ORDER BY b.CreatedTime DESC";

return await _context.Database.SqlQueryRaw<BookingWithDetailsDto>(sql).ToListAsync();
```

### 2. Tách biệt Repository và Service theo nguyên tắc SOLID

**Repository Layer:**
- Chỉ chứa logic truy cập database
- Sử dụng SQL syntax với JOIN
- Không chứa business logic
- Trả về raw data hoặc DTOs đơn giản

**Service Layer:**
- Chỉ gọi Repository methods
- Chứa business logic và validation
- Không trực tiếp truy cập database
- Sử dụng manual mapping

### 3. Cải thiện Performance

**Trước đây:**
- Sử dụng Include() gây ra N+1 query problem
- Multiple database calls cho related data
- Không tối ưu cho large datasets

**Sau khi cải thiện:**
- Single SQL query với JOIN
- Tối ưu sử dụng existing indexes
- Pagination với OFFSET/FETCH
- Dynamic WHERE clause building

### 4. Tuân thủ nguyên tắc SOLID

**Single Responsibility Principle (SRP):**
- Repository: Chỉ quản lý data access
- Service: Chỉ quản lý business logic
- Controller: Chỉ quản lý HTTP requests

**Open/Closed Principle (OCP):**
- Interface-based design
- Dễ dàng extend functionality
- Không modify existing code

**Liskov Substitution Principle (LSP):**
- Interface implementations có thể thay thế
- Consistent method signatures

**Interface Segregation Principle (ISP):**
- Interfaces nhỏ và focused
- Clients chỉ depend on methods cần thiết

**Dependency Inversion Principle (DIP):**
- Service depend on Repository interface
- Không depend on concrete implementations

### 5. Cải thiện Database Operations

**SQL Query Optimization:**
```sql
-- Sử dụng existing indexes
WHERE b.Active = 1  -- IX_Booking_Active
AND b.StatusTypeId = @StatusTypeId  -- IX_Booking_StatusTypeId
AND b.TourId = @TourId  -- IX_Booking_TourId
AND b.StartDate >= @StartDateFrom  -- IX_Booking_StartDate

-- Pagination với OFFSET/FETCH
ORDER BY b.CreatedTime DESC
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY
```

**Dynamic WHERE Clause:**
```csharp
var whereConditions = new List<string> { "b.Active = 1" };
var parameters = new List<object>();

if (filter.StatusTypeId.HasValue)
{
    whereConditions.Add("b.StatusTypeId = {0}");
    parameters.Add(filter.StatusTypeId.Value);
}
```

### 6. Manual Mapping thay vì AutoMapper

**Trước đây:**
```csharp
// Sử dụng AutoMapper (đã loại bỏ)
var bookingDto = _mapper.Map<BookingDto>(booking);
```

**Sau khi cải thiện:**
```csharp
private BookingDto MapToBookingDto(BookingWithDetailsDto booking)
{
    if (booking == null) return null;

    return new BookingDto
    {
        Id = booking.Id,
        TourId = booking.TourId,
        TourTitle = booking.TourTitle,
        // ... manual mapping
    };
}
```

### 7. Transaction Management

**Repository Level:**
- UpdateAsync() method handle transaction internally
- Không expose transaction details to Service

**Service Level:**
- Chỉ gọi Repository methods
- Không quản lý transaction trực tiếp
- Focus on business logic

### 8. Error Handling

**Repository:**
- Throw exceptions cho database errors
- Không handle business logic errors

**Service:**
- Catch và wrap exceptions
- Return ApiResponse với error messages
- Log errors appropriately

### 9. Code Structure

**Repository Interface:**
```csharp
public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(int id);
    Task<PagedResult<Booking>> GetPagedAsync(int page, int pageSize, string sortBy, string sortDirection);
    Task<List<BookingWithDetailsDto>> GetBookingsWithDetailsAsync(List<int> bookingIds);
    // ... other methods
}
```

**Service Interface:**
```csharp
public interface IBookingService
{
    Task<ApiResponse> GetBookingByIdAsync(int id);
    Task<PagedResult<BookingDto>> GetBookingsPagedAsync(int page, int pageSize, string sortBy, string sortDirection);
    // ... other methods
}
```

### 10. Benefits của cải thiện

1. **Performance:**
   - Giảm database calls
   - Tối ưu query execution
   - Sử dụng indexes hiệu quả

2. **Maintainability:**
   - Code rõ ràng và dễ đọc
   - Separation of concerns
   - Dễ test và debug

3. **Scalability:**
   - Có thể handle large datasets
   - Efficient pagination
   - Optimized queries

4. **SOLID Compliance:**
   - Tuân thủ tất cả 5 nguyên tắc SOLID
   - Dễ extend và modify
   - Loose coupling

5. **Database Optimization:**
   - Sử dụng SQL syntax trực tiếp
   - JOIN thay vì multiple queries
   - Parameterized queries

## Kết luận

Các cải thiện này đảm bảo:
- **Performance tốt hơn** với single SQL queries
- **Code maintainable** với separation of concerns
- **Tuân thủ SOLID principles** 
- **Database optimization** với proper indexing
- **Manual mapping** thay vì AutoMapper dependency
- **Transaction management** ở đúng layer

Hệ thống bây giờ có architecture rõ ràng, performance tốt và dễ maintain.
