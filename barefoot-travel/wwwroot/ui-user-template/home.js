// Home Page JavaScript
$(document).ready(() => {
  // Loading animation
  setTimeout(() => {
    $(".loading").addClass("hide");
  }, 1500);

  // Header scroll effect
  $(window).scroll(function () {
    if ($(this).scrollTop() > 100) {
      $(".header").addClass("scrolled");
    } else {
      $(".header").removeClass("scrolled");
    }
  });

  // Smooth scrolling for anchor links
  $('a[href^="#"]').on("click", function (event) {
    var target = $(this.getAttribute("href"));
    if (target.length) {
      event.preventDefault();
      $("html, body")
        .stop()
        .animate(
          {
            scrollTop: target.offset().top - 80,
          },
          1000
        );
    }
  });

  // Animate elements on scroll
  function animateOnScroll() {
    $(".animate-on-scroll").each(function () {
      var elementTop = $(this).offset().top;
      var elementBottom = elementTop + $(this).outerHeight();
      var viewportTop = $(window).scrollTop();
      var viewportBottom = viewportTop + $(window).height();

      if (elementBottom > viewportTop && elementTop < viewportBottom) {
        $(this).addClass("animated");
      }
    });
  }

  $(window).on("scroll resize", animateOnScroll);
  animateOnScroll(); // Initial check

  // Search functionality
  $(".search-form button").on("click", function (e) {
    e.preventDefault();
    var searchTerm = $(".search-form input").val();
    if (searchTerm.trim() !== "") {
      console.log("Searching for:", searchTerm);
      alert("Search functionality would be implemented here!");
    }
  });

  $(".search-form input").on("keypress", function (e) {
    if (e.which === 13) {
      $(".search-form button").click();
    }
  });

  // Header search box functionality
  $(".header .search-box input").on("keypress", function (e) {
    if (e.which === 13) {
      var searchTerm = $(this).val();
      if (searchTerm.trim() !== "") {
        console.log("Header search for:", searchTerm);
        alert("Header search functionality would be implemented here!");
      }
    }
  });

  // Dropdown menu functionality
  $(".nav-item").hover(
    function () {
      $(this).find(".dropdown-menu").stop(true, true).fadeIn(300);
    },
    function () {
      $(this).find(".dropdown-menu").stop(true, true).fadeOut(300);
    }
  );

  // Language and currency selectors
  $(".language-selector").on("click", function () {
    console.log("Language selector clicked");
    // Language selection logic would go here
  });

  $(".currency-selector").on("click", function () {
    console.log("Currency selector clicked");
    // Currency selection logic would go here
  });

  // Tour card interactions
  $(".tour-card").hover(
    function () {
      $(this).find(".card-background").css("transform", "scale(1.05)");
    },
    function () {
      $(this).find(".card-background").css("transform", "scale(1)");
    }
  );

  // Daily tour card interactions
  $(".daily-tour-card").hover(
    function () {
      $(this).find(".daily-card-background").css("transform", "scale(1.05)");
    },
    function () {
      $(this).find(".daily-card-background").css("transform", "scale(1)");
    }
  );

  // Book button functionality
  $(".book-btn").on("click", function (e) {
    e.preventDefault();
    var tourTitle = $(this).closest(".tour-card").find("h3").text();
    console.log("Booking tour:", tourTitle);
    alert("Booking functionality would be implemented here for: " + tourTitle);
  });

  // View tours button functionality
  $(".view-tours-btn").on("click", function (e) {
    e.preventDefault();
    var destination = $(this).closest(".daily-tour-card").find("h3").text();
    console.log("Viewing tours for:", destination);
    alert("View tours functionality would be implemented here for: " + destination);
  });

  // Product card interactions
  $(".product-card").hover(
    function () {
      $(this).find(".main-image").css("transform", "scale(1.05)");
      $(this).find(".gallery-image").css("opacity", "1");
    },
    function () {
      $(this).find(".main-image").css("transform", "scale(1)");
      $(this).find(".gallery-image").css("opacity", "0");
    }
  );

  // Wishlist functionality
  $(".wishlist-btn").on("click", function (e) {
    e.preventDefault();
    var productTitle = $(this).closest(".product-card").find(".product-title a").text();
    console.log("Added to wishlist:", productTitle);
    
    // Toggle wishlist state
    if ($(this).hasClass("active")) {
      $(this).removeClass("active");
      $(this).find("i").removeClass("fas").addClass("far");
      alert("Removed from wishlist: " + productTitle);
    } else {
      $(this).addClass("active");
      $(this).find("i").removeClass("far").addClass("fas");
      alert("Added to wishlist: " + productTitle);
    }
  });

  // Read more button functionality
  $(".read-more-btn").on("click", function (e) {
    e.preventDefault();
    var productTitle = $(this).closest(".product-card").find(".product-title a").text();
    console.log("Reading more about:", productTitle);
    alert("Read more functionality would be implemented here for: " + productTitle);
  });

  // Travel item interactions
  $(".travel-item").on("click", function () {
    var travelType = $(this).find("h4").text();
    console.log("Selected travel type:", travelType);
    alert("Travel type selection would be implemented here for: " + travelType);
  });

  // Tab functionality for best sellers
  $(".tab-btn").on("click", function () {
    $(".tab-btn").removeClass("active");
    $(this).addClass("active");
    
    var tabValue = $(this).text();
    console.log("Selected tab:", tabValue);
    
    // Filter products based on tab selection
    // This would be implemented based on your data structure
    alert("Tab functionality would filter products for: " + tabValue + " days");
  });

  // Back to top functionality
  $("#backToTop").on("click", function () {
    $("html, body").animate(
      {
        scrollTop: 0,
      },
      1000
    );
  });

  // Show/hide back to top button
  $(window).scroll(function () {
    if ($(this).scrollTop() > 300) {
      $("#backToTop").fadeIn();
    } else {
      $("#backToTop").fadeOut();
    }
  });

  // Initialize back to top button as hidden
  $("#backToTop").hide();

  // Social media links
  $(".social-link").on("click", function (e) {
    e.preventDefault();
    var platform = $(this).find("i").attr("class").split("-")[2];
    console.log("Social media link clicked:", platform);
    // Social media functionality would be implemented here
  });

  // Footer links
  $(".footer-links-col a").on("click", function (e) {
    e.preventDefault();
    var linkText = $(this).text();
    console.log("Footer link clicked:", linkText);
    // Footer link functionality would be implemented here
  });

  // Payment method icons hover effect
  $(".payment-icons i").hover(
    function () {
      $(this).css("transform", "scale(1.2)");
    },
    function () {
      $(this).css("transform", "scale(1)");
    }
  );

  // Parallax effect for hero section
  $(window).scroll(function () {
    var scrolled = $(this).scrollTop();
    var parallax = $(".hero-background");
    var speed = scrolled * 0.5;
    parallax.css("transform", "translateY(" + speed + "px)");
  });

  // Mobile menu toggle (if needed for responsive design)
  function initMobileMenu() {
    if ($(window).width() <= 768) {
      // Mobile menu functionality would go here
      console.log("Mobile menu initialized");
    }
  }

  $(window).on("resize", initMobileMenu);
  initMobileMenu();

  // Loading animation (if needed)
  function showLoadingAnimation() {
    $("body").addClass("loading");
    setTimeout(function () {
      $("body").removeClass("loading");
    }, 1000);
  }

  // Initialize page
  function initPage() {
    console.log("Home page initialized");
    // Any additional initialization code would go here
  }

  initPage();
});

// Additional utility functions
function debounce(func, wait, immediate) {
  var timeout;
  return function () {
    var context = this,
      args = arguments;
    var later = function () {
      timeout = null;
      if (!immediate) func.apply(context, args);
    };
    var callNow = immediate && !timeout;
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
    if (callNow) func.apply(context, args);
  };
}

// Optimized scroll handler
var optimizedScrollHandler = debounce(function () {
  // Header scroll effect
  if ($(window).scrollTop() > 100) {
    $(".header").addClass("scrolled");
  } else {
    $(".header").removeClass("scrolled");
  }

  // Back to top button
  if ($(window).scrollTop() > 300) {
    $("#backToTop").fadeIn();
  } else {
    $("#backToTop").fadeOut();
  }

  // Parallax effect
  var scrolled = $(window).scrollTop();
  var parallax = $(".hero-background");
  var speed = scrolled * 0.5;
  parallax.css("transform", "translateY(" + speed + "px)");
}, 10);

$(window).on("scroll", optimizedScrollHandler);

// Intersection Observer for better performance
if ("IntersectionObserver" in window) {
  const observerOptions = {
    threshold: 0.1,
    rootMargin: "0px 0px -50px 0px",
  };

  const observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add("animate__animated", "animate__fadeInUp");
      }
    });
  }, observerOptions);

  // Observe elements for animation
  document.querySelectorAll(".product-card, .tour-card, .daily-tour-card, .contact-us-card").forEach((el) => {
    observer.observe(el);
  });
}