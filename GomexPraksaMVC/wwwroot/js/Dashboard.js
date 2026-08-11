document.addEventListener('DOMContentLoaded', function () {
    initPeriodPicker();
    initDeptFilter();
    initPrometChart();
    initDobavljacSearch();
    renderCriticalTopFromData();
    renderRucFromData();
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

function renderCriticalTopFromData() {
    try {
        var data = window.__dashboardData && window.__dashboardData.criticalTop ? window.__dashboardData.criticalTop : [];
        if (!data || data.length === 0) return;

        var labels = data.map(function (d) { return d.NazivArtikla || d.nazivArtikla || d.name; });
        var values = data.map(function (d) { return d.ProcenjeniUticaj || d.procenjeniUticaj || 0; });

        var canvas = document.getElementById('criticalTopChart');
        if (!canvas) return;

        var ctx = canvas.getContext('2d');

        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Procenjeni uticaj (RSD)',
                    data: values,
                    backgroundColor: labels.map(function () { return 'rgba(193,68,59,0.85)'; }),
                    borderColor: labels.map(function () { return 'rgba(193,68,59,1)'; }),
                    borderWidth: 1
                }]
            },
            options: {
                indexAxis: 'y',
                plugins: { legend: { display: false } },
                scales: { x: { beginAtZero: true } }
            }
        });
    } catch (e) { console.error(e); }
}

function renderRucFromData() {
    try {
        var ruc = window.__dashboardData && window.__dashboardData.rucChange ? window.__dashboardData.rucChange : null;
        if (!ruc) return;

        var labels = ['Početni RUC', 'Margin effect', 'Volume effect', 'Mix effect', 'Konačni RUC'];
        var values = [ruc.PocetniRuc || ruc.pocetniRuc || 0, ruc.MarginEffect || ruc.marginEffect || 0, ruc.VolumeEffect || ruc.volumeEffect || 0, ruc.MixEffect || ruc.mixEffect || 0, ruc.KonacniRuc || ruc.konacniRuc || 0];

        var canvas = document.getElementById('rucWaterfall');
        if (!canvas) return;
        var ctx = canvas.getContext('2d');

        var colors = values.map(function (v, idx) {
            if (idx === 0 || idx === values.length - 1) return '#4B5563';
            return v >= 0 ? '#2D9F5D' : '#C1443B';
        });

        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'RUC change',
                    data: values,
                    backgroundColor: colors,
                    borderColor: colors,
                    borderWidth: 1
                }]
            },
            options: {
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: false } }
            }
        });
    } catch (e) { console.error(e); }
}

function initDeptFilter() {
    var deptSelect = document.getElementById('deptFilter');
    if (!deptSelect) return;

    // If a previous Choices.js wrapper exists around this select (from earlier loads), unwrap it
    try {
        var wrapper = deptSelect.closest('.choices');
        if (wrapper && wrapper.parentNode) {
            wrapper.parentNode.insertBefore(deptSelect, wrapper);
            wrapper.parentNode.removeChild(wrapper);
        }
    } catch (e) { }

    var deptRows = document.querySelectorAll('.dept-row');
    // Keep select value in sync with query string
    var currentDept = '';
    try {
        var params = new URLSearchParams(window.location.search);
        currentDept = params.get('dept') || 'sva';
        if (deptSelect.value !== currentDept) deptSelect.value = currentDept;
    } catch (ex) { }

    // Do NOT initialize Choices.js for deptFilter — keep native select styling
    var deptChoices = null;

    function applyDeptVisibility(selected) {
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
    }

    // Native select behavior: update visibility immediately and redirect preserving other params
    deptSelect.addEventListener('change', function () {
        var selected = this.value;
        applyDeptVisibility(selected);

        try {
            var params = new URLSearchParams(window.location.search);
            if (selected === 'sva' || selected === '') params.delete('dept'); else params.set('dept', selected);
            var qs = params.toString();
            window.location.href = window.location.pathname + (qs ? ('?' + qs) : '');
        } catch (err) {
            if (selected === 'sva' || selected === '') window.location.href = '/Dashboard'; else window.location.href = '/Dashboard?dept=' + encodeURIComponent(selected);
        }
    });

    // Apply initial visibility based on currentDept
    applyDeptVisibility(currentDept);
}

function initPrometChart() {
    var canvas = document.getElementById('prometChart');
    if (!canvas) return;

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
    // If a previous Choices.js wrapper exists around this select (from earlier loads), unwrap it
    try {
        var wrapper = el.closest('.choices');
        if (wrapper && wrapper.parentNode) {
            wrapper.parentNode.insertBefore(el, wrapper);
            wrapper.parentNode.removeChild(wrapper);
        }
    } catch (e) { }

    // Native select redirect preserving other params
    try {
        var params = new URLSearchParams(window.location.search);
        var currentValue = params.get('dobavljacId') || '';
        if (!el.value && currentValue) el.value = currentValue;
    } catch (ex) { var currentValue = ''; }

    el.addEventListener('change', function () {
        var selected = el.value || '';
        if (selected === currentValue) return;
        try {
            var params2 = new URLSearchParams(window.location.search);
            if (selected === '') params2.delete('dobavljacId'); else params2.set('dobavljacId', selected);
            var qs = params2.toString();
            window.location.href = window.location.pathname + (qs ? ('?' + qs) : '');
        } catch (err) {
            if (selected === '') window.location.href = '/Dashboard'; else window.location.href = '/Dashboard?dobavljacId=' + encodeURIComponent(selected);
        }
    });
}