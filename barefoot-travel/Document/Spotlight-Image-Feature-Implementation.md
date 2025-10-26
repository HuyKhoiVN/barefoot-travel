# Spotlight Image Feature Implementation

## Overview
Added support for background image selection when configuring homepage sections with Spotlight layout. The image is displayed as the background of the spotlight card on the user-facing homepage.

## Database Changes

### No Direct Database Changes
No direct database schema changes are required. The spotlight image URL is stored in the existing `HomepageConfig` JSON field in the `Category` table.

### Data Migration
No data migration is needed as this is a new feature. Existing spotlight sections without images will continue to work (they just won't have a background image).

## API Changes

### DTOs Modified
**File:** `DTOs/HomePageConfigDto.cs`

Added new property to both `HomepageConfigDto` and `ConfigureHomepageDto`:
```csharp
[JsonPropertyName("spotlightImageUrl")]
public string? SpotlightImageUrl { get; set; }
```

### Service Layer Changes
**File:** `Services/HomePageService.cs`

Updated `ConfigureCategoryHomepageAsync` method to include `SpotlightImageUrl` when creating the config:
```csharp
var config = new HomepageConfigDto
{
    // ... existing properties
    SpotlightImageUrl = dto.SpotlightImageUrl
};
```

### API Endpoints
No new endpoints were added. The existing endpoint handles the new field:
- **PUT** `/api/HomePage/category/{categoryId}/homepage`

**Request Body:**
```json
{
    "homepageTitle": "Section Title",
    "layoutStyle": "spotlight",
    "maxItems": 3,
    "displayOrder": 0,
    "isActive": true,
    "badgeText": "FEATURED",
    "customClass": null,
    "spotlightImageUrl": "/uploads/images/spotlight-bg.jpg"
}
```

## Frontend Changes

### Admin UI (HomepageContent/Index.cshtml)

#### 1. Form Field Addition
Added new spotlight image upload field that:
- Only shows when "Spotlight" layout style is selected
- Is required when spotlight layout is selected
- Uses the same image gallery system as other image uploads

```html
<!-- Spotlight Image Container (shown only for Spotlight layout) -->
<div class="mb-3" id="spotlightImageContainer" style="display: none;">
    <label class="form-label">Spotlight Background Image <span class="text-danger">*</span></label>
    <div class="row">
        <div class="col-md-6">
            <div id="spotlightImagePreview" class="mb-2" style="display: none;">
                <img id="spotlightImagePreviewImg" src="" alt="Preview" class="img-thumbnail" style="max-height: 200px;">
            </div>
            <button type="button" class="btn btn-sm btn-outline-primary" onclick="openSpotlightImageGallery()">
                <i class="ti ti-photo me-1"></i>Select Image
            </button>
            <input type="hidden" id="spotlightImageUrl">
        </div>
    </div>
    <small class="text-muted d-block mt-2">This image will be used as the background for the spotlight card (Required for Spotlight layout)</small>
</div>
```

#### 2. JavaScript Functions
Added/modified functions:
- `openSpotlightImageGallery()` - Opens image gallery for spotlight image selection
- Layout style change handler - Shows/hides spotlight image field
- Save section validation - Validates spotlight image is selected for spotlight layout
- Edit section handler - Loads existing spotlight image URL

#### 3. Validation
- When layout style is "spotlight", the spotlight image field is required
- Validation error message: "Please select a spotlight background image"

### User-Facing Homepage (Home/Index.cshtml)

#### Rendering Changes
Updated both `renderLeftSpotlightLayout()` and `renderRightSpotlightLayout()` functions to apply background image with proper overlay structure:

```javascript
// Left spotlight layout
html += `<div class="left-spotlight-left">`;
html += `<div class="spotlight-card-background" style="background-image: url('${section.config.spotlightImageUrl || '/ui-user-template/images/home_background.jpg'}');"></div>`;
html += '<div class="spotlight-card-overlay"></div>';
html += '<div class="spotlight-card-content">';
// ... content ...
html += '</div>';
html += `</div>`;

// Right spotlight layout
html += `<div class="right-spotlight-right">`;
html += `<div class="right-spotlight-background" style="background-image: url('${section.config.spotlightImageUrl || '/ui-user-template/images/home_background.jpg'}');"></div>`;
html += '<div class="right-spotlight-overlay"></div>';
html += '<div class="right-spotlight-card-content">';
// ... content ...
html += '</div>';
html += `</div>`;
```

This structure (background → overlay → content) ensures the content is always visible and clickable, similar to the daily-tour-card implementation.

## Usage Flow

### For Administrators

1. Navigate to **Homepage Content Management**
2. Click **"Configure New Section"** or edit existing section
3. Select a category (type: TOURS)
4. Set Layout Style to **"Spotlight (Large cards)"**
5. The spotlight image upload field appears automatically
6. Click **"Select Image"** button
7. Choose an image from uploaded images or upload a new one
8. Fill in other required fields (Section Title, Badge Text, etc.)
9. Click **"Save Section"**

### Validation Rules
- Spotlight image is **required** when layout style is "spotlight"
- Image must be selected from uploaded images or uploaded new
- Supported formats: JPG, PNG, GIF, WebP
- Maximum file size: 10MB

## Display on User Homepage

### Spotlight Card Structure
```html
<div class="left-spotlight-left">
    <div class="spotlight-card-background" style="background-image: url('/uploads/images/spotlight-bg.jpg');"></div>
    <div class="spotlight-card-overlay"></div>
    <div class="spotlight-card-content">
        <div class="spotlight-badge">FEATURED</div>
        <h1 class="responsive-text-xl">Best Choice</h1>
        <p>Arrange your border crossings stress-free by handling everything at once!</p>
        <button class="book-now-btn">BOOK NOW</button>
    </div>
</div>
```

**Alternative structure for right-spotlight layout:**
```html
<div class="right-spotlight-right">
    <div class="right-spotlight-background" style="background-image: url('/uploads/images/spotlight-bg.jpg');"></div>
    <div class="right-spotlight-overlay"></div>
    <div class="right-spotlight-card-content">
        <div class="spotlight-badge">TOP LIST</div>
        <h1 class="responsive-text-xl">Best Tours</h1>
        <p>Discover our expertly crafted tours!</p>
        <button class="book-now-btn">BOOK NOW</button>
    </div>
</div>
```

### Visual Effect
- Image covers the entire spotlight card area
- Dark overlay is applied on top of the image for better text readability
- Content (badge, title, description, button) is displayed on top of the overlay with proper z-index
- Image maintains aspect ratio and covers the entire area (`background-size: cover`)
- Image is centered (`background-position: center`)
- Overlay uses gradient: `linear-gradient(135deg, rgba(0, 0, 0, 0.5) 0%, rgba(0, 0, 0, 0.7) 100%)`
- Hover effect: Background image scales to 1.05 on hover
- **Z-index layers:**
  - Background: z-index: 0
  - Overlay: z-index: 1
  - Content: z-index: 2 (ensures content is always clickable and visible)

## Technical Notes

### JSON Storage Format
The spotlight image URL is stored in the `HomepageConfig` JSON field:

```json
{
    "layoutStyle": "spotlight",
    "maxItems": 3,
    "displayOrder": 0,
    "isActive": true,
    "badgeText": "FEATURED",
    "customClass": null,
    "spotlightImageUrl": "/uploads/images/spotlight-bg.jpg"
}
```

### Image Path
- Images are uploaded via `/api/FileUpload/image` endpoint
- Uploaded images are stored in `wwwroot/uploads/` directory
- URLs are returned as relative paths (e.g., `/uploads/images/filename.jpg`)

## Testing Checklist

- [ ] Spotlight image field appears when "Spotlight" layout is selected
- [ ] Spotlight image field is hidden for other layout styles
- [ ] Validation error when saving spotlight section without image
- [ ] Image preview works correctly
- [ ] Image is displayed as background on user homepage
- [ ] Existing spotlight sections without images still work
- [ ] Edit existing section loads spotlight image correctly
- [ ] Modal reset clears spotlight image field

## Files Modified

1. `DTOs/HomePageConfigDto.cs` - Added SpotlightImageUrl property
2. `Services/HomePageService.cs` - Updated config creation
3. `Views/HomePageContent/Index.cshtml` - Added UI and JavaScript
4. `Views/Home/Index.cshtml` - Updated rendering with background image and overlay structure
5. `wwwroot/css/user/style.css` - Updated CSS for spotlight cards with proper layering and override old styles

## CSS Override Fix

Due to old CSS styles in `ui-user-template/style.css` conflicting with the new structure, we added important overrides in `css/user/style.css`:

```css
.left-spotlight-left,
.right-spotlight-right {
  background: none !important;
}

.left-spotlight-left::before,
.right-spotlight-right::before {
  display: none !important;
}

.left-spotlight-left > h1,
.left-spotlight-left > p,
.right-spotlight-right > h1,
.right-spotlight-right > p {
  display: none !important;
}
```

This ensures the new layered structure (background → overlay → content) works correctly without interference from old CSS.

## Future Enhancements

- Add image cropping/editing capabilities
- Support multiple images per spotlight card
- Add image overlay effects (darkening, blurring)
- Support responsive images for different screen sizes
