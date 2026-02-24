// Apollo Risk - Site JavaScript

// Theme management
window.ThemeManager = {
    isDark: false,

    init: function () {
        // Check for saved preference
        const saved = localStorage.getItem('theme-dark');
        if (saved !== null) {
            this.isDark = saved === 'true';
        } else {
            // Check system preference
            this.isDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        }
        this.apply();
    },

    toggle: function () {
        this.isDark = !this.isDark;
        localStorage.setItem('theme-dark', this.isDark);
        this.apply();
        return this.isDark;
    },

    apply: function () {
        if (this.isDark) {
            document.documentElement.classList.add('theme-dark');
        } else {
            document.documentElement.classList.remove('theme-dark');
        }
    },

    get: function () {
        return this.isDark;
    }
};

// Initialize theme on page load
document.addEventListener('DOMContentLoaded', function () {
    window.ThemeManager.init();
});

// Chart helpers
window.ChartHelpers = {
    drawRiskTrendChart: function (elementId, labels, data) {
        const ctx = document.getElementById(elementId);
        if (!ctx) return;

        new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Average Risk Score',
                    data: data,
                    borderColor: '#F59E0B',
                    backgroundColor: 'rgba(245, 158, 11, 0.1)',
                    borderWidth: 2,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: '#F59E0B',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    pointRadius: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 10,
                        grid: {
                            color: 'rgba(0, 0, 0, 0.05)'
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    },

    drawStackedBarChart: function (elementId, labels, datasets) {
        const ctx = document.getElementById(elementId);
        if (!ctx) return;

        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: datasets.map((ds, i) => ({
                    label: ds.label,
                    data: ds.data,
                    backgroundColor: ['#ef4444', '#f59e0b', '#10b981'][i] || '#64748b',
                    borderRadius: 4
                }))
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                },
                scales: {
                    x: {
                        stacked: true,
                        grid: {
                            display: false
                        }
                    },
                    y: {
                        stacked: true,
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(0, 0, 0, 0.05)'
                        }
                    }
                }
            }
        });
    }
};

// Export functionality
window.ExportHelpers = {
    downloadFile: function (filename, content, contentType) {
        const blob = new Blob([content], { type: contentType });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },

    print: function () {
        window.print();
    }
};

// Scroll helpers
window.ScrollHelpers = {
    scrollToBottom: function (selector) {
        const element = document.querySelector(selector);
        if (element) {
            element.scrollTo({
                top: element.scrollHeight,
                behavior: 'smooth'
            });
        }
    },

    scrollToTop: function (selector) {
        const element = selector ? document.querySelector(selector) : window;
        if (element) {
            element.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        }
    }
};

// Notification helpers
window.NotificationHelpers = {
    show: function (message, type) {
        // Simple notification - can be enhanced with a proper notification library
        const notification = document.createElement('div');
        notification.className = `notification notification-${type || 'info'}`;
        notification.textContent = message;
        notification.style.cssText = `
            position: fixed;
            top: 1rem;
            right: 1rem;
            padding: 1rem 1.5rem;
            border-radius: 8px;
            background: ${type === 'error' ? '#ef4444' : type === 'success' ? '#10b981' : '#F59E0B'};
            color: white;
            font-weight: 500;
            z-index: 10000;
            animation: slideIn 0.3s ease;
        `;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.animation = 'slideOut 0.3s ease';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    }
};

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from { opacity: 0; transform: translateX(100px); }
        to { opacity: 1; transform: translateX(0); }
    }
    @keyframes slideOut {
        from { opacity: 1; transform: translateX(0); }
        to { opacity: 0; transform: translateX(100px); }
    }
`;
document.head.appendChild(style);
