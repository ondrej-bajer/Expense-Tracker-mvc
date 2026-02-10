(function () {
    const container = document.getElementById("dashboardCharts");
    if (!container) return;

    const monthlyRaw = container.dataset.monthly || "[]";
    const categoriesRaw = container.dataset.categories || "[]";

    let monthly = [];
    let categories = [];

    try {
        monthly = JSON.parse(monthlyRaw);
        categories = JSON.parse(categoriesRaw);
    } catch (e) {
        console.error("Failed to parse dashboard chart data:", e);
        return;
    }

    if (!Array.isArray(monthly)) monthly = [];
    if (!Array.isArray(categories)) categories = [];

    // helper: generate unlimited colors
    function generateColors(count) {
        const colors = [];
        if (count <= 0) return colors;

        for (let i = 0; i < count; i++) {
            const hue = (i * 360 / count);
            colors.push(`hsla(${hue}, 65%, 55%, 0.75)`);
        }
        return colors;
    }

    // 1) Income vs Expense (bar)
    const incomeExpenseCanvas = document.getElementById("incomeExpenseChart");
    if (incomeExpenseCanvas && monthly.length) {
        const labels = monthly.map(x => x.Month);
        const income = monthly.map(x => x.Income);
        const expense = monthly.map(x => x.Expense);

        new Chart(incomeExpenseCanvas, {
            type: "bar",
            data: {
                labels,
                datasets: [
                    {
                        label: "Income",
                        data: income,
                        backgroundColor: "rgba(25, 135, 84, 0.6)",
                        borderColor: "rgba(25, 135, 84, 1)",
                        hoverBackgroundColor: "rgba(25, 135, 84, 0.8)",
                        borderWidth: 1
                    },
                    {
                        label: "Expense",
                        data: expense,
                        backgroundColor: "rgba(220, 53, 69, 0.6)",
                        borderColor: "rgba(220, 53, 69, 1)",
                        hoverBackgroundColor: "rgba(220, 53, 69, 0.8)",
                        borderWidth: 1
                    }
                ]
            },
            options: {
                plugins: { legend: { position: "top" } },
                scales: { y: { beginAtZero: true } }
            }
        });
    }

    // 2) Category donut
    const categoryCanvas = document.getElementById("categoryChart");
    if (categoryCanvas && categories.length) {
        const amounts = categories.map(x => x.Amount);
        const dynamicColors = generateColors(amounts.length);

        new Chart(categoryCanvas, {
            type: "doughnut",
            data: {
                labels: categories.map(x => x.Category),
                datasets: [{
                    data: amounts,
                    backgroundColor: dynamicColors,
                    borderWidth: 1
                }]
            },
            options: {
                plugins: { legend: { position: "top" } }
            }
        });
    }
})();
