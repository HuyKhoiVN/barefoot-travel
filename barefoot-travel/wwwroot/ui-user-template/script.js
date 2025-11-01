$(document).ready(() => {
  // Loading animation
  setTimeout(() => {
    $(".loading").addClass("hide")
  }, 1500)

  // Header scroll effect
  $(window).scroll(function () {
    if ($(this).scrollTop() > 100) {
      $(".header").addClass("scrolled")
    } else {
      $(".header").removeClass("scrolled")
    }
  })

  // Smooth scrolling for anchor links
  $('a[href^="#"]').on("click", function (event) {
    var target = $(this.getAttribute("href"))
    if (target.length) {
      event.preventDefault()
      $("html, body")
        .stop()
        .animate(
          {
            scrollTop: target.offset().top - 80,
          },
          1000,
        )
    }
  })

  // Animate elements on scroll
  function animateOnScroll() {
    $(".animate-on-scroll").each(function () {
      var elementTop = $(this).offset().top
      var elementBottom = elementTop + $(this).outerHeight()
      var viewportTop = $(window).scrollTop()
      var viewportBottom = viewportTop + $(window).height()

      if (elementBottom > viewportTop && elementTop < viewportBottom) {
        $(this).addClass("animated")
      }
    })
  }

  $(window).on("scroll resize", animateOnScroll)
  animateOnScroll() // Initial check

  // Search form animation
  $(".search-form input")
    .on("focus", function () {
      $(this).parent().addClass("focused")
    })
    .on("blur", function () {
      $(this).parent().removeClass("focused")
    })

  // Tour card hover effects
  $(".tour-card").hover(
    function () {
      $(this).find(".card-image img").css("transform", "scale(1.1)")
      $(this).find(".card-overlay").css("opacity", "1")
    },
    function () {
      $(this).find(".card-image img").css("transform", "scale(1)")
      $(this).find(".card-overlay").css("opacity", "0")
    },
  )

  // Book button click effect
  $(".book-btn").on("click", function (e) {
    e.preventDefault()

    // Create ripple effect
    var $button = $(this)
    var $ripple = $('<span class="ripple"></span>')

    $button.append($ripple)

    var btnOffset = $button.offset()
    var xPos = e.pageX - btnOffset.left
    var yPos = e.pageY - btnOffset.top

    $ripple.css({
      position: "absolute",
      top: yPos + "px",
      left: xPos + "px",
      width: "0",
      height: "0",
      borderRadius: "50%",
      background: "rgba(255, 255, 255, 0.5)",
      transform: "translate(-50%, -50%)",
      animation: "ripple-animation 0.6s linear",
    })

    setTimeout(() => {
      $ripple.remove()
    }, 600)

    // Show booking modal (placeholder)
    showBookingModal()
  })

  // Parallax effect for hero background
  $(window).scroll(function () {
    var scrolled = $(this).scrollTop()
    var parallax = $(".hero-background")
    var speed = 0.5

    parallax.css("transform", "translateY(" + scrolled * speed + "px)")
  })

  // Navigation dropdown effects
  $(".nav-item").hover(
    function () {
      $(this).find(".dropdown-menu").stop().fadeIn(300).css("display", "block")
    },
    function () {
      $(this).find(".dropdown-menu").stop().fadeOut(300)
    },
  )

  // Search functionality
  $(".search-form").on("submit", function (e) {
    e.preventDefault()
    var searchTerm = $(this).find("input").val()
    if (searchTerm.trim() !== "") {
      // Simulate search (replace with actual search functionality)
      showSearchResults(searchTerm)
    }
  })

  // Feature items animation
  $(".feature-item").on("mouseenter", function () {
    $(this).find("i").addClass("animated-icon")
    setTimeout(() => {
      $(this).find("i").removeClass("animated-icon")
    }, 600)
  })

  // Travel items stagger animation
  $(".travel-item").each(function (index) {
    $(this).css("animation-delay", index * 0.1 + "s")
    $(this).addClass("animate-on-scroll")
  })

  // Dynamic background color change based on scroll
  $(window).scroll(() => {
    var scrollPercent = ($(window).scrollTop() / ($(document).height() - $(window).height())) * 100
    var hue = Math.floor(scrollPercent * 3.6) // 0-360 degrees
  })

  // Product card hover effects with image switching
  $(".product-card").hover(
    function () {
      $(this).find(".main-image").css({
        opacity: "0",
        transform: "scale(1.15) rotate(2deg)",
      })
      $(this).find(".gallery-image").css({
        opacity: "1",
        transform: "scale(1.08) rotate(-1deg)",
      })
    },
    function () {
      $(this).find(".main-image").css({
        opacity: "1",
        transform: "scale(1) rotate(0deg)",
      })
      $(this).find(".gallery-image").css({
        opacity: "0",
        transform: "scale(1) rotate(0deg)",
      })
    },
  )

  // Product card click to show details
  $(".product-card").on("click", function (e) {
    if (!$(e.target).closest(".wishlist-btn").length && !$(e.target).closest(".read-more-btn").length) {
      var productTitle = $(this).find(".product-title a").text()
      var productPrice = $(this).find(".product-price").text()
      var productImage = $(this).find(".main-image").attr("src")

      showProductModal(productTitle, productPrice, productImage)
    }
  })

  // Read more button with ripple effect
  $(".read-more-btn").on("click", function (e) {
    e.preventDefault()
    e.stopPropagation()

    // Create ripple effect
    var $button = $(this)
    var $ripple = $('<span class="ripple"></span>')

    $button.append($ripple)

    var btnOffset = $button.offset()
    var xPos = e.pageX - btnOffset.left
    var yPos = e.pageY - btnOffset.top

    $ripple.css({
      position: "absolute",
      top: yPos + "px",
      left: xPos + "px",
      width: "0",
      height: "0",
      borderRadius: "50%",
      background: "rgba(255, 255, 255, 0.5)",
      transform: "translate(-50%, -50%)",
      animation: "ripple-animation 0.6s linear",
    })

    setTimeout(() => {
      $ripple.remove()
    }, 600)

    // Show product details
    var productTitle = $(this).closest(".product-card").find(".product-title a").text()
    showNotification(`Loading details for ${productTitle}...`, "info")
  })

  // Products grid stagger animation
  $(".product-card").each(function (index) {
    $(this).css("animation-delay", index * 0.1 + "s")
    $(this).addClass("animate-on-scroll")
  })

  // Best sellers tab functionality
  $(".tab-btn").on("click", function () {
    var $this = $(this)
    var tabNumber = $this.text()

    // Remove active class from all tabs
    $(".tab-btn").removeClass("active")
    $this.addClass("active")

    // Show loading animation
    $(".products-grid").css({
      opacity: "0.3",
      transform: "translateY(20px)",
    })

    setTimeout(() => {
      $(".products-grid").css({
        opacity: "1",
        transform: "translateY(0)",
      })
      showNotification(`Showing top ${tabNumber} best sellers`, "success")
    }, 500)
  })

  // Parallax effect for spotlight sections
  $(window).scroll(function () {
    var scrolled = $(this).scrollTop()
    var rate = scrolled * -0.3
  })

  // Enhanced section animations on scroll
  function enhancedScrollAnimations() {
    $(".left-spotlight-section, .no-spotlight-section, .right-spotlight-section").each(function () {
      var elementTop = $(this).offset().top
      var elementBottom = elementTop + $(this).outerHeight()
      var viewportTop = $(window).scrollTop()
      var viewportBottom = viewportTop + $(window).height()

      if (elementBottom > viewportTop && elementTop < viewportBottom) {
        $(this).addClass("section-visible")

        // Stagger animate cards within the section
        $(this)
          .find(".indochina-card, .vietnam-card, .love-card")
          .each(function (index) {
            setTimeout(() => {
              $(this).addClass("card-animated")
            }, index * 100)
          })
      }
    })
  }

  $(window).on("scroll resize", enhancedScrollAnimations)
  enhancedScrollAnimations()

  // Indochina packages section animations
  $(".indochina-card").each(function (index) {
    $(this).css("animation-delay", index * 0.15 + "s")
    $(this).addClass("animate-on-scroll")
  })

  // Vietnam packages section animations
  $(".vietnam-card").each(function (index) {
    $(this).css("animation-delay", index * 0.1 + "s")
    $(this).addClass("animate-on-scroll")
  })

  // Tours we love section animations
  $(".love-card").each(function (index) {
    $(this).css("animation-delay", index * 0.12 + "s")
    $(this).addClass("animate-on-scroll")
  })

  // Enhanced wishlist functionality for new sections
  $(".indochina-card .wishlist-btn, .vietnam-card .wishlist-btn, .love-card .wishlist-btn").on("click", function (e) {
    e.preventDefault()
    e.stopPropagation()

    var $btn = $(this)
    var $icon = $btn.find("i")
    var cardTitle = $btn.closest(".indochina-card, .vietnam-card, .love-card").find(".card-title a").text()

    // Toggle wishlist state with enhanced animation
    if ($icon.hasClass("fas")) {
      $icon.removeClass("fas").addClass("far")
      $btn.removeClass("active")
      showNotification(`Removed "${cardTitle}" from wishlist`, "info")
    } else {
      $icon.removeClass("far").addClass("fas")
      $btn.addClass("active")
      showNotification(`Added "${cardTitle}" to wishlist`, "success")
    }

    // Enhanced bounce animation
    $btn.css("transform", "scale(1.3) rotate(15deg)")
    setTimeout(() => {
      $btn.css("transform", "scale(1) rotate(0deg)")
    }, 250)
  })

  // Enhanced card hover effects for new sections
  $(".indochina-card, .vietnam-card, .love-card").hover(
    function () {
      $(this).find(".main-image").css({
        opacity: "0",
        transform: "scale(1.15) rotate(2deg)",
      })
      $(this).find(".gallery-image").css({
        opacity: "1",
        transform: "scale(1.08) rotate(-1deg)",
      })
    },
    function () {
      $(this).find(".main-image").css({
        opacity: "1",
        transform: "scale(1) rotate(0deg)",
      })
      $(this).find(".gallery-image").css({
        opacity: "0",
        transform: "scale(1) rotate(0deg)",
      })
    },
  )

  // Enhanced read more buttons for new sections
  $(".indochina-card .read-more-btn, .vietnam-card .read-more-btn, .love-card .read-more-btn").on(
    "click",
    function (e) {
      e.preventDefault()
      e.stopPropagation()

      var $button = $(this)
      var cardTitle = $button.closest(".indochina-card, .vietnam-card, .love-card").find(".card-title a").text()
      var cardPrice = $button.closest(".indochina-card, .vietnam-card, .love-card").find(".card-price").text()
      var cardImage = $button.closest(".indochina-card, .vietnam-card, .love-card").find(".main-image").attr("src")

      // Enhanced ripple effect
      createEnhancedRipple($button, e)

      // Show enhanced product modal
      setTimeout(() => {
        showEnhancedProductModal(cardTitle, cardPrice, cardImage)
      }, 300)
    },
  )

  // Book now buttons for spotlight sections
  $(".book-now-btn").on("click", function (e) {
    e.preventDefault()

    var $button = $(this)
    var sectionTitle = $button.closest(".left-spotlight-left, .right-spotlight-right").find("h1").text()

    // Create enhanced ripple effect
    createEnhancedRipple($button, e)

    setTimeout(() => {
      showEnhancedBookingModal(sectionTitle)
    }, 300)
  })

  // Browse all tours links
  $(".browse-all").on("click", function (e) {
    e.preventDefault()

    var sectionName = $(this).closest("section").find("h2").first().text()
    showNotification(`Loading all tours for ${sectionName}...`, "info")

    // Animate the arrow
    var $arrow = $(this).find("i")
    $arrow.css("transform", "translateX(10px) scale(1.2)")
    setTimeout(() => {
      $arrow.css("transform", "translateX(0) scale(1)")
    }, 300)
  })
})

// Helper functions
function showBookingModal() {
  // Create and show booking modal
  var modal = $(`
        <div class="booking-modal" style="
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.8);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 10000;
            opacity: 0;
            transition: opacity 0.3s ease;
        ">
            <div class="modal-content" style="
                background: white;
                padding: 40px;
                border-radius: 15px;
                text-align: center;
                max-width: 400px;
                transform: scale(0.8);
                transition: transform 0.3s ease;
            ">
                <h3 style="margin-bottom: 20px; color: #333;">Booking Confirmation</h3>
                <p style="margin-bottom: 30px; color: #666;">Thank you for your interest! This is a demo booking system.</p>
                <button class="close-modal" style="
                    background: #00d4aa;
                    color: white;
                    border: none;
                    padding: 12px 30px;
                    border-radius: 25px;
                    cursor: pointer;
                    font-weight: 600;
                ">Close</button>
            </div>
        </div>
    `)

  $("body").append(modal)

  setTimeout(() => {
    modal.css("opacity", "1")
    modal.find(".modal-content").css("transform", "scale(1)")
  }, 10)

  modal.find(".close-modal").on("click", () => {
    modal.css("opacity", "0")
    modal.find(".modal-content").css("transform", "scale(0.8)")
    setTimeout(() => modal.remove(), 300)
  })

  modal.on("click", function (e) {
    if (e.target === this) {
      $(this).find(".close-modal").click()
    }
  })
}

function showSearchResults(searchTerm) {
  // Simulate search results
  var results = $(`
        <div class="search-results" style="
            position: fixed;
            top: 80px;
            left: 50%;
            transform: translateX(-50%);
            background: white;
            padding: 20px;
            border-radius: 10px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
            z-index: 1000;
            opacity: 0;
            transition: opacity 0.3s ease;
        ">
            <h4 style="margin-bottom: 15px;">Search Results for "${searchTerm}"</h4>
            <p style="color: #666;">Demo: Found 12 tours matching your search.</p>
            <button class="close-results" style="
                background: #00d4aa;
                color: white;
                border: none;
                padding: 8px 20px;
                border-radius: 20px;
                cursor: pointer;
                margin-top: 15px;
            ">Close</button>
        </div>
    `)

  $("body").append(results)

  setTimeout(() => results.css("opacity", "1"), 10)

  results.find(".close-results").on("click", () => {
    results.css("opacity", "0")
    setTimeout(() => results.remove(), 300)
  })

  // Auto close after 5 seconds
  setTimeout(() => {
    if (results.length) {
      results.find(".close-results").click()
    }
  }, 5000)
}

function showProductModal(title, price, image) {
  var modal = $(`
        <div class="product-modal" style="
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.8);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 10000;
            opacity: 0;
            transition: opacity 0.3s ease;
        ">
            <div class="modal-content" style="
                background: white;
                padding: 0;
                border-radius: 15px;
                max-width: 500px;
                width: 90%;
                transform: scale(0.8);
                transition: transform 0.3s ease;
                overflow: hidden;
            ">
                <div style="height: 250px; overflow: hidden;">
                    <img src="${image}" style="width: 100%; height: 100%; object-fit: cover;" alt="${title}">
                </div>
                <div style="padding: 30px;">
                    <h3 style="margin-bottom: 15px; color: #333; font-size: 20px;">${title}</h3>
                    <div style="margin-bottom: 20px; font-size: 24px; font-weight: 700; color: #00d4aa;">${price}</div>
                    <p style="margin-bottom: 25px; color: #666; line-height: 1.6;">Experience an unforgettable journey with our expertly crafted tour. This demo shows product details.</p>
                    <div style="display: flex; gap: 15px;">
                        <button class="book-now-modal" style="
                            background: #00d4aa;
                            color: white;
                            border: none;
                            padding: 12px 25px;
                            border-radius: 25px;
                            cursor: pointer;
                            font-weight: 600;
                            flex: 1;
                        ">Book Now</button>
                        <button class="close-product-modal" style="
                            background: #f8f9fa;
                            color: #666;
                            border: 1px solid #ddd;
                            padding: 12px 25px;
                            border-radius: 25px;
                            cursor: pointer;
                            font-weight: 600;
                        ">Close</button>
                    </div>
                </div>
            </div>
        </div>
    `)

  $("body").append(modal)

  setTimeout(() => {
    modal.css("opacity", "1")
    modal.find(".modal-content").css("transform", "scale(1)")
  }, 10)

  modal.find(".close-product-modal").on("click", () => {
    modal.css("opacity", "0")
    modal.find(".modal-content").css("transform", "scale(0.8)")
    setTimeout(() => modal.remove(), 300)
  })

  modal.find(".book-now-modal").on("click", () => {
    modal.find(".close-product-modal").click()
    setTimeout(() => showBookingModal(), 300)
  })

  modal.on("click", function (e) {
    if (e.target === this) {
      $(this).find(".close-product-modal").click()
    }
  })
}

function showNotification(message, type = "info") {
  var notification = $(`
        <div class="notification ${type}">
            ${message}
        </div>
    `)

  $("body").append(notification)

  setTimeout(() => {
    notification.addClass("show")
  }, 10)

  setTimeout(() => {
    notification.removeClass("show")
    setTimeout(() => notification.remove(), 300)
  }, 3000)
}

// Add loading HTML to body
$("body").prepend(`
    <div class="loading">
        <div class="spinner"></div>
    </div>
`)

// Intersection Observer for better scroll animations
if ("IntersectionObserver" in window) {
  const observerOptions = {
    threshold: 0.1,
    rootMargin: "0px 0px -50px 0px",
  }

  const observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add("animated")
      }
    })
  }, observerOptions)

  // Observe elements when DOM is ready
  $(document).ready(() => {
    $(".animate-on-scroll").each(function () {
      observer.observe(this)
    })
  })
}

// Enhanced helper functions
function createEnhancedRipple($button, event) {
  var $ripple = $('<span class="enhanced-ripple"></span>')
  $button.append($ripple)

  var btnOffset = $button.offset()
  var xPos = event.pageX - btnOffset.left
  var yPos = event.pageY - btnOffset.top

  $ripple.css({
    position: "absolute",
    top: yPos + "px",
    left: xPos + "px",
    width: "0",
    height: "0",
    borderRadius: "50%",
    background: "rgba(255, 255, 255, 0.6)",
    transform: "translate(-50%, -50%)",
    animation: "enhanced-ripple-animation 0.8s ease-out",
    "pointer-events": "none",
  })

  setTimeout(() => {
    $ripple.remove()
  }, 800)
}

function showEnhancedProductModal(title, price, image) {
  var modal = $(`
    <div class="enhanced-product-modal" style="
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.9);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10000;
      opacity: 0;
      transition: all 0.4s ease;
      backdrop-filter: blur(10px);
    ">
      <div class="modal-content" style="
        background: white;
        border-radius: 20px;
        max-width: 600px;
        width: 90%;
        transform: scale(0.7) rotateY(90deg);
        transition: all 0.4s ease;
        overflow: hidden;
        box-shadow: 0 25px 50px rgba(0, 0, 0, 0.3);
      ">
        <div style="height: 300px; overflow: hidden; position: relative;">
          <img src="${image}" style="width: 100%; height: 100%; object-fit: cover;" alt="${title}">
          <div style="
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: linear-gradient(45deg, rgba(0, 212, 170, 0.3) 0%, rgba(0, 0, 0, 0.2) 100%);
          "></div>
        </div>
        <div style="padding: 40px;">
          <h3 style="margin-bottom: 15px; color: #333; font-size: 24px; font-weight: 700;">${title}</h3>
          <div style="margin-bottom: 20px; font-size: 28px; font-weight: 800; color: #00d4aa;">${price}</div>
          <p style="margin-bottom: 30px; color: #666; line-height: 1.8; font-size: 16px;">
            Embark on an extraordinary journey with our expertly curated tour experience. 
            Discover breathtaking landscapes, immerse yourself in local culture, and create unforgettable memories 
            that will last a lifetime.
          </p>
          <div style="display: flex; gap: 15px;">
            <button class="enhanced-book-now" style="
              background: linear-gradient(135deg, #00d4aa 0%, #00b894 100%);
              color: white;
              border: none;
              padding: 15px 30px;
              border-radius: 30px;
              cursor: pointer;
              font-weight: 600;
              flex: 1;
              font-size: 16px;
              transition: all 0.3s ease;
              box-shadow: 0 8px 20px rgba(0, 212, 170, 0.3);
            ">Book This Tour</button>
            <button class="close-enhanced-modal" style="
              background: #f8f9fa;
              color: #666;
              border: 2px solid #e9ecef;
              padding: 15px 25px;
              border-radius: 30px;
              cursor: pointer;
              font-weight: 600;
              transition: all 0.3s ease;
            ">Close</button>
          </div>
        </div>
      </div>
    </div>
  `)

  $("body").append(modal)

  setTimeout(() => {
    modal.css("opacity", "1")
    modal.find(".modal-content").css({
      transform: "scale(1) rotateY(0deg)",
    })
  }, 10)

  modal.find(".close-enhanced-modal").on("click", () => {
    modal.css("opacity", "0")
    modal.find(".modal-content").css("transform", "scale(0.7) rotateY(-90deg)")
    setTimeout(() => modal.remove(), 400)
  })

  modal.find(".enhanced-book-now").on("click", () => {
    modal.find(".close-enhanced-modal").click()
    setTimeout(() => showEnhancedBookingModal(title), 400)
  })

  modal.on("click", function (e) {
    if (e.target === this) {
      $(this).find(".close-enhanced-modal").click()
    }
  })
}

function showEnhancedBookingModal(tourTitle) {
  var modal = $(`
    <div class="enhanced-booking-modal" style="
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.9);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10001;
      opacity: 0;
      transition: all 0.4s ease;
      backdrop-filter: blur(15px);
    ">
      <div class="modal-content" style="
        background: linear-gradient(135deg, #00d4aa 0%, #00b894 100%);
        padding: 50px;
        border-radius: 25px;
        text-align: center;
        max-width: 500px;
        width: 90%;
        color: white;
        transform: scale(0.5) rotate(180deg);
        transition: all 0.5s ease;
        box-shadow: 0 30px 60px rgba(0, 212, 170, 0.4);
      ">
        <div style="
          width: 80px;
          height: 80px;
          background: rgba(255, 255, 255, 0.2);
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          margin: 0 auto 30px;
          backdrop-filter: blur(10px);
        ">
          <i class="fas fa-check" style="font-size: 40px; color: white;"></i>
        </div>
        <h3 style="margin-bottom: 20px; font-size: 28px; font-weight: 800;">Booking Confirmed!</h3>
        <p style="margin-bottom: 15px; font-size: 18px; opacity: 0.9;">Thank you for choosing</p>
        <p style="margin-bottom: 30px; font-size: 20px; font-weight: 600;">"${tourTitle}"</p>
        <p style="margin-bottom: 40px; opacity: 0.8; line-height: 1.6;">
          Your booking request has been received. Our travel experts will contact you within 24 hours 
          to finalize your amazing journey!
        </p>
        <button class="close-enhanced-booking" style="
          background: rgba(255, 255, 255, 0.2);
          color: white;
          border: 2px solid rgba(255, 255, 255, 0.3);
          padding: 15px 40px;
          border-radius: 30px;
          cursor: pointer;
          font-weight: 600;
          font-size: 16px;
          transition: all 0.3s ease;
          backdrop-filter: blur(10px);
        ">Continue Exploring</button>
      </div>
    </div>
  `)

  $("body").append(modal)

  setTimeout(() => {
    modal.css("opacity", "1")
    modal.find(".modal-content").css("transform", "scale(1) rotate(0deg)")
  }, 10)

  modal.find(".close-enhanced-booking").on("click", () => {
    modal.css("opacity", "0")
    modal.find(".modal-content").css("transform", "scale(0.5) rotate(-180deg)")
    setTimeout(() => modal.remove(), 500)
  })

  modal.on("click", function (e) {
    if (e.target === this) {
      $(this).find(".close-enhanced-booking").click()
    }
  })

  // Auto close after 8 seconds
  setTimeout(() => {
    if (modal.length) {
      modal.find(".close-enhanced-booking").click()
    }
  }, 8000)
}

// Add enhanced CSS animations
$("<style>")
  .prop("type", "text/css")
  .html(`
    @keyframes ripple-animation {
      to {
        width: 100px;
        height: 100px;
        opacity: 0;
      }
    }
    @keyframes enhanced-ripple-animation {
      to {
        width: 120px;
        height: 120px;
        opacity: 0;
      }
    }
    .animated-icon {
      animation: bounce 0.6s ease !important;
    }
    .notification {
      position: fixed;
      top: 100px;
      right: 20px;
      padding: 15px 25px;
      border-radius: 8px;
      color: white;
      font-weight: 500;
      z-index: 10001;
      transform: translateX(100%);
      transition: transform 0.3s ease;
    }
    .notification.success {
      background: #00d4aa;
    }
    .notification.info {
      background: #3498db;
    }
    .notification.show {
      transform: translateX(0);
    }
    .section-visible {
      animation: sectionFadeIn 0.8s ease-out;
    }
    @keyframes sectionFadeIn {
      from {
        opacity: 0;
        transform: translateY(50px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
    .card-animated {
      animation: cardSlideIn 0.6s ease-out;
    }
    @keyframes cardSlideIn {
      from {
        opacity: 0;
        transform: translateY(30px) scale(0.9);
      }
      to {
        opacity: 1;
        transform: translateY(0) scale(1);
      }
    }
    .enhanced-book-now:hover {
      transform: translateY(-3px);
      box-shadow: 0 12px 30px rgba(0, 212, 170, 0.4) !important;
    }
    .close-enhanced-modal:hover {
      background: #e9ecef !important;
      border-color: #dee2e6 !important;
      transform: translateY(-2px);
    }
    .close-enhanced-booking:hover {
      background: rgba(255, 255, 255, 0.3) !important;
      border-color: rgba(255, 255, 255, 0.5) !important;
      transform: translateY(-2px);
    }
  `)
  .appendTo("head")

// =====================
// Dynamic JSON Rendering helpers
// =====================
function rotateImages($img, urls, ms = 3000) {
  if (!urls || urls.length < 2) return
  let idx = 0
  const swap = () => {
    idx = (idx + 1) % urls.length
    $img.attr("src", urls[idx])
  }
  const id = setInterval(swap, ms)
  $img.on("mouseenter", () => swap())
  $img.on("remove", () => clearInterval(id))
}

// Safety helpers to ensure JSON values cannot affect CSS
function sanitizeUrl(raw) {
  try {
    if (typeof raw !== "string") return ""
    const trimmed = raw.trim()
    // Disallow javascript:, data: (except data images if you wish), and about:
    if (/^(javascript:|vbscript:|about:)/i.test(trimmed)) return ""
    // Allow relative paths and http/https
    if (/^(https?:\/\/|\.\/|\.\.\/|\/)/i.test(trimmed)) {
      // Strip dangerous quotes and closing parens that can break CSS url()
      return trimmed.replace(/["')]/g, "")
    }
    return ""
  } catch (e) {
    return ""
  }
}

function setSafeImgSrc($img, url) {
  const safe = sanitizeUrl(url)
  if (safe) $img.attr("src", safe)
}

function setSafeBackgroundImage($el, url) {
  const safe = sanitizeUrl(url)
  if (safe) $el.css("background-image", `url("${safe}")`)
}

function renderFeaturedTours(data) {
  const list = (data || []).slice(0, 2)
  const $wrap = $("#tourCards")
  if (!$wrap.length) return
  $wrap.html("")
  list.forEach((t) => {
    const firstImg = (t.tourImageUrl && t.tourImageUrl[0]) || ""
    const $card = $(`
      <div class="tour-card anim-reveal anim-hover-zoom">
        <div class="card-background"></div>
        <div class="card-overlay"></div>
        <div class="card-content">
          <div class="card-category">IN THE SPOTLIGHTS</div>
          <h3></h3>
          <p></p>
          <button class="book-btn">BOOK NOW</button>
        </div>
      </div>`)
    $card.find("h3").text(t.tourName || "")
    $card.find("p").text(t.tourDescription || "")
    setSafeBackgroundImage($card.find(".card-background"), firstImg)
    $wrap.append($card)
  })
}

function renderWaysToTravel(data) {
  const $grid = $("#travelGrid")
  if (!$grid.length) return
  $grid.html("")

  const start = 0
  const pageSize = 5
  const renderSlice = () => {
    ;(data || []).slice(start, start + pageSize).forEach((item) => {
      const img0 = (item.tourImageUrl && item.tourImageUrl[0]) || ""
      const $el = $(`
        <div class="travel-item anim-reveal anim-hover-zoom anim-hover-tilt">
          <div class="travel-image"><img alt="" /></div>
          <div class="travel-content"><h4></h4><span></span></div>
        </div>`)
      setSafeImgSrc($el.find("img"), img0)
      $el.find("img").attr({ width: 300, height: 150, loading: "lazy", decoding: "async", alt: item.travelName || "" })
      rotateImages($el.find("img"), item.tourImageUrl)
      $el.find("h4").text(item.travelName || "")
      $el.find("span").text(`${item.tourTotalCount || 0}+ tours`)
      $grid.append($el)
    })
  }
  renderSlice()
}

function renderDailyTours(data) {
  const $grid = $("#dailyTourGrid")
  if (!$grid.length) return
  $grid.html("")
  const start = 0
  const pageSize = 3
  const renderSlice = () => {
    ;(data || []).slice(start, start + pageSize).forEach((item) => {
      const img0 = (item.tourImageUrl && item.tourImageUrl[0]) || ""
      const $el = $(`
        <div class="daily-tour-card anim-reveal anim-hover-zoom anim-hover-tilt">
          <div class="daily-card-background"></div>
          <div class="daily-card-overlay"></div>
          <div class="daily-card-content">
            <div class="daily-card-category">DAILY TOURS</div>
            <h3></h3><p></p>
            <button class="view-tours-btn">VIEW ALL TOURS</button>
          </div>
        </div>`)
      setSafeBackgroundImage($el.find(".daily-card-background"), img0)
      $el.find("h3").text(item.dailyTourName || "")
      $el.find("p").text(item.dailyDescription || "")
      $grid.append($el)
    })
  }
  renderSlice()
}

function buildProductCard(t) {
  const imgMain = (t.ImageUrl && t.ImageUrl[0]) || ""
  const imgAlt = (t.ImageUrl && t.ImageUrl[1]) || imgMain
  const $card = $(`
    <div class="product-card anim-reveal anim-hover-zoom">
      <div class="product-image-wrapper">
        <img class="main-image" alt="" />
        <img class="gallery-image" alt="" />
        <div class="wishlist-btn${t.IsLove ? " active" : ""}"><i class="fas fa-heart"></i></div>
      </div>
      <div class="product-content">
        <p class="product-categories"></p>
        <h3 class="product-title"><a href="#"></a></h3>
        <div class="product-price"><span class="currency">$</span><span class="amount"></span></div>
        <a href="#" class="read-more-btn">Read more</a>
      </div>
    </div>`)
  setSafeImgSrc($card.find(".main-image"), imgMain)
  $card
    .find(".main-image")
    .attr({ width: 1200, height: 800, loading: "lazy", decoding: "async", alt: t.TourName || "" })
  setSafeImgSrc($card.find(".gallery-image"), imgAlt)
  $card
    .find(".gallery-image")
    .attr({ width: 1200, height: 800, loading: "lazy", decoding: "async", alt: "Gallery Image" })
  const $cats = $card.find(".product-categories").empty()
  ;(t.CategoryListName || []).forEach((c, i) => {
    const $a = $('<a href="#"></a>').text(String(c || ""))
    $cats.append($a)
    if (i < (t.CategoryListName || []).length - 1) $cats.append(document.createTextNode(", "))
  })
  $card.find(".product-title a").text(t.TourName || "")
  $card.find(".amount").text(t.Price || "")
  return $card
}

function renderGridWithLoadMore($grid, tours, pageSize) {
  const start = 0
  $grid.html("")
  const renderSlice = () => {
    ;(tours || []).slice(start, start + pageSize).forEach((t) => $grid.append(buildProductCard(t)))
  }
  renderSlice()
}

function renderCombinedSections(data) {
  const $wrapper = $("#combinedSections")
  if (!$wrapper.length) return
  $wrapper.empty() // clear toàn bộ nội dung cũ
  ;(data || []).forEach((section, idx) => {
    if (idx % 2 === 0) {
      // === NO-SPOTLIGHT (0,2,4,...)
      const $section = $(`
        <section class="no-spotlight-section">
          <div class="bare-container">
            <div class="section-header">
              <h2>${section.categoryName || ""}</h2>
              <a href="#" class="browse-all">
                <i class="fas fa-arrow-right"></i>
                <span>Browse all tours</span>
              </a>
            </div>
            <div class="divider"></div>
            <div class="no-spotlight-grid"></div>
          </div>
        </section>
      `)

      // render grid
      const $grid = $section.find(".no-spotlight-grid")
      renderGridWithLoadMore($grid, section.tourList || [], 4)

      $wrapper.append($section)
    } else {
      // === SPOTLIGHT (1,3,5,...)
      const oddSeq = Math.floor((idx - 1) / 2)
      const isLeftSpotlight = oddSeq % 2 === 0

      if (isLeftSpotlight) {
        // spotlight LEFT
        const $section = $(`
          <section class="left-spotlight-section">
            <div class="bare-container">
              <div class="left-spotlight-content">
                <div class="left-spotlight-left">
                  <div class="spotlight-badge">${section.spotlightBadge || ""}</div>
                  <h1>${section.spotlightTitle || ""}</h1>
                  <p>${section.spotlightDescription || ""}</p>
                  <button class="book-now-btn">${section.spotlightBtnText || "BOOK NOW"}</button>
                </div>
                <div class="left-spotlight-right">
                  <div class="section-header">
                    <h2>${section.categoryName || ""}</h2>
                    <a href="#" class="browse-all">
                      <i class="fas fa-arrow-right"></i>
                      <span>Browse all tours</span>
                    </a>
                  </div>
                  <div class="divider"></div>
                  <div class="left-spotlight-grid"></div>
                </div>
              </div>
            </div>
          </section>
        `)

        const $grid = $section.find(".left-spotlight-grid")
        renderGridWithLoadMore($grid, section.tourList || [], 3)

        if (section.spotlightImageUrl) {
          let safe = section.spotlightImageUrl
          if (typeof sanitizeUrl === "function") safe = sanitizeUrl(section.spotlightImageUrl)
          if (safe) {
            $section
              .find(".left-spotlight-left")
              .css(
                "background-image",
                `linear-gradient(135deg, rgba(0, 212, 170, 0.9) 0%, rgba(0, 150, 120, 0.9) 100%), url("${safe}")`,
              )
          }
        }

        $wrapper.append($section)
      } else {
        // spotlight RIGHT
        const $section = $(`
          <section class="right-spotlight-section">
            <div class="bare-container">
              <div class="right-spotlight-content">
                <div class="right-spotlight-left">
                  <div class="section-header">
                    <h2>${section.categoryName || ""}</h2>
                    <a href="#" class="browse-all">
                      <i class="fas fa-arrow-right"></i>
                      <span>Browse all tours</span>
                    </a>
                  </div>
                  <div class="divider"></div>
                  <div class="right-spotlight-grid"></div>
                </div>
                <div class="right-spotlight-right">
                  <div class="spotlight-badge">${section.spotlightBadge || ""}</div>
                  <h1>${section.spotlightTitle || ""}</h1>
                  <p>${section.spotlightDescription || ""}</p>
                  <button class="book-now-btn">${section.spotlightBtnText || "BOOK NOW"}</button>
                </div>
              </div>
            </div>
          </section>
        `)

        const $grid = $section.find(".right-spotlight-grid")
        renderGridWithLoadMore($grid, section.tourList || [], 3)

        if (section.spotlightImageUrl) {
          let safe = section.spotlightImageUrl
          if (typeof sanitizeUrl === "function") safe = sanitizeUrl(section.spotlightImageUrl)
          if (safe) {
            $section
              .find(".right-spotlight-right")
              .css(
                "background-image",
                `linear-gradient(135deg, rgba(0, 212, 170, 0.9) 0%, rgba(0, 150, 120, 0.9) 100%), url("${safe}")`,
              )
          }
        }

        $wrapper.append($section)
      }
    }
  })
}

// =====================
// Data loading (AJAX)
// =====================
// NOTE: sections.json loading removed - using API data instead
// Home page now loads data dynamically from backend APIs:
// - Featured Tours: via loadFeaturedTours() in Views/Home/Index.cshtml
// - Daily Tours: via loadDailyTours() 
// - Ways to Travel: via loadWaysToTravel()
// - Combined Sections: via loadHomepageData()
