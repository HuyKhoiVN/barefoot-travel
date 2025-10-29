/**
 * Modern Interactive Effects for Travel Booking Website
 * Advanced animations, parallax, and user interactions
 */

(function($) {
    'use strict';

    // ============================================
    // SCROLL REVEAL ANIMATIONS
    // ============================================
    
    function initScrollReveal() {
        // Create Intersection Observer for scroll animations
        const revealObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('revealed');
                    // Optional: unobserve after reveal
                    // revealObserver.unobserve(entry.target);
                }
            });
        }, {
            threshold: 0.15,
            rootMargin: '0px 0px -50px 0px'
        });

        // Observe all elements with scroll-reveal classes
        document.querySelectorAll('.scroll-reveal, .scroll-reveal-left, .scroll-reveal-right, .scroll-reveal-scale').forEach(el => {
            revealObserver.observe(el);
        });

        // Cards hiển thị ngay, không cần scroll reveal
        // $('.tour-card, .daily-tour-card, .product-card, .travel-item, .contact-us-card').each(function(index) {
        //     $(this).addClass('scroll-reveal').css('animation-delay', `${index * 0.1}s`);
        // });
    }

    // ============================================
    // PARALLAX EFFECTS
    // ============================================
    
    function initParallax() {
        // Tắt parallax effect để giữ ảnh nguyên vị trí
        // Chỉ giữ hero background parallax nhẹ nếu cần
        let ticking = false;

        $(window).on('scroll', function() {
            if (!ticking) {
                window.requestAnimationFrame(function() {
                    const scrolled = $(window).scrollTop();

                    // Hero parallax nhẹ (optional - có thể comment out nếu không muốn)
                    // $('.hero-background').css('transform', `translateY(${scrolled * 0.3}px)`);
                    
                    // Tắt parallax cho section backgrounds để giữ ảnh nguyên vị trí
                    // $('.about-bg, .spotlight-card-background, .right-spotlight-background').each(function() {
                    //     // Parallax disabled
                    // });

                    ticking = false;
                });

                ticking = true;
            }
        });
    }

    // ============================================
    // CARD 3D TILT EFFECT
    // ============================================
    
    function init3DTilt() {
        $('.product-card, .tour-card, .daily-tour-card').on('mousemove', function(e) {
            const $card = $(this);
            const rect = this.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            
            const centerX = rect.width / 2;
            const centerY = rect.height / 2;
            
            const rotateX = (y - centerY) / 10;
            const rotateY = (centerX - x) / 10;
            
            $card.css({
                'transform': `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) scale3d(1.02, 1.02, 1.02)`,
                'transition': 'transform 0.1s ease'
            });
        });

        $('.product-card, .tour-card, .daily-tour-card').on('mouseleave', function() {
            $(this).css({
                'transform': 'perspective(1000px) rotateX(0) rotateY(0) scale3d(1, 1, 1)',
                'transition': 'transform 0.5s ease'
            });
        });
    }

    // ============================================
    // MAGNETIC BUTTONS
    // ============================================
    
    function initMagneticButtons() {
        $('.book-btn, .book-now-btn, .read-more-btn, .view-tours-btn').each(function() {
            const $btn = $(this);
            let rafId = null;

            $btn.on('mousemove', function(e) {
                if (rafId) cancelAnimationFrame(rafId);
                
                rafId = requestAnimationFrame(() => {
                    const rect = this.getBoundingClientRect();
                    const x = e.clientX - rect.left - rect.width / 2;
                    const y = e.clientY - rect.top - rect.height / 2;
                    
                    const moveX = x * 0.3;
                    const moveY = y * 0.3;
                    
                    $btn.css('transform', `translate(${moveX}px, ${moveY}px) scale(1.05)`);
                });
            });

            $btn.on('mouseleave', function() {
                if (rafId) cancelAnimationFrame(rafId);
                $btn.css('transform', 'translate(0, 0) scale(1)');
            });
        });
    }

    // ============================================
    // CURSOR EFFECTS
    // ============================================
    
    function initCustomCursor() {
        // Create custom cursor elements
        const $cursor = $('<div class="custom-cursor"></div>');
        const $cursorFollower = $('<div class="custom-cursor-follower"></div>');
        
        $('body').append($cursor).append($cursorFollower);

        // Add styles
        $('<style>')
            .prop('type', 'text/css')
            .html(`
                .custom-cursor,
                .custom-cursor-follower {
                    position: fixed;
                    border-radius: 50%;
                    pointer-events: none;
                    z-index: 9999;
                    mix-blend-mode: difference;
                }
                .custom-cursor {
                    width: 10px;
                    height: 10px;
                    background: #00d4aa;
                    transition: transform 0.1s ease;
                }
                .custom-cursor-follower {
                    width: 40px;
                    height: 40px;
                    border: 2px solid #00d4aa;
                    transition: transform 0.3s ease, width 0.3s ease, height 0.3s ease;
                }
                .custom-cursor.active {
                    transform: scale(0.5);
                }
                .custom-cursor-follower.active {
                    transform: scale(1.5);
                }
            `)
            .appendTo('head');

        let mouseX = 0, mouseY = 0;
        let cursorX = 0, cursorY = 0;
        let followerX = 0, followerY = 0;

        $(document).on('mousemove', function(e) {
            mouseX = e.clientX;
            mouseY = e.clientY;
        });

        // Smooth cursor animation
        function animateCursor() {
            cursorX += (mouseX - cursorX) * 0.9;
            cursorY += (mouseY - cursorY) * 0.9;
            
            followerX += (mouseX - followerX) * 0.1;
            followerY += (mouseY - followerY) * 0.1;

            $cursor.css({
                left: cursorX + 'px',
                top: cursorY + 'px'
            });

            $cursorFollower.css({
                left: followerX - 20 + 'px',
                top: followerY - 20 + 'px'
            });

            requestAnimationFrame(animateCursor);
        }

        animateCursor();

        // Cursor interactions
        $('a, button, .product-card, .tour-card').on('mouseenter', function() {
            $cursor.addClass('active');
            $cursorFollower.addClass('active');
        }).on('mouseleave', function() {
            $cursor.removeClass('active');
            $cursorFollower.removeClass('active');
        });
    }

    // ============================================
    // IMAGE LAZY LOADING WITH BLUR EFFECT
    // ============================================
    
    function initLazyLoadingWithBlur() {
        const imageObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    const actualSrc = img.dataset.src;
                    
                    if (actualSrc) {
                        // Add blur effect
                        img.style.filter = 'blur(10px)';
                        img.style.transition = 'filter 0.5s ease';
                        
                        // Load image
                        const tempImg = new Image();
                        tempImg.onload = function() {
                            img.src = actualSrc;
                            setTimeout(() => {
                                img.style.filter = 'blur(0)';
                            }, 50);
                        };
                        tempImg.src = actualSrc;
                        
                        imageObserver.unobserve(img);
                    }
                }
            });
        });

        document.querySelectorAll('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    }

    // ============================================
    // FLOATING ELEMENTS
    // ============================================
    
    function initFloatingElements() {
        $('.badge, .spotlight-badge').each(function(index) {
            const $elem = $(this);
            const delay = index * 0.2;
            const duration = 3 + (index % 3);
            
            $elem.css({
                'animation': `float ${duration}s ease-in-out ${delay}s infinite`
            });
        });
    }

    // ============================================
    // SMOOTH SCROLL WITH EASING
    // ============================================
    
    function initSmoothScroll() {
        $('a[href^="#"]').on('click', function(e) {
            const target = $(this.getAttribute('href'));
            
            if (target.length) {
                e.preventDefault();
                
                $('html, body').stop().animate({
                    scrollTop: target.offset().top - 80
                }, 1000, 'easeInOutCubic');
            }
        });

        // Custom easing function
        $.easing.easeInOutCubic = function(x) {
            return x < 0.5 ? 4 * x * x * x : 1 - Math.pow(-2 * x + 2, 3) / 2;
        };
    }

    // ============================================
    // COUNTER ANIMATION
    // ============================================
    
    function animateCounter($element, start, end, duration) {
        const range = end - start;
        const increment = range / (duration / 16);
        let current = start;
        
        const timer = setInterval(() => {
            current += increment;
            if ((increment > 0 && current >= end) || (increment < 0 && current <= end)) {
                current = end;
                clearInterval(timer);
            }
            $element.text(Math.floor(current));
        }, 16);
    }

    function initCounters() {
        const counterObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const $counter = $(entry.target);
                    const endValue = parseInt($counter.data('count'));
                    animateCounter($counter, 0, endValue, 2000);
                    counterObserver.unobserve(entry.target);
                }
            });
        });

        $('.indicator-item span').each(function() {
            const text = $(this).text();
            const match = text.match(/(\d+)/);
            if (match) {
                const number = parseInt(match[1]);
                $(this).data('count', number).text('0');
                counterObserver.observe(this);
            }
        });
    }

    // ============================================
    // RIPPLE EFFECT ON CLICK
    // ============================================
    
    function initRippleEffect() {
        $('.book-btn, .book-now-btn, .read-more-btn, .view-tours-btn, .product-card').on('click', function(e) {
            const $ripple = $('<span class="ripple-effect"></span>');
            const $this = $(this);
            
            const diameter = Math.max($this.width(), $this.height());
            const radius = diameter / 2;
            
            const offset = $this.offset();
            const x = e.pageX - offset.left - radius;
            const y = e.pageY - offset.top - radius;
            
            $ripple.css({
                width: diameter,
                height: diameter,
                top: y + 'px',
                left: x + 'px'
            }).appendTo($this);
            
            setTimeout(() => $ripple.remove(), 600);
        });

        // Add ripple styles
        $('<style>')
            .prop('type', 'text/css')
            .html(`
                .ripple-effect {
                    position: absolute;
                    border-radius: 50%;
                    background: rgba(255, 255, 255, 0.6);
                    transform: scale(0);
                    animation: ripple 0.6s ease-out;
                    pointer-events: none;
                }
                @keyframes ripple {
                    to {
                        transform: scale(4);
                        opacity: 0;
                    }
                }
            `)
            .appendTo('head');
    }

    // ============================================
    // BACKGROUND PARTICLES
    // ============================================
    
    function initParticles() {
        const canvas = document.createElement('canvas');
        const ctx = canvas.getContext('2d');
        
        canvas.style.position = 'fixed';
        canvas.style.top = '0';
        canvas.style.left = '0';
        canvas.style.width = '100%';
        canvas.style.height = '100%';
        canvas.style.pointerEvents = 'none';
        canvas.style.zIndex = '0';
        canvas.style.opacity = '0.3';
        
        document.body.insertBefore(canvas, document.body.firstChild);
        
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        
        const particles = [];
        const particleCount = 50;
        
        class Particle {
            constructor() {
                this.reset();
            }
            
            reset() {
                this.x = Math.random() * canvas.width;
                this.y = Math.random() * canvas.height;
                this.vx = (Math.random() - 0.5) * 0.5;
                this.vy = (Math.random() - 0.5) * 0.5;
                this.radius = Math.random() * 2 + 1;
            }
            
            update() {
                this.x += this.vx;
                this.y += this.vy;
                
                if (this.x < 0 || this.x > canvas.width) this.vx *= -1;
                if (this.y < 0 || this.y > canvas.height) this.vy *= -1;
            }
            
            draw() {
                ctx.beginPath();
                ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2);
                ctx.fillStyle = 'rgba(0, 212, 170, 0.5)';
                ctx.fill();
            }
        }
        
        for (let i = 0; i < particleCount; i++) {
            particles.push(new Particle());
        }
        
        function animate() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            
            particles.forEach((particle, i) => {
                particle.update();
                particle.draw();
                
                // Draw connections
                particles.forEach((otherParticle, j) => {
                    if (i !== j) {
                        const dx = particle.x - otherParticle.x;
                        const dy = particle.y - otherParticle.y;
                        const distance = Math.sqrt(dx * dx + dy * dy);
                        
                        if (distance < 150) {
                            ctx.beginPath();
                            ctx.moveTo(particle.x, particle.y);
                            ctx.lineTo(otherParticle.x, otherParticle.y);
                            ctx.strokeStyle = `rgba(0, 212, 170, ${0.2 * (1 - distance / 150)})`;
                            ctx.lineWidth = 1;
                            ctx.stroke();
                        }
                    }
                });
            });
            
            requestAnimationFrame(animate);
        }
        
        animate();
        
        $(window).on('resize', () => {
            canvas.width = window.innerWidth;
            canvas.height = window.innerHeight;
        });
    }

    // ============================================
    // GRADIENT ANIMATION ON HOVER
    // ============================================
    
    function initGradientAnimation() {
        $('.tour-card, .daily-tour-card').each(function() {
            const $card = $(this);
            $card.on('mouseenter', function() {
                const $overlay = $(this).find('.card-overlay, .daily-card-overlay');
                $overlay.css({
                    'background': 'linear-gradient(135deg, rgba(0, 212, 170, 0.7) 0%, rgba(102, 126, 234, 0.7) 100%)',
                    'transition': 'background 0.5s ease'
                });
            }).on('mouseleave', function() {
                const $overlay = $(this).find('.card-overlay, .daily-card-overlay');
                $overlay.css({
                    'background': 'linear-gradient(135deg, rgba(0, 0, 0, 0.3) 0%, rgba(0, 0, 0, 0.7) 100%)',
                    'transition': 'background 0.5s ease'
                });
            });
        });
    }

    // ============================================
    // SCROLL PROGRESS INDICATOR
    // ============================================
    
    function initScrollProgress() {
        const $progressBar = $('<div class="scroll-progress"></div>');
        $('body').append($progressBar);
        
        $('<style>')
            .prop('type', 'text/css')
            .html(`
                .scroll-progress {
                    position: fixed;
                    top: 0;
                    left: 0;
                    width: 0;
                    height: 4px;
                    background: linear-gradient(90deg, #00d4aa 0%, #667eea 100%);
                    z-index: 10000;
                    transition: width 0.1s ease;
                    box-shadow: 0 0 10px rgba(0, 212, 170, 0.5);
                }
            `)
            .appendTo('head');
        
        $(window).on('scroll', function() {
            const winScroll = $(this).scrollTop();
            const height = $(document).height() - $(window).height();
            const scrolled = (winScroll / height) * 100;
            $progressBar.css('width', scrolled + '%');
        });
    }

    // ============================================
    // TYPING EFFECT FOR HERO
    // ============================================
    
    function initTypingEffect() {
        const $heroTitle = $('.hero-text h1');
        
        if ($heroTitle.length) {
            const originalHTML = $heroTitle.html();
            const lines = originalHTML.split('<br>');
            
            if (lines.length === 2) {
                // Two lines with typing effect
                $heroTitle.html(`
                    <span class="typing-line-1">${lines[0]}</span><br>
                    <span class="typing-line-2">${lines[1]}</span>
                `);
                
                // Remove caret after animation completes
                setTimeout(() => {
                    $('.typing-line-1').css('border-right', 'none');
                }, 2000);
                
                setTimeout(() => {
                    $('.typing-line-2').addClass('finished');
                }, 4100);
            } else {
                // Single line with typing effect
                $heroTitle.addClass('typing-effect');
                
                setTimeout(() => {
                    $heroTitle.removeClass('typing-effect').addClass('typing-effect no-caret');
                }, 3500);
            }
        }
    }

    // ============================================
    // SECTION TITLE HOVER EFFECTS
    // ============================================
    
    function initSectionTitleHover() {
        $('.section-header h2').each(function() {
            $(this).on('mouseenter', function() {
                $(this).css('transform', 'translateX(5px)');
            }).on('mouseleave', function() {
                $(this).css('transform', 'translateX(0)');
            });
        });
    }

    // ============================================
    // INITIALIZE ALL EFFECTS
    // ============================================
    
    $(document).ready(function() {
        // Initialize all effects with a slight delay for better performance
        setTimeout(() => {
            initScrollReveal();
            initParallax();
            init3DTilt();
            initMagneticButtons();
            initFloatingElements();
            initSmoothScroll();
            initCounters();
            initRippleEffect();
            initGradientAnimation();
            initScrollProgress();
            initTypingEffect();
            initSectionTitleHover();
            
            // Optional: Uncomment for additional effects
            // initCustomCursor();
            // initParticles();
            // initLazyLoadingWithBlur();
            
            console.log('✨ Modern effects initialized successfully!');
        }, 500);
    });

})(jQuery);

