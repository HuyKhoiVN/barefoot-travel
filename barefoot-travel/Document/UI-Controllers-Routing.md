# UI Controllers Routing Documentation

## 📋 Overview
This document describes the routing structure for UI controllers in the Barefoot Travel application. All UI controllers are designed to return only `View()` and handle all business logic through jQuery AJAX calls to the API endpoints.

## 🏗️ Controller Structure

### 1. HomeController
**Location**: `Controllers/HomeController.cs`  
**Namespace**: `barefoot_travel.Controllers`

#### Actions:
- **`Login()`** - Returns login view

#### Routes:
- **`/Home/Login`** - Login page

#### View:
- **Location**: `Views/Home/Login.cshtml`
- **Layout**: `~/Views/Shared/_LoginLayout.cshtml`
- **Purpose**: User authentication with jQuery AJAX

---

### 2. TourManagerController
**Location**: `Controllers/TourManagerController.cs`  
**Namespace**: `barefoot_travel.Controllers`  
**Route Attribute**: `[Route("tour-manager")]`

#### Actions:
- **`Index()`** - Returns tour management view

#### Routes:
- **`/tour-manager`** - Tour management page (main dashboard)

#### View:
- **Location**: `Views/TourManager/Index.cshtml`
- **Layout**: `~/Views/Admin/_Layout.cshtml`
- **Purpose**: Complete tour CRUD operations with jQuery AJAX

---

### 3. AdminController
**Location**: `Controllers/AdminController.cs`  
**Namespace**: `barefoot_travel.Controllers`

#### Actions:
- **`Index()`** - Returns admin dashboard view
- **`Demo()`** - Returns demo page view

#### Routes:
- **`/Admin`** - Admin dashboard
- **`/Admin/Demo`** - Demo page

#### Views:
- **Location**: `Views/Admin/Index.cshtml` và `Views/Admin/Demo.cshtml`
- **Layout**: `~/Views/Admin/_Layout.cshtml`
- **Purpose**: Admin dashboard and demo pages

---

## 🔗 Routing Configuration

### Program.cs Configuration
```csharp
// Configure routing
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.AppendTrailingSlash = false;
});
```

### Route Patterns

| Controller | Action | Route | Purpose |
|------------|--------|-------|---------|
| HomeController | Login | `/Home/Login` | User authentication |
| TourManagerController | Index | `/tour-manager` | Tour management dashboard |
| AdminController | Index | `/Admin` | Admin dashboard |
| AdminController | Demo | `/Admin/Demo` | Demo page |

---

## 🎯 View Structure

### 1. Login View (`Views/Home/Login.cshtml`)
**Features:**
- JWT token authentication
- Automatic token validation
- Registration modal
- Redirect to `/tour-manager` after successful login
- Token storage in localStorage

**JavaScript Functions:**
- `checkAuthStatus()` - Verify existing token
- `performLogin()` - Handle login form submission
- `performRegister()` - Handle registration
- `showRegisterModal()` - Display registration modal

### 2. Tour Management View (`Views/TourManager/Index.cshtml`)
**Features:**
- Complete CRUD operations for tours
- Pagination with search and filters
- Modal-based forms (Create, Edit, View, Delete)
- Real-time search with debouncing
- Category and policy management
- Authentication check and auto-redirect

**JavaScript Functions:**
- `loadTours()` - Load paginated tour list
- `displayTours()` - Render tours in table
- `displayPagination()` - Generate pagination controls
- `loadCategories()` - Load categories for filters
- `loadPolicies()` - Load policies for forms
- `viewTour(tourId)` - Display tour details modal
- `editTour(tourId)` - Open edit modal
- `deleteTour(tourId)` - Handle tour deletion
- `changePage(page)` - Handle pagination

---

## 🔐 Authentication Flow

### Login Process
1. User accesses `/Home/Login`
2. Check if token exists in localStorage
3. If token exists, verify with `/api/user/profile`
4. If valid, redirect to `/tour-manager`
5. If invalid or missing, show login form

### Tour Management Access
1. User accesses `/tour-manager`
2. Check authentication status
3. If not authenticated, redirect to `/Home/Login`
4. If authenticated, load tour management interface

### Logout Process
1. Call `window.logout()` function
2. Clear localStorage tokens
3. Redirect to `/Home/Login`

---

## 🌐 API Integration

### Authentication APIs
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `GET /api/user/profile` - Token validation

### Tour Management APIs
- `GET /api/tour/paged` - Paginated tour list
- `GET /api/tour/{id}` - Tour details
- `POST /api/tour` - Create new tour
- `PUT /api/tour/{id}` - Update tour
- `DELETE /api/tour/{id}` - Delete tour

### Supporting APIs
- `GET /api/category` - Category list
- `GET /api/policy` - Policy list

---

## 🎨 UI Components

### Layout Structure
- **Admin Layout**: `Views/Admin/_Layout.cshtml` (with sidebar and header)
- **Login Layout**: `Views/Shared/_LoginLayout.cshtml` (without sidebar, centered design)
- **Sidebar**: `Views/Admin/_Sidebar.cshtml`
- **Header**: `Views/Admin/_Header.cshtml`

### Global JavaScript Configuration
```javascript
// Base URL configuration
var baseUrl = window.location.origin;
if (!baseUrl.endsWith("/")) {
    baseUrl += "/";
}

// Global AJAX setup
$.ajaxSetup({
    beforeSend: function(xhr) {
        var token = localStorage.getItem('jwt_token');
        if (token) {
            xhr.setRequestHeader('Authorization', 'Bearer ' + token);
        }
    },
    error: function(xhr, status, error) {
        if (xhr.status === 401) {
            localStorage.removeItem('jwt_token');
            localStorage.removeItem('refresh_token');
            window.location.href = '/Home/Login';
        }
    }
});

// Logout function
window.logout = function() {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('refresh_token');
    window.location.href = '/Home/Login';
};
```

---

## 📱 Navigation Structure

### Sidebar Navigation (`Views/Admin/_Sidebar.cshtml`)
- **Dashboard**: `/Admin` (existing)
- **Tour Management**: `/tour-manager` (new)
- **Categories**: `/Admin/Category` (existing)
- **Bookings**: `/Admin/Booking` (existing)
- **Users**: `/Admin/User` (existing)
- **Roles & Permissions**: `/Admin/Role` (existing)
- **Pricing**: `/Admin/Pricing` (existing)

### Header Navigation (`Views/Admin/_Header.cshtml`)
- **Profile**: User profile link
- **Settings**: User settings link
- **Back to Site**: Home page link
- **Logout**: JavaScript logout function

---

## 🚀 Usage Examples

### Accessing Login Page
```
URL: /Home/Login
Method: GET
Controller: HomeController
Action: Login
View: Views/Home/Login.cshtml
```

### Accessing Tour Management
```
URL: /tour-manager
Method: GET
Controller: TourManagerController
Action: Index
View: Views/TourManager/Index.cshtml
```

### Authentication Check
```javascript
// Automatic check on page load
checkAuthStatus();

// Manual logout
window.logout();
```

---

## 🔧 Development Notes

### Controller Design Principles
1. **Single Responsibility**: Each controller handles one specific area
2. **View-Only Actions**: All actions return only `View()`
3. **jQuery Logic**: All business logic handled in client-side JavaScript
4. **API Integration**: All data operations through AJAX calls

### View Design Principles
1. **Modal-Based CRUD**: All operations in modals for better UX
2. **Real-Time Updates**: Immediate UI updates after API calls
3. **Error Handling**: Comprehensive error handling and user feedback
4. **Responsive Design**: Bootstrap 5 for mobile-friendly interface

### Security Considerations
1. **Token Management**: Automatic token inclusion in all requests
2. **Authentication Check**: Verify token validity on page load
3. **Auto-Logout**: Redirect to login on authentication failure
4. **Error Handling**: Graceful handling of 401 unauthorized responses

---

## 📝 File Structure Summary

```
Controllers/
├── HomeController.cs (Login action)
├── TourManagerController.cs (Index action with /tour-manager route)
├── AdminController.cs (Index and Demo actions)
└── Api/ (existing API controllers)

Views/
├── Home/
│   └── Login.cshtml (Authentication page)
├── TourManager/
│   └── Index.cshtml (Tour management dashboard)
├── Admin/
│   ├── Index.cshtml (Admin dashboard)
│   └── Demo.cshtml (Demo page)
├── Shared/
│   └── _LoginLayout.cshtml (Login page layout)
└── Admin/ (existing admin views)
```

---

## ✅ Testing Checklist

- [ ] `/Home/Login` loads correctly
- [ ] `/tour-manager` loads correctly
- [ ] Authentication flow works properly
- [ ] Token validation functions correctly
- [ ] Logout functionality works
- [ ] All CRUD operations function properly
- [ ] Pagination and search work correctly
- [ ] Error handling displays appropriate messages
- [ ] Responsive design works on mobile devices
