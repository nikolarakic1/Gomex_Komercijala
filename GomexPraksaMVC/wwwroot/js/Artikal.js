document.addEventListener('DOMContentLoaded', function () {
    initYoyChart();
});

function initYoyChart() {
    var canvas = document.getElementById('yoyChart');
    if (!canvas) return;

    // Privremeni podaci — kasnije zamenjujemo pozivom ka API-ju po šifri artikla
    var series = {
        nedelja: {
            title: 'Prodata količina po nedeljama',
            labels: ['N15', 'N16', 'N17', 'N18', 'N19', 'N20', 'N21', 'N22', 'N23', 'N24', 'N25', 'N26', 'N27', 'N28', 'N29', 'N30', 'N31', 'N32'],
            byYear: {
                2026: [58, 62, 65, 70, 68, 65, 70, 68, 75, 80, 78, 84, 90, 88, 95, 102, 91, 88],
                2025: [50, 55, 58, 60, 63, 60, 64, 62, 68, 72, 71, 75, 80, 79, 85, 90, 82, 79],
                2024: [45, 48, 50, 54, 56, 55, 58, 57, 60, 65, 64, 68, 72, 70, 76, 80, 75, 71],
                2023: [40, 43, 46, 48, 50, 49, 52, 51, 55, 58, 57, 60, 64, 63, 68, 72, 66, 63]
            },
            window: 8
        },
        mesec: {
            title: 'Prodata količina po mesecima',
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'Maj', 'Jun', 'Jul', 'Avg', 'Sep', 'Okt', 'Nov', 'Dec'],
            byYear: {
                2026: [340, 365, 310, 402, 388, 415, 390, null, null, null, null, null],
                2025: [300, 320, 305, 360, 350, 370, 355, 340, 365, 380, 395, 410],
                2024: [280, 300, 290, 335, 325, 345, 330, 315, 340, 355, 368, 385],
                2023: [260, 275, 270, 310, 300, 320, 305, 290, 315, 330, 340, 358]
            },
            window: 6
        },
        godina: {
            title: 'Ukupna prodata količina po godinama',
            labels: ['2018', '2019', '2020', '2021', '2022', '2023', '2024', '2025', '2026'],
            actual: [2800, 3050, 2600, 3100, 3200, 3450, 3680, 3900, 2650],
            window: 5
        }
    };

    var chart = null;
    var currentGranularity = 'nedelja';
    var compareYear = 2025;
    var offset = 0;

    function currentIndex(arr) {
        for (var i = arr.length - 1; i >= 0; i--) {
            if (arr[i] !== null && arr[i] !== undefined) return i;
        }
        return arr.length - 1;
    }

    function defaultOffset(d, actualArr) {
        var end = currentIndex(actualArr);
        return Math.max(0, end - d.window + 1);
    }

    function render() {
        var d = series[currentGranularity];

        if (currentGranularity === 'godina') {
            renderGodina(d);
            return;
        }

        var actualArr = d.byYear[2026];
        var compareArr = d.byYear[compareYear] || null;

        var start = offset;
        var end = Math.min(start + d.window, d.labels.length);

        var labels = d.labels.slice(start, end);
        var actual = actualArr.slice(start, end);
        var compare = compareArr ? compareArr.slice(start, end) : null;

        document.getElementById('chartTitle').textContent = d.title;
        document.getElementById('chartLegend').style.display = 'flex';
        document.getElementById('compareYearText').textContent = compareYear;
        document.getElementById('compareYearSelect').style.display = 'inline-block';

        var datasets = [{
            label: '2026',
            data: actual,
            borderColor: '#1F4E5C',
            backgroundColor: 'rgba(31,78,92,0.08)',
            fill: true,
            spanGaps: false,
            tension: 0.3
        }];

        if (compare) {
            datasets.push({
                label: String(compareYear),
                data: compare,
                borderColor: '#B4B2A9',
                borderDash: [4, 4],
                fill: false,
                tension: 0.3
            });
        }

        drawChart('line', labels, datasets);

        document.getElementById('btnPrev').disabled = (start <= 0);
        document.getElementById('btnNext').disabled = (end >= d.labels.length);
    }

    function renderGodina(d) {
        var start = offset;
        var end = Math.min(start + d.window, d.labels.length);

        document.getElementById('chartTitle').textContent = d.title;
        document.getElementById('chartLegend').style.display = 'none';
        document.getElementById('compareYearSelect').style.display = 'none';

        var labels = d.labels.slice(start, end);
        var actual = d.actual.slice(start, end);

        drawChart('bar', labels, [{
            label: 'Ukupno',
            data: actual,
            backgroundColor: '#1F4E5C'
        }]);

        document.getElementById('btnPrev').disabled = (start <= 0);
        document.getElementById('btnNext').disabled = (end >= d.labels.length);
    }

    function drawChart(type, labels, datasets) {
        if (chart) chart.destroy();
        chart = new Chart(canvas, {
            type: type,
            data: { labels: labels, datasets: datasets },
            options: {
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true } }
            }
        });
    }

    function switchGranularity(key) {
        currentGranularity = key;
        var d = series[key];
        var refArr = key === 'godina' ? d.actual : d.byYear[2026];
        offset = defaultOffset(d, refArr);
        render();
    }

    // Inicijalni prikaz
    switchGranularity('nedelja');

    // Dugmići za granularnost
    var buttons = document.querySelectorAll('#granularityToggle button');
    buttons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            buttons.forEach(function (b) { b.classList.remove('active'); });
            this.classList.add('active');
            switchGranularity(this.getAttribute('data-granularity'));
        });
    });

    // Selektor godine za poređenje
    document.getElementById('compareYearSelect').addEventListener('change', function () {
        compareYear = parseInt(this.value, 10);
        render();
    });

    // Strelice — pomeraju prozor unazad/unapred
    document.getElementById('btnPrev').addEventListener('click', function () {
        offset = Math.max(0, offset - 1);
        render();
    });
    document.getElementById('btnNext').addEventListener('click', function () {
        var maxStart = series[currentGranularity].labels.length - series[currentGranularity].window;
        offset = Math.min(maxStart, offset + 1);
        render();
    });
}