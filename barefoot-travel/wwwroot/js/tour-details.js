// Tour Details JavaScript

// Global tour data
let currentTourData = null;

$(document).ready(function() {
    // Check if TOUR_SLUG is defined
    if (!window.TOUR_SLUG) {
        console.error('TOUR_SLUG is not defined');
        showError();
        return;
    }

    
    // Initialize currency selector if exists
    if (typeof initCurrencySelector === 'function') {
        initCurrencySelector();
    }
    
    // Load tour details
    loadTourDetails();
    
    // Handle booking form submission - Open modal instead
    $('#bookingForm').on('submit', function(e) {
        e.preventDefault();
        openBookingModal();
    });
    
    // Auto-limit number of people to 100
    $('#numberOfPeople').on('input', function() {
        let val = parseInt($(this).val());
        if (val > 100) {
            $(this).val(100);
        }
        if (val < 1) {
            $(this).val(1);
        }
    });
    
    // Auto-limit modal number of guests
    $('#modalNumberOfGuests').on('input', function() {
        let val = parseInt($(this).val());
        if (val > 100) {
            $(this).val(100);
        }
        if (val < 1) {
            $(this).val(1);
        }
        updateModalTotalPrice();
    });
    
    // Modal controls
    $('#closeBookingModal, .booking-modal-overlay').on('click', closeBookingModal);
    $('#cancelBooking').on('click', closeBookingModal);
    
    // Handle public booking form submission
    $('#publicBookingForm').on('submit', handlePublicBookingSubmit);
    
    // Initialize Lucide icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
});

// Load tour details from API
function loadTourDetails() {
    $.ajax({
        url: `/api/Tour/by-slug/${window.TOUR_SLUG}`,
        type: 'GET',
        success: function(response) {
            if (response.success && response.data) {
                renderTourDetails(response.data);
                $('#loadingState').hide();
                $('#tourDetailsLayout').fadeIn();
            } else {
                console.error('Failed to load tour details:', response.message);
                showError();
            }
        },
        error: function(xhr) {
            console.error('Error loading tour details:', xhr);
            showError();
        }
    });
}

// Render tour details
function renderTourDetails(tour) {
    
    // Store tour data globally
    currentTourData = tour;
    
    // Update breadcrumb
    updateBreadcrumb(tour);
    
    // Update page title
    document.title = `${tour.title} - Barefoot Travel`;
    
    // Render category navigation
    renderCategoryNavigation(tour.categories);
    
    // Render tour title
    $('#tourTitle').text(capitalizeWords(tour.title));
    
    // Render tour meta information
    renderTourMeta(tour);
    
    // Render tour categories row
    renderTourCategoriesRow(tour.categories);
    
    // Render tour images
    renderTourImages(tour.images);
    
    // Render tour description
    renderTourDescription(tour.description);
    
    // Render tour policies
    renderTourPolicies(tour.policies);
    
    // Render booking form info
    renderBookingInfo(tour);
    
    // Reinitialize Lucide icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

// Update breadcrumb
function updateBreadcrumb(tour) {
    if (tour.categories && tour.categories.length > 0) {
        const mainCategory = tour.categories[0];
        const categoryName = mainCategory.categoryName || 'Tours';
        const breadcrumbHtml = `<a href="/categories/${mainCategory.slug}">${categoryName}</a>
                                <i class="fas fa-chevron-right"></i>
                                <span>${capitalizeWords(tour.title)}</span>`;
        $('#tourBreadcrumb').html(breadcrumbHtml);
    } else {
        $('#tourBreadcrumb').text(capitalizeWords(tour.title));
    }
}

// Render category navigation
function renderCategoryNavigation(categories) {
    const container = $('#categoryNavigation');
    container.empty();
    
    // Show Home / {First Category Name}
    let navigationName = 'Tours';
    let navigationLink = '/';
    
    if (categories && categories.length > 0) {
        navigationName = categories[0].categoryName || 'Tours';
        navigationLink = categories[0].slug ? `/categories/${categories[0].slug}` : '/';
    }
    
    container.html(`<a href="/">Home</a> <span>/</span> <a href="${navigationLink}">${navigationName}</a>`);
}

// Render tour categories row
function renderTourCategoriesRow(categories) {
    const container = $('#tourCategoriesRow');
    container.empty();
    
    if (!categories || categories.length === 0) {
        container.hide();
        return;
    }
    
    const listHtml = '<div class="tour-categories-list">' +
        categories.map(function(category, index) {
            const separator = index < categories.length - 1 ? '<span>/</span>' : '';
            if (category.slug) {
                return `<a href="/categories/${category.slug}">${category.categoryName.toUpperCase()}</a>${separator}`;
            } else {
                return `<span style="color: #00a88e; font-weight: 600;">${category.categoryName.toUpperCase()}</span>${separator}`;
            }
        }).join('') +
        '</div>';
    
    container.html(listHtml);
    container.show();
}

// Render tour meta information
function renderTourMeta(tour) {
    const container = $('#tourMeta');
    container.empty();
    
    // Max People
    container.append(`
        <div class="tour-meta-item">
            <i data-lucide="users" class="lucide-icon"></i>
            <span>${tour.maxPeople} People</span>
        </div>
    `);
    
    // Duration
    container.append(`
        <div class="tour-meta-item">
            <i data-lucide="clock" class="lucide-icon"></i>
            <span>${tour.duration}</span>
        </div>
    `);
    
    // Price Per Person
    const formattedPrice = formatPrice(tour.pricePerPerson);
    container.append(`
        <div class="tour-meta-item">
            <i data-lucide="banknote" class="lucide-icon"></i>
            <span>${formattedPrice} / person</span>
        </div>
    `);
}

// Render tour images in Pinterest-style grid
function renderTourImages(images) {
    const container = $('#tourImagesGrid');
    container.empty();
    
    if (!images || images.length === 0) {
        return;
    }
    
    // Add masonry class for Pinterest-style layout
    container.addClass('masonry');
    
    images.forEach(function(image) {
        container.append(`
            <div class="tour-image-item">
                <img src="${image.imageUrl}" alt="Tour Image" loading="lazy">
            </div>
        `);
    });
    
    // Add click handler to open images in lightbox (if you have a lightbox library)
    $('.tour-image-item').on('click', function() {
        const imgSrc = $(this).find('img').attr('src');
        // You can add lightbox functionality here
    });
}

// Render tour description (HTML from rich text editor)
function renderTourDescription(description) {
    const container = $('#tourDescription');
    
    if (!description || description.trim() === '') {
        container.hide();
        return;
    }
    
    // Clean empty HTML tags and convert empty headings to paragraphs
    let cleanedDescription = cleanEmptyTags(description);
    cleanedDescription = convertEmptyHeadingsToParagraphs(cleanedDescription);
    
    // Render HTML content
    container.html(cleanedDescription);
    
    // Check if content is too long and add READ MORE button
    const contentHeight = container[0].scrollHeight;
    const visibleHeight = 500; // Match the max-height in CSS
    
    if (contentHeight > visibleHeight) {
        container.addClass('collapsed');
        
        const toggleBtn = $('<button class="tour-description-toggle">READ MORE <i class="fas fa-chevron-down"></i></button>');
        
        toggleBtn.on('click', function() {
            if (container.hasClass('collapsed')) {
                container.removeClass('collapsed');
                $(this).html('READ LESS <i class="fas fa-chevron-up"></i>');
                $(this).addClass('expanded');
            } else {
                container.addClass('collapsed');
                $(this).html('READ MORE <i class="fas fa-chevron-down"></i>');
                $(this).removeClass('expanded');
                // Scroll back to top of description
                $('html, body').animate({
                    scrollTop: container.offset().top - 100
                }, 300);
            }
        });
        
        container.after(toggleBtn);
    }
    
    container.show();
}

// Clean consecutive empty HTML tags
function cleanEmptyTags(html) {
    if (!html) return '';
    
    // Create a temporary div to parse HTML
    const temp = $('<div>').html(html);
    
    // Remove empty tags (tags with only whitespace or <br>)
    let hasEmptyTags = true;
    while (hasEmptyTags) {
        hasEmptyTags = false;
        temp.find('*').each(function() {
            const $this = $(this);
            const text = $this.text().trim();
            const hasOnlyBr = $this.children().length === 1 && $this.children().first().is('br');
            const isEmpty = text === '' && !hasOnlyBr && $this.children().length === 0;
            const isEmptyWithBr = text === '' && hasOnlyBr;
            
            if (isEmpty || isEmptyWithBr) {
                // Check if previous sibling is also empty
                const $prev = $this.prev();
                if ($prev.length > 0) {
                    const prevText = $prev.text().trim();
                    const prevHasOnlyBr = $prev.children().length === 1 && $prev.children().first().is('br');
                    const prevIsEmpty = prevText === '' && (!prevHasOnlyBr && $prev.children().length === 0 || prevHasOnlyBr);
                    
                    if (prevIsEmpty) {
                        $this.remove();
                        hasEmptyTags = true;
                        return false;
                    }
                }
            }
        });
    }
    
    return temp.html();
}

// Convert empty heading tags to paragraph tags
function convertEmptyHeadingsToParagraphs(html) {
    if (!html) return '';
    
    const temp = $('<div>').html(html);
    
    // Find all heading tags (h1-h6)
    temp.find('h1, h2, h3, h4, h5, h6').each(function() {
        const $this = $(this);
        const text = $this.text().trim();
        
        // If heading has no text content (only whitespace or <br>)
        if (text === '') {
            // Replace with <p> tag
            const $p = $('<p></p>').html($this.html());
            $this.replaceWith($p);
        }
    });
    
    return temp.html();
}

// Render tour policies
function renderTourPolicies(policies) {
    const container = $('#tourPolicies');
    container.empty();
    
    if (!policies || policies.length === 0) {
        container.hide();
        return;
    }
    
    policies.forEach(function(policy) {
        // Build content list
        let contentHtml = '';
        if (policy.contentItems && policy.contentItems.length > 0) {
            contentHtml = '<ul>';
            policy.contentItems.forEach(function(item) {
                contentHtml += `<li>${item}</li>`;
            });
            contentHtml += '</ul>';
        } else if (policy.content) {
            // Fallback to plain content if no contentItems
            contentHtml = `<p>${policy.content}</p>`;
        }
        
        const policyHtml = `
            <div class="policy-item">
                <div class="row">
                    <div class="col-md-3 col-12">
                        <div class="policy-title">
                            <span>${policy.policyType}</span>
                        </div>
                    </div>
                    <div class="col-md-8 col-12">
                        <div class="policy-content">
                            ${contentHtml}
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        container.append(policyHtml);
    });
    
    container.show();
}

// Get icon for policy type
function getPolicyIcon(policyType) {
    const policyIcons = {
        'cancellation': 'fas fa-ban',
        'refund': 'fas fa-undo',
        'insurance': 'fas fa-shield-alt',
        'luggage': 'fas fa-suitcase',
        'food': 'fas fa-utensils',
        'accommodation': 'fas fa-bed',
        'transportation': 'fas fa-bus',
        'guide': 'fas fa-user-tie',
        'ticket': 'fas fa-ticket-alt',
        'wifi': 'fas fa-wifi',
        'photography': 'fas fa-camera',
        'safety': 'fas fa-first-aid',
        'payment': 'fas fa-credit-card',
        'age': 'fas fa-child',
        'health': 'fas fa-heartbeat'
    };
    
    // Try to match policy type (case-insensitive)
    const type = policyType.toLowerCase();
    for (const key in policyIcons) {
        if (type.includes(key)) {
            return policyIcons[key];
        }
    }
    
    // Default icon
    return 'fas fa-check-circle';
}

// Render booking information
function renderBookingInfo(tour) {
    // Update price
    const formattedPrice = formatPrice(tour.pricePerPerson);
    $('#priceAmount').text(formattedPrice);
    
    // Update start time
    if (tour.startTime) {
        $('#startTime').text(formatTime(tour.startTime));
    } else {
        $('#startTime').text('_');
    }
    
    // Update return time
    if (tour.returnTime) {
        $('#returnTime').text(formatTime(tour.returnTime));
    } else {
        $('#returnTime').text('_');
    }
    
    // Update duration
    $('#duration').val(tour.duration);
    
    // Store tour ID for booking
    $('#bookingForm').data('tourId', tour.id);
    $('#bookingForm').data('maxPeople', tour.maxPeople);
}

// Format time (HH:mm:ss to HH:mm AM/PM)
function formatTime(timeString) {
    if (!timeString) return '--:--';
    
    try {
        // Parse time string (format: HH:mm:ss)
        const parts = timeString.split(':');
        if (parts.length < 2) return timeString;
        
        let hours = parseInt(parts[0]);
        const minutes = parts[1];
        const ampm = hours >= 12 ? 'PM' : 'AM';
        
        hours = hours % 12;
        hours = hours ? hours : 12; // 0 should be 12
        
        return `${hours}:${minutes} ${ampm}`;
    } catch (e) {
        return timeString;
    }
}

// Format price with currency
function formatPrice(price) {
    if (typeof CurrencyConverter !== 'undefined') {
        return CurrencyConverter.formatPrice(price, true);
    }
    
    // Fallback to USD if CurrencyConverter is not available
    return '$' + parseFloat(price).toLocaleString('en-US', {
        minimumFractionDigits: 0,
        maximumFractionDigits: 2
    });
}

// Capitalize each word
function capitalizeWords(str) {
    if (!str) return '';
    return str.replace(/\b\w/g, function(char) {
        return char.toUpperCase();
    });
}

// Open booking modal
function openBookingModal() {
    if (!currentTourData) {
        showToast('Tour data not loaded. Please try again.', 'danger');
        return;
    }
    
    // Validate form before opening modal
    const bookingDate = $('#bookingDate').val();
    const numberOfGuests = parseInt($('#numberOfPeople').val());
    
    if (!bookingDate) {
        showToast('Please select a booking date first.', 'warning');
        return;
    }
    
    if (!numberOfGuests || numberOfGuests < 1) {
        showToast('Please select number of guests.', 'warning');
        return;
    }
    
    // Populate modal with tour information
    $('#modalTourName').text(capitalizeWords(currentTourData.title));
    $('#modalPricePerPerson').text(formatPrice(currentTourData.pricePerPerson));
    $('#modalStartTime').text(currentTourData.startTime ? formatTime(currentTourData.startTime) : 'Not specified');
    $('#modalReturnTime').text(currentTourData.returnTime ? formatTime(currentTourData.returnTime) : 'Not specified');
    $('#modalDuration').text(currentTourData.duration);
    
    // Copy values from main form
    $('#modalBookingDate').val(bookingDate);
    $('#modalNumberOfGuests').val(numberOfGuests);
    
    // Update total price
    updateModalTotalPrice();
    
    // Show modal
    $('#bookingModal').fadeIn(300);
    $('body').css('overflow', 'hidden');
}

// Close booking modal
function closeBookingModal() {
    $('#bookingModal').fadeOut(300);
    $('body').css('overflow', 'auto');
    resetBookingForm();
}

// Reset booking form
function resetBookingForm() {
    $('#publicBookingForm')[0].reset();
    $('.btn-confirm-booking').prop('disabled', false).text('Confirm Booking');
}

// Update modal total price
function updateModalTotalPrice() {
    if (!currentTourData) return;
    
    const numberOfGuests = parseInt($('#modalNumberOfGuests').val()) || 1;
    const pricePerPerson = currentTourData.pricePerPerson;
    const totalPrice = numberOfGuests * pricePerPerson;
    
    $('#modalTotalPrice').text(formatPrice(totalPrice));
}

// Handle public booking form submission
function handlePublicBookingSubmit(e) {
    e.preventDefault();
    
    if (!currentTourData) {
        showToast('Tour data not loaded. Please try again.', 'danger');
        return;
    }
    
    // Validate form
    const bookingDate = $('#modalBookingDate').val();
    const numberOfGuests = parseInt($('#modalNumberOfGuests').val());
    const customerName = $('#modalCustomerName').val().trim();
    const email = $('#modalEmail').val().trim();
    const phone = $('#modalPhone').val().trim();
    const note = $('#modalNote').val().trim();
    
    // Validate date (must be tomorrow or later)
    const selectedDate = new Date(bookingDate);
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    tomorrow.setHours(0, 0, 0, 0);
    
    if (selectedDate < tomorrow) {
        showToast('Please select a date from tomorrow onwards.', 'warning');
        return;
    }
    
    // Validate number of guests
    if (numberOfGuests < 1 || numberOfGuests > 100) {
        showToast('Number of guests must be between 1 and 100.', 'warning');
        return;
    }
    
    if (numberOfGuests > currentTourData.maxPeople) {
        showToast(`Sorry, the maximum number of guests for this tour is ${currentTourData.maxPeople}.`, 'warning');
        return;
    }
    
    // Prepare booking data
    const bookingData = {
        tourId: currentTourData.id,
        startDate: bookingDate,
        people: numberOfGuests,
        nameCustomer: customerName,
        email: email,
        phoneNumber: phone,
        note: note || null
    };
    
    
    // Show loading state
    const submitBtn = $('.btn-confirm-booking');
    submitBtn.prop('disabled', true).text('Processing...');
    
    // Send booking request to API
    $.ajax({
        url: '/api/admin/Booking/public',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(bookingData),
        success: function(response) {
            
            if (response.success) {
                submitBtn.text('Booking Confirmed!');
                
                // Show success message
                showToast('Your booking request has been sent successfully! We will contact you within 24 hours via email.', 'success');
                
                // Close modal and redirect to home after a short delay
                setTimeout(function() {
                    closeBookingModal();
                    // Redirect to home page
                    window.location.href = '/';
                }, 2000);
            } else {
                submitBtn.prop('disabled', false).text('Confirm Booking');
                showToast('Failed to create booking: ' + (response.message || 'Unknown error'), 'danger');
            }
        },
        error: function(xhr) {
            console.error('Booking error:', xhr);
            submitBtn.prop('disabled', false).text('Confirm Booking');
            
            let errorMessage = 'Failed to send booking request. Please try again.';
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            }
            showToast(errorMessage, 'danger');
        }
    });
}

// (Old handleBookingSubmit removed - now using modal workflow with public API)

// Show error state
function showError() {
    $('#loadingState').hide();
    $('#errorState').fadeIn();
}

// Initialize currency selector (if not already defined)
function initCurrencySelector() {
    if ($('#currencySelector').length === 0) return;
    
    // Set initial display
    const currentCurrency = typeof CurrencyConverter !== 'undefined' ? 
        CurrencyConverter.getCurrency() : 'USD';
    const currencySymbol = getCurrencySymbol(currentCurrency);
    $('#currentCurrencyDisplay').text(currencySymbol);
    
    // Add click handlers
    $('.currency-option').on('click', function(e) {
        e.preventDefault();
        const currency = $(this).data('currency');
        
        if (typeof CurrencyConverter !== 'undefined') {
            CurrencyConverter.setCurrency(currency);
            $('#currentCurrencyDisplay').text(CurrencyConverter.getSymbol());
            $('.currency-dropdown-menu').hide();
            
            // Reload tour details to update prices
            loadTourDetails();
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

// Get currency symbol
function getCurrencySymbol(currency) {
    const symbols = {
        'USD': '$',
        'VND': '₫',
        'EUR': '€',
        'GBP': '£'
    };
    return symbols[currency] || currency;
}


