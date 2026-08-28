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

// Initialize searchable selects for kategorija and odeljenje too
function initOtherSelectSearch() {
    var selIds = ['kategorijaId', 'odeljenjeId'];
    selIds.forEach(function (name) {
        var el = document.querySelector('select[name=' + name + ']');
        if (!el) return;
        try {
            if (el.choicesInstance && typeof el.choicesInstance.destroy === 'function') el.choicesInstance.destroy();
        } catch (e) { }

        var choices = new Choices(el, {
            searchEnabled: true,
            shouldSort: false,
            placeholder: true,
            placeholderValue: name === 'kategorijaId' ? 'Sve kategorije' : 'Sva odeljenja',
            itemSelectText: '',
            searchResultLimit: 100,
            renderChoiceLimit: 200,
            classNames: { containerOuter: 'choices filter-choices' }
        });
        el.choicesInstance = choices;
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

    var podaci = window.__dashboardData && window.__dashboardData.promet
        ? window.__dashboardData.promet
        : null;

    if (!podaci) return;

    var trenutni = podaci.trenutni || 0;
    var promena = podaci.promenaProcenat || 0;

    // promenaProcenat = (trenutni - prethodni) / prethodni  =>  prethodni = trenutni / (1 + promena)
    var prethodni = (1 + promena) !== 0 ? trenutni / (1 + promena) : 0;

    new Chart(canvas, {
        type: 'bar',
        data: {
            labels: ['Prethodni period', 'Trenutni period'],
            datasets: [{
                label: 'Promet (RSD)',
                data: [prethodni, trenutni],
                backgroundColor: ['#B4B2A9', '#1F4E5C'],
                borderRadius: 4
            }]
        },
        options: {
            plugins: { legend: { display: false } },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) {
                            return (value / 1000000).toFixed(1) + 'M';
                        }
                    }
                }
            }
        }
    });
}


// Articles modal and dynamic article loading removed; navigation moved to /Artikal/Index page

function initDobavljacSearch() {
    var el = document.getElementById('dobavljacFilter');
    if (!el) return;

    // initialize Choices.js with search enabled and small dropdown
    try {
        // destroy previous instance if exists
        if (el.choicesInstance && typeof el.choicesInstance.destroy === 'function') el.choicesInstance.destroy();
    } catch (e) { }

    var params = new URLSearchParams(window.location.search);
    var currentValue = params.get('dobavljacId') || '';

    var choices = new Choices(el, {
        searchEnabled: true,
        shouldSort: false,
        placeholder: true,
        placeholderValue: 'Svi dobavljači',
        itemSelectText: '',
        searchResultLimit: 100,
        renderChoiceLimit: 200,
        classNames: {
            containerOuter: 'choices filter-choices',
        }
    });
    el.choicesInstance = choices;

    if (currentValue && !el.value) {
        try { choices.setChoiceByValue(currentValue.toString()); } catch (e) { }
    }

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