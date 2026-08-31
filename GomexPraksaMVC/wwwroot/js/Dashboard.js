document.addEventListener('DOMContentLoaded', function () {
    initPrometChart();
    renderCriticalTopFromData();
    renderRucFromData();
});


/* =========================================================
   CHART QUALITY
   ========================================================= */

function getChartPixelRatio() {
    return Math.max(
        window.devicePixelRatio || 1,
        2
    );
}


/* =========================================================
   PROMET CHART
   ========================================================= */

function initPrometChart() {

    var canvas =
        document.getElementById('prometChart');

    if (!canvas) {
        return;
    }

    if (typeof Chart === 'undefined') {
        return;
    }

    var podaci =
        window.__dashboardData &&
            window.__dashboardData.promet
            ? window.__dashboardData.promet
            : null;

    if (!podaci) {
        return;
    }

    var trenutni =
        Number(podaci.trenutni) || 0;

    var promena =
        Number(podaci.promenaProcenat) || 0;

    var prethodni = 0;

    /*
        promena =
        (trenutni - prethodni) / prethodni

        prethodni =
        trenutni / (1 + promena)
    */

    if ((1 + promena) !== 0) {
        prethodni =
            trenutni / (1 + promena);
    }

    new Chart(
        canvas,
        {
            type: 'bar',

            data: {
                labels: [
                    'Prethodni period',
                    'Trenutni period'
                ],

                datasets: [
                    {
                        label: 'Promet (RSD)',

                        data: [
                            prethodni,
                            trenutni
                        ],

                        backgroundColor: [
                            '#B4B2A9',
                            '#1F4E5C'
                        ],

                        borderRadius: 6,

                        borderSkipped: false,

                        maxBarThickness: 120
                    }
                ]
            },

            options: {

                responsive: true,

                maintainAspectRatio: false,

                devicePixelRatio:
                    getChartPixelRatio(),

                animation: {
                    duration: 400
                },

                layout: {
                    padding: {
                        top: 5,
                        right: 8,
                        bottom: 0,
                        left: 4
                    }
                },

                plugins: {

                    legend: {
                        display: false
                    },

                    tooltip: {
                        displayColors: false,

                        callbacks: {
                            label: function (context) {
                                return formatRsd(
                                    context.raw
                                );
                            }
                        }
                    }
                },

                scales: {

                    x: {
                        border: {
                            display: false
                        },

                        grid: {
                            display: false
                        },

                        ticks: {
                            color: '#6B706C',

                            font: {
                                size: 12,
                                weight: '500'
                            }
                        }
                    },

                    y: {
                        beginAtZero: true,

                        border: {
                            display: false
                        },

                        grid: {
                            color:
                                'rgba(0, 0, 0, 0.07)'
                        },

                        ticks: {
                            color: '#6B706C',

                            font: {
                                size: 11
                            },

                            padding: 8,

                            callback: function (value) {
                                return formatShortNumber(
                                    value
                                );
                            }
                        }
                    }
                }
            }
        }
    );
}


/* =========================================================
   TOP 5 KRITICNIH ARTIKALA
   ========================================================= */

function renderCriticalTopFromData() {

    try {

        var data =
            window.__dashboardData &&
                window.__dashboardData.criticalTop
                ? window.__dashboardData.criticalTop
                : [];

        if (!data || data.length === 0) {
            return;
        }

        if (typeof Chart === 'undefined') {
            return;
        }

        var canvas =
            document.getElementById(
                'criticalTopChart'
            );

        if (!canvas) {
            return;
        }

        var labels =
            data.map(function (item) {

                return (
                    item.NazivArtikla ||
                    item.nazivArtikla ||
                    ''
                );
            });

        var values =
            data.map(function (item) {

                return Number(
                    item.ProcenjeniUticaj ??
                    item.procenjeniUticaj ??
                    0
                );
            });

        new Chart(
            canvas,
            {
                type: 'bar',

                data: {

                    labels: labels,

                    datasets: [
                        {
                            label:
                                'Procenjeni uticaj (RSD)',

                            data: values,

                            backgroundColor:
                                'rgba(193, 68, 59, 0.82)',

                            borderColor:
                                '#C1443B',

                            borderWidth: 1,

                            borderRadius: 6,

                            borderSkipped: false,

                            maxBarThickness: 32
                        }
                    ]
                },

                options: {

                    responsive: true,

                    maintainAspectRatio: false,

                    devicePixelRatio:
                        getChartPixelRatio(),

                    indexAxis: 'y',

                    animation: {
                        duration: 400
                    },

                    layout: {
                        padding: {
                            right: 8
                        }
                    },

                    plugins: {

                        legend: {
                            display: false
                        },

                        tooltip: {

                            displayColors: false,

                            callbacks: {

                                label: function (context) {

                                    return formatRsd(
                                        context.raw
                                    );
                                }
                            }
                        }
                    },

                    scales: {

                        x: {

                            border: {
                                display: false
                            },

                            grid: {
                                color:
                                    'rgba(0, 0, 0, 0.07)'
                            },

                            ticks: {

                                color: '#6B706C',

                                font: {
                                    size: 11
                                },

                                callback: function (value) {

                                    return formatShortNumber(
                                        value
                                    );
                                }
                            }
                        },

                        y: {

                            border: {
                                display: false
                            },

                            grid: {
                                display: false
                            },

                            ticks: {

                                color: '#4D514E',

                                font: {
                                    size: 11,
                                    weight: '500'
                                }
                            }
                        }
                    }
                }
            }
        );
    }
    catch (e) {

        console.error(
            'Greška Critical Top chart:',
            e
        );
    }
}


/* =========================================================
   RUC CHANGE CHART
   ========================================================= */

function renderRucFromData() {

    try {

        var ruc =
            window.__dashboardData &&
                window.__dashboardData.rucChange
                ? window.__dashboardData.rucChange
                : null;

        if (!ruc) {
            return;
        }

        if (typeof Chart === 'undefined') {
            return;
        }

        var canvas =
            document.getElementById(
                'rucWaterfall'
            );

        if (!canvas) {
            return;
        }

        var pocetni =
            Number(
                ruc.PocetniRuc ??
                ruc.pocetniRuc ??
                0
            );

        var margin =
            Number(
                ruc.MarginEffect ??
                ruc.marginEffect ??
                0
            );

        var volume =
            Number(
                ruc.VolumeEffect ??
                ruc.volumeEffect ??
                0
            );

        var mix =
            Number(
                ruc.MixEffect ??
                ruc.mixEffect ??
                0
            );

        var konacni =
            Number(
                ruc.KonacniRuc ??
                ruc.konacniRuc ??
                0
            );

        var labels = [
            'Početni RUC',
            'Margin effect',
            'Volume effect',
            'Mix effect',
            'Konačni RUC'
        ];

        var values = [
            pocetni,
            margin,
            volume,
            mix,
            konacni
        ];

        var colors =
            values.map(
                function (value, index) {

                    if (
                        index === 0 ||
                        index === values.length - 1
                    ) {
                        return '#56606E';
                    }

                    return value >= 0
                        ? '#2E7D5B'
                        : '#C1443B';
                }
            );

        new Chart(
            canvas,
            {
                type: 'bar',

                data: {

                    labels: labels,

                    datasets: [
                        {
                            label: 'RUC promena',

                            data: values,

                            backgroundColor:
                                colors,

                            borderColor:
                                colors,

                            borderWidth: 1,

                            borderRadius: 6,

                            borderSkipped: false,

                            maxBarThickness: 70
                        }
                    ]
                },

                options: {

                    responsive: true,

                    maintainAspectRatio: false,

                    devicePixelRatio:
                        getChartPixelRatio(),

                    animation: {
                        duration: 400
                    },

                    plugins: {

                        legend: {
                            display: false
                        },

                        tooltip: {

                            displayColors: false,

                            callbacks: {

                                label: function (context) {

                                    return formatRsd(
                                        context.raw
                                    );
                                }
                            }
                        }
                    },

                    scales: {

                        x: {

                            border: {
                                display: false
                            },

                            grid: {
                                display: false
                            },

                            ticks: {

                                color: '#6B706C',

                                font: {
                                    size: 11,
                                    weight: '500'
                                }
                            }
                        },

                        y: {

                            beginAtZero: false,

                            border: {
                                display: false
                            },

                            grid: {
                                color:
                                    'rgba(0, 0, 0, 0.07)'
                            },

                            ticks: {

                                color: '#6B706C',

                                font: {
                                    size: 11
                                },

                                callback: function (value) {

                                    return formatShortNumber(
                                        value
                                    );
                                }
                            }
                        }
                    }
                }
            }
        );
    }
    catch (e) {

        console.error(
            'Greška RUC chart:',
            e
        );
    }
}


/* =========================================================
   FORMAT RSD
   ========================================================= */

function formatRsd(value) {

    var number =
        Number(value) || 0;

    return new Intl.NumberFormat(
        'sr-RS',
        {
            maximumFractionDigits: 0
        }
    ).format(number) + ' RSD';
}


/* =========================================================
   FORMAT KRATKI BROJEVI
   ========================================================= */

function formatShortNumber(value) {

    var number =
        Number(value) || 0;

    var absolute =
        Math.abs(number);

    if (absolute >= 1000000000) {

        return (
            number / 1000000000
        ).toFixed(1) + 'B';
    }

    if (absolute >= 1000000) {

        return (
            number / 1000000
        ).toFixed(1) + 'M';
    }

    if (absolute >= 1000) {

        return (
            number / 1000
        ).toFixed(0) + 'K';
    }

    return number.toFixed(0);
}