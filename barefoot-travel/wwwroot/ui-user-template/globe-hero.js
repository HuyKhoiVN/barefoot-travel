/**
 * ============================================
 * 3D GLOBE HERO SECTION - MAIN LOGIC
 * ============================================
 * 
 * Features:
 * - 3D Interactive Globe with Earth texture
 * - Highlight Vietnam region
 * - Animated arcs between cities
 * - Zoom and rotation controls
 * - Auto-rotation with pause on interaction
 * - Responsive design
 */

class GlobeHero {
    constructor(containerId = 'globe-canvas') {
        this.containerId = containerId;
        this.globe = null;
        this.autoRotate = true;
        this.rotationSpeed = 0.1; // Slower rotation
        this.isLoading = true;
        
        // Vietnam coordinates for initial view - Closer zoom to see provinces
        this.vietnamCoords = {
            lat: 16.0,
            lng: 107.0,
            altitude: 1.2 // Closer view to see provinces
        };
        
        // City data
        this.cities = [
            { 
                name: 'Hà Nội', 
                nameEn: 'Hanoi',
                lat: 21.0278, 
                lng: 105.8342, 
                size: 0.8, 
                color: '#ff4757',
                tours: 85
            },
            { 
                name: 'Đà Nẵng', 
                nameEn: 'Da Nang',
                lat: 16.0544, 
                lng: 108.2022, 
                size: 0.6, 
                color: '#ffa502',
                tours: 62
            },
            { 
                name: 'TP Hồ Chí Minh', 
                nameEn: 'Ho Chi Minh City',
                lat: 10.8231, 
                lng: 106.6297, 
                size: 0.8, 
                color: '#ff6348',
                tours: 103
            }
        ];
        
        // Arc routes between cities
        this.routes = [
            { 
                startLat: 21.0278, 
                startLng: 105.8342, 
                endLat: 16.0544, 
                endLng: 108.2022,
                color: '#00d2ff'
            },
            { 
                startLat: 16.0544, 
                startLng: 108.2022, 
                endLat: 10.8231, 
                endLng: 106.6297,
                color: '#00d2ff'
            },
            { 
                startLat: 21.0278, 
                startLng: 105.8342, 
                endLat: 10.8231, 
                endLng: 106.6297,
                color: '#3a7bd5'
            }
        ];
        
        this.init();
    }
    
    /**
     * Initialize Globe
     */
    async init() {
        try {
            this.showLoading(true);
            
            // Get container size
            const container = document.getElementById(this.containerId);
            const containerWidth = container.offsetWidth;
            const containerHeight = container.offsetHeight;
            
            // Create globe instance with local textures
            this.globe = Globe()
                (container)
                .width(containerWidth)
                .height(containerHeight)
                .backgroundColor('rgba(0,0,0,0)')
                .globeImageUrl('/ui-user-template/textures/earth-blue-marble.jpg')
                .bumpImageUrl('/ui-user-template/textures/earth-topology.png')
                .showAtmosphere(true)
                .atmosphereColor('#00d2ff')
                .atmosphereAltitude(0.15);
            
            // Load world topology and setup features
            await this.loadWorldTopology();
            this.setupCities();
            this.setupRoutes();
            this.setupControls();
            this.setupCamera();
            this.startAutoRotation();
            
            // Hide loading
            setTimeout(() => {
                this.showLoading(false);
            }, 1000);
        } catch (error) {
            console.error('❌ Error initializing globe:', error);
            this.showError('Failed to load 3D Globe. Please refresh the page.');
        }
    }
    
    /**
     * Load world map topology
     */
    async loadWorldTopology() {
        try {
            // Load countries data
            const response = await fetch('/ui-user-template/globe-data/earth-topology.json');
            
            if (!response.ok) {
                // Fallback to online CDN if local file not found
                console.warn('Local topology not found, using CDN...');
                const cdnResponse = await fetch('https://cdn.jsdelivr.net/npm/world-atlas@2/countries-110m.json');
                const worldData = await cdnResponse.json();
                this.applyWorldTopology(worldData);
                return;
            }
            
            const worldData = await response.json();
            this.applyWorldTopology(worldData);
            
        } catch (error) {
            console.error('Error loading topology:', error);
            // Continue without world map
        }
    }
    
    /**
     * Apply world topology to globe
     */
    applyWorldTopology(worldData) {
        const countries = topojson.feature(worldData, worldData.objects.countries);
        
        this.globe
            .polygonsData(countries.features)
            .polygonCapColor(feat => {
                // Highlight Vietnam (VNM)
                if (feat.properties.name === 'Vietnam' || 
                    feat.properties.ISO_A3 === 'VNM') {
                    return 'rgba(0, 210, 255, 0.3)';
                }
                return 'rgba(100, 100, 100, 0.1)';
            })
            .polygonSideColor(() => 'rgba(0, 0, 0, 0.05)')
            .polygonStrokeColor(() => 'rgba(255, 255, 255, 0.1)')
            .polygonAltitude(feat => {
                // Elevate Vietnam
                if (feat.properties.name === 'Vietnam' || 
                    feat.properties.ISO_A3 === 'VNM') {
                    return 0.01;
                }
                return 0.001;
            });
    }
    
    /**
     * Setup city markers
     */
    setupCities() {
        this.globe
            .pointsData(this.cities)
            .pointAltitude(0.01)
            .pointRadius('size')
            .pointColor('color')
            .pointsMerge(false)
            .pointLabel(d => `
                <div style="
                    background: rgba(0, 0, 0, 0.9);
                    padding: 12px 16px;
                    border-radius: 8px;
                    border: 1px solid ${d.color};
                    color: white;
                    font-family: 'Inter', sans-serif;
                ">
                    <div style="font-size: 16px; font-weight: 600; margin-bottom: 6px;">
                        ${d.name}
                    </div>
                    <div style="font-size: 13px; color: rgba(255,255,255,0.7);">
                        ${d.nameEn}
                    </div>
                    <div style="
                        margin-top: 8px; 
                        padding-top: 8px; 
                        border-top: 1px solid rgba(255,255,255,0.2);
                        font-size: 13px;
                    ">
                        ${d.tours} tours available
                    </div>
                </div>
            `);
    }
    
    /**
     * Setup animated arcs between cities
     */
    setupRoutes() {
        // Add arc animation data
        const arcsData = this.routes.map(route => ({
            ...route,
            arcAltitude: 0.2,
            arcStroke: 0.4,
            arcDashLength: 0.4,
            arcDashGap: 0.2,
            arcDashAnimateTime: 3000
        }));
        
        this.globe
            .arcsData(arcsData)
            .arcColor('color')
            .arcAltitude('arcAltitude')
            .arcStroke('arcStroke')
            .arcDashLength('arcDashLength')
            .arcDashGap('arcDashGap')
            .arcDashAnimateTime('arcDashAnimateTime')
            .arcsTransitionDuration(0);
    }
    
    /**
     * Setup camera and initial view
     */
    setupCamera() {
        this.globe.pointOfView({
            lat: this.vietnamCoords.lat,
            lng: this.vietnamCoords.lng,
            altitude: this.vietnamCoords.altitude
        }, 2000);
    }
    
    /**
     * Setup interactive controls
     */
    setupControls() {
        const container = document.getElementById(this.containerId);
        
        // Pause auto-rotation on interaction
        container.addEventListener('mousedown', () => {
            this.autoRotate = false;
        });
        
        // Zoom controls
        const zoomInBtn = document.getElementById('globe-zoom-in');
        const zoomOutBtn = document.getElementById('globe-zoom-out');
        const resetBtn = document.getElementById('globe-reset');
        const rotateBtn = document.getElementById('globe-rotate-toggle');
        
        if (zoomInBtn) {
            zoomInBtn.addEventListener('click', () => this.zoomIn());
        }
        
        if (zoomOutBtn) {
            zoomOutBtn.addEventListener('click', () => this.zoomOut());
        }
        
        if (resetBtn) {
            resetBtn.addEventListener('click', () => this.resetView());
        }
        
        if (rotateBtn) {
            rotateBtn.addEventListener('click', () => this.toggleRotation());
        }
    }
    
    /**
     * Start auto-rotation
     */
    startAutoRotation() {
        const animate = () => {
            if (this.autoRotate && this.globe) {
                const currentPOV = this.globe.pointOfView();
                this.globe.pointOfView({
                    lat: currentPOV.lat,
                    lng: currentPOV.lng + this.rotationSpeed,
                    altitude: currentPOV.altitude
                }, 0);
            }
            requestAnimationFrame(animate);
        };
        animate();
    }
    
    /**
     * Zoom in - Limited to prevent globe from exceeding container
     */
    zoomIn() {
        const currentPOV = this.globe.pointOfView();
        const newAltitude = Math.max(currentPOV.altitude - 0.15, 0.8); // Limit zoom to 0.8 to stay within frame
        this.globe.pointOfView({
            lat: currentPOV.lat,
            lng: currentPOV.lng,
            altitude: newAltitude
        }, 500);
    }
    
    /**
     * Zoom out
     */
    zoomOut() {
        const currentPOV = this.globe.pointOfView();
        const newAltitude = Math.min(currentPOV.altitude + 0.15, 3.0);
        this.globe.pointOfView({
            lat: currentPOV.lat,
            lng: currentPOV.lng,
            altitude: newAltitude
        }, 500);
    }
    
    /**
     * Reset view to Vietnam
     */
    resetView() {
        this.globe.pointOfView({
            lat: this.vietnamCoords.lat,
            lng: this.vietnamCoords.lng,
            altitude: this.vietnamCoords.altitude
        }, 1000);
        
        this.autoRotate = true;
        this.updateRotateButton();
    }
    
    /**
     * Focus to province level view
     */
    focusProvince() {
        const currentPOV = this.globe.pointOfView();
        this.globe.pointOfView({
            lat: currentPOV.lat,
            lng: currentPOV.lng,
            altitude: 0.5 // Very close to see provinces clearly
        }, 1000);
    }
    
    /**
     * Toggle auto-rotation
     */
    toggleRotation() {
        this.autoRotate = !this.autoRotate;
        this.updateRotateButton();
    }
    
    /**
     * Update rotate button icon
     */
    updateRotateButton() {
        const rotateBtn = document.getElementById('globe-rotate-toggle');
        if (rotateBtn) {
            const icon = rotateBtn.querySelector('i');
            if (icon) {
                icon.className = this.autoRotate ? 'fas fa-pause' : 'fas fa-play';
            }
        }
    }
    
    /**
     * Focus on specific city - Limited zoom to stay within frame
     */
    focusCity(cityName) {
        const city = this.cities.find(c => c.nameEn.toLowerCase() === cityName.toLowerCase());
        if (city) {
            this.globe.pointOfView({
                lat: city.lat,
                lng: city.lng,
                altitude: 1.0 // Closer but controlled zoom
            }, 1500);
        }
    }
    
    /**
     * Show/hide loading state
     */
    showLoading(show) {
        const loadingEl = document.querySelector('.globe-loading');
        if (loadingEl) {
            loadingEl.style.display = show ? 'block' : 'none';
        }
        this.isLoading = show;
    }
    
    /**
     * Show error message
     */
    showError(message) {
        const container = document.getElementById(this.containerId);
        if (container) {
            container.innerHTML = `
                <div style="
                    display: flex;
                    flex-direction: column;
                    align-items: center;
                    justify-content: center;
                    height: 100%;
                    color: white;
                    text-align: center;
                    padding: 40px;
                ">
                    <i class="fas fa-exclamation-triangle" style="font-size: 48px; margin-bottom: 20px; color: #ff4757;"></i>
                    <p style="font-size: 18px;">${message}</p>
                </div>
            `;
        }
    }
    
    /**
     * Handle window resize
     */
    handleResize() {
        if (this.globe) {
            const container = document.getElementById(this.containerId);
            if (container) {
                const width = container.offsetWidth;
                const height = container.offsetHeight;
                this.globe.width(width).height(height);
            }
        }
    }
    
    /**
     * Cleanup and destroy globe
     */
    destroy() {
        if (this.globe) {
            // Clean up Three.js resources
            this.autoRotate = false;
            this.globe = null;
        }
    }
}

/**
 * Text Animation - Simple fade in (character animation disabled for layout stability)
 */
function animateText() {
    const h1 = document.querySelector('.hero-globe-text h1');
    if (!h1) return;
    
    // Simply trigger the fade-in animation defined in CSS
    // No character-by-character animation to prevent layout shift
    h1.style.opacity = '0';
    setTimeout(() => {
        h1.style.animation = 'fadeInUp 0.8s ease forwards';
    }, 300);
}

/**
 * Number counting animation
 */
function animateNumbers() {
    const numbers = document.querySelectorAll('.city-stat-value');
    
    numbers.forEach((element, index) => {
        const text = element.textContent;
        const match = text.match(/(\d+)/);
        
        if (match) {
            const targetNumber = parseInt(match[1]);
            const suffix = text.replace(/\d+/, '');
            
            let currentNumber = 0;
            const duration = 2000; // 2 seconds
            const increment = targetNumber / (duration / 16); // 60fps
            const delay = 1800 + (index * 200); // Stagger animation
            
            element.textContent = '0' + suffix;
            
            setTimeout(() => {
                const timer = setInterval(() => {
                    currentNumber += increment;
                    
                    if (currentNumber >= targetNumber) {
                        element.textContent = targetNumber + suffix;
                        clearInterval(timer);
                    } else {
                        element.textContent = Math.floor(currentNumber) + suffix;
                    }
                }, 16);
            }, delay);
        }
    });
}

/**
 * Setup search bar toggle
 */
function setupSearchToggle() {
    const searchContainer = document.getElementById('globeSearchContainer');
    const searchToggleBtn = document.getElementById('searchToggleBtn');
    const searchInput = document.getElementById('globeSearchInput');
    
    if (searchToggleBtn) {
        searchToggleBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            searchContainer.classList.toggle('expanded');
            
            if (searchContainer.classList.contains('expanded')) {
                setTimeout(() => {
                    searchInput.focus();
                }, 400);
            }
        });
    }
    
    // Close search when clicking outside
    document.addEventListener('click', (e) => {
        if (searchContainer && !searchContainer.contains(e.target)) {
            // Don't close if clicking on search results dropdown
            const searchDropdown = searchContainer.querySelector('.search-results-dropdown');
            if (searchDropdown && searchDropdown.contains(e.target)) {
                return;
            }
            
            // Don't close if dropdown is visible and has results
            if (searchDropdown && searchDropdown.style.display !== 'none') {
                return;
            }
            
            searchContainer.classList.remove('expanded');
        }
    });
    
    // Prevent closing when clicking inside search
    if (searchInput) {
        searchInput.addEventListener('click', (e) => {
            e.stopPropagation();
        });
    }
}

/**
 * Setup Explore Tours navigation
 */
function setupExploreTours() {
    const exploreTourBtn = document.getElementById('exploreTourBtn');
    
    if (exploreTourBtn) {
        exploreTourBtn.addEventListener('click', () => {
            // Find featured section by ID first (most specific)
            const featuredSection = document.getElementById('featuredToursSection') || 
                                   document.querySelector('.featured-tours');
            
            if (featuredSection) {
                // Calculate offset for fixed header
                const headerHeight = 80;
                const sectionTop = featuredSection.offsetTop - headerHeight;
                
                window.scrollTo({ 
                    top: sectionTop,
                    behavior: 'smooth'
                });
            } else {
                // Fallback: scroll down by viewport height
                window.scrollBy({
                    top: window.innerHeight,
                    behavior: 'smooth'
                });
            }
        });
    }
}

// Initialize globe when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    // Check if globe container exists
    if (document.getElementById('globe-canvas')) {
        window.globeHero = new GlobeHero('globe-canvas');
        
        // Expose city focus function for buttons
        window.focusGlobeCity = (cityName) => {
            if (window.globeHero) {
                window.globeHero.focusCity(cityName);
            }
        };
        
        // Start text animations
        setTimeout(() => {
            animateText();
            animateNumbers();
        }, 500);
        
        // Handle window resize
        let resizeTimeout;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                if (window.globeHero) {
                    window.globeHero.handleResize();
                }
            }, 250);
        });
        
        // Setup search toggle and navigation
        setupSearchToggle();
        setupExploreTours();
    }
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    if (window.globeHero) {
        window.globeHero.destroy();
    }
});

