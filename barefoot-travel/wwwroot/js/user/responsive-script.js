// Responsive JavaScript functionality
$(document).ready(() => {
  // Mobile menu toggle
  const mobileMenuToggle = $("#mobileMenuToggle")
  const mainNav = $("#mainNav")
  const mobileOverlay = $("#mobileOverlay")
  const body = $("body")

  // Toggle mobile menu
  mobileMenuToggle.on("click", function () {
    const icon = $(this).find("i")

    if (mainNav.hasClass("active")) {
      // Close menu
      mainNav.removeClass("active")
      mobileOverlay.removeClass("active")
      icon.removeClass("fa-times").addClass("fa-bars")
      body.removeClass("menu-open")
    } else {
      // Open menu
      mainNav.addClass("active")
      mobileOverlay.addClass("active")
      icon.removeClass("fa-bars").addClass("fa-times")
      body.addClass("menu-open")
    }
  })

  // Close menu when overlay is clicked
  mobileOverlay.on("click", () => {
    mobileMenuToggle.click()
  })

  // Handle dropdown menus on mobile
  $(".nav-item").on("click", function (e) {
    if ($(window).width() <= 768) {
      e.preventDefault()
      $(this).toggleClass("active")
    }
  })

  // Close mobile menu when window is resized to desktop
  $(window).on("resize", () => {
    if ($(window).width() > 768) {
      mainNav.removeClass("active")
      mobileOverlay.removeClass("active")
      mobileMenuToggle.find("i").removeClass("fa-times").addClass("fa-bars")
      body.removeClass("menu-open")
      $(".nav-item").removeClass("active")
    }
  })

  // Prevent body scroll when mobile menu is open
  body.addClass("menu-open", () => {
    body.css("overflow", "hidden")
  })

  body.removeClass("menu-open", () => {
    body.css("overflow", "auto")
  })

  // Enhanced touch interactions for mobile
  if ("ontouchstart" in window) {
    // Add touch-friendly hover effects
    $(".tour-card, .product-card, .travel-item, .daily-tour-card").on("touchstart", function () {
      $(this).addClass("touch-active")
    })

    $(".tour-card, .product-card, .travel-item, .daily-tour-card").on("touchend", function () {
      
      setTimeout(() => {
        $(this).removeClass("touch-active")
      }, 300)
    })
  }

  // Responsive image loading
  function loadResponsiveImages() {
    const images = $("img[data-src]")

    images.each(function () {
      const img = $(this)
      const src = img.data("src")

      if (src) {
        img.attr("src", src)
        img.removeAttr("data-src")
      }
    })
  }

  // Intersection Observer for lazy loading (if supported)
  if ("IntersectionObserver" in window) {
    const imageObserver = new IntersectionObserver((entries, observer) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          const img = entry.target
          const src = img.getAttribute("data-src")

          if (src) {
            img.src = src
            img.removeAttribute("data-src")
            img.classList.add("loaded")
          }

          observer.unobserve(img)
        }
      })
    })

    // Observe all images with data-src
    $("img[data-src]").each(function () {
      imageObserver.observe(this)
    })
  } else {
    // Fallback for browsers without Intersection Observer
    loadResponsiveImages()
  }

  // Responsive text adjustments
  function adjustResponsiveText() {
    const windowWidth = $(window).width()

    if (windowWidth <= 480) {
      // Extra small screens
      $(".hero-text h1").css("font-size", "1.8rem")
      $(".section-header h2").css("font-size", "1.5rem")
    } else if (windowWidth <= 768) {
      // Small screens
      $(".hero-text h1").css("font-size", "2.2rem")
      $(".section-header h2").css("font-size", "1.8rem")
    } else if (windowWidth <= 1024) {
      // Medium screens
      $(".hero-text h1").css("font-size", "2.8rem")
      $(".section-header h2").css("font-size", "2rem")
    } else {
      // Large screens - reset to default
      $(".hero-text h1").css("font-size", "")
      $(".section-header h2").css("font-size", "")
    }
  }

  // Call on load and resize
  adjustResponsiveText()
  $(window).on("resize", adjustResponsiveText)

  // Enhanced scroll behavior for mobile
  let lastScrollTop = 0
  const header = $(".header")

  $(window).scroll(function () {
    const scrollTop = $(this).scrollTop()

    if ($(window).width() <= 768) {
      // Hide/show header on mobile scroll
      if (scrollTop > lastScrollTop && scrollTop > 100) {
        // Scrolling down
        header.addClass("header-hidden")
      } else {
        // Scrolling up
        header.removeClass("header-hidden")
      }
    }

    lastScrollTop = scrollTop
  })

  // Smooth scrolling for mobile
  $('a[href^="#"]').on("click", function (e) {
    e.preventDefault()

    const target = $($(this).attr("href"))
    if (target.length) {
      const offsetTop = target.offset().top - ($(window).width() <= 768 ? 60 : 80)

      $("html, body").animate(
        {
          scrollTop: offsetTop,
        },
        800,
      )
    }
  })

  // Enhanced form interactions for mobile
  $(".search-form input").on("focus", function () {
    if ($(window).width() <= 768) {
      $(this).parent().addClass("mobile-focused")
      // Scroll to form on mobile
      $("html, body").animate(
        {
          scrollTop: $(this).offset().top - 100,
        },
        300,
      )
    }
  })

  $(".search-form input").on("blur", function () {
    $(this).parent().removeClass("mobile-focused")
  })

  // Responsive grid adjustments
  function adjustGrids() {
    const windowWidth = $(window).width()

    if (windowWidth <= 480) {
      // Single column on very small screens
      $(".travel-grid").removeClass("grid-2 grid-3").addClass("grid-1")
      $(".daily-tours-grid").removeClass("grid-1 grid-3").addClass("grid-1")
    } else if (windowWidth <= 768) {
      // Two columns on small screens
      $(".travel-grid").removeClass("grid-1 grid-3").addClass("grid-2")
      $(".daily-tours-grid").removeClass("grid-1 grid-3").addClass("grid-1")
    } else if (windowWidth <= 1024) {
      // Three columns on medium screens
      $(".travel-grid").removeClass("grid-1 grid-2").addClass("grid-3")
      $(".daily-tours-grid").removeClass("grid-1 grid-3").addClass("grid-2")
    }
  }

  adjustGrids()
  $(window).on("resize", adjustGrids)

  // Performance optimization: debounce resize events
  function debounce(func, wait) {
    let timeout
    return function executedFunction(...args) {
      const later = () => {
        clearTimeout(timeout)
        func(...args)
      }
      clearTimeout(timeout)
      timeout = setTimeout(later, wait)
    }
  }

  // Debounced resize handler
  const debouncedResize = debounce(() => {
    adjustResponsiveText()
    adjustGrids()
  }, 250)

  $(window).on("resize", debouncedResize)

  // Add loading states for better UX
  $(".book-btn, .book-now-btn, .read-more-btn").on("click", function () {
    const btn = $(this)
    const originalText = btn.text()

    btn.addClass("loading").text("Loading...")

    setTimeout(() => {
      btn.removeClass("loading").text(originalText)
    }, 2000)
  })
})

// Add CSS for additional responsive features
$("<style>")
  .prop("type", "text/css")
  .html(`
        /* Mobile menu body lock */
        body.menu-open {
            overflow: hidden;
            position: fixed;
            width: 100%;
        }
        
        /* Header hide animation */
        .header-hidden {
            transform: translateY(-100%);
            transition: transform 0.3s ease;
        }
        
        /* Touch active states */
        .touch-active {
            transform: scale(0.98);
            transition: transform 0.1s ease;
        }
        
        /* Mobile focused form */
        .search-form.mobile-focused {
            transform: scale(1.02);
            box-shadow: 0 10px 30px rgba(0, 212, 170, 0.3);
        }
        
        /* Loading button states */
        .loading {
            opacity: 0.7;
            pointer-events: none;
        }
        
        /* Responsive grid utilities */
        .grid-1 {
            grid-template-columns: 1fr !important;
        }
        
        .grid-2 {
            grid-template-columns: repeat(2, 1fr) !important;
        }
        
        .grid-3 {
            grid-template-columns: repeat(3, 1fr) !important;
        }
        
        /* Image loading states */
        img[data-src] {
            opacity: 0;
            transition: opacity 0.3s ease;
        }
        
        img.loaded {
            opacity: 1;
        }
        
        /* Enhanced mobile interactions */
        @media (max-width: 768px) {
            .tour-card:active,
            .product-card:active,
            .travel-item:active {
                transform: scale(0.98);
            }
            
            .btn:active,
            .book-btn:active,
            .book-now-btn:active {
                transform: scale(0.95);
            }
        }
        
        /* Landscape mobile optimizations */
        @media (max-width: 768px) and (orientation: landscape) {
            .hero {
                height: 100vh;
            }
            
            .hero-content {
                padding: 20px;
            }
            
            .hero-badges {
                margin-bottom: 20px;
            }
            
            .hero-text {
                margin-bottom: 20px;
            }
            
            .hero-text h1 {
                font-size: 2rem;
                margin-bottom: 10px;
            }
        }
    `)
  .appendTo("head")
