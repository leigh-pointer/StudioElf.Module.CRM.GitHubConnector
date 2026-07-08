using Microsoft.JSInterop;

namespace StudioElf.Module.GitHubConnector.Client;

/// <summary>JS interop for Chart.js bar charts in dashboard widgets and analytics tabs.</summary>
public static class ChartInterop
{
    private static bool _loaded;
    private static readonly object _lock = new();

    /// <summary>Ensure Chart.js is loaded from CDN, then draw a bar chart.</summary>
    public static async Task DrawBarChartAsync(IJSRuntime js, string canvasId, string[] labels, int[] data, string? label = null, string? color = null)
    {
        // Load Chart.js once
        if (!_loaded)
        {
            lock (_lock)
            {
                if (!_loaded)
                {
                    _loaded = true; // optimistic — avoids repeated CDN loads
                    _ = Task.Run(async () =>
                    {
                        await js.InvokeVoidAsync("eval", @"
(function(){
    if (window.GitHubChartLoading) return;
    window.GitHubChartLoading = true;
    var s = document.createElement('script');
    s.src = 'https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js';
    s.onload = function() { window.GitHubChartReady = true; };
    document.head.appendChild(s);
})();");
                    });
                }
            }
        }

        // Wait for Chart.js to load
        for (var i = 0; i < 50; i++)
        {
            var ready = await js.InvokeAsync<bool>("eval", "!!window.GitHubChartReady");
            if (ready) break;
            await Task.Delay(100);
        }

        // Draw chart
        var colorVal = color ?? "rgba(13,110,253,0.7)";
        await js.InvokeVoidAsync("eval", $@"
(function(){{
    var ctx = document.getElementById('{canvasId}');
    if (!ctx) return;
    if (ctx._chart) ctx._chart.destroy();
    ctx._chart = new Chart(ctx, {{
        type: 'bar',
        data: {{
            labels: {System.Text.Json.JsonSerializer.Serialize(labels)},
            datasets: [{{ label: '{label ?? ""}', data: {System.Text.Json.JsonSerializer.Serialize(data)}, backgroundColor: '{colorVal}', borderRadius: 3 }}]
        }},
        options: {{
            responsive: true, maintainAspectRatio: false,
            plugins: {{ legend: {{ display: false }} }},
            scales: {{ y: {{ beginAtZero: true, ticks: {{ precision: 0 }} }}, x: {{ grid: {{ display: false }} }} }}
        }}
    }});
}})();");
    }

    /// <summary>Destroy a chart by canvas ID.</summary>
    public static async Task DestroyChartAsync(IJSRuntime js, string canvasId)
    {
        await js.InvokeVoidAsync("eval", $@"(function(){{var e=document.getElementById('{canvasId}');if(e&&e._chart){{e._chart.destroy();delete e._chart;}}}})();");
    }
}
