# Barefoot Travel Admin UI Rules & Guidelines

## Overview
This document outlines the UI/UX rules and guidelines for the Barefoot Travel Admin Panel. The admin interface follows a modern, responsive design pattern with consistent styling and user experience principles.

## Design System

### Color Palette
- **Primary**: `#5D87FF` (Blue)
- **Secondary**: `#49BEFF` (Light Blue)
- **Success**: `#13DEB9` (Teal)
- **Warning**: `#FFAE1F` (Orange)
- **Danger**: `#FA896B` (Red)
- **Info**: `#539BFF` (Info Blue)
- **Dark**: `#2A3547` (Dark Gray)
- **Light**: `#F8F9FA` (Light Gray)

### Typography
- **Font Family**: Inter, sans-serif
- **Font Sizes**:
  - H1: 2.5rem (40px)
  - H2: 2rem (32px)
  - H3: 1.75rem (28px)
  - H4: 1.5rem (24px)
  - H5: 1.25rem (20px)
  - H6: 1rem (16px)
  - Body: 0.875rem (14px)
  - Small: 0.75rem (12px)

### Spacing System
- **Base Unit**: 8px
- **Spacing Scale**: 4px, 8px, 12px, 16px, 24px, 32px, 48px, 64px
- **Margin/Padding Classes**: 
  - `m-1` to `m-5` (4px to 24px)
  - `p-1` to `p-5` (4px to 24px)
  - `mt-`, `mb-`, `ml-`, `mr-` for directional spacing

---

## Layout Structure

### Main Layout Components

#### 1. Sidebar Navigation
**File**: `Views/Admin/_Sidebar.cshtml`

**Structure**:
```html
<aside class="left-sidebar">
  <div class="brand-logo">
    <!-- Logo and close button -->
  </div>
  <nav class="sidebar-nav">
    <ul id="sidebarnav">
      <!-- Navigation items -->
    </ul>
  </nav>
</aside>
```

**Navigation Rules**:
- Use collapsible menu groups with `nav-small-cap` class
- Each menu item should have appropriate icons from Tabler Icons
- Sub-menus use `has-arrow` class and collapse functionality
- Active states should be clearly indicated
- Menu items should follow logical grouping

**Menu Structure**:
```
Dashboard
├── Dashboard (Overview)

Travel Management
├── Tours
│   ├── All Tours
│   └── Add New Tour
├── Categories
│   ├── All Categories
│   └── Add Category
├── Bookings
│   ├── All Bookings
│   ├── Pending
│   └── Confirmed
├── Users
│   ├── All Users
│   └── Add User
├── Roles & Permissions
│   ├── Roles
│   └── Policies
└── Pricing
    └── Price Types

Reports
├── Analytics
└── Reports

Settings
└── System Settings
```

#### 2. Header
**File**: `Views/Admin/_Header.cshtml`

**Components**:
- Mobile menu toggle button
- Notification bell with dropdown
- User profile dropdown with:
  - Profile link
  - Settings link
  - Back to Site link
  - Logout button

**Rules**:
- Header should be fixed at top
- Responsive design for mobile devices
- Notification badge should show count
- User avatar should be circular (35x35px)

#### 3. Main Content Area
**File**: `Views/Admin/_Layout.cshtml`

**Structure**:
```html
<div class="body-wrapper">
  <header><!-- Header --></header>
  <div class="body-wrapper-inner">
    <div class="container-fluid">
      @RenderBody()
    </div>
  </div>
</div>
```

---

## Page Components

### 1. Dashboard Page
**File**: `Views/Admin/Index.cshtml`

**Components**:
- **Overview Cards**: Key metrics and statistics
- **Charts**: Booking trends, revenue charts
- **Recent Bookings Table**: Latest booking information
- **Quick Stats**: Daily/weekly statistics
- **Comments Widget**: Recent customer feedback

**Layout Rules**:
- Use Bootstrap grid system (`row`, `col-lg-*`)
- Cards should have consistent padding and shadows
- Charts should be responsive and interactive
- Tables should be scrollable on mobile

### 2. Data Tables

**Standard Table Structure**:
```html
<div class="card">
  <div class="card-body">
    <div class="d-md-flex align-items-center">
      <div>
        <h4 class="card-title">Page Title</h4>
        <p class="card-subtitle">Description</p>
      </div>
      <div class="ms-auto">
        <!-- Action buttons -->
      </div>
    </div>
    <div class="table-responsive mt-4">
      <table class="table mb-0 text-nowrap varient-table align-middle fs-3">
        <!-- Table content -->
      </table>
    </div>
  </div>
</div>
```

**Table Rules**:
- Always use `table-responsive` wrapper
- Use `varient-table` class for styling
- Headers should be `text-muted`
- Action buttons should be grouped together
- Status badges should use appropriate colors

### 3. Forms

**Form Structure**:
```html
<div class="card">
  <div class="card-body">
    <h4 class="card-title">Form Title</h4>
    <form>
      <div class="row">
        <div class="col-md-6">
          <div class="mb-3">
            <label class="form-label">Label</label>
            <input type="text" class="form-control" />
          </div>
        </div>
      </div>
      <div class="d-flex gap-2">
        <button type="submit" class="btn btn-primary">Save</button>
        <button type="button" class="btn btn-secondary">Cancel</button>
      </div>
    </form>
  </div>
</div>
```

**Form Rules**:
- Use Bootstrap form classes
- Group related fields in rows
- Use appropriate input types
- Include validation messages
- Action buttons should be right-aligned

---

## Component Guidelines

### 1. Cards
**Usage**: All content should be wrapped in cards
**Classes**: `card`, `card-body`, `card-title`, `card-subtitle`

**Example**:
```html
<div class="card">
  <div class="card-body">
    <h4 class="card-title">Card Title</h4>
    <p class="card-subtitle">Card description</p>
    <!-- Card content -->
  </div>
</div>
```

### 2. Buttons
**Primary Actions**: `btn btn-primary`
**Secondary Actions**: `btn btn-secondary`
**Danger Actions**: `btn btn-danger`
**Success Actions**: `btn btn-success`
**Small Buttons**: `btn btn-sm`
**Large Buttons**: `btn btn-lg`

### 3. Badges
**Status Badges**:
- Success: `badge bg-success`
- Warning: `badge text-bg-warning`
- Danger: `badge bg-danger`
- Info: `badge bg-info`
- Secondary: `badge bg-secondary`

### 4. Icons
**Icon Library**: Tabler Icons (`ti ti-*`)
**Usage**: Always use semantic icons
**Sizes**: `fs-3`, `fs-4`, `fs-5`, `fs-6`, `fs-7`

### 5. Alerts
**Success**: `alert alert-success`
**Warning**: `alert alert-warning`
**Danger**: `alert alert-danger`
**Info**: `alert alert-info`

---

## Responsive Design Rules

### Breakpoints
- **Extra Small**: < 576px
- **Small**: ≥ 576px
- **Medium**: ≥ 768px
- **Large**: ≥ 992px
- **Extra Large**: ≥ 1200px

### Mobile Rules
- Sidebar should collapse on mobile
- Tables should be horizontally scrollable
- Forms should stack vertically
- Cards should take full width
- Touch targets should be at least 44px

### Desktop Rules
- Sidebar should be visible by default
- Tables should show all columns
- Forms can use multi-column layout
- Cards can be arranged in grid

---

## JavaScript Guidelines

### Required Scripts
```html
<!-- Core Scripts -->
<script src="~/assets/libs/jquery/dist/jquery.min.js"></script>
<script src="~/assets/libs/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
<script src="~/assets/js/sidebarmenu.js"></script>
<script src="~/assets/js/app.min.js"></script>

<!-- Charts -->
<script src="~/assets/libs/apexcharts/dist/apexcharts.min.js"></script>
<script src="~/assets/js/dashboard.js"></script>

<!-- Icons -->
<script src="https://cdn.jsdelivr.net/npm/iconify-icon@1.0.8/dist/iconify-icon.min.js"></script>
```

### Custom Scripts
- Place page-specific scripts in `@section Scripts`
- Use jQuery for DOM manipulation
- Follow Bootstrap JavaScript patterns
- Implement proper error handling

---

## Accessibility Guidelines

### ARIA Labels
- Use `aria-expanded` for collapsible elements
- Use `aria-labelledby` for dropdowns
- Use `aria-label` for icon-only buttons

### Keyboard Navigation
- All interactive elements should be keyboard accessible
- Use proper tab order
- Include focus indicators

### Screen Readers
- Use semantic HTML elements
- Provide alt text for images
- Use proper heading hierarchy

---

## Performance Guidelines

### Image Optimization
- Use appropriate image formats (WebP, PNG, JPG)
- Implement lazy loading for large images
- Optimize image sizes for different screen densities

### CSS Optimization
- Use minified CSS files
- Avoid inline styles
- Use CSS classes instead of IDs where possible

### JavaScript Optimization
- Use minified JavaScript files
- Implement proper caching
- Avoid blocking scripts

---

## Security Guidelines

### Input Validation
- Always validate user inputs
- Use proper encoding for output
- Implement CSRF protection

### Authentication
- Require authentication for all admin pages
- Implement proper session management
- Use secure cookies

---

## File Organization

### View Files Structure
```
Views/Admin/
├── _Layout.cshtml          # Main layout
├── _Header.cshtml          # Header component
├── _Sidebar.cshtml         # Sidebar navigation
├── _ViewImports.cshtml     # View imports
├── _ViewStart.cshtml       # View start
├── Index.cshtml            # Dashboard
├── Tour/                   # Tour management views
├── Booking/                # Booking management views
├── User/                   # User management views
├── Category/               # Category management views
├── Role/                   # Role management views
└── Policy/                 # Policy management views
```

### Asset Files Structure
```
wwwroot/assets/
├── css/
│   └── styles.min.css      # Main stylesheet
├── js/
│   ├── app.min.js          # Main JavaScript
│   ├── dashboard.js        # Dashboard scripts
│   └── sidebarmenu.js     # Sidebar scripts
├── images/
│   ├── logos/              # Logo files
│   └── profile/            # Profile images
└── libs/                   # Third-party libraries
```

---

## Testing Guidelines

### Browser Support
- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

### Device Testing
- Desktop (1920x1080, 1366x768)
- Tablet (768x1024)
- Mobile (375x667, 414x896)

### Functionality Testing
- All forms should work correctly
- All navigation should be functional
- All interactive elements should respond
- All data should display correctly

---

## Maintenance Guidelines

### Code Standards
- Follow consistent naming conventions
- Use meaningful variable and class names
- Comment complex functionality
- Keep code DRY (Don't Repeat Yourself)

### Documentation
- Update documentation when making changes
- Include inline comments for complex logic
- Maintain changelog for major updates

### Performance Monitoring
- Monitor page load times
- Check for JavaScript errors
- Validate HTML/CSS
- Test accessibility compliance

---

## Common Patterns

### 1. Data Loading States
```html
<div class="d-flex justify-content-center">
  <div class="spinner-border" role="status">
    <span class="visually-hidden">Loading...</span>
  </div>
</div>
```

### 2. Empty States
```html
<div class="text-center py-5">
  <i class="ti ti-inbox fs-1 text-muted"></i>
  <h4 class="mt-3">No Data Found</h4>
  <p class="text-muted">There are no items to display.</p>
  <button class="btn btn-primary">Add New Item</button>
</div>
```

### 3. Confirmation Dialogs
```html
<div class="modal fade" id="confirmModal">
  <div class="modal-dialog">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Confirm Action</h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
      </div>
      <div class="modal-body">
        <p>Are you sure you want to perform this action?</p>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
        <button type="button" class="btn btn-danger">Confirm</button>
      </div>
    </div>
  </div>
</div>
```

### 4. Success/Error Messages
```html
<!-- Success Message -->
<div class="alert alert-success alert-dismissible fade show" role="alert">
  <i class="ti ti-check-circle me-2"></i>
  Operation completed successfully!
  <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
</div>

<!-- Error Message -->
<div class="alert alert-danger alert-dismissible fade show" role="alert">
  <i class="ti ti-alert-circle me-2"></i>
  An error occurred. Please try again.
  <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
</div>
```

---

## Conclusion

This UI rule document provides comprehensive guidelines for maintaining consistency and quality in the Barefoot Travel Admin Panel. All developers should follow these rules to ensure a cohesive user experience across all admin pages.

For questions or clarifications about these guidelines, please contact the development team.
