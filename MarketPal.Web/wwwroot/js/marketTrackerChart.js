// Small helper that renders a Chart.js line chart showing price, MA50 and MA200.
function renderStockChart(canvasId, labels, prices, ma50, ma200) {
    const ctx = document.getElementById(canvasId).getContext('2d');

    // Convert nullable arrays to numeric arrays, using nulls for gaps (Chart.js will skip nulls)
    const ma50Data = ma50.map(x => x === null ? null : Number(x));
    const ma200Data = ma200.map(x => x === null ? null : Number(x));

    // If a previous chart instance exists on the canvas, destroy it.
    if (ctx._chartInstance) {
        ctx._chartInstance.destroy();
    }

    const chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Close',
                    data: prices,
                    borderColor: '#007bff',
                    backgroundColor: 'rgba(0,123,255,0.05)',
                    pointRadius: 0,
                    tension: 0.15
                },
                {
                    label: 'MA50',
                    data: ma50Data,
                    borderColor: '#28a745',
                    borderDash: [4, 2],
                    pointRadius: 0,
                    tension: 0.15
                },
                {
                    label: 'MA200',
                    data: ma200Data,
                    borderColor: '#ffc107',
                    borderDash: [6, 3],
                    pointRadius: 0,
                    tension: 0.15
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: { display: true },
                y: { display: true }
            },
            plugins: {
                legend: { position: 'top' }
            }
        }
    });

    ctx._chartInstance = chart;
    return chart;
}
