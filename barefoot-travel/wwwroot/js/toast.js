/**
 * Toast Notification System
 * Provides a modern toast notification system with different types and animations
 */

// Toast function
function showToast(message, type = 'default', options = {}) {
    // Create container if it doesn't exist
    let container = document.querySelector('.toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container';
        document.body.appendChild(container);
    }
    
    // Create toast element
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.style.cssText = `
        position: relative;
        display: flex;
        align-items: center;
        padding: 12px 16px;
        margin-bottom: 8px;
        background: ${getBackgroundColor(type)};
        border: 1px solid ${getBorderColor(type)};
        border-radius: 16px;
        box-shadow: 0px 16px 20px -8px rgba(3, 5, 18, 0.1);
        width: 350px;
        min-height: 56px;
        z-index: 100000;
        animation: slideInRight 0.3s ease-out;
    `;
    
    // Add content
    toast.innerHTML = `
        <div style="margin-right: 12px; font-size: 18px; color: ${getIconColor(type)};">${getIcon(type)}</div>
        <div style="flex: 1; font-size: 14px; font-weight: 500;">${message}</div>
        <button onclick="this.parentElement.remove()" style="background: none; border: none; font-size: 18px; cursor: pointer; margin-left: 8px;">&times;</button>
    `;
    
    // Add to container
    container.appendChild(toast);
    
    // Auto-hide after 5 seconds
    setTimeout(() => {
        if (toast.parentNode) {
            toast.style.animation = 'slideOutRight 0.3s ease-in forwards';
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }
    }, 5000);
    
    return toast;
}

function getBackgroundColor(type) {
    const colors = {
        default: '#FFFFFF',
        info: '#EDF2FD',
        success: '#E5FCF1',
        warning: '#FFFAE8',
        danger: '#FEF2F2'
    };
    return colors[type] || colors.default;
}

function getBorderColor(type) {
    const colors = {
        default: '#FBFBFB',
        info: '#4B85F5',
        success: '#01E17B',
        warning: '#FDCD0F',
        danger: '#EF4444'
    };
    return colors[type] || colors.default;
}

function getIconColor(type) {
    const colors = {
        default: '#6B7280',
        info: '#4B85F5',
        success: '#01E17B',
        warning: '#FDCD0F',
        danger: '#EF4444'
    };
    return colors[type] || colors.default;
}

function getIcon(type) {
    const icons = {
        default: '<i class="fas fa-info-circle"></i>',
        info: '<i class="fas fa-info-circle"></i>',
        success: '<i class="fas fa-check-circle"></i>',
        warning: '<i class="fas fa-exclamation-triangle"></i>',
        danger: '<i class="fas fa-times-circle"></i>'
    };
    return icons[type] || icons.default;
}

// Legacy compatibility
function showAlert(message, type = 'default', options = {}) {
    return showToast(message, type, options);
}

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);
