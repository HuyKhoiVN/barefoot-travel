/**
 * Currency Converter Module
 * Handles VND <-> USD conversion and formatting for user-facing pages
 */

const CurrencyConverter = (function() {
    // Exchange rate: 1 USD = 25,000 VND (you can update this or fetch from API)
    const EXCHANGE_RATE = 25000;
    const STORAGE_KEY = 'preferred_currency';
    const STORAGE_TIMESTAMP_KEY = 'preferred_currency_timestamp';
    const DEFAULT_CURRENCY = 'VND';
    const EXPIRE_TIME = 24 * 60 * 60 * 1000; // 1 day in milliseconds
    
    // Private variables
    let currentCurrency = DEFAULT_CURRENCY;
    let listeners = [];
    
    /**
     * Check if saved preference is expired
     */
    function isExpired() {
        const timestamp = localStorage.getItem(STORAGE_TIMESTAMP_KEY);
        if (!timestamp) return true;
        
        const now = new Date().getTime();
        const saved = parseInt(timestamp, 10);
        return (now - saved) > EXPIRE_TIME;
    }
    
    /**
     * Initialize the converter and load saved preference
     */
    function init() {
        // Check if saved preference is expired
        if (isExpired()) {
            // Clear expired data and use default
            localStorage.removeItem(STORAGE_KEY);
            localStorage.removeItem(STORAGE_TIMESTAMP_KEY);
            currentCurrency = DEFAULT_CURRENCY;
        } else {
            // Load saved currency preference from localStorage
            const saved = localStorage.getItem(STORAGE_KEY);
            if (saved && (saved === 'VND' || saved === 'USD')) {
                currentCurrency = saved;
            }
        }
    }
    
    /**
     * Get current currency
     */
    function getCurrency() {
        return currentCurrency;
    }
    
    /**
     * Set currency and save to localStorage with timestamp
     * @param {string} currency - 'VND' or 'USD'
     */
    function setCurrency(currency) {
        if (currency !== 'VND' && currency !== 'USD') {
            return;
        }
        
        currentCurrency = currency;
        const timestamp = new Date().getTime();
        localStorage.setItem(STORAGE_KEY, currency);
        localStorage.setItem(STORAGE_TIMESTAMP_KEY, timestamp.toString());
        
        // Notify all listeners
        listeners.forEach(callback => callback(currency));
    }
    
    /**
     * Add listener for currency changes
     * @param {function} callback - Function to call when currency changes
     */
    function onChange(callback) {
        if (typeof callback === 'function') {
            listeners.push(callback);
        }
    }
    
    /**
     * Convert price from VND to USD
     * @param {number} priceVND - Price in VND
     * @returns {number} Price in USD
     */
    function vndToUsd(priceVND) {
        return priceVND / EXCHANGE_RATE;
    }
    
    /**
     * Convert price from USD to VND
     * @param {number} priceUSD - Price in USD
     * @returns {number} Price in VND
     */
    function usdToVnd(priceUSD) {
        return priceUSD * EXCHANGE_RATE;
    }
    
    /**
     * Format price based on current currency
     * @param {number} price - Price in VND (base currency)
     * @param {boolean} includeSymbol - Whether to include currency symbol
     * @returns {string} Formatted price string
     */
    function formatPrice(price, includeSymbol = true) {
        if (!price || isNaN(price)) {
            return includeSymbol ? (currentCurrency === 'VND' ? '0 ₫' : '$0.00') : '0';
        }
        
        if (currentCurrency === 'USD') {
            // Convert to USD and format with 2 decimal places
            const usdPrice = vndToUsd(price);
            const formatted = usdPrice.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            return includeSymbol ? `$${formatted}` : formatted;
        } else {
            // Format VND with no decimal places
            const formatted = Math.round(price).toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            return includeSymbol ? `${formatted} ₫` : formatted;
        }
    }
    
    /**
     * Get currency symbol
     * @returns {string} Currency symbol
     */
    function getSymbol() {
        return currentCurrency === 'VND' ? '₫' : '$';
    }
    
    /**
     * Get currency display name
     * @returns {string} Currency display name
     */
    function getCurrencyName() {
        return currentCurrency === 'VND' ? 'Vietnamese Dong' : 'US Dollar';
    }
    
    // Initialize on load
    init();
    
    // Public API
    return {
        init,
        getCurrency,
        setCurrency,
        onChange,
        formatPrice,
        getSymbol,
        getCurrencyName,
        vndToUsd,
        usdToVnd
    };
})();

// Make it available globally
window.CurrencyConverter = CurrencyConverter;

