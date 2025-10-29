/**
 * Enhanced Footer with Dynamic Content
 * Loads categories and company information
 */

(function($) {
    'use strict';

    // ============================================
    // LOAD FOOTER CATEGORIES
    // ============================================
    
    async function loadFooterCategories() {
        try {
            // Load category tree
            const response = await $.ajax({
                url: '/api/Category/tree',
                type: 'GET',
                dataType: 'json'
            });

            if (response.success && response.data) {
                populateFooterCategories(response.data);
            }
        } catch (error) {
            console.error('Error loading footer categories:', error);
        }
    }

    // ============================================
    // POPULATE FOOTER CATEGORIES
    // ============================================
    
    function populateFooterCategories(categories) {
        // Filter active categories - only top 5 of each type
        const tourCategories = categories.filter(cat => 
            cat.type === 'TOURS' && cat.active
        ).slice(0, 5); // Max 5 items
        
        const homeCategories = categories.filter(cat => 
            cat.type === 'HOME-CATEGORY' && cat.active
        ).slice(0, 5); // Max 5 items

        // Update Destinations column (TOURS) - Single column
        const $destinationsCol = $('.footer-links-col[data-type="destinations"] ul');
        
        if ($destinationsCol.length) {
            $destinationsCol.empty();
            if (tourCategories.length > 0) {
                tourCategories.forEach(cat => {
                    $destinationsCol.append(`
                        <li><a href="/tours?category=${cat.id}">${sanitizeHtml(cat.categoryName)}</a></li>
                    `);
                });
            } else {
                $destinationsCol.append('<li><a href="#">No destinations available</a></li>');
            }
        }

        // Update Tour Types column (HOME-CATEGORY) - Single column
        const $tourTypesCol = $('.footer-links-col[data-type="tour-types"] ul');
        
        if ($tourTypesCol.length) {
            $tourTypesCol.empty();
            if (homeCategories.length > 0) {
                homeCategories.forEach(cat => {
                    $tourTypesCol.append(`
                        <li><a href="/tours?category=${cat.id}">${sanitizeHtml(cat.categoryName)}</a></li>
                    `);
                });
            } else {
                $tourTypesCol.append('<li><a href="#">No tour types available</a></li>');
            }
        }
    }

    // ============================================
    // SANITIZE HTML
    // ============================================
    
    function sanitizeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // ============================================
    // BACK TO TOP FUNCTIONALITY
    // ============================================
    
    function initBackToTop() {
        const $backToTop = $('#backToTop');
        
        // Show/hide based on scroll position
        $(window).on('scroll', function() {
            if ($(this).scrollTop() > 300) {
                $backToTop.fadeIn();
            } else {
                $backToTop.fadeOut();
            }
        });

        // Smooth scroll to top
        $backToTop.on('click', function(e) {
            e.preventDefault();
            $('html, body').animate({ scrollTop: 0 }, 800, 'swing');
        });
    }

    // ============================================
    // FOOTER ANIMATIONS
    // ============================================
    
    function initFooterAnimations() {
        // Animate footer links on hover
        $('.footer-links-col a').on('mouseenter', function() {
            $(this).css('transform', 'translateX(5px)');
        }).on('mouseleave', function() {
            $(this).css('transform', 'translateX(0)');
        });

        // Social links hover effect
        $('.social-link').on('mouseenter', function() {
            $(this).css('transform', 'translateY(-3px) scale(1.1)');
        }).on('mouseleave', function() {
            $(this).css('transform', 'translateY(0) scale(1)');
        });
    }

    // ============================================
    // NEWSLETTER SUBSCRIPTION
    // ============================================
    
    function initNewsletterForm() {
        const $form = $('.newsletter-form');
        
        if ($form.length) {
            $form.on('submit', function(e) {
                e.preventDefault();
                
                const email = $(this).find('input[type="email"]').val();
                
                if (email && validateEmail(email)) {
                    // Show success message
                    showNotification('Thank you for subscribing!', 'success');
                    $(this).find('input[type="email"]').val('');
                } else {
                    showNotification('Please enter a valid email address.', 'error');
                }
            });
        }
    }

    // ============================================
    // EMAIL VALIDATION
    // ============================================
    
    function validateEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    // ============================================
    // SHOW NOTIFICATION
    // ============================================
    
    function showNotification(message, type) {
        const $notification = $(`
            <div class="footer-notification ${type}" style="
                position: fixed;
                bottom: 20px;
                right: 20px;
                background: ${type === 'success' ? '#00d4aa' : '#ff6b6b'};
                color: white;
                padding: 15px 25px;
                border-radius: 8px;
                box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
                z-index: 10000;
                animation: slideInRight 0.3s ease;
            ">
                <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i>
                ${message}
            </div>
        `);

        $('body').append($notification);

        setTimeout(() => {
            $notification.fadeOut(300, function() {
                $(this).remove();
            });
        }, 3000);
    }

    // ============================================
    // CURRENT YEAR
    // ============================================
    
    function updateCurrentYear() {
        const currentYear = new Date().getFullYear();
        $('.copyright').each(function() {
            const text = $(this).text();
            $(this).text(text.replace(/\d{4}/, currentYear));
        });
    }

    // ============================================
    // INITIALIZE FOOTER
    // ============================================
    
    function initEnhancedFooter() {
        loadFooterCategories();
        initBackToTop();
        initFooterAnimations();
        initNewsletterForm();
        updateCurrentYear();
        
        console.log('👣 Enhanced footer initialized');
    }

    // ============================================
    // AUTO INITIALIZE
    // ============================================
    
    $(document).ready(function() {
        initEnhancedFooter();
    });

    // Expose to global scope
    window.EnhancedFooter = {
        reload: loadFooterCategories
    };

})(jQuery);

// Add slideInRight animation if not exists
if (!document.querySelector('style[data-footer-animations]')) {
    const style = document.createElement('style');
    style.setAttribute('data-footer-animations', 'true');
    style.textContent = `
        @keyframes slideInRight {
            from {
                opacity: 0;
                transform: translateX(100%);
            }
            to {
                opacity: 1;
                transform: translateX(0);
            }
        }
    `;
    document.head.appendChild(style);
}

