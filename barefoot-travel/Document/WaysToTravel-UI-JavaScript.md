# Ways to Travel - JavaScript Implementation

## JavaScript Code to Add to Views/HomePageContent/Index.cshtml

Add this JavaScript after the existing homepage sections code (after line 611):

```javascript
<script>
    // Ways to Travel Variables
    let allWaysToTravelCategories = [];
    let currentImageGalleryTarget = null;
    let currentImagePreviewDiv = null;
    let currentImagePreviewImg = null;

    $(document).ready(function() {
        loadCategories();
        loadHomepageSections();
        loadWaysToTravelCategories();
        loadCategoriesForWaysToTravel();
    });

    // Load Ways to Travel categories
    function loadWaysToTravelCategories() {
        $.ajax({
            url: '/api/HomePage/ways-to-travel',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    allWaysToTravelCategories = response.data.categories;
                    displayWaysToTravelCategories(allWaysToTravelCategories);
                }
            },
            error: function() {
                showToast('Error loading Ways to Travel categories', 'danger');
            }
        });
    }

    // Load categories for dropdown
    function loadCategoriesForWaysToTravel() {
        $.ajax({
            url: '/api/Category',
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    const select = $('#wtCategorySelect');
                    select.empty();
                    select.append('<option value="">Choose a category...</option>');
                    
                    response.data.forEach(function(category) {
                        select.append(`<option value="${category.id}">${category.categoryName} (Type: ${category.type})</option>`);
                    });
                }
            }
        });
    }

    // Display Ways to Travel categories
    function displayWaysToTravelCategories(categories) {
        const container = $('#waysToTravelList');
        container.empty();

        if (!categories || categories.length === 0) {
            container.append(`
                <div class="col-12">
                    <div class="alert alert-info text-center">
                        <i class="ti ti-info-circle me-2"></i>
                        No categories configured yet. Click "Add Category to Ways to Travel" to get started.
                    </div>
                </div>
            `);
            return;
        }

        // Sort by display order
        categories.sort((a, b) => a.displayOrder - b.displayOrder);

        categories.forEach(function(category) {
            const imagesHtml = category.imageUrl2 
                ? `<div class="d-flex gap-2">
                     <img src="${category.imageUrl1}" alt="Image 1" class="img-thumbnail" style="max-width: 150px; max-height: 100px;">
                     <img src="${category.imageUrl2}" alt="Image 2" class="img-thumbnail" style="max-width: 150px; max-height: 100px;">
                   </div>`
                : `<img src="${category.imageUrl1}" alt="Image 1" class="img-thumbnail" style="max-width: 150px; max-height: 100px;">`;

            container.append(`
                <div class="col-md-6 col-lg-4 mb-4">
                    <div class="card border h-100">
                        <div class="card-header bg-light">
                            <div class="d-flex justify-content-between align-items-center">
                                <h6 class="mb-0">${category.categoryName}</h6>
                                <span class="badge bg-success">Active</span>
                            </div>
                        </div>
                        <div class="card-body">
                            <div class="mb-2">
                                <small class="text-muted">Tours:</small>
                                <div class="fw-bold">${category.totalTours} tours available</div>
                            </div>
                            <div class="mb-2">
                                <small class="text-muted">Images:</small>
                                ${imagesHtml}
                            </div>
                            <div class="mb-2">
                                <small class="text-muted">Display Order:</small>
                                <div>${category.displayOrder}</div>
                            </div>
                        </div>
                        <div class="card-footer bg-transparent border-0 pt-0">
                            <div class="btn-group w-100" role="group">
                                <button class="btn btn-sm btn-outline-primary" onclick="editWaysToTravel(${category.categoryId})" title="Edit">
                                    <i class="ti ti-edit"></i>
                                </button>
                                <button class="btn btn-sm btn-outline-warning" onclick="moveWaysToTravelUp(${category.categoryId})" title="Move Up">
                                    <i class="ti ti-arrow-up"></i>
                                </button>
                                <button class="btn btn-sm btn-outline-warning" onclick="moveWaysToTravelDown(${category.categoryId})" title="Move Down">
                                    <i class="ti ti-arrow-down"></i>
                                </button>
                                <button class="btn btn-sm btn-outline-danger" onclick="deleteWaysToTravel(${category.categoryId})" title="Delete">
                                    <i class="ti ti-trash"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            `);
        });
    }

    // Open image gallery modal
    window.openImageGallery = function(targetInputId, previewDivId, previewImgId) {
        currentImageGalleryTarget = targetInputId;
        currentImagePreviewDiv = previewDivId;
        currentImagePreviewImg = previewImgId;
        
        $('#imageGalleryModal').modal('show');
        loadUploadedImages();
    };

    // Load uploaded images
    function loadUploadedImages() {
        // This would load images from /uploads directory
        // For now, we'll use placeholder logic
        const uploadedImages = [
            '/uploads/e9094506.png',
            '/uploads/fbf5fcf5.jpg',
            '/uploads/b56feeab.jpg',
            '/uploads/1d658ab8.jpg',
            '/uploads/3ef43fff.jpg'
        ];

        const grid = $('#uploadedImagesGrid');
        grid.empty();

        uploadedImages.forEach(function(imageUrl) {
            grid.append(`
                <div class="col-md-3 col-sm-4 col-6">
                    <div class="card image-select-card" onclick="selectUploadedImage('${imageUrl}')">
                        <img src="${imageUrl}" class="card-img-top" alt="Image" style="height: 150px; object-fit: cover;">
                        <div class="card-body p-2 text-center">
                            <small class="text-muted">Click to select</small>
                        </div>
                    </div>
                </div>
            `);
        });
    }

    // Select uploaded image
    window.selectUploadedImage = function(imageUrl) {
        $('#' + currentImageGalleryTarget).val(imageUrl);
        $('#' + currentImagePreviewDiv).show();
        $('#' + currentImagePreviewImg).attr('src', imageUrl);
        $('#selectImageBtn').prop('disabled', false);
        $('#selectedImageUrl').val(imageUrl);
    };

    // Handle new image upload
    $('#newImageUpload').on('change', function(e) {
        const file = e.target.files[0];
        if (file) {
            // Validate file type
            const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
            if (!validTypes.includes(file.type)) {
                showToast('Invalid file type. Please select an image file.', 'danger');
                return;
            }

            // Validate file size (5MB max)
            if (file.size > 5 * 1024 * 1024) {
                showToast('File size too large. Maximum 5MB allowed.', 'danger');
                return;
            }

            // Preview image
            const reader = new FileReader();
            reader.onload = function(e) {
                $('#newImagePreview').show();
                $('#newImagePreviewImg').attr('src', e.target.result);
                $('#selectImageBtn').prop('disabled', false);
            };
            reader.readAsDataURL(file);
        }
    });

    // Apply selected image
    $('#selectImageBtn').on('click', function() {
        const imageUrl = $('#selectedImageUrl').val() || $('#newImagePreviewImg').attr('src');
        if (imageUrl) {
            $('#' + currentImageGalleryTarget).val(imageUrl);
            $('#' + currentImagePreviewDiv).show();
            $('#' + currentImagePreviewImg).attr('src', imageUrl);
            $('#imageGalleryModal').modal('hide');
            
            // Reset
            $('#newImageUpload').val('');
            $('#newImagePreview').hide();
            $('#selectImageBtn').prop('disabled', true);
            $('#selectedImageUrl').val('');
        }
    });

    // Save Ways to Travel category
    $('#saveWaysToTravelBtn').on('click', function() {
        const form = $('#waysToTravelForm')[0];
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        const categoryId = $('#wtCategorySelect').val();
        const imageUrl1 = $('#wtImage1Url').val();

        if (!imageUrl1) {
            showToast('Please select at least Image 1', 'danger');
            return;
        }

        const waysToTravelData = {
            imageUrl1: imageUrl1,
            imageUrl2: $('#wtImage2Url').val() || null,
            displayOrder: parseInt($('#wtDisplayOrder').val()),
            showInWaysToTravel: $('#wtIsActive').val() === 'true'
        };

        $.ajax({
            url: `/api/HomePage/category/${categoryId}/ways-to-travel`,
            type: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify(waysToTravelData),
            success: function(response) {
                if (response.success) {
                    showToast('Ways to Travel category configured successfully!', 'success');
                    $('#waysToTravelModal').modal('hide');
                    loadWaysToTravelCategories();
                } else {
                    showToast(response.message || 'Failed to save configuration', 'danger');
                }
            },
            error: function(xhr) {
                const errorMsg = xhr.responseJSON?.message || 'Failed to save configuration';
                showToast(errorMsg, 'danger');
            }
        });
    });

    // Edit Ways to Travel category
    window.editWaysToTravel = function(categoryId) {
        const category = allWaysToTravelCategories.find(c => c.categoryId === categoryId);
        if (!category) {
            showToast('Category not found', 'danger');
            return;
        }

        $('#wtCategoryId').val(category.categoryId);
        $('#wtCategorySelect').val(category.categoryId);
        $('#wtImage1Url').val(category.imageUrl1);
        $('#wtImage2Url').val(category.imageUrl2 || '');
        $('#wtDisplayOrder').val(category.displayOrder);
        $('#wtIsActive').val('true');

        // Show image previews
        if (category.imageUrl1) {
            $('#wtImage1Preview').show();
            $('#wtImage1PreviewImg').attr('src', category.imageUrl1);
        }
        if (category.imageUrl2) {
            $('#wtImage2Preview').show();
            $('#wtImage2PreviewImg').attr('src', category.imageUrl2);
        }

        $('#waysToTravelModal').modal('show');
    };

    // Delete Ways to Travel category
    window.deleteWaysToTravel = function(categoryId) {
        if (!confirm('Are you sure you want to remove this category from Ways to Travel?')) {
            return;
        }

        $.ajax({
            url: `/api/HomePage/category/${categoryId}/ways-to-travel`,
            type: 'DELETE',
            success: function(response) {
                if (response.success) {
                    showToast('Category removed from Ways to Travel', 'success');
                    loadWaysToTravelCategories();
                } else {
                    showToast(response.message || 'Failed to remove category', 'danger');
                }
            },
            error: function(xhr) {
                const errorMsg = xhr.responseJSON?.message || 'Failed to remove category';
                showToast(errorMsg, 'danger');
            }
        });
    };

    // Move up/down functions (simplified versions)
    window.moveWaysToTravelUp = function(categoryId) {
        // Implement reorder logic
        showToast('Reorder functionality to be implemented', 'info');
    };

    window.moveWaysToTravelDown = function(categoryId) {
        // Implement reorder logic
        showToast('Reorder functionality to be implemented', 'info');
    };

    // Reset modal on close
    $('#waysToTravelModal').on('hidden.bs.modal', function () {
        $('#waysToTravelForm')[0].reset();
        $('#wtImage1Preview').hide();
        $('#wtImage2Preview').hide();
        $('#waysToTravelModalLabel').text('Add Category to Ways to Travel');
    });

    // Add hidden input for selected image URL
    if ($('#selectedImageUrl').length === 0) {
        $('body').append('<input type="hidden" id="selectedImageUrl">');
    }
</script>
```

## Add CSS for Image Gallery

Add this CSS to the page (in the `<style>` section or CSS file):

```css
.image-select-card {
    cursor: pointer;
    transition: all 0.3s ease;
}

.image-select-card:hover {
    border-color: #0d6efd;
    box-shadow: 0 0 0 0.2rem rgba(13, 110, 253, 0.25);
    transform: translateY(-2px);
}
```

## Notes

1. **Image Upload**: Currently loads images from /uploads directory. You may need to implement a file upload API endpoint if you want users to upload new images.

2. **Validation**: File type and size validation is implemented client-side. Add server-side validation for security.

3. **Reorder**: Move up/down functionality is stubbed. You need to implement the API call to `/api/HomePage/ways-to-travel/reorder`.

4. **Image Gallery**: The modal shows uploaded images. You'll need to implement an API endpoint to list files from the `/uploads` directory.

