# Homepage Content - Image Upload Implementation

## Overview
The homepage content management system now properly handles image uploads by sending actual image files to the backend API, which processes, stores them in `wwwroot/uploads`, and returns the file path for database storage.

## Key Changes

### 1. Backend Image Handling
The backend already has proper image upload infrastructure:

**API Endpoint:** `POST /api/FileUpload/image`
- Accepts `IFormFile` parameter
- Validates file type (JPG, JPEG, PNG, GIF, WebP)
- Validates file size (max 10MB)
- Saves file to `wwwroot/uploads/` folder
- Generates unique filename using SHA256 hash
- Returns file URL (e.g., `/uploads/abc123de.jpg`)

**Image Listing:** `GET /api/FileUpload/images`
- Returns list of all uploaded images from `wwwroot/uploads/`

**Service:** `Services/IFileUploadService.cs`
- `UploadImageAsync()`: Handles file upload and storage
- `GetImagesAsync()`: Lists all uploaded images
- Files are saved to: `wwwroot/{folder}/{uniqueFileName}`

### 2. Frontend Changes

#### Image Upload Flow
1. User selects file from "Upload New" tab
2. File is immediately uploaded to `/api/FileUpload/image`
3. Backend saves file to `wwwroot/uploads/`
4. Backend returns URL (e.g., `/uploads/abc123de.jpg`)
5. URL is stored in hidden input
6. User clicks "Select This Image" button
7. URL is applied to the target input field

#### Uploaded Images Display
- Images are loaded from `/api/FileUpload/images` API
- Shows all files from `wwwroot/uploads/` directory
- Images are clickable for selection
- Displays file name and preview

### 3. Database Storage
- Database stores **file paths** (e.g., `/uploads/abc123de.jpg`), NOT base64 data
- Images are stored in `wwwroot/uploads/` directory
- File paths are stored in:
  - `Category.WaysToTravelImage1`
  - `Category.WaysToTravelImage2`

## Implementation Details

### File Upload Process
```javascript
// 1. User selects file
$('#newImageUpload').on('change', function(e) {
    const file = e.target.files[0];
    
    // 2. Validate file type and size
    // 3. Create FormData
    const formData = new FormData();
    formData.append('file', file);
    
    // 4. Upload to backend
    $.ajax({
        url: '/api/FileUpload/image',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(response) {
            // 5. Backend returns: { fileUrl: "/uploads/abc123de.jpg" }
            // 6. Store URL for later use
            $('#selectedImageUrl').val(response.data.fileUrl);
        }
    });
});

// 7. User clicks "Select This Image"
$('#selectImageBtn').on('click', function() {
    const imageUrl = $('#selectedImageUrl').val();
    
    // 8. Apply URL to target input
    $('#' + currentImageGalleryTarget).val(imageUrl);
});
```

### Saving to Database
```javascript
// When saving Ways to Travel configuration
const waysToTravelData = {
    imageUrl1: $('#wtImage1Url').val(), // e.g., "/uploads/abc123de.jpg"
    imageUrl2: $('#wtImage2Url').val() || null,
    displayOrder: parseInt($('#wtDisplayOrder').val()),
    showInWaysToTravel: $('#wtIsActive').val() === 'true'
};

$.ajax({
    url: `/api/HomePage/category/${categoryId}/ways-to-travel`,
    type: 'PUT',
    contentType: 'application/json',
    data: JSON.stringify(waysToTravelData),
    // ...
});
```

Backend receives the URL and stores it in the database:
```csharp
category.WaysToTravelImage1 = dto.ImageUrl1; // Store path, not file
category.WaysToTravelImage2 = dto.ImageUrl2;
```

## Security Considerations

### File Validation
- ✅ File type validation (only image types allowed)
- ✅ File size limit (10MB max)
- ✅ Unique filename generation (prevents overwrites)
- ✅ Secure file storage in `wwwroot/uploads/`

### File Storage
- Files are stored in `wwwroot/uploads/`
- Generated unique filenames prevent conflicts
- File paths are stored in database, not base64 data
- Images can be accessed via static file serving

## API Flow Summary

### 1. Upload Image
```
Frontend → POST /api/FileUpload/image
         → Backend receives IFormFile
         → Validates & saves to wwwroot/uploads/
         → Returns: { fileUrl: "/uploads/abc123de.jpg" }
         ← Frontend receives URL
```

### 2. List Images
```
Frontend → GET /api/FileUpload/images
         → Backend scans wwwroot/uploads/
         → Returns: [{ fileUrl, fileName, fileSize, ... }]
         ← Frontend displays in gallery
```

### 3. Save Configuration
```
Frontend → PUT /api/HomePage/category/{id}/ways-to-travel
         → Body: { imageUrl1: "/uploads/abc123de.jpg", ... }
         → Backend saves URL to database
         → Returns: Success
```

## Benefits of This Approach

1. **No Base64 in Database**: Database stores small file paths, not large base64 strings
2. **Efficient Storage**: Files are stored on disk, not in database
3. **Security**: File validation and size limits prevent abuse
4. **Scalability**: Can easily move files to CDN or cloud storage
5. **Browser Caching**: Static files can be cached by browser
6. **Performance**: Smaller database, faster queries

## Testing Checklist

- [x] Upload new image file
- [x] List existing uploaded images
- [x] Select image from gallery
- [x] Validate file type (JPG, PNG, GIF, WebP)
- [x] Validate file size (max 10MB)
- [x] Save configuration with image URLs
- [x] Images display correctly on homepage
- [x] Image paths stored correctly in database

## Notes

- Files are permanently stored in `wwwroot/uploads/`
- Consider implementing cleanup for unused images
- File upload requires authentication (Authorize attribute)
- Image upload endpoint is at `/api/FileUpload/image`
- Images are served as static files from `wwwroot/`
