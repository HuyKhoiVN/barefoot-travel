# Database Schema - Barefoot Travel System

## Tổng quan Database

**Database Engine**: SQL Server  
**Database Name**: `barefoot`  
**Đặc điểm**: Không sử dụng Foreign Key constraints (quan hệ được quản lý ở tầng ứng dụng)

## Cấu trúc bảng và mối quan hệ

### 1. Bảng Account (Tài khoản người dùng)

**Mục đích**: Quản lý thông tin tài khoản người dùng và admin

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính, tự động tăng |
| `Username` | `nvarchar(50)` | NOT NULL, UNIQUE | Tên đăng nhập (duy nhất) |
| `FullName` | `nvarchar(255)` | NOT NULL | Họ và tên đầy đủ |
| `PasswordHash` | `varchar(255)` | NOT NULL | Mật khẩu đã hash (BCrypt) |
| `Photo` | `varchar(500)` | NULL | URL ảnh đại diện |
| `Email` | `nvarchar(255)` | NULL | Địa chỉ email |
| `Phone` | `varchar(20)` | NULL | Số điện thoại |
| `RoleId` | `int` | NOT NULL | ID vai trò (1=Admin, 2=User) |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Mối quan hệ**:
- `RoleId` → `Role.Id` (không có FK constraint)

---

### 2. Bảng Role (Vai trò)

**Mục đích**: Định nghĩa các vai trò trong hệ thống

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `RoleName` | `nvarchar(100)` | NOT NULL, UNIQUE | Tên vai trò (Admin, User) |
| `Description` | `nvarchar(255)` | NULL | Mô tả vai trò |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Dữ liệu mặc định**:
- `Id = 1`: Admin
- `Id = 2`: User

---

### 3. Bảng Permission (Quyền hạn)

**Mục đích**: Định nghĩa các quyền hạn trong hệ thống

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `PermissionKey` | `nvarchar(100)` | NOT NULL, UNIQUE | Khóa quyền hạn (VD: "tour.create") |
| `Description` | `nvarchar(255)` | NULL | Mô tả quyền hạn |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

---

### 4. Bảng RolePermission (Phân quyền)

**Mục đích**: Liên kết vai trò với quyền hạn (Many-to-Many)

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `RoleId` | `int` | NOT NULL | ID vai trò |
| `PermissionId` | `int` | NOT NULL | ID quyền hạn |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Mối quan hệ**:
- `RoleId` → `Role.Id`
- `PermissionId` → `Permission.Id`

---

### 5. Bảng Tour (Tour du lịch)

**Mục đích**: Lưu trữ thông tin các tour du lịch

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `Title` | `nvarchar(255)` | NOT NULL | Tiêu đề tour |
| `Description` | `nvarchar(MAX)` | NULL | Mô tả chi tiết tour |
| `MapLink` | `varchar(500)` | NULL | Link bản đồ (Google Maps) |
| `PricePerPerson` | `decimal(18,2)` | NOT NULL | Giá mỗi người |
| `MaxPeople` | `int` | NOT NULL | Số người tối đa |
| `Duration` | `nvarchar(50)` | NOT NULL | Thời gian tour (VD: "3 ngày 2 đêm") |
| `StartTime` | `time` | NULL | Giờ khởi hành |
| `ReturnTime` | `time` | NULL | Giờ về |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

---

### 6. Bảng Category (Danh mục)

**Mục đích**: Phân loại tour theo danh mục (có thể có cấu trúc cây)

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `ParentId` | `int` | NULL | ID danh mục cha (cho cấu trúc cây) |
| `CategoryName` | `nvarchar(255)` | NOT NULL | Tên danh mục |
| `Enable` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái kích hoạt |
| `Type` | `nvarchar(50)` | NOT NULL | Loại danh mục (VD: "Tour", "Hotel") |
| `Priority` | `int` | NOT NULL | Độ ưu tiên hiển thị |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Mối quan hệ**:
- `ParentId` → `Category.Id` (self-reference cho cấu trúc cây)

---

### 7. Bảng TourCategory (Liên kết Tour-Danh mục)

**Mục đích**: Liên kết tour với danh mục (Many-to-Many)

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `TourId` | `int` | NOT NULL | ID tour |
| `CategoryId` | `int` | NOT NULL | ID danh mục |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Mối quan hệ**:
- `TourId` → `Tour.Id`
- `CategoryId` → `Category.Id`

---

### 8. Bảng TourImage (Hình ảnh Tour)

**Mục đích**: Lưu trữ hình ảnh của tour

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `TourId` | `int` | NOT NULL | ID tour |
| `ImageUrl` | `varchar(500)` | NOT NULL | URL hình ảnh |
| `IsBanner` | `bit` | NOT NULL, DEFAULT 0 | Có phải ảnh banner không |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Mối quan hệ**:
- `TourId` → `Tour.Id`

---

### 9. Bảng Booking (Đặt tour)

**Mục đích**: Quản lý thông tin đặt tour của khách hàng

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `TourId` | `int` | NOT NULL | ID tour |
| `UserId` | `int` | NULL | ID người dùng (có thể null nếu khách vãng lai) |
| `StartDate` | `date` | NOT NULL | Ngày khởi hành |
| `People` | `int` | NOT NULL | Số người |
| `PhoneNumber` | `varchar(20)` | NOT NULL | Số điện thoại |
| `NameCustomer` | `nvarchar(255)` | NOT NULL | Tên khách hàng |
| `Email` | `nvarchar(255)` | NULL | Email khách hàng |
| `Note` | `nvarchar(MAX)` | NULL | Ghi chú |
| `TotalPrice` | `decimal(18,2)` | NOT NULL | Tổng tiền |
| `StatusTypeId` | `int` | NOT NULL | ID trạng thái đặt tour |
| `PaymentStatus` | `nvarchar(50)` | NOT NULL | Trạng thái thanh toán |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Mối quan hệ**:
- `TourId` → `Tour.Id`
- `UserId` → `Account.Id`
- `StatusTypeId` → `BookingStatus.Id`

---

### 10. Bảng BookingStatus (Trạng thái đặt tour)

**Mục đích**: Định nghĩa các trạng thái đặt tour

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `StatusName` | `nvarchar(255)` | NOT NULL | Tên trạng thái |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Dữ liệu mặc định**:
- `Id = 1`: Pending (Chờ xử lý)
- `Id = 2`: Confirmed (Đã xác nhận)
- `Id = 3`: Cancelled (Đã hủy)
- `Id = 4`: Completed (Hoàn thành)

---

### 11. Bảng Policy (Chính sách)

**Mục đích**: Định nghĩa các chính sách tour (hủy tour, hoàn tiền, etc.)

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `PolicyType` | `nvarchar(255)` | NOT NULL | Loại chính sách |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

---

### 12. Bảng TourPolicy (Liên kết Tour-Chính sách)

**Mục đích**: Liên kết tour với chính sách (Many-to-Many)

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `TourId` | `int` | NOT NULL | ID tour |
| `PolicyId` | `int` | NOT NULL | ID chính sách |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Mối quan hệ**:
- `TourId` → `Tour.Id`
- `PolicyId` → `Policy.Id`

---

### 13. Bảng PriceType (Loại giá)

**Mục đích**: Định nghĩa các loại giá tour (người lớn, trẻ em, em bé)

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `PriceTypeName` | `nvarchar(255)` | NOT NULL | Tên loại giá |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Dữ liệu mặc định**:
- `Id = 1`: Adult (Người lớn)
- `Id = 2`: Child (Trẻ em)
- `Id = 3`: Infant (Em bé)

---

### 14. Bảng TourPrice (Giá tour theo loại)

**Mục đích**: Lưu trữ giá tour theo từng loại giá

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `TourId` | `int` | NOT NULL | ID tour |
| `PriceTypeId` | `int` | NOT NULL | ID loại giá |
| `Price` | `decimal(18,2)` | NOT NULL | Giá tour |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Mối quan hệ**:
- `TourId` → `Tour.Id`
- `PriceTypeId` → `PriceType.Id`

---

### 15. Bảng CompanyInfo (Thông tin công ty)

**Mục đích**: Lưu trữ thông tin cấu hình công ty

| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|-------------|-----------|-------|
| `Id` | `int` | PK, Identity | Khóa chính |
| `Icon` | `varchar(500)` | NULL | Icon/Logo |
| `Title` | `nvarchar(255)` | NOT NULL | Tiêu đề thông tin |
| `Value` | `nvarchar(MAX)` | NOT NULL | Giá trị thông tin |
| `CreatedTime` | `datetime` | NOT NULL, DEFAULT GETDATE() | Thời gian tạo |
| `UpdatedTime` | `datetime` | NULL | Thời gian cập nhật |
| `UpdatedBy` | `nvarchar(100)` | NULL | Người cập nhật |
| `Active` | `bit` | NOT NULL, DEFAULT 1 | Trạng thái hoạt động |

**Dữ liệu mặc định**:
- Company Name
- Address
- Phone
- Email
- Website
- Social Media Links

---

## Sơ đồ mối quan hệ (ERD)

```
Account (1) ←→ (M) RolePermission (M) ←→ (1) Role
Account (1) ←→ (M) Booking
Tour (1) ←→ (M) Booking
Tour (1) ←→ (M) TourCategory (M) ←→ (1) Category
Tour (1) ←→ (M) TourImage
Tour (1) ←→ (M) TourPolicy (M) ←→ (1) Policy
Tour (1) ←→ (M) TourPrice (M) ←→ (1) PriceType
BookingStatus (1) ←→ (M) Booking
Category (1) ←→ (M) Category (Self-Reference)
```