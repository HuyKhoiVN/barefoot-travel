# Admin Layout Documentation

## Tổng quan
Layout admin được xây dựng dựa trên template Flexy Free Bootstrap Admin Template, được tùy chỉnh cho ứng dụng Barefoot Travel.

## Cấu trúc thư mục
```
Views/Admin/
├── _Layout.cshtml          # Layout chính
├── _Sidebar.cshtml         # Sidebar navigation
├── _Header.cshtml          # Header với user menu
├── _ViewStart.cshtml       # Cấu hình layout mặc định
├── _ViewImports.cshtml     # Import namespaces
├── Index.cshtml           # Dashboard chính
└── Demo.cshtml             # Trang demo
```

## Cách sử dụng

### 1. Tạo trang mới trong Admin
```csharp
public class YourController : Controller
{
    public IActionResult YourAction()
    {
        ViewData["Title"] = "Your Page Title";
        return View();
    }
}
```

### 2. Trong View (.cshtml)
```html
@{
    ViewData["Title"] = "Your Page Title";
    Layout = "~/Views/Admin/_Layout.cshtml";
}

<!-- Nội dung trang của bạn -->
```

### 3. Sử dụng các section có sẵn
```html
@section Scripts {
    <script>
        // Custom JavaScript cho trang
    </script>
}
```

## Tính năng chính

### Sidebar Navigation
- Dashboard
- Tours Management (Tours, Categories)
- Bookings Management
- Users Management
- Roles & Permissions
- Pricing Management
- Reports & Analytics
- Settings

### Header Features
- Responsive mobile menu
- Notification dropdown
- User profile dropdown
- Quick navigation

### Layout Features
- Responsive design
- Bootstrap 5
- Tabler icons
- Chart.js support
- ApexCharts integration
- Modern UI components

## Assets được sử dụng
- CSS: `~/assets/css/_styles.min.css`
- JavaScript: `~/assets/js/app.min.js`
- Icons: Tabler icons và FontAwesome
- Charts: ApexCharts

## Customization

### Thêm menu item mới
Chỉnh sửa file `_Sidebar.cshtml`:

```html
<li class="sidebar-item">
    <a class="sidebar-link" href="@Url.Action("YourAction", "YourController")">
        <i class="ti ti-your-icon"></i>
        <span class="hide-menu">Your Menu Text</span>
    </a>
</li>
```

### Thêm JavaScript custom
Sử dụng section Scripts:

```html
@section Scripts {
    <script src="~/js/your-custom-script.js"></script>
    <script>
        // Your custom JavaScript
    </script>
}
```

## Demo
Truy cập `/Admin/Demo` để xem layout demo và các tính năng.
