// Optimized site.js with performance improvements

// Debounce function for search
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Optimized cart operations with loading states
const CartManager = {
    isLoading: false,
    
    async updateCart(productId, count) {
        if (this.isLoading) return;
        
        this.isLoading = true;
        this.showLoadingState();
        
        try {
            const response = await fetch('/Cart/UpdateCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ ProductId: productId, Count: count })
            });
            
            if (response.ok) {
                const result = await response.text();
                this.updateCartBadge(result);
                this.refreshSmallCart();
            } else {
                this.showError('Failed to update cart');
            }
        } catch (error) {
            console.error('Cart update error:', error);
            this.showError('Network error occurred');
        } finally {
            this.isLoading = false;
            this.hideLoadingState();
        }
    },
    
    async refreshSmallCart() {
        try {
            const response = await fetch('/Cart/SmallCart');
            if (response.ok) {
                const html = await response.text();
                const cartContainer = document.getElementById('small-cart-container');
                if (cartContainer) {
                    cartContainer.innerHTML = html;
                }
            }
        } catch (error) {
            console.error('Small cart refresh error:', error);
        }
    },
    
    updateCartBadge(count) {
        const badge = document.querySelector('.cart-badge');
        if (badge) {
            badge.textContent = count;
            badge.style.display = count > 0 ? 'inline' : 'none';
        }
    },
    
    showLoadingState() {
        const buttons = document.querySelectorAll('.cart-btn');
        buttons.forEach(btn => {
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> Loading...';
        });
    },
    
    hideLoadingState() {
        const buttons = document.querySelectorAll('.cart-btn');
        buttons.forEach(btn => {
            btn.disabled = false;
            btn.innerHTML = btn.dataset.originalText || 'Add to Cart';
        });
    },
    
    showError(message) {
        // Create or update error message
        let errorDiv = document.getElementById('cart-error');
        if (!errorDiv) {
            errorDiv = document.createElement('div');
            errorDiv.id = 'cart-error';
            errorDiv.className = 'alert alert-danger alert-dismissible fade show';
            errorDiv.innerHTML = `
                <span class="error-message">${message}</span>
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;
            document.body.prepend(errorDiv);
        } else {
            errorDiv.querySelector('.error-message').textContent = message;
            errorDiv.style.display = 'block';
        }
        
        // Auto-hide after 5 seconds
        setTimeout(() => {
            if (errorDiv) {
                errorDiv.style.display = 'none';
            }
        }, 5000);
    }
};

// Optimized search with debouncing
const SearchManager = {
    init() {
        const searchInput = document.getElementById('search-input');
        if (searchInput) {
            const debouncedSearch = debounce(this.performSearch.bind(this), 300);
            searchInput.addEventListener('input', debouncedSearch);
        }
    },
    
    async performSearch(event) {
        const searchTerm = event.target.value.trim();
        if (searchTerm.length < 2) return;
        
        // Show loading state
        this.showSearchLoading();
        
        try {
            const response = await fetch(`/Product/SearchProducts?SearchText=${encodeURIComponent(searchTerm)}`, {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            if (response.ok) {
                const html = await response.text();
                this.updateSearchResults(html);
            }
        } catch (error) {
            console.error('Search error:', error);
        } finally {
            this.hideSearchLoading();
        }
    },
    
    showSearchLoading() {
        const searchContainer = document.getElementById('search-results');
        if (searchContainer) {
            searchContainer.innerHTML = '<div class="text-center p-3"><div class="spinner-border" role="status"></div></div>';
        }
    },
    
    hideSearchLoading() {
        // Loading will be replaced by actual results
    },
    
    updateSearchResults(html) {
        const searchContainer = document.getElementById('search-results');
        if (searchContainer) {
            searchContainer.innerHTML = html;
        }
    }
};

// Image lazy loading optimization
const ImageLoader = {
    init() {
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        img.src = img.dataset.src;
                        img.classList.remove('lazy');
                        observer.unobserve(img);
                    }
                });
            });
            
            document.querySelectorAll('img[data-src]').forEach(img => {
                imageObserver.observe(img);
            });
        } else {
            // Fallback for older browsers
            document.querySelectorAll('img[data-src]').forEach(img => {
                img.src = img.dataset.src;
            });
        }
    }
};

// Performance monitoring
const PerformanceMonitor = {
    init() {
        if ('performance' in window) {
            window.addEventListener('load', () => {
                setTimeout(() => {
                    const loadTime = performance.timing.loadEventEnd - performance.timing.navigationStart;
                    console.log('Page load time:', loadTime + 'ms');
                    
                    // Send to analytics if available
                    if (typeof gtag !== 'undefined') {
                        gtag('event', 'timing_complete', {
                            name: 'load',
                            value: loadTime
                        });
                    }
                }, 0);
            });
        }
    }
};

// Initialize everything when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    SearchManager.init();
    ImageLoader.init();
    PerformanceMonitor.init();
    
    // Store original button text for loading states
    document.querySelectorAll('.cart-btn').forEach(btn => {
        btn.dataset.originalText = btn.innerHTML;
    });
});

// Expose CartManager globally for onclick handlers
window.CartManager = CartManager;