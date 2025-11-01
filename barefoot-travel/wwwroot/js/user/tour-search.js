/**
 * Tour Live Search with Dropdown Results
 * Features:
 * - Debounced search input
 * - Live dropdown results with tour info
 * - Click to navigate to tour detail
 * - Keyboard navigation support (arrow keys, enter, escape)
 */

(function() {
    'use strict';

    // Configuration
    const CONFIG = {
        searchDelay: 300, // ms
        minQueryLength: 2,
        maxResults: 10,
        apiEndpoint: '/api/tour/search'
    };

    // Debounce function
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Format currency
    function formatCurrency(amount) {
        // Check if global currency formatter exists
        if (typeof window.formatCurrency === 'function') {
            return window.formatCurrency(amount);
        }
        // Fallback to VND format
        return new Intl.NumberFormat('vi-VN', { 
            style: 'currency', 
            currency: 'VND',
            maximumFractionDigits: 0
        }).format(amount);
    }

    // Initialize search for a specific input
    function initializeSearch(inputElement) {
        if (!inputElement) return;

        const searchContainer = inputElement.closest('.search-box') || 
                               inputElement.closest('.globe-search-container') ||
                               inputElement.closest('.search-form');
        
        if (!searchContainer) {
            console.warn('Search container not found for input:', inputElement);
            return;
        }

        // For globe search, ensure container stays expanded when interacting with dropdown
        const isGlobeSearch = searchContainer.classList.contains('globe-search-container');

        // Create dropdown container if it doesn't exist
        let dropdown = searchContainer.querySelector('.search-results-dropdown');
        if (!dropdown) {
            dropdown = document.createElement('div');
            dropdown.className = 'search-results-dropdown';
            dropdown.style.display = 'none';
            
            // For globe search, append to the search container
            if (isGlobeSearch) {
                searchContainer.style.position = 'relative';
            } else {
                searchContainer.style.position = 'relative';
            }
            
            searchContainer.appendChild(dropdown);
        }

        let currentSelectedIndex = -1;
        let currentResults = [];

        // Search function
        async function performSearch(query) {
            if (!query || query.length < CONFIG.minQueryLength) {
                hideDropdown();
                return;
            }

            try {
                const response = await fetch(
                    `${CONFIG.apiEndpoint}?q=${encodeURIComponent(query)}&limit=${CONFIG.maxResults}`
                );
                
                if (!response.ok) {
                    throw new Error('Search request failed');
                }

                const data = await response.json();
                
                if (data.success && data.data && data.data.length > 0) {
                    currentResults = data.data;
                    showResults(data.data);
                } else {
                    showNoResults(query);
                }
            } catch (error) {
                console.error('Search error:', error);
                showError();
            }
        }

        // Debounced search
        const debouncedSearch = debounce(performSearch, CONFIG.searchDelay);

        // Show results in dropdown
        function showResults(results) {
            dropdown.innerHTML = results.map((tour, index) => `
                <a href="/tours/${tour.slug || tour.id}" 
                   class="search-result-item" 
                   data-index="${index}"
                   data-tour-id="${tour.id}">
                    <div class="search-result-image">
                        ${tour.imageUrl ? 
                            `<img src="${tour.imageUrl}" alt="${tour.title}" onerror="this.src='/images/placeholder-tour.jpg'">` :
                            `<div class="no-image"><i class="fas fa-image"></i></div>`
                        }
                    </div>
                    <div class="search-result-content">
                        <div class="search-result-title">${highlightQuery(tour.title, inputElement.value)}</div>
                        ${tour.categories && tour.categories.length > 0 ? 
                            `<div class="search-result-categories">
                                ${tour.categories.map(cat => `<span class="category-tag">${cat}</span>`).join('')}
                            </div>` : ''
                        }
                        <div class="search-result-price">${formatCurrency(tour.price)}</div>
                    </div>
                </a>
            `).join('');

            dropdown.style.display = 'block';
            currentSelectedIndex = -1;

            // Add click handlers
            dropdown.querySelectorAll('.search-result-item').forEach(item => {
                item.addEventListener('click', (e) => {
                    e.preventDefault();
                    window.location.href = item.getAttribute('href');
                });
            });
        }

        // Highlight query in title
        function highlightQuery(title, query) {
            if (!query) return title;
            const regex = new RegExp(`(${escapeRegex(query)})`, 'gi');
            return title.replace(regex, '<mark>$1</mark>');
        }

        // Escape regex special characters
        function escapeRegex(string) {
            return string.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
        }

        // Show no results message
        function showNoResults(query) {
            dropdown.innerHTML = `
                <div class="search-no-results">
                    <i class="fas fa-search"></i>
                    <p>No tours found for "${query}"</p>
                    <small>Try searching with different keywords</small>
                </div>
            `;
            dropdown.style.display = 'block';
        }

        // Show error message
        function showError() {
            dropdown.innerHTML = `
                <div class="search-error">
                    <i class="fas fa-exclamation-triangle"></i>
                    <p>An error occurred while searching</p>
                    <small>Please try again later</small>
                </div>
            `;
            dropdown.style.display = 'block';
        }

        // Hide dropdown
        function hideDropdown() {
            dropdown.style.display = 'none';
            currentSelectedIndex = -1;
        }

        // Keyboard navigation
        function handleKeyboardNavigation(e) {
            const items = dropdown.querySelectorAll('.search-result-item');
            
            if (items.length === 0) return;

            switch(e.key) {
                case 'ArrowDown':
                    e.preventDefault();
                    currentSelectedIndex = Math.min(currentSelectedIndex + 1, items.length - 1);
                    updateSelectedItem(items);
                    break;
                    
                case 'ArrowUp':
                    e.preventDefault();
                    currentSelectedIndex = Math.max(currentSelectedIndex - 1, 0);
                    updateSelectedItem(items);
                    break;
                    
                case 'Enter':
                    e.preventDefault();
                    if (currentSelectedIndex >= 0 && items[currentSelectedIndex]) {
                        items[currentSelectedIndex].click();
                    }
                    break;
                    
                case 'Escape':
                    e.preventDefault();
                    hideDropdown();
                    inputElement.blur();
                    break;
            }
        }

        // Update selected item styling
        function updateSelectedItem(items) {
            items.forEach((item, index) => {
                if (index === currentSelectedIndex) {
                    item.classList.add('selected');
                    item.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
                } else {
                    item.classList.remove('selected');
                }
            });
        }

        // Event listeners
        inputElement.addEventListener('input', (e) => {
            const query = e.target.value.trim();
            debouncedSearch(query);
        });

        inputElement.addEventListener('keydown', handleKeyboardNavigation);

        inputElement.addEventListener('focus', () => {
            if (inputElement.value.trim().length >= CONFIG.minQueryLength && currentResults.length > 0) {
                dropdown.style.display = 'block';
            }
        });

        // Click outside to close
        document.addEventListener('click', (e) => {
            if (!searchContainer.contains(e.target)) {
                hideDropdown();
            }
        });

        // For globe search: prevent closing container when dropdown is visible
        if (isGlobeSearch) {
            inputElement.addEventListener('focus', () => {
                // Ensure globe search container is expanded
                searchContainer.classList.add('expanded');
            });

            dropdown.addEventListener('click', (e) => {
                // Prevent globe search container from closing when clicking dropdown
                e.stopPropagation();
            });
        }

        // Prevent form submission on Enter when dropdown is visible
        const form = inputElement.closest('form');
        if (form) {
            form.addEventListener('submit', (e) => {
                if (dropdown.style.display !== 'none') {
                    e.preventDefault();
                }
            });
        }
    }

    // Initialize all search inputs on page load
    function initializeAllSearches() {
        // Header search box
        const headerSearch = document.querySelector('.header .search-box input');
        if (headerSearch) {
            initializeSearch(headerSearch);
        }

        // Home page globe search
        const globeSearch = document.querySelector('#globeSearchInput');
        if (globeSearch) {
            initializeSearch(globeSearch);
        }

        // Hero search form
        const heroSearch = document.querySelector('.search-form input');
        if (heroSearch) {
            initializeSearch(heroSearch);
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeAllSearches);
    } else {
        initializeAllSearches();
    }

    // Expose initialization function for dynamic content
    window.initTourSearch = initializeSearch;

})();

