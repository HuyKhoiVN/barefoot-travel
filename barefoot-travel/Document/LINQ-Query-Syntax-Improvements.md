# LINQ Query Syntax Improvements

## Tóm tắt các cải thiện đã thực hiện

### 1. Sử dụng LINQ Query Syntax thay vì SQL Raw Queries

**Trước đây (SQL Raw):**
```csharp
var sql = $@"
    SELECT b.*
    FROM Booking b
    WHERE b.Active = 1
    ORDER BY {orderByClause}
    OFFSET {(page - 1) * pageSize} ROWS
    FETCH NEXT {pageSize} ROWS ONLY";

var items = await _context.Bookings
    .FromSqlRaw(dataSql)
    .ToListAsync();
```

**Sau khi cải thiện (LINQ Query Syntax):**
```csharp
var query = from b in _context.Bookings
            where b.Active
            select b;

// Apply sorting
query = sortBy.ToLower() switch
{
    "startdate" => sortDirection.ToLower() == "asc" 
        ? query.OrderBy(b => b.StartDate) 
        : query.OrderByDescending(b => b.StartDate),
    // ... other cases
};

var items = await query
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### 2. Sử dụng JOIN với LINQ Query Syntax

**Trước đây (SQL Raw với JOIN):**
```csharp
var sql = $@"
    SELECT 
        b.Id, b.TourId, ISNULL(t.Title, 'N/A') as TourTitle,
        b.UserId, ISNULL(a.FullName, 'Guest') as UserFullName,
        -- ... other fields
    FROM Booking b
    LEFT JOIN Tour t ON b.TourId = t.Id AND t.Active = 1
    LEFT JOIN Account a ON b.UserId = a.Id AND a.Active = 1
    LEFT JOIN BookingStatus bs ON b.StatusTypeId = bs.Id AND bs.Active = 1
    WHERE b.Active = 1";
```

**Sau khi cải thiện (LINQ Query Syntax với JOIN):**
```csharp
var query = from b in _context.Bookings
            join t in _context.Tours on b.TourId equals t.Id into tourGroup
            from t in tourGroup.DefaultIfEmpty()
            join a in _context.Accounts on b.UserId equals a.Id into accountGroup
            from a in accountGroup.DefaultIfEmpty()
            join bs in _context.BookingStatuses on b.StatusTypeId equals bs.Id into statusGroup
            from bs in statusGroup.DefaultIfEmpty()
            where b.Active && (t == null || t.Active) && (a == null || a.Active) && (bs == null || bs.Active)
            orderby b.CreatedTime descending
            select new BookingWithDetailsDto
            {
                Id = b.Id,
                TourId = b.TourId,
                TourTitle = t != null ? t.Title : "N/A",
                UserId = b.UserId,
                UserFullName = a != null ? a.FullName : "Guest",
                // ... other properties
            };
```

### 3. Dynamic Filtering với LINQ Query Syntax

**Trước đây (Dynamic SQL):**
```csharp
var whereConditions = new List<string> { "b.Active = 1" };
var parameters = new List<object>();

if (filter.StatusTypeId.HasValue)
{
    whereConditions.Add("b.StatusTypeId = {0}");
    parameters.Add(filter.StatusTypeId.Value);
}
```

**Sau khi cải thiện (LINQ Query Syntax):**
```csharp
var query = from b in _context.Bookings
            where b.Active
            select b;

// Apply filters
if (filter.StatusTypeId.HasValue)
    query = from b in query where b.StatusTypeId == filter.StatusTypeId.Value select b;

if (filter.TourId.HasValue)
    query = from b in query where b.TourId == filter.TourId.Value select b;

if (!string.IsNullOrEmpty(filter.PhoneNumber))
    query = from b in query where b.PhoneNumber.Contains(filter.PhoneNumber) select b;
```

### 4. Complex JOIN với Filtering

**Export với JOIN và Filtering:**
```csharp
public async Task<List<BookingWithDetailsDto>> GetBookingsWithDetailsForExportAsync(ExportBookingDto exportFilter)
{
    var query = from b in _context.Bookings
                join t in _context.Tours on b.TourId equals t.Id into tourGroup
                from t in tourGroup.DefaultIfEmpty()
                join a in _context.Accounts on b.UserId equals a.Id into accountGroup
                from a in accountGroup.DefaultIfEmpty()
                join bs in _context.BookingStatuses on b.StatusTypeId equals bs.Id into statusGroup
                from bs in statusGroup.DefaultIfEmpty()
                where b.Active && (t == null || t.Active) && (a == null || a.Active) && (bs == null || bs.Active)
                select new { b, t, a, bs };

    // Apply export filters
    if (exportFilter.StatusTypeId.HasValue)
        query = from item in query where item.b.StatusTypeId == exportFilter.StatusTypeId.Value select item;

    if (exportFilter.TourId.HasValue)
        query = from item in query where item.b.TourId == exportFilter.TourId.Value select item;

    // ... other filters

    var result = from item in query
                 orderby item.b.CreatedTime descending
                 select new BookingWithDetailsDto
                 {
                     Id = item.b.Id,
                     TourId = item.b.TourId,
                     TourTitle = item.t != null ? item.t.Title : "N/A",
                     // ... other properties
                 };

    return await result.ToListAsync();
}
```

### 5. Benefits của LINQ Query Syntax

#### **Type Safety:**
- Compile-time checking
- IntelliSense support
- Refactoring safety

#### **Readability:**
- Code dễ đọc và hiểu
- Syntax gần giống SQL
- Structured query building

#### **Maintainability:**
- Dễ debug và modify
- Consistent với .NET ecosystem
- No SQL injection risks

#### **Performance:**
- Entity Framework optimization
- Query translation to SQL
- Lazy evaluation support

### 6. LINQ Query Syntax Patterns

#### **Basic Query:**
```csharp
var query = from b in _context.Bookings
            where b.Active
            select b;
```

#### **JOIN Query:**
```csharp
var query = from b in _context.Bookings
            join t in _context.Tours on b.TourId equals t.Id into tourGroup
            from t in tourGroup.DefaultIfEmpty()
            where b.Active && (t == null || t.Active)
            select new { b, t };
```

#### **Filtering Query:**
```csharp
var query = from b in _context.Bookings
            where b.Active
            select b;

if (filter.StatusTypeId.HasValue)
    query = from b in query where b.StatusTypeId == filter.StatusTypeId.Value select b;
```

#### **Projection Query:**
```csharp
var query = from b in _context.Bookings
            join t in _context.Tours on b.TourId equals t.Id into tourGroup
            from t in tourGroup.DefaultIfEmpty()
            where b.Active
            select new BookingWithDetailsDto
            {
                Id = b.Id,
                TourTitle = t != null ? t.Title : "N/A",
                // ... other properties
            };
```

### 7. So sánh với Method Syntax

**LINQ Query Syntax (Preferred):**
```csharp
var query = from b in _context.Bookings
            where b.Active
            orderby b.CreatedTime descending
            select b;
```

**LINQ Method Syntax (Alternative):**
```csharp
var query = _context.Bookings
    .Where(b => b.Active)
    .OrderByDescending(b => b.CreatedTime);
```

### 8. Best Practices

#### **Query Composition:**
- Build queries step by step
- Apply filters conditionally
- Use proper ordering

#### **JOIN Operations:**
- Use `join...into...from...DefaultIfEmpty()` for LEFT JOIN
- Handle null values properly
- Check active status for related entities

#### **Performance:**
- Use `AsNoTracking()` for read-only operations
- Apply filters early in the query
- Use projection to select only needed fields

#### **Error Handling:**
- Validate input parameters
- Handle null values gracefully
- Use proper exception handling

### 9. Code Structure

#### **Repository Pattern:**
```csharp
public async Task<List<BookingWithDetailsDto>> GetBookingsWithDetailsAsync(List<int> bookingIds)
{
    if (!bookingIds.Any())
        return new List<BookingWithDetailsDto>();

    var query = from b in _context.Bookings
                join t in _context.Tours on b.TourId equals t.Id into tourGroup
                from t in tourGroup.DefaultIfEmpty()
                // ... other joins
                where b.Active && bookingIds.Contains(b.Id)
                select new BookingWithDetailsDto { /* ... */ };

    return await query.ToListAsync();
}
```

#### **Service Pattern:**
```csharp
public async Task<PagedResult<BookingDto>> GetBookingsPagedAsync(int page, int pageSize, string sortBy, string sortDirection)
{
    var pagedResult = await _bookingRepository.GetPagedAsync(page, pageSize, sortBy, sortDirection);
    var bookingIds = pagedResult.Items.Select(b => b.Id).ToList();
    var bookingsWithDetails = await _bookingRepository.GetBookingsWithDetailsAsync(bookingIds);
    var bookingDtos = bookingsWithDetails.Select(MapToBookingDto).ToList();
    
    return new PagedResult<BookingDto>
    {
        Items = bookingDtos,
        TotalItems = pagedResult.TotalItems,
        TotalPages = pagedResult.TotalPages,
        CurrentPage = pagedResult.CurrentPage,
        PageSize = pagedResult.PageSize
    };
}
```

## Kết luận

Việc sử dụng LINQ Query Syntax thay vì SQL Raw queries mang lại:

1. **Type Safety**: Compile-time checking và IntelliSense
2. **Readability**: Code dễ đọc và maintain
3. **Performance**: Entity Framework optimization
4. **Security**: No SQL injection risks
5. **Maintainability**: Dễ debug và modify
6. **Consistency**: Tuân thủ .NET best practices

Tất cả các queries bây giờ sử dụng LINQ Query Syntax với proper JOIN operations, dynamic filtering, và type-safe projections.
