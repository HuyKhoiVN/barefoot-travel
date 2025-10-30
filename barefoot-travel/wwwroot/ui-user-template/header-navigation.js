/**
 * Dynamic Header Navigation with Categories from API
 * Loads categories and builds dropdown menu structure
 */

(function($) {
    'use strict';

    // Cache for categories
    let categoriesCache = {
        homeCategories: [],
        tourCategories: [],
        tree: null
    };

    // ============================================
    // LOAD CATEGORIES FROM API
    // ============================================
    
    async function loadCategories() {
        try {
            // Load category tree structure
            const treeResponse = await $.ajax({
                url: '/api/Category/tree',
                type: 'GET',
                dataType: 'json'
            });

            if (treeResponse.success && treeResponse.data) {
                categoriesCache.tree = treeResponse.data;
                
                // Separate categories by type
                categoriesCache.homeCategories = filterCategoriesByType(treeResponse.data, 'HOME-CATEGORY');
                categoriesCache.tourCategories = filterCategoriesByType(treeResponse.data, 'TOURS');
                
                // Build navigation menu
                buildNavigationMenu();
            }
        } catch (error) {
            console.error('Error loading categories:', error);
            // Fallback to basic menu if API fails
            buildFallbackMenu();
        }
    }

    // ============================================
    // FILTER CATEGORIES BY TYPE
    // ============================================
    
    function filterCategoriesByType(categories, type) {
        if (!categories || !Array.isArray(categories)) return [];
        
        let filtered = [];
        
        categories.forEach(cat => {
            if (cat.type === type && cat.active) {
                filtered.push(cat);
            }
            
            // Recursively check children
            if (cat.children && cat.children.length > 0) {
                const childFiltered = filterCategoriesByType(cat.children, type);
                filtered = filtered.concat(childFiltered);
            }
        });
        
        return filtered;
    }

    // ============================================
    // BUILD NAVIGATION MENU
    // ============================================
    
    function buildNavigationMenu() {
        const $mainNav = $('#mainNav');
        $mainNav.empty();

        // 1. Type of Tours (HOME-CATEGORY)
        if (categoriesCache.homeCategories.length > 0) {
            const $typeOfToursItem = buildDropdownItem('Type of Tours', categoriesCache.homeCategories);
            $mainNav.append($typeOfToursItem);
        }

        // 2. Tour Categories (TOURS) - Group by parent
        const tourParents = categoriesCache.tourCategories.filter(cat => !cat.parentId);
        
        tourParents.forEach(parent => {
            const children = categoriesCache.tourCategories.filter(cat => cat.parentId === parent.id);
            const $tourItem = buildDropdownItem(parent.categoryName, children.length > 0 ? children : [parent]);
            $mainNav.append($tourItem);
        });

        // Initialize dropdown interactions
        initDropdownInteractions();
    }

    // ============================================
    // BUILD DROPDOWN ITEM
    // ============================================
    
    function buildDropdownItem(title, categories) {
        const $item = $('<div class="nav-item dropdown"></div>');
        
        // Title with chevron
        const $title = $(`
            <span class="nav-title">${sanitizeHtml(title)} <i class="fas fa-chevron-down"></i></span>
        `);
        $item.append($title);
        
        // Dropdown menu
        const $dropdown = $('<div class="dropdown-menu"></div>');
        
        if (categories && categories.length > 0) {
            categories.forEach(cat => {
                if (cat.active) {
                    // Parent category
                    if (cat.children && cat.children.length > 0) {
                        // Has children - create nested dropdown
                        const $parentItem = $(`
                            <div class="dropdown-item-parent">
                                <a href="/tours/${cat.id}" 
                                   data-category-id="${cat.id}"
                                   class="parent-link">
                                    ${sanitizeHtml(cat.categoryName)}
                                    <i class="fas fa-chevron-right"></i>
                                </a>
                                <div class="dropdown-submenu"></div>
                            </div>
                        `);
                        
                        const $submenu = $parentItem.find('.dropdown-submenu');
                        cat.children.forEach(child => {
                            if (child.active) {
                                const $childLink = $(`
                                    <a href="/tours/${child.id}" 
                                       data-category-id="${child.id}"
                                       class="sub-category">
                                        ${sanitizeHtml(child.categoryName)}
                                    </a>
                                `);
                                $submenu.append($childLink);
                            }
                        });
                        
                        $dropdown.append($parentItem);
                    } else {
                        // No children - direct link
                        const $link = $(`
                            <a href="/tours/${cat.id}" 
                               data-category-id="${cat.id}"
                               class="simple-link">
                                ${sanitizeHtml(cat.categoryName)}
                            </a>
                        `);
                        $dropdown.append($link);
                    }
                }
            });
        } else {
            $dropdown.append('<a href="#" class="simple-link">No categories available</a>');
        }
        
        $item.append($dropdown);
        return $item;
    }

    // ============================================
    // FALLBACK MENU
    // ============================================
    
    function buildFallbackMenu() {
        const $mainNav = $('#mainNav');
        $mainNav.html(`
            <div class="nav-item dropdown">
                <span>Type of Tours <i class="fas fa-chevron-down"></i></span>
                <div class="dropdown-menu">
                    <a href="#">Adventure Tours</a>
                    <a href="#">Cultural Tours</a>
                    <a href="#">Beach Tours</a>
                </div>
            </div>
            <div class="nav-item dropdown">
                <span>Vietnam Tours <i class="fas fa-chevron-down"></i></span>
                <div class="dropdown-menu">
                    <a href="#">North Vietnam</a>
                    <a href="#">Central Vietnam</a>
                    <a href="#">South Vietnam</a>
                </div>
            </div>
        `);
        
        initDropdownInteractions();
    }

    // ============================================
    // DROPDOWN INTERACTIONS
    // ============================================
    
    function initDropdownInteractions() {
        // Desktop hover for main dropdown
        $('.nav-item.dropdown').off('mouseenter mouseleave').hover(
            function() {
                $(this).find('> .dropdown-menu').stop().fadeIn(300);
            },
            function() {
                $(this).find('> .dropdown-menu').stop().fadeOut(300);
                $(this).find('.dropdown-submenu').hide();
            }
        );

        // Submenu hover/click
        $('.dropdown-item-parent').off('mouseenter mouseleave click').hover(
            function() {
                // Desktop hover
                if ($(window).width() > 768) {
                    $(this).find('.dropdown-submenu').stop().fadeIn(200);
                }
            },
            function() {
                if ($(window).width() > 768) {
                    $(this).find('.dropdown-submenu').stop().fadeOut(200);
                }
            }
        ).on('click', '.parent-link', function(e) {
            // Mobile/Desktop click toggle
            if ($(window).width() <= 768) {
                e.preventDefault();
                const $submenu = $(this).siblings('.dropdown-submenu');
                $('.dropdown-submenu').not($submenu).slideUp();
                $submenu.slideToggle();
            }
        });

        // Mobile click for main nav
        $('.nav-item.dropdown > span').off('click').on('click', function(e) {
            if ($(window).width() <= 768) {
                e.preventDefault();
                const $dropdown = $(this).siblings('.dropdown-menu');
                $('.dropdown-menu').not($dropdown).slideUp();
                $dropdown.slideToggle();
            }
        });

        // Close dropdowns when clicking outside
        $(document).off('click.dropdown').on('click.dropdown', function(e) {
            if (!$(e.target).closest('.nav-item.dropdown').length) {
                $('.dropdown-menu').fadeOut(200);
                $('.dropdown-submenu').hide();
            }
        });
    }

    // ============================================
    // SANITIZE HTML
    // ============================================
    
    function sanitizeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Removed categoryNameToSlug function - no longer needed with ID-based routing

    // ============================================
    // MOBILE MENU TOGGLE
    // ============================================
    
    function initMobileMenu() {
        $('#mobileMenuToggle').on('click', function() {
            $('#mainNav').toggleClass('active');
            $('#mobileOverlay').toggleClass('active');
            $(this).find('i').toggleClass('fa-bars fa-times');
        });

        $('#mobileOverlay').on('click', function() {
            $('#mainNav').removeClass('active');
            $(this).removeClass('active');
            $('#mobileMenuToggle').find('i').removeClass('fa-times').addClass('fa-bars');
        });
    }

    // ============================================
    // SEARCH FUNCTIONALITY
    // ============================================
    
    function initSearchBox() {
        $('.search-box input').on('keypress', function(e) {
            if (e.which === 13) { // Enter key
                e.preventDefault();
                const searchTerm = $(this).val().trim();
                if (searchTerm) {
                    window.location.href = `/tours?search=${encodeURIComponent(searchTerm)}`;
                }
            }
        });

        $('.search-box i').on('click', function() {
            const searchTerm = $('.search-box input').val().trim();
            if (searchTerm) {
                window.location.href = `/tours?search=${encodeURIComponent(searchTerm)}`;
            }
        });
    }

    // ============================================
    // INITIALIZE
    // ============================================
    
    function initHeaderNavigation() {
        loadCategories();
        initMobileMenu();
        initSearchBox();
        
        // Reload categories on window focus (in case they changed)
        $(window).on('focus', function() {
            if (document.visibilityState === 'visible') {
                // Debounce reload
                clearTimeout(window.categoryReloadTimeout);
                window.categoryReloadTimeout = setTimeout(loadCategories, 5000);
            }
        });
    }

    // ============================================
    // AUTO INITIALIZE ON DOM READY
    // ============================================
    
    $(document).ready(function() {
        initHeaderNavigation();
        console.log('📍 Header navigation initialized with dynamic categories');
    });

    // Expose to global scope if needed
    window.HeaderNavigation = {
        reload: loadCategories,
        getCategories: () => categoriesCache
    };

})(jQuery);

