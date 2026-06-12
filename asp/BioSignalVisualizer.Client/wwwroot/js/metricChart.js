(() => {
    const charts = {};

    const annotationPlugin = {
        id: "annotationPlugin",
        beforeDatasetsDraw(chart) {
            const pluginOptions = chart.options.plugins.annotationPlugin ?? {};
            const regions = pluginOptions.regions ?? [];
            const markers = pluginOptions.markers ?? [];
            if (!markers.length && !regions.length) {
                return;
            }

            const ctx = chart.ctx;
            const xScale = chart.scales.x;
            const { top, bottom, right } = chart.chartArea;
            const height = bottom - top;

            regions.forEach(region => {
                const start = xScale.getPixelForValue(region.startIndex);
                const end = xScale.getPixelForValue(region.endIndex);
                if (!Number.isFinite(start) || !Number.isFinite(end)) {
                    return;
                }

                const left = Math.min(start, end);
                const width = Math.abs(end - start);
                ctx.save();
                ctx.fillStyle = toRgba(region.color || "#cfd0d1", 0.7);
                ctx.fillRect(left, top, width, height);
                ctx.restore();
            });

            markers.forEach(marker => {
                const x = xScale.getPixelForValue(marker.index);
                if (!Number.isFinite(x)) {
                    return;
                }

                ctx.save();
                ctx.strokeStyle = "rgba(244,67,54,0.95)";
                ctx.lineWidth = 1.5;
                ctx.beginPath();
                ctx.moveTo(x, top);
                ctx.lineTo(x, bottom);
                ctx.stroke();

                const label = marker.label ?? "";
                if (!label) {
                    ctx.restore();
                    return;
                }

                ctx.fillStyle = "rgba(33,33,33,0.95)";
                ctx.font = "10px 'Roboto', sans-serif";
                const padding = 4;
                const textWidth = ctx.measureText(label).width;
                const boxWidth = textWidth + padding * 2;
                const boxHeight = 14;
                const boxX = x - boxWidth / 2;
                const boxY = top + 6; // place inside chart so it's always visible

                ctx.fillRect(boxX, boxY, boxWidth, boxHeight);
                ctx.fillStyle = "#ffffff";
                ctx.fillText(label, boxX + padding, boxY + boxHeight - 4);
                ctx.restore();
            });

        }
    };

    if (window.Chart && !Chart.registry.plugins.get("annotationPlugin")) {
        Chart.register(annotationPlugin);
    }

    window.metricChart = {
        render(payload) {
            const canvas = document.getElementById(payload.canvasId);
            if (!canvas || !window.Chart) {
                return;
            }

            const ctx = canvas.getContext("2d");
            const data = {
                labels: payload.labels,
                datasets: [
                    {
                        label: payload.title,
                        data: payload.values,
                        fill: false,
                        borderColor: "#4f46e5",
                        borderWidth: 1.5,
                        tension: 0.2,
                        pointRadius: 0
                    }
                ]
            };

            const options = {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    annotationPlugin: {
                        markers: payload.annotations ?? [],
                        regions: payload.regions ?? []
                    }
                },
                interaction: { intersect: false, mode: "index" },
                scales: {
                    x: {
                        ticks: {
                            autoSkip: true,
                            maxTicksLimit: Math.min(payload.labels.length, 48),
                            maxRotation: 0,
                            minRotation: 0
                        },
                        grid: { color: "rgba(255,255,255,0.08)" }
                    },
                    y: {
                        beginAtZero: false,
                        grid: { color: "rgba(0,0,0,0.08)" }
                    }
                }
            };

            const existing = charts[payload.canvasId];
            if (existing) {
                existing.data = data;
                existing.options.plugins.annotationPlugin.markers = payload.annotations ?? [];
                existing.options.plugins.annotationPlugin.regions = payload.regions ?? [];
                existing.update();
                return;
            }

            const attached = typeof Chart.getChart === "function" ? Chart.getChart(canvas) : null;
            if (attached) {
                attached.destroy();
            }

            charts[payload.canvasId] = new Chart(ctx, {
                type: "line",
                data,
                options
            });
        },
        destroy(canvasId) {
            const existing = charts[canvasId];
            if (existing) {
                existing.destroy();
            } else if (typeof Chart.getChart === "function") {
                const canvas = document.getElementById(canvasId);
                if (canvas) {
                    const attached = Chart.getChart(canvas);
                    if (attached) {
                        attached.destroy();
                    }
                }
            }
            delete charts[canvasId];
        },
        calculateFitHeight(chartCount) {
            const count = Math.max(1, chartCount || 1);
            const viewportHeight = window.innerHeight || document.documentElement.clientHeight || 900;
            const appBar = document.querySelector(".mud-app-bar");
            const headerHeight = appBar ? appBar.offsetHeight : 64;
            const reserved = headerHeight + 170; // space for sidebar + padding
            const available = Math.max(260, viewportHeight - reserved);
            const spacing = 24;
            const perChart = (available - spacing * (count - 1)) / count;
            const adjusted = perChart * 0.95;
            return Math.max(140, Math.min(280, adjusted));
        }
    };

    function toRgba(hex, alpha) {
        if (!hex) {
            return `rgba(0,0,0,${alpha})`;
        }

        const normalized = hex.replace("#", "");
        if (normalized.length !== 6) {
            return `rgba(0,0,0,${alpha})`;
        }

        const r = parseInt(normalized.substring(0, 2), 16);
        const g = parseInt(normalized.substring(2, 4), 16);
        const b = parseInt(normalized.substring(4, 6), 16);
        return `rgba(${r}, ${g}, ${b}, ${alpha})`;
    }
})();
