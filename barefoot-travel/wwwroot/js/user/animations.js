(function ($) {
  // Inject animation CSS (keeps color styling in sections-theme.css)
  const css = `
  /* Keyframes */
  @keyframes subtleFloat { 0%,100%{transform: translateY(0)} 50%{transform: translateY(-4px)} }
  @keyframes subtlePulse { 0%,100%{opacity:1} 50%{opacity:0.95} }
  @keyframes riseIn { from{opacity:0; transform: translateY(24px)} to{opacity:1; transform: translateY(0)} }
  @keyframes fadeSlideUp { from{opacity:0; transform: translate3d(0,20px,0)} to{opacity:1; transform: translate3d(0,0,0)} }
  @keyframes tiltHover { from{transform: perspective(600px) rotateX(0) rotateY(0)} to{transform: perspective(600px) rotateX(2deg) rotateY(-2deg)} }

  /* Default gentle motion */
  .anim-float { animation: subtleFloat 6s ease-in-out infinite; will-change: transform; }
  .anim-pulse { animation: subtlePulse 5s ease-in-out infinite; }

  /* Scroll reveal base */
  
  .anim-reveal.in-view { opacity: 1; transform: translateY(0); transition: transform .6s ease, opacity .6s ease; }
  .anim-slide-up { opacity: 0; transform: translateY(20px); }
  .anim-slide-up.in-view { opacity: 1; transform: translateY(0); transition: transform .6s ease, opacity .6s ease; }

  /* Hover interactions */
  .anim-hover-zoom { transition: transform .25s ease, box-shadow .25s ease; }
  .anim-hover-zoom:hover { transform: translateY(-6px) scale(1.02); box-shadow: 0 14px 34px rgba(0,0,0,.15); }
  .anim-hover-tilt { transition: transform .25s ease; }
  .anim-hover-tilt:hover { transform: perspective(600px) rotateX(2deg) rotateY(-2deg) translateY(-4px); }
  `

  $(function () {
    $('<style type="text/css">').text(css).appendTo(document.head)

    // Helper: observe elements for scroll reveal
    function observeReveal($elements) {
      if (!('IntersectionObserver' in window)) {
        $elements.addClass('in-view')
        return
      }
      const observer = new IntersectionObserver(
        (entries) => {
          entries.forEach((entry) => {
            if (entry.isIntersecting) entry.target.classList.add('in-view')
          })
        },
        { threshold: 0.12, rootMargin: '0px 0px -60px 0px' },
      )
      $elements.each(function () { observer.observe(this) })
    }

    // Target non-hero sections and cards
    const sectionSelectors = [
      '.featured-tours',
      '.ways-to-travel',
      '.left-spotlight-section',
      '.no-spotlight-section',
      '.right-spotlight-section',
      '.about-compass',
      '.contact-us',
      '.footer-ending',
    ]

    // Apply default gentle motion at section level (skip heavy containers on mobile)
    $(sectionSelectors.join(', ')).each(function (i) {
      const $s = $(this)
      if (window.matchMedia('(max-width: 480px)').matches) return
      $s.addClass(i % 2 === 0 ? 'anim-pulse' : 'anim-float')
    })

    // Card/grid item animations
    const itemSelectors = [
      '.tour-card',
      '.travel-item',
      '.daily-tour-card',
      '.product-card',
      '.right-spotlight-grid .product-card',
      '.left-spotlight-grid .product-card',
      '.no-spotlight-grid .product-card',
      '.about-compass .about-visual',
      '.contact-us-card',
      '.footer-links-grid .footer-links-col',
    ]

    const $items = $(itemSelectors.join(', '))
    $items.addClass('anim-reveal anim-hover-zoom')
    observeReveal($items)

    // Add a second animation to some elements for richer motion
    $('.travel-item, .daily-tour-card, .contact-us-card').addClass('anim-hover-tilt')

    // Minor parallax feel for background sections (avoid hero)
    const $parallaxSections = $('.featured-tours, .ways-to-travel, .left-spotlight-section, .no-spotlight-section, .right-spotlight-section, .about-compass, .contact-us')
    $(window).on('scroll', function () {
      const scrolled = $(this).scrollTop()
      $parallaxSections.each(function (idx) {
        const rate = (idx % 2 === 0 ? 0.2 : -0.15) * scrolled
        $(this).css('background-position', `center ${rate}px`)
      })
    })

    // Enhance existing buttons with ripple using existing helper if present
    $(document).on('click', '.book-btn, .book-now-btn, .read-more-btn', function (e) {
      const $btn = $(this)
      if (typeof createEnhancedRipple === 'function') {
        createEnhancedRipple($btn, e)
      }
    })
  })
})(jQuery)


