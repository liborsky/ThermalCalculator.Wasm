// Graf pro optimalizaci tloušťky izolace
let optimizationChartInstance = null;

/**
 * Vykreslí graf ekonomické optimalizace izolace
 * @param {Object} chartData - Data pro Chart.js (labels, datasets)
 * @param {number} optimalThickness - Optimální tloušťka pro označení na grafu
 */
window.renderOptimizationChart = function(chartData, optimalThickness) {
    const canvas = document.getElementById('optimizationChart');
    if (!canvas) {
        console.error('Canvas element not found');
        return;
    }

    const ctx = canvas.getContext('2d');

    // Zničit existující graf
    if (optimizationChartInstance) {
        optimizationChartInstance.destroy();
    }

    // Plugin pro vertikální čáru označující optimum
    const verticalLinePlugin = {
        id: 'verticalLine',
        afterDraw: (chart) => {
            if (chart.tooltip?._active?.length) {
                return; // Nekreslíme při hover
            }

            const ctx = chart.ctx;
            const xAxis = chart.scales.x;
            const yAxis = chart.scales.y;

            // Najít X pozici pro optimální tloušťku
            const optimalIndex = chartData.labels.findIndex(label =>
                Math.abs(label - optimalThickness) < 0.1
            );

            if (optimalIndex === -1) return;

            const x = xAxis.getPixelForValue(chartData.labels[optimalIndex]);

            // Nakreslit vertikální čáru
            ctx.save();
            ctx.beginPath();
            ctx.moveTo(x, yAxis.top);
            ctx.lineTo(x, yAxis.bottom);
            ctx.lineWidth = 2;
            ctx.strokeStyle = 'rgba(54, 162, 235, 0.5)';
            ctx.setLineDash([5, 5]);
            ctx.stroke();
            ctx.restore();

            // Přidat label
            ctx.save();
            ctx.font = 'bold 12px Arial';
            ctx.fillStyle = 'rgba(54, 162, 235, 0.8)';
            ctx.textAlign = 'center';
            ctx.fillText(
                `Optimum: ${optimalThickness.toFixed(1)} cm`,
                x,
                yAxis.top - 10
            );
            ctx.restore();
        }
    };

    // Vytvoření grafu
    optimizationChartInstance = new Chart(ctx, {
        type: 'line',
        data: chartData,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false,
            },
            plugins: {
                legend: {
                    position: 'top',
                    labels: {
                        usePointStyle: true,
                        padding: 15,
                        font: {
                            size: 12
                        }
                    }
                },
                tooltip: {
                    callbacks: {
                        title: function(context) {
                            return 'Tloušťka: ' + context[0].label + ' cm';
                        },
                        label: function(context) {
                            let label = context.dataset.label || '';
                            if (label) {
                                label += ': ';
                            }
                            label += Math.round(context.parsed.y) + ' Kč';
                            return label;
                        }
                    },
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    padding: 12,
                    titleFont: {
                        size: 14
                    },
                    bodyFont: {
                        size: 13
                    }
                }
            },
            scales: {
                x: {
                    type: 'linear',
                    title: {
                        display: true,
                        text: 'Tloušťka izolace [cm]',
                        font: {
                            size: 14,
                            weight: 'bold'
                        }
                    },
                    ticks: {
                        stepSize: 5,
                        callback: function(value) {
                            return value + ' cm';
                        }
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    }
                },
                y: {
                    type: 'linear',
                    title: {
                        display: true,
                        text: 'Náklady / Úspora [Kč/m²]',
                        font: {
                            size: 14,
                            weight: 'bold'
                        }
                    },
                    ticks: {
                        callback: function(value) {
                            return value.toLocaleString('cs-CZ') + ' Kč';
                        }
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                }
            }
        },
        plugins: [verticalLinePlugin]
    });
};

// Cleanup při odstranění stránky
window.destroyOptimizationChart = function() {
    if (optimizationChartInstance) {
        optimizationChartInstance.destroy();
        optimizationChartInstance = null;
    }
};
