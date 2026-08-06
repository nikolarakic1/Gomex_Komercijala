document.addEventListener('DOMContentLoaded', function () {
    initPeriodPicker();
    initDeptFilter();
    initPrometChart();
    initDobavljacSearch();
});

function initPeriodPicker() {
    var el = document.getElementById('periodPicker');
    if (!el) return;

    flatpickr(el, {
        mode: "range",
        dateFormat: "d.m.Y",
        locale: "sr",
        maxDate: "today"
    });
}

// Fetch dashboard summary from API and populate metric cards
function fetchSummary() {
    var apiBase = 'https://localhost:7212';
    var url = apiBase + '/api/Dashboard/summary';

    fetch(url)
        .then(function (resp) { if (!resp.ok) throw resp; return resp.json(); })
        .then(function (data) {
            if (!data) return;

            var promet = document.getElementById('prometValue');
            var prometYoY = document.getElementById('prometYoY');
            var zarada = document.getElementById('zaradaValue');
            var zaradaYoY = document.getElementById('zaradaYoY');
            var marza = document.getElementById('marzaValue');
            var marzaYoY = document.getElementById('marzaYoY');
            var artikli = document.getElementById('artikliValue');
            var artikliYoY = document.getElementById('artikliYoY');
            var najveci = document.getElementById('najveciRastValue');
            var najveciYoY = document.getElementById('najveciRastYoY');

            if (promet) promet.textContent = formatCurrency(data.prometBezPdv);
            if (prometYoY) prometYoY.textContent = formatPercent(data.prometPromenaProcenat) + ' vs prethodni period';

            if (zarada) zarada.textContent = formatCurrency(data.ruc12); // using RUC12 as proxy for zarada
            if (zaradaYoY) zaradaYoY.textContent = formatPercent(data.ruc12PromenaProcenat) + ' vs prethodni period';

            if (marza) marza.textContent = formatPercent(data.ruc12Procenat); // RUC12% value
            if (marzaYoY) marzaYoY.textContent = (data.ruc12PromenaProcentniPoeni || 0) + ' p.p.';

            if (artikli) artikli.textContent = (data.kriticniArtikli ?? 0).toString();
            if (artikliYoY) artikliYoY.textContent = (data.kriticniArtikliPromena >= 0 ? '+' : '') + (data.kriticniArtikliPromena ?? 0) + ' vs prethodni period';

            if (najveci) najveci.textContent = formatCurrency(data.nedostatakMarze ?? 0);
            if (najveciYoY) najveciYoY.textContent = (data.nedostatakMarzePromenaProcenat >= 0 ? '+' : '') + formatPercent(data.nedostatakMarzePromenaProcenat ?? 0) + ' vs prethodni period';
        })
        .catch(function (err) {
            console.debug('Failed to load dashboard summary', err);
        });
}

function formatCurrency(v) {
    if (v === null || v === undefined) return '-';
    return new Intl.NumberFormat('sr-RS', { style: 'currency', currency: 'RSD', maximumFractionDigits: 0 }).format(v);
}

function formatPercent(v) {
    if (v === null || v === undefined) return '-';
    return (Number(v) * 100).toFixed(1) + '%';
}

function initDeptFilter() {
    var deptSelect = document.getElementById('deptFilter');
    if (!deptSelect) return;

    var deptRows = document.querySelectorAll('.dept-row');

    deptSelect.addEventListener('change', function () {
        var selected = this.value;

        deptRows.forEach(function (row) {
            var deptKey = row.getAttribute('data-dept');
            var collapseEl = document.querySelector(row.getAttribute('data-bs-target'));
            var collapseInstance = bootstrap.Collapse.getOrCreateInstance(collapseEl, { toggle: false });

            var visible = (selected === 'sva' || selected === deptKey);
            row.style.display = visible ? '' : 'none';

            if (selected === 'sva') {
                collapseInstance.hide();
            } else if (deptKey === selected) {
                collapseInstance.show();
            } else {
                collapseInstance.hide();
            }
        });
    });
}

function initPrometChart() {
    var canvas = document.getElementById('prometChart');
    if (!canvas) return;

    // Privremeni podaci — kasnije zamenjujemo pozivom ka API-ju za agregatni promet
    var series = {
        nedelja: {
            title: 'Promet po nedeljama',
            labels: ['N18', 'N19', 'N20', 'N21', 'N22', 'N23', 'N24', 'N25', 'N26', 'N27', 'N28', 'N29'],
            actual: [602, 618, 638, 651, 715, 734, 745, 780, 812, 852, 899, 905],
            compare: [610, 631, 663, 674, 692, 704, 719, 733, 774, 794, 823, 852],
            type: 'line',
            window: 8
        },
        mesec: {
            title: 'Promet po mesecima',
            labels: ['Jan', 'Feb', 'Mar', 'Apr', 'Maj', 'Jun', 'Jul'],
            actual: [2450, 2680, 2510, 2890, 2760, 3020, 2950],
            compare: [2300, 2520, 2480, 2700, 2650, 2880, 2810],
            type: 'line',
            window: 6
        },
        godina: {
            title: 'Ukupan promet po godinama',
            labels: ['2021', '2022', '2023', '2024', '2025', '2026'],
            actual: [12800, 14200, 15600, 17100, 18900, 11200],
            compare: null,
            type: 'bar',
            window: 5
        }
    };

    var chart = null;
    var granularity = 'nedelja';
    var offset = 0;

    function lastIndex(arr) {
        for (var i = arr.length - 1; i >= 0; i--) {
            if (arr[i] !== null && arr[i] !== undefined) return i;
        }
        return arr.length - 1;
    }

    function defaultOffset(d) {
        var end = lastIndex(d.actual);
        return Math.max(0, end - d.window + 1);
    }

    function render() {
        var d = series[granularity];
        var start = offset;
        var end = Math.min(start + d.window, d.labels.length);

        var labels = d.labels.slice(start, end);
        var actual = d.actual.slice(start, end);
        var compare = d.compare ? d.compare.slice(start, end) : null;

        document.getElementById('prometChartTitle').textContent = d.title;
        document.getElementById('prometChartLegend').style.display = compare ? 'flex' : 'none';

        var datasets = [{
            label: 'Trenutni period',
            data: actual,
            borderColor: '#1F4E5C',
            backgroundColor: d.type === 'bar' ? '#1F4E5C' : 'rgba(31,78,92,0.08)',
            fill: d.type === 'line',
            spanGaps: false,
            tension: 0.3
        }];

        if (compare) {
            datasets.push({
                label: 'Prethodni period',
                data: compare,
                borderColor: '#B4B2A9',
                borderDash: [4, 4],
                fill: false,
                tension: 0.3
            });
        }

        if (chart) chart.destroy();

        chart = new Chart(canvas, {
            type: d.type,
            data: { labels: labels, datasets: datasets },
            options: {
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: false } }
            }
        });

        document.getElementById('prometBtnPrev').disabled = (start <= 0);
        document.getElementById('prometBtnNext').disabled = (end >= d.labels.length);
    }

    function switchGranularity(key) {
        granularity = key;
        offset = defaultOffset(series[key]);
        render();
    }

    switchGranularity('nedelja');

    var buttons = document.querySelectorAll('#prometGranularityToggle button');
    buttons.forEach(function (btn) {
        btn.addEventListener('click', function () {
            buttons.forEach(function (b) { b.classList.remove('active'); });
            this.classList.add('active');
            switchGranularity(this.getAttribute('data-granularity'));
        });
    });

    document.getElementById('prometBtnPrev').addEventListener('click', function () {
        offset = Math.max(0, offset - 1);
        render();
    });
    document.getElementById('prometBtnNext').addEventListener('click', function () {
        var maxStart = series[granularity].labels.length - series[granularity].window;
        offset = Math.min(maxStart, offset + 1);
        render();
    });
}
function initDobavljacSearch() {
    var el = document.getElementById('dobavljacFilter');
    if (!el) return;

    try {
        new Choices(el, {
            searchEnabled: true,
            itemSelectText: '',
            placeholder: true,
            placeholderValue: 'Pretraži dobavljača...',
            shouldSort: false
        });
    } catch (err) {
        console.error('Choices.js nije učitan:', err);
    }

    // Trenutna vrednost iz URL-a, PRE nego što Choices stigne da okine svoj initial change
    var currentParams = new URLSearchParams(window.location.search);
    var currentValue = currentParams.get('dobavljacId') || '';

    el.addEventListener('change', function () {
        // Choices.js okine change i pri inicijalizaciji — ako je vrednost ISTA kao u URL-u, ignoriši
        if (el.value === currentValue) return;
        window.location.href = '/Dashboard?dobavljacId=' + el.value;
    });
}