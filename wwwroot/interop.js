// GitHub Connector Chart.js interop
window.GitHubConnectorCharts = {
    _loaded: false,
    _loading: false,
    _queue: [],

    load: function () {
        if (this._loaded || this._loading) return;
        this._loading = true;
        var s = document.createElement('script');
        s.src = 'https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js';
        s.onload = function () {
            window.GitHubConnectorCharts._loaded = true;
            window.GitHubConnectorCharts._loading = false;
            window.GitHubConnectorCharts._queue.forEach(function (f) { f(); });
            window.GitHubConnectorCharts._queue = [];
        };
        document.head.appendChild(s);
    },

    createBarChart: function (canvasId, labels, data, label, color) {
        var self = this;
        var draw = function () {
            var ctx = document.getElementById(canvasId);
            if (!ctx) return;
            if (self._charts) self._charts[canvasId] && self._charts[canvasId].destroy();
            self._charts = self._charts || {};
            self._charts[canvasId] = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: labels,
                    datasets: [{
                        label: label || '',
                        data: data,
                        backgroundColor: color || 'rgba(13,110,253,0.7)',
                        borderRadius: 3
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: {
                        y: { beginAtZero: true, ticks: { precision: 0 } },
                        x: { grid: { display: false } }
                    }
                }
            });
        };
        if (this._loaded) draw();
        else this._queue.push(draw);
    },

    destroyChart: function (canvasId) {
        if (this._charts && this._charts[canvasId]) {
            this._charts[canvasId].destroy();
            delete this._charts[canvasId];
        }
    }
};
