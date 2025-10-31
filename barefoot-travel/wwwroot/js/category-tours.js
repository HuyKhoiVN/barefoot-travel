/**
 * Category Tours Page JavaScript
 * Handles tour listing, filtering, sorting, and pagination
 */

(function() {
    'use strict';

    // State management
    const state = {
        currentCategory: null,
        currentCategoryId: null,
        currentPage: 1,
        pageSize: 12, // 4 columns x 3 rows = 12 tours per page
        totalPages: 0,
        totalTours: 0,
        sortBy: 'title',
        sortOrder: 'asc',
        searchQuery: '',
        childCategories: []
    };

    // Initialize on document ready
    $(document).ready(function() {
        console.log('📄 Document ready - Starting initialization');
        console.log('🔍 window.CATEGORY_ID at document.ready:', window.CATEGORY_ID);
        
        initializePage();
        initEventListeners();
        initCurrencySelector();
    });

    /**
     * Initialize page
     */
    function initializePage() {
        // Get category ID from window
        console.log('🔍 DEBUG - window.CATEGORY_ID:', window.CATEGORY_ID);
        console.log('🔍 DEBUG - typeof:', typeof window.CATEGORY_ID);
        
        const categoryId = window.CATEGORY_ID;
        
        if (!categoryId || categoryId <= 0) {
            console.error('❌ ERROR - Category ID not provided or invalid');
            console.error('   window.CATEGORY_ID value:', window.CATEGORY_ID);
            showError('Category not found');
            return;
        }

        console.log('✅ Initializing page for category ID:', categoryId);
        
        // Load category data by ID
        loadCategoryDataById(categoryId);
    }

    /**
     * Initialize event listeners
     */
    function initEventListeners() {
        // Search input
        let searchTimeout;
        $('#tourSearch').on('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(function() {
                state.searchQuery = $('#tourSearch').val().trim();
                state.currentPage = 1;
                loadTours();
            }, 500);
        });

        // Sort select
        $('#sortSelect').on('change', function() {
            const sortValue = $(this).val().split('_');
            state.sortBy = sortValue[0];
            state.sortOrder = sortValue[1];
            state.currentPage = 1;
            loadTours();
        });

        // Back to top button
        $('#backToTop').on('click', function() {
            $('html, body').animate({ scrollTop: 0 }, 600);
        });

        // Show/hide back to top button
        $(window).on('scroll', function() {
            if ($(this).scrollTop() > 300) {
                $('#backToTop').fadeIn();
            } else {
                $('#backToTop').fadeOut();
            }
        });
    }

    /**
     * Load category data by ID
     */
    function loadCategoryDataById(categoryId) {
        $.ajax({
            url: `/api/Category/${categoryId}`,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    state.currentCategory = response.data;
                    state.currentCategoryId = response.data.id;
                    
                    console.log('✅ Category loaded:', response.data);
                    
                    // Update page elements
                    updateCategoryInfo(response.data);
                    
                    // Load child categories
                    loadChildCategories(response.data.id);
                    
                    // Load tours
                    loadTours();
                } else {
                    console.error('❌ Category not found in response');
                    showError('Category not found');
                }
            },
            error: function(xhr) {
                console.error('❌ Error loading category:', xhr);
                if (xhr.status === 404) {
                    showError('Category not found');
                } else {
                    showError('Failed to load category');
                }
            }
        });
    }

    /**
     * Update category information on page
     */
    function updateCategoryInfo(category) {
        $('#categoryBreadcrumb').text(category.categoryName);
        $('#categoryTitle').text(category.categoryName);
        // Tour count will be updated after loading tours
        document.title = `${category.categoryName} Tours - Barefoot Atlas Travel`;
    }

    /**
     * Load child categories (tree structure)
     */
    function loadChildCategories(categoryId) {
        $.ajax({
            url: `/api/Category/${categoryId}/children-tree`,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    state.childCategories = response.data;
                    renderCategoryTree(response.data);
                } else {
                    // No children, hide category tree or show current category only
                    renderEmptyTree();
                }
            },
            error: function(xhr) {
                console.error('Error loading child categories:', xhr);
                renderEmptyTree();
            }
        });
    }

    /**
     * Render category tree
     */
    function renderCategoryTree(categories) {
        const container = $('#categoryTree');
        
        if (!categories || categories.length === 0) {
            renderEmptyTree();
            return;
        }

        let html = '<ul>';
        
        categories.forEach(function(category) {
            html += renderCategoryTreeItem(category);
        });
        
        html += '</ul>';
        
        container.html(html);
        
        // Show reset button
        $('#resetFiltersBtn').show();
        
        // Add checkbox change handlers
        $('.category-checkbox').on('change', function() {
            const categorySlug = $(this).data('category-slug');
            const categoryName = $(this).closest('.category-tree-label').find('.category-tree-name').text();
            const isChecked = $(this).is(':checked');
            
            if (isChecked) {
                // Navigate ONLY if category has slug
                if (categorySlug) {
                    window.location.href = `/categories/${categorySlug}`;
                } else {
                    // No slug - uncheck and stay on page
                    $(this).prop('checked', false);
                    console.warn('⚠️ Category has no slug, cannot navigate:', categoryName);
                    alert('This category is not yet configured with a URL. Please contact admin.');
                }
            }
        });
        
        // Add reset button handler
        $('#resetFiltersBtn').off('click').on('click', function() {
            // Navigate back to parent/main category
            window.location.reload();
        });
    }

    /**
     * Render single category tree item
     */
    function renderCategoryTreeItem(category) {
        const isActive = category.id === state.currentCategoryId;
        const tourCount = category.totalTours || 0;
        const categorySlug = category.slug || '';
        
        let html = '<li class="category-tree-item">';
        html += `<label class="category-tree-label">`;
        html += `<input type="checkbox" class="category-checkbox" 
                    data-category-id="${category.id}"
                    data-category-slug="${categorySlug}"
                    ${isActive ? 'checked' : ''}>`;
        html += `<div class="category-name-wrapper">`;
        html += `<span class="category-tree-name">${escapeHtml(category.categoryName)}</span>`;
        if (tourCount > 0) {
            html += `<span class="category-tree-count">(${tourCount})</span>`;
        }
        html += `</div>`;
        html += `</label>`;
        
        // Render children if any
        if (category.children && category.children.length > 0) {
            html += '<ul class="category-tree-children">';
            category.children.forEach(function(child) {
                html += renderCategoryTreeItem(child);
            });
            html += '</ul>';
        }
        
        html += '</li>';
        
        return html;
    }

    /**
     * Render empty tree (when no children)
     */
    function renderEmptyTree() {
        const container = $('#categoryTree');
        
        let html = '<ul>';
        html += '<li class="category-tree-item">';
        html += `<label class="category-tree-label">`;
        html += `<input type="checkbox" class="category-checkbox" checked disabled>`;
        html += `<div class="category-name-wrapper">`;
        html += `<span class="category-tree-name">${escapeHtml(state.currentCategory.categoryName)}</span>`;
        html += `</div>`;
        html += `</label>`;
        html += '</li>';
        html += '</ul>';
        
        container.html(html);
        
        // Hide reset button if no children
        $('#resetFiltersBtn').hide();
    }

    // Removed categoryNameToSlug function - no longer needed with ID-based routing

    /**
     * Load tours with current filters
     */
    function loadTours() {
        if (!state.currentCategoryId) {
            console.error('❌ Cannot load tours: currentCategoryId is null');
            return;
        }
        
        // Show loading
        showToursLoading();
        
        // Build query params
        const params = {
            page: state.currentPage,
            pageSize: state.pageSize,
            sortBy: state.sortBy,
            sortOrder: state.sortOrder
        };
        
        if (state.searchQuery) {
            params.search = state.searchQuery;
        }
        
        // Use new API endpoint that handles category hierarchy
        const apiUrl = `/api/Tour/by-category/${state.currentCategoryId}/paged`;
        
        console.log('🔍 Loading tours from API:', apiUrl);
        console.log('🔍 Params:', params);
        
        $.ajax({
            url: apiUrl,
            type: 'GET',
            data: params,
            success: function(response) {
                console.log('✅ Tours API response:', response);
                console.log('   Sample tour images:', response.items && response.items[0] ? response.items[0].images : 'No items');
                
                if (response && response.items) {
                    state.totalPages = response.totalPages;
                    state.totalTours = response.totalItems;
                    
                    // Update tour count in banner (just the number)
                    $('#tourCount').text(response.totalItems);
                    
                    // Render tours
                    renderTours(response.items);
                    
                    // Update results info
                    updateResultsInfo(response);
                    
                    // Render pagination
                    renderPagination(response);
                } else {
                    showToursEmpty();
                }
            },
            error: function(xhr) {
                console.error('❌ Error loading tours:', xhr);
                showToursError();
            }
        });
    }

    /**
     * Show tours loading state
     */
    function showToursLoading() {
        const container = $('#toursGrid');
        container.html(`
            <div class="grid-loading">
                <i class="fas fa-spinner fa-spin"></i>
                <p>Loading tours...</p>
            </div>
        `);
    }

    /**
     * Show tours empty state
     */
    function showToursEmpty() {
        const container = $('#toursGrid');
        container.html(`
            <div class="tours-grid-empty">
                <i class="fas fa-search"></i>
                <h3>No tours found</h3>
                <p>Try adjusting your search or filters</p>
            </div>
        `);
        
        $('#resultsCount').text('No tours found');
        $('#paginationContainer').empty();
    }

    /**
     * Show tours error state
     */
    function showToursError() {
        const container = $('#toursGrid');
        container.html(`
            <div class="tours-grid-empty">
                <i class="fas fa-exclamation-triangle"></i>
                <h3>Error loading tours</h3>
                <p>Please try again later</p>
            </div>
        `);
    }

    /**
     * Render tours grid
     */
    function renderTours(tours) {
        const container = $('#toursGrid');
        
        if (!tours || tours.length === 0) {
            showToursEmpty();
            return;
        }
        
        container.empty();
        
        tours.forEach(function(tour) {
            const tourCard = createTourCard(tour);
            container.append(tourCard);
        });
    }

    /**
     * Create tour card HTML (full copy from Home page structure)
     */
    function createTourCard(tour) {
        // Get images from API response (images is array of objects with imageUrl property)
        // API returns: images: [{ id: 1, imageUrl: "url.jpg", isBanner: true }, ...]
        const imageObjects = tour.images || [];
        
        // Extract imageUrl from objects and sort by isBanner (banner first)
        const images = imageObjects
            .sort((a, b) => {
                const aBanner = a.isBanner || a.IsBanner || false;
                const bBanner = b.isBanner || b.IsBanner || false;
                return (bBanner ? 1 : 0) - (aBanner ? 1 : 0);
            })
            .map(img => img.imageUrl || img.ImageUrl || img);
        
        const mainImage = images[0] || '/ui-user-template/images/home_background.jpg';
        const galleryImage = images.length >= 2 ? images[1] : mainImage;
        
        const price = formatPrice(tour.pricePerPerson);
        
        // Get category name from categories array (API returns array of objects)
        // categories: [{ id: 1, categoryName: "Ha Long", type: "Destination" }, ...]
        const categoryObjects = tour.categories || [];
        const categoryName = categoryObjects.length > 0 && categoryObjects[0].categoryName 
            ? categoryObjects[0].categoryName 
            : (state.currentCategory ? state.currentCategory.categoryName : '');
        
        // Create card with exact structure from Home page
        const card = $(`
            <div class="product-card" data-tour-id="${tour.id}">
                <div class="product-image-wrapper">
                    <img src="${mainImage}" alt="${escapeHtml(tour.title)}" class="main-image" loading="lazy">
                    <img src="${galleryImage}" alt="Gallery Image" class="gallery-image" loading="lazy">
                    <div class="wishlist-btn" title="Add to wishlist">
                        <i class="fas fa-heart"></i>
                    </div>
                </div>
                <div class="product-content">
                    <p class="product-categories">
                        <a href="#" onclick="return false;">${escapeHtml(categoryName)}</a>
                    </p>
                    <h3 class="product-title">
                        <a href="#" data-tour-id="${tour.id}">${escapeHtml(tour.title)}</a>
                    </h3>
                    <div class="product-price">
                        <span class="amount">${price}</span>
                    </div>
                    <a href="#" class="read-more-btn" data-tour-id="${tour.id}">Read more</a>
                </div>
            </div>
        `);
        
        // Wishlist button handler - with active state toggle
        card.find('.wishlist-btn').on('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            // Toggle active state and icon
            $(this).toggleClass('active');
            const icon = $(this).find('i');
            
            if ($(this).hasClass('active')) {
                icon.removeClass('far').addClass('fas');
                // Add a small bounce animation
                $(this).css('transform', 'scale(1.3)');
                setTimeout(() => {
                    $(this).css('transform', '');
                }, 200);
            } else {
                icon.removeClass('fas').addClass('far');
            }
            
            // TODO: Add to wishlist API call
        });
        
        // Add click handlers for title and read more button
        card.find('.product-title a, .read-more-btn').on('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            const tourId = $(this).data('tour-id');
            viewTourDetails(tourId);
        });
        
        // Card click handler - entire card is clickable
        card.on('click', function(e) {
            // Don't trigger if clicking wishlist button or links
            if (!$(e.target).closest('.wishlist-btn, a').length) {
                viewTourDetails(tour.id);
            }
        });
        
        return card;
    }

    /**
     * View tour details
     */
    function viewTourDetails(tourId) {
        $.ajax({
            url: `/api/Tour/public/${tourId}`,
            type: 'GET',
            success: function(response) {
                if (response.success && response.data) {
                    const tour = response.data;
                    console.log('Tour details:', tour);
                    // TODO: Navigate to tour detail page or show modal
                    alert(`Tour: ${tour.title}\nPrice: ${formatPrice(tour.pricePerPerson)}`);
                } else {
                    alert('Tour not found');
                }
            },
            error: function(xhr) {
                console.error('Error loading tour details:', xhr);
                alert('Failed to load tour details');
            }
        });
    }

    /**
     * Update results info text
     */
    function updateResultsInfo(response) {
        const start = (response.currentPage - 1) * response.pageSize + 1;
        const end = Math.min(response.currentPage * response.pageSize, response.totalItems);
        const total = response.totalItems;
        
        $('#resultsCount').text(`Showing ${start} - ${end} of ${total} tour${total !== 1 ? 's' : ''}`);
    }

    /**
     * Render pagination
     */
    function renderPagination(response) {
        const container = $('#paginationContainer');
        container.empty();
        
        if (response.totalPages <= 1) {
            return;
        }
        
        // Previous button
        const prevButton = $(`
            <button class="pagination-button" ${response.currentPage === 1 ? 'disabled' : ''}>
                <i class="fas fa-chevron-left"></i>
            </button>
        `);
        prevButton.on('click', function() {
            if (state.currentPage > 1) {
                state.currentPage--;
                loadTours();
                scrollToTop();
            }
        });
        container.append(prevButton);
        
        // Page numbers
        const pages = generatePageNumbers(response.currentPage, response.totalPages);
        pages.forEach(function(page) {
            if (page === '...') {
                container.append('<span class="pagination-ellipsis">...</span>');
            } else {
                const pageButton = $(`
                    <button class="pagination-button ${page === response.currentPage ? 'active' : ''}">
                        ${page}
                    </button>
                `);
                pageButton.on('click', function() {
                    state.currentPage = page;
                    loadTours();
                    scrollToTop();
                });
                container.append(pageButton);
            }
        });
        
        // Next button
        const nextButton = $(`
            <button class="pagination-button" ${response.currentPage === response.totalPages ? 'disabled' : ''}>
                <i class="fas fa-chevron-right"></i>
            </button>
        `);
        nextButton.on('click', function() {
            if (state.currentPage < state.totalPages) {
                state.currentPage++;
                loadTours();
                scrollToTop();
            }
        });
        container.append(nextButton);
    }

    /**
     * Generate page numbers for pagination
     */
    function generatePageNumbers(current, total) {
        const pages = [];
        const delta = 2; // Number of pages to show on each side of current page
        
        for (let i = 1; i <= total; i++) {
            if (i === 1 || i === total || (i >= current - delta && i <= current + delta)) {
                pages.push(i);
            } else if (pages[pages.length - 1] !== '...') {
                pages.push('...');
            }
        }
        
        return pages;
    }

    /**
     * Scroll to top of tours section
     */
    function scrollToTop() {
        $('html, body').animate({
            scrollTop: $('.category-tours-section').offset().top - 80
        }, 400);
    }

    /**
     * Format price with currency
     */
    function formatPrice(price) {
        if (typeof CurrencyConverter !== 'undefined') {
            return CurrencyConverter.formatPrice(price, true);
        }
        return '₫' + price.toLocaleString('en-US');
    }

    /**
     * Escape HTML to prevent XSS
     */
    function escapeHtml(text) {
        if (!text) return '';
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, function(m) { return map[m]; });
    }

    /**
     * Show error message
     */
    function showError(message) {
        $('#categoryTitle').text('Error');
        $('#categoryDescription').text(message);
        $('#tourCount').text('0 Tours');
        showToursError();
    }

    /**
     * Initialize currency selector
     */
    function initCurrencySelector() {
        // Set initial display
        if (typeof CurrencyConverter !== 'undefined') {
            const currentCurrency = CurrencyConverter.getCurrency();
            $('#currentCurrencyDisplay').text(CurrencyConverter.getSymbol());
        }
        
        // Add click handlers
        $('.currency-option').on('click', function(e) {
            e.preventDefault();
            const currency = $(this).data('currency');
            if (typeof CurrencyConverter !== 'undefined') {
                CurrencyConverter.setCurrency(currency);
                $('#currentCurrencyDisplay').text(CurrencyConverter.getSymbol());
                $('.currency-dropdown-menu').hide();
                // Reload tours to update prices
                loadTours();
            }
        });
        
        // Toggle dropdown
        $('#currencySelector').on('click', function(e) {
            e.stopPropagation();
            $('.currency-dropdown-menu').toggle();
        });
        
        // Close dropdown when clicking outside
        $(document).on('click', function() {
            $('.currency-dropdown-menu').hide();
        });
    }

})();

