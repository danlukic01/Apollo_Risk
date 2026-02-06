// wwwroot/js/chatCharts.js
// Chart.js interop for ChatWidget visualizations

window.ChatCharts = {
    charts: {},

    // Get current theme colors (returns light mode colors if theme system not initialized)
    getThemeColors: function () {
        const isDark = document.documentElement && document.documentElement.classList.contains('theme-dark');
        return {
            textColor: isDark ? '#F5F5F5' : '#1e293b',
            textMuted: isDark ? '#A3A3A3' : '#64748b',
            gridColor: isDark ? 'rgba(61, 61, 61, 0.5)' : 'rgba(148, 163, 184, 0.4)',
            tooltipBg: isDark ? 'rgba(45, 45, 45, 0.95)' : 'rgba(30, 41, 59, 0.9)',
            tooltipText: isDark ? '#F5F5F5' : '#fff',
            tooltipBody: isDark ? '#A3A3A3' : '#e2e8f0',
            borderColor: isDark ? '#3D3D3D' : '#cbd5e1',
            gaugeBg: isDark ? 'rgba(61, 61, 61, 0.5)' : 'rgba(226, 232, 240, 0.5)'
        };
    },

    // Initialize a bar chart
    createBarChart: function (canvasId, data) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        // Destroy existing chart if any
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }

        const themeColors = this.getThemeColors();

        this.charts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: data.labels,
                datasets: [{
                    label: data.title || 'Value',
                    data: data.values,
                    backgroundColor: data.colors || [
                        'rgba(245, 158, 11, 0.8)',  // Amber (primary)
                        'rgba(16, 185, 129, 0.8)',  // Green
                        'rgba(239, 68, 68, 0.8)',   // Red
                        'rgba(59, 130, 246, 0.8)',  // Blue
                        'rgba(168, 85, 247, 0.8)', // Purple
                        'rgba(99, 102, 241, 0.8)'  // Indigo
                    ],
                    borderColor: data.borderColors || [
                        'rgb(245, 158, 11)',
                        'rgb(16, 185, 129)',
                        'rgb(239, 68, 68)',
                        'rgb(59, 130, 246)',
                        'rgb(168, 85, 247)',
                        'rgb(99, 102, 241)'
                    ],
                    borderWidth: 1,
                    borderRadius: 6,
                    barThickness: data.barThickness || 'flex',
                    maxBarThickness: 50
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: data.showLegend || false
                    },
                    tooltip: {
                        backgroundColor: themeColors.tooltipBg,
                        titleColor: themeColors.tooltipText,
                        bodyColor: themeColors.tooltipBody,
                        padding: 12,
                        cornerRadius: 8,
                        displayColors: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: themeColors.gridColor
                        },
                        ticks: {
                            color: themeColors.textMuted,
                            font: { size: 11 }
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: themeColors.textMuted,
                            font: { size: 11 },
                            maxRotation: 45,
                            minRotation: 0
                        }
                    }
                }
            }
        });
    },

    // Initialize a horizontal bar chart
    createHorizontalBarChart: function (canvasId, data) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }

        const themeColors = this.getThemeColors();

        this.charts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: data.labels,
                datasets: [{
                    label: data.title || 'Value',
                    data: data.values,
                    backgroundColor: this.generateGradientColors(data.values.length, data.colorScheme || 'amber'),
                    borderRadius: 4,
                    barThickness: 20
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: themeColors.tooltipBg,
                        titleColor: themeColors.tooltipText,
                        bodyColor: themeColors.tooltipBody,
                        padding: 12,
                        cornerRadius: 8
                    }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        grid: { color: themeColors.gridColor },
                        ticks: { color: themeColors.textMuted, font: { size: 11 } }
                    },
                    y: {
                        grid: { display: false },
                        ticks: { color: themeColors.textColor, font: { size: 12, weight: '500' } }
                    }
                }
            }
        });
    },

    // Initialize a line chart
    createLineChart: function (canvasId, data) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }

        const themeColors = this.getThemeColors();

        const datasets = data.series ? data.series.map((s, i) => ({
            label: s.name,
            data: s.values,
            borderColor: this.getColor(i),
            backgroundColor: this.getColor(i, 0.1),
            fill: data.fill || false,
            tension: 0.3,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: themeColors.borderColor,
            pointBorderWidth: 2
        })) : [{
            label: data.title || 'Value',
            data: data.values,
            borderColor: 'rgb(245, 158, 11)',
            backgroundColor: 'rgba(245, 158, 11, 0.1)',
            fill: data.fill || false,
            tension: 0.3,
            pointRadius: 4,
            pointHoverRadius: 6,
            pointBackgroundColor: themeColors.borderColor,
            pointBorderWidth: 2
        }];

        this.charts[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: data.labels,
                datasets: datasets
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    intersect: false,
                    mode: 'index'
                },
                plugins: {
                    legend: {
                        display: data.series && data.series.length > 1,
                        labels: { color: themeColors.textColor }
                    },
                    tooltip: {
                        backgroundColor: themeColors.tooltipBg,
                        titleColor: themeColors.tooltipText,
                        bodyColor: themeColors.tooltipBody,
                        padding: 12,
                        cornerRadius: 8
                    }
                },
                scales: {
                    y: {
                        beginAtZero: data.beginAtZero !== false,
                        grid: { color: themeColors.gridColor },
                        ticks: { color: themeColors.textMuted, font: { size: 11 } }
                    },
                    x: {
                        grid: { display: false },
                        ticks: { color: themeColors.textMuted, font: { size: 11 } }
                    }
                }
            }
        });
    },

    // Initialize a pie/doughnut chart
    createPieChart: function (canvasId, data) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }

        const themeColors = this.getThemeColors();

        // RAG colors for risk data
        const ragColors = [
            'rgba(239, 68, 68, 0.85)',   // Red
            'rgba(245, 158, 11, 0.85)',  // Amber
            'rgba(16, 185, 129, 0.85)'   // Green
        ];

        this.charts[canvasId] = new Chart(ctx, {
            type: data.doughnut ? 'doughnut' : 'pie',
            data: {
                labels: data.labels,
                datasets: [{
                    data: data.values,
                    backgroundColor: data.colors || ragColors,
                    borderColor: themeColors.borderColor,
                    borderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            color: themeColors.textColor,
                            padding: 15,
                            usePointStyle: true
                        }
                    },
                    tooltip: {
                        backgroundColor: themeColors.tooltipBg,
                        titleColor: themeColors.tooltipText,
                        bodyColor: themeColors.tooltipBody,
                        padding: 12,
                        cornerRadius: 8,
                        callbacks: {
                            label: function (context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((context.parsed / total) * 100).toFixed(1);
                                return `${context.label}: ${context.parsed} (${percentage}%)`;
                            }
                        }
                    }
                },
                cutout: data.doughnut ? '60%' : 0
            }
        });
    },

    // Create a gauge/progress chart
    createGaugeChart: function (canvasId, data) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
        }

        const themeColors = this.getThemeColors();
        const value = data.value;
        const max = data.max || 100;
        const remaining = max - value;

        let color = 'rgba(16, 185, 129, 0.9)'; // Green
        if (data.thresholds) {
            if (value < data.thresholds.critical) color = 'rgba(239, 68, 68, 0.9)'; // Red
            else if (value < data.thresholds.warning) color = 'rgba(245, 158, 11, 0.9)'; // Amber
        }

        this.charts[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: [value, remaining],
                    backgroundColor: [color, themeColors.gaugeBg],
                    borderWidth: 0,
                    circumference: 180,
                    rotation: 270
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '75%',
                plugins: {
                    legend: { display: false },
                    tooltip: { enabled: false }
                }
            },
            plugins: [{
                id: 'gaugeText',
                afterDraw: (chart) => {
                    const { ctx, width, height } = chart;
                    // Get fresh theme colors for dynamic text rendering
                    const colors = window.ChatCharts.getThemeColors();
                    ctx.save();
                    ctx.font = 'bold 24px system-ui';
                    ctx.fillStyle = colors.textColor;
                    ctx.textAlign = 'center';
                    ctx.fillText(`${value}${data.suffix || '%'}`, width / 2, height - 20);
                    if (data.label) {
                        ctx.font = '12px system-ui';
                        ctx.fillStyle = colors.textMuted;
                        ctx.fillText(data.label, width / 2, height);
                    }
                    ctx.restore();
                }
            }]
        });
    },

    // Update all charts for theme change (called when theme switches)
    updateAllChartsTheme: function () {
        const themeColors = this.getThemeColors();

        Object.keys(this.charts).forEach(canvasId => {
            const chart = this.charts[canvasId];
            if (!chart) return;

            try {
                // Update scales if they exist
                if (chart.options.scales) {
                    if (chart.options.scales.x) {
                        if (chart.options.scales.x.ticks) {
                            chart.options.scales.x.ticks.color = themeColors.textMuted;
                        }
                        if (chart.options.scales.x.grid) {
                            chart.options.scales.x.grid.color = themeColors.gridColor;
                        }
                    }
                    if (chart.options.scales.y) {
                        if (chart.options.scales.y.ticks) {
                            chart.options.scales.y.ticks.color = themeColors.textMuted;
                        }
                        if (chart.options.scales.y.grid) {
                            chart.options.scales.y.grid.color = themeColors.gridColor;
                        }
                    }
                }

                // Update legend if it exists
                if (chart.options.plugins && chart.options.plugins.legend && chart.options.plugins.legend.labels) {
                    chart.options.plugins.legend.labels.color = themeColors.textColor;
                }

                // Update tooltip if it exists
                if (chart.options.plugins && chart.options.plugins.tooltip) {
                    chart.options.plugins.tooltip.backgroundColor = themeColors.tooltipBg;
                    chart.options.plugins.tooltip.titleColor = themeColors.tooltipText;
                    chart.options.plugins.tooltip.bodyColor = themeColors.tooltipBody;
                }

                // Update border colors for pie/doughnut charts
                if (chart.config.type === 'pie' || chart.config.type === 'doughnut') {
                    chart.data.datasets.forEach(dataset => {
                        // Only update if it's not the gauge background color
                        if (dataset.borderColor && dataset.borderWidth > 0) {
                            dataset.borderColor = themeColors.borderColor;
                        }
                        // Update gauge background if applicable
                        if (dataset.backgroundColor && Array.isArray(dataset.backgroundColor) && dataset.backgroundColor.length === 2) {
                            // This is likely a gauge chart - update the background color
                            dataset.backgroundColor[1] = themeColors.gaugeBg;
                        }
                    });
                }

                chart.update('none'); // 'none' for no animation during theme switch
            } catch (e) {
                console.warn('Error updating chart theme for:', canvasId, e);
            }
        });
    },

    // Helper: Generate gradient colors
    generateGradientColors: function (count, scheme) {
        const schemes = {
            amber: ['rgba(245, 158, 11, 0.9)', 'rgba(217, 119, 6, 0.9)', 'rgba(180, 83, 9, 0.9)'],
            green: ['rgba(16, 185, 129, 0.9)', 'rgba(5, 150, 105, 0.9)', 'rgba(4, 120, 87, 0.9)'],
            blue: ['rgba(59, 130, 246, 0.9)', 'rgba(37, 99, 235, 0.9)', 'rgba(29, 78, 216, 0.9)'],
            multi: [
                'rgba(245, 158, 11, 0.85)',
                'rgba(16, 185, 129, 0.85)',
                'rgba(239, 68, 68, 0.85)',
                'rgba(59, 130, 246, 0.85)',
                'rgba(168, 85, 247, 0.85)',
                'rgba(99, 102, 241, 0.85)'
            ]
        };
        const colors = schemes[scheme] || schemes.multi;
        const result = [];
        for (let i = 0; i < count; i++) {
            result.push(colors[i % colors.length]);
        }
        return result;
    },

    // Helper: Get color by index
    getColor: function (index, alpha = 1) {
        const colors = [
            `rgba(245, 158, 11, ${alpha})`,
            `rgba(16, 185, 129, ${alpha})`,
            `rgba(239, 68, 68, ${alpha})`,
            `rgba(59, 130, 246, ${alpha})`,
            `rgba(168, 85, 247, ${alpha})`,
            `rgba(99, 102, 241, ${alpha})`
        ];
        return colors[index % colors.length];
    },

    // Destroy a chart
    destroyChart: function (canvasId) {
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
            delete this.charts[canvasId];
        }
    },

    // Destroy all charts
    destroyAllCharts: function () {
        Object.keys(this.charts).forEach(id => {
            this.charts[id].destroy();
        });
        this.charts = {};
    }
};

// Listen for theme changes and update charts automatically
window.addEventListener('themeChanged', function (e) {
    if (window.ChatCharts) {
        window.ChatCharts.updateAllChartsTheme();
    }
});
