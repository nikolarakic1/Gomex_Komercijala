document.addEventListener('DOMContentLoaded', function () {
    initPrometChart();
    renderCriticalTopFromData();
    renderRucFromData();
});

function getChartPixelRatio() {
    return Math.max(
        window.devicePixelRatio || 1,
        2
    );
}

function createVerticalGradient(
    context,
    topColor,
    bottomColor
) {
    const chart = context.chart;

    if (!chart.chartArea) {
        return topColor;
    }

    const gradient =
        chart.ctx.createLinearGradient(
            0,
            chart.chartArea.top,
            0,
            chart.chartArea.bottom
        );

    gradient.addColorStop(
        0,
        topColor
    );

    gradient.addColorStop(
        1,
        bottomColor
    );

    return gradient;
}

function createHorizontalGradient(
    context,
    leftColor,
    rightColor
) {
    const chart = context.chart;

    if (!chart.chartArea) {
        return leftColor;
    }

    const gradient =
        chart.ctx.createLinearGradient(
            chart.chartArea.left,
            0,
            chart.chartArea.right,
            0
        );

    gradient.addColorStop(
        0,
        leftColor
    );

    gradient.addColorStop(
        1,
        rightColor
    );

    return gradient;
}

const valueLabelPlugin = {
    id: 'valueLabelPlugin',

    afterDatasetsDraw(
        chart,
        args,
        pluginOptions
    ) {
        if (
            !pluginOptions ||
            pluginOptions.enabled === false
        ) {
            return;
        }

        const ctx =
            chart.ctx;

        const dataset =
            chart.data.datasets[0];

        const meta =
            chart.getDatasetMeta(0);

        ctx.save();

        meta.data.forEach(
            function (
                element,
                index
            ) {
                let value =
                    dataset.data[index];

                if (
                    Array.isArray(value)
                ) {
                    return;
                }

                value =
                    Number(value) || 0;

                const props =
                    element.getProps(
                        [
                            'x',
                            'y',
                            'base'
                        ],
                        true
                    );

                ctx.font =
                    '700 11px Inter, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';

                ctx.fillStyle =
                    '#3E4742';

                if (
                    chart.options.indexAxis ===
                    'y'
                ) {
                    const positive =
                        value >= 0;

                    ctx.textAlign =
                        positive
                            ? 'left'
                            : 'right';

                    ctx.textBaseline =
                        'middle';

                    ctx.fillText(
                        formatShortNumber(
                            value
                        ),
                        props.x +
                        (
                            positive
                                ? 9
                                : -9
                        ),
                        props.y
                    );
                }
                else {
                    ctx.textAlign =
                        'center';

                    ctx.textBaseline =
                        value >= 0
                            ? 'bottom'
                            : 'top';

                    ctx.fillText(
                        formatShortNumber(
                            value
                        ),
                        props.x,
                        props.y +
                        (
                            value >= 0
                                ? -9
                                : 9
                        )
                    );
                }
            }
        );

        ctx.restore();
    }
};

function initPrometChart() {
    const canvas =
        document.getElementById(
            'prometChart'
        );

    if (
        !canvas ||
        typeof Chart === 'undefined'
    ) {
        return;
    }

    const data =
        window.__dashboardData?.promet;

    if (!data) {
        return;
    }

    const trenutni =
        Number(
            data.trenutni
        ) || 0;

    const promena =
        Number(
            data.promenaProcenat
        ) || 0;

    let prethodni = 0;

    if (
        (1 + promena) !== 0
    ) {
        prethodni =
            trenutni /
            (1 + promena);
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
                        label:
                            'Promet',

                        data: [
                            prethodni,
                            trenutni
                        ],

                        backgroundColor:
                            function (
                                context
                            ) {
                                if (
                                    context.dataIndex ===
                                    0
                                ) {
                                    return createVerticalGradient(
                                        context,
                                        'rgba(141, 153, 147, 0.96)',
                                        'rgba(177, 188, 182, 0.55)'
                                    );
                                }

                                return createVerticalGradient(
                                    context,
                                    'rgba(25, 119, 87, 1)',
                                    'rgba(58, 174, 132, 0.62)'
                                );
                            },

                        borderColor:
                            function (
                                context
                            ) {
                                return context
                                    .dataIndex === 0
                                    ? '#84918A'
                                    : '#197757';
                            },

                        borderWidth:
                            1.4,

                        borderRadius: {
                            topLeft: 13,
                            topRight: 13,
                            bottomLeft: 4,
                            bottomRight: 4
                        },

                        borderSkipped:
                            false,

                        maxBarThickness:
                            100,

                        hoverBorderWidth:
                            2
                    }
                ]
            },

            options: {
                responsive:
                    true,

                maintainAspectRatio:
                    false,

                devicePixelRatio:
                    getChartPixelRatio(),

                animation: {
                    duration:
                        950,
                    easing:
                        'easeOutQuart'
                },

                interaction: {
                    mode:
                        'nearest',
                    intersect:
                        false
                },

                layout: {
                    padding: {
                        top: 30,
                        right: 15,
                        bottom: 4,
                        left: 8
                    }
                },

                plugins: {
                    legend: {
                        display:
                            false
                    },

                    valueLabelPlugin: {
                        enabled:
                            true
                    },

                    tooltip: {
                        displayColors:
                            false,

                        backgroundColor:
                            'rgba(21, 29, 25, 0.98)',

                        titleColor:
                            '#FFFFFF',

                        bodyColor:
                            '#FFFFFF',

                        titleFont: {
                            size: 12,
                            weight:
                                '600'
                        },

                        bodyFont: {
                            size: 13,
                            weight:
                                '700'
                        },

                        padding:
                            13,

                        cornerRadius:
                            11,

                        caretPadding:
                            8,

                        callbacks: {
                            label:
                                function (
                                    context
                                ) {
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
                            display:
                                false
                        },

                        grid: {
                            display:
                                false
                        },

                        ticks: {
                            color:
                                '#39433E',

                            padding:
                                8,

                            font: {
                                size: 12,
                                weight:
                                    '700'
                            }
                        }
                    },

                    y: {
                        beginAtZero:
                            true,

                        border: {
                            display:
                                false
                        },

                        grid: {
                            color:
                                'rgba(55, 70, 62, 0.22)',

                            lineWidth:
                                1.15,

                            drawTicks:
                                false
                        },

                        ticks: {
                            color:
                                '#56615B',

                            padding:
                                10,

                            font: {
                                size: 11,
                                weight:
                                    '600'
                            },

                            callback:
                                function (
                                    value
                                ) {
                                    return formatShortNumber(
                                        value
                                    );
                                }
                        }
                    }
                }
            },

            plugins: [
                valueLabelPlugin
            ]
        }
    );
}

function renderCriticalTopFromData() {
    try {
        const data =
            window
                .__dashboardData
                ?.criticalTop ||
            [];

        if (
            !data.length ||
            typeof Chart ===
            'undefined'
        ) {
            return;
        }

        const canvas =
            document.getElementById(
                'criticalTopChart'
            );

        if (!canvas) {
            return;
        }

        const labels =
            data.map(
                function (
                    item
                ) {
                    return (
                        item.NazivArtikla ||
                        item.nazivArtikla ||
                        ''
                    );
                }
            );

        const values =
            data.map(
                function (
                    item
                ) {
                    return Number(
                        item.ProcenjeniUticaj ??
                        item.procenjeniUticaj ??
                        0
                    );
                }
            );

        new Chart(
            canvas,
            {
                type: 'bar',

                data: {
                    labels:
                        labels,

                    datasets: [
                        {
                            label:
                                'Procenjeni uticaj',

                            data:
                                values,

                            backgroundColor:
                                function (
                                    context
                                ) {
                                    const value =
                                        Number(
                                            context.raw
                                        ) || 0;

                                    if (
                                        value < 0
                                    ) {
                                        return createHorizontalGradient(
                                            context,
                                            'rgba(147, 37, 37, 0.82)',
                                            'rgba(210, 66, 57, 0.98)'
                                        );
                                    }

                                    return createHorizontalGradient(
                                        context,
                                        'rgba(22, 112, 80, 0.82)',
                                        'rgba(55, 174, 128, 0.98)'
                                    );
                                },

                            borderColor:
                                function (
                                    context
                                ) {
                                    const value =
                                        Number(
                                            context.raw
                                        ) || 0;

                                    return value < 0
                                        ? '#A93530'
                                        : '#197757';
                                },

                            borderWidth:
                                1.4,

                            borderRadius:
                                10,

                            borderSkipped:
                                false,

                            maxBarThickness:
                                28,

                            minBarLength:
                                3,

                            hoverBorderWidth:
                                2
                        }
                    ]
                },

                options: {
                    responsive:
                        true,

                    maintainAspectRatio:
                        false,

                    devicePixelRatio:
                        getChartPixelRatio(),

                    indexAxis:
                        'y',

                    animation: {
                        duration:
                            950,
                        easing:
                            'easeOutQuart'
                    },

                    interaction: {
                        mode:
                            'nearest',
                        intersect:
                            false
                    },

                    layout: {
                        padding: {
                            top: 7,
                            right: 62,
                            bottom: 5,
                            left: 3
                        }
                    },

                    plugins: {
                        legend: {
                            display:
                                false
                        },

                        valueLabelPlugin: {
                            enabled:
                                true
                        },

                        tooltip: {
                            displayColors:
                                false,

                            backgroundColor:
                                'rgba(21, 29, 25, 0.98)',

                            titleColor:
                                '#FFFFFF',

                            bodyColor:
                                '#FFFFFF',

                            titleFont: {
                                size: 12,
                                weight:
                                    '600'
                            },

                            bodyFont: {
                                size: 13,
                                weight:
                                    '700'
                            },

                            padding:
                                13,

                            cornerRadius:
                                11,

                            callbacks: {
                                label:
                                    function (
                                        context
                                    ) {
                                        return formatSignedRsd(
                                            context.raw
                                        );
                                    }
                            }
                        }
                    },

                    scales: {
                        x: {
                            border: {
                                display:
                                    false
                            },

                            grid: {
                                color:
                                    function (
                                        context
                                    ) {
                                        if (
                                            context
                                                .tick
                                                .value ===
                                            0
                                        ) {
                                            return 'rgba(42, 58, 50, 0.62)';
                                        }

                                        return 'rgba(55, 70, 62, 0.20)';
                                    },

                                lineWidth:
                                    function (
                                        context
                                    ) {
                                        return context
                                            .tick
                                            .value ===
                                            0
                                            ? 1.6
                                            : 1.1;
                                    },

                                drawTicks:
                                    false
                            },

                            ticks: {
                                color:
                                    '#56615B',

                                padding:
                                    7,

                                font: {
                                    size: 11,
                                    weight:
                                        '600'
                                },

                                callback:
                                    function (
                                        value
                                    ) {
                                        return formatShortNumber(
                                            value
                                        );
                                    }
                            }
                        },

                        y: {
                            border: {
                                display:
                                    false
                            },

                            grid: {
                                display:
                                    false
                            },

                            ticks: {
                                color:
                                    '#344039',

                                padding:
                                    9,

                                autoSkip:
                                    false,

                                font: {
                                    size: 11,
                                    weight:
                                        '700'
                                },

                                callback:
                                    function (
                                        value,
                                        index
                                    ) {
                                        const label =
                                            labels[index] ||
                                            '';

                                        if (
                                            label.length >
                                            24
                                        ) {
                                            return (
                                                label.substring(
                                                    0,
                                                    22
                                                ) +
                                                '…'
                                            );
                                        }

                                        return label;
                                    }
                            }
                        }
                    }
                },

                plugins: [
                    valueLabelPlugin
                ]
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

function renderRucFromData() {
    try {
        const ruc =
            window
                .__dashboardData
                ?.rucChange;

        if (
            !ruc ||
            typeof Chart ===
            'undefined'
        ) {
            return;
        }

        const canvas =
            document.getElementById(
                'rucWaterfall'
            );

        if (!canvas) {
            return;
        }

        const pocetni =
            Number(
                ruc.PocetniRuc ??
                ruc.pocetniRuc ??
                0
            );

        const margin =
            Number(
                ruc.MarginEffect ??
                ruc.marginEffect ??
                0
            );

        const volume =
            Number(
                ruc.VolumeEffect ??
                ruc.volumeEffect ??
                0
            );

        const mix =
            Number(
                ruc.MixEffect ??
                ruc.mixEffect ??
                0
            );

        const konacni =
            Number(
                ruc.KonacniRuc ??
                ruc.konacniRuc ??
                0
            );

        const posleMargin =
            pocetni +
            margin;

        const posleVolume =
            posleMargin +
            volume;

        const posleMix =
            posleVolume +
            mix;

        const labels = [
            'Plan RUC',
            'Margin effect',
            'Volume effect',
            'Mix effect',
            'Actual RUC'
        ];

        const floatingValues = [
            [
                Math.min(
                    0,
                    pocetni
                ),
                Math.max(
                    0,
                    pocetni
                )
            ],

            [
                Math.min(
                    pocetni,
                    posleMargin
                ),
                Math.max(
                    pocetni,
                    posleMargin
                )
            ],

            [
                Math.min(
                    posleMargin,
                    posleVolume
                ),
                Math.max(
                    posleMargin,
                    posleVolume
                )
            ],

            [
                Math.min(
                    posleVolume,
                    posleMix
                ),
                Math.max(
                    posleVolume,
                    posleMix
                )
            ],

            [
                Math.min(
                    0,
                    konacni
                ),
                Math.max(
                    0,
                    konacni
                )
            ]
        ];

        const effects = [
            pocetni,
            margin,
            volume,
            mix,
            konacni
        ];

        const connectorPlugin = {
            id:
                'waterfallConnectors',

            afterDatasetsDraw(
                chart
            ) {
                const meta =
                    chart
                        .getDatasetMeta(
                            0
                        );

                const ctx =
                    chart.ctx;

                const levels = [
                    pocetni,
                    posleMargin,
                    posleVolume,
                    posleMix
                ];

                ctx.save();

                ctx.strokeStyle =
                    'rgba(44, 60, 52, 0.92)';

                ctx.lineWidth =
                    1.8;

                ctx.setLineDash([
                    5,
                    3
                ]);

                for (
                    let i = 0;
                    i <
                    meta.data.length -
                    1;
                    i++
                ) {
                    const current =
                        meta.data[i];

                    const next =
                        meta.data[
                        i + 1
                        ];

                    if (
                        !current ||
                        !next
                    ) {
                        continue;
                    }

                    const y =
                        chart
                            .scales
                            .y
                            .getPixelForValue(
                                levels[i]
                            );

                    ctx.beginPath();

                    ctx.moveTo(
                        current.x +
                        current.width /
                        2,
                        y
                    );

                    ctx.lineTo(
                        next.x -
                        next.width /
                        2,
                        y
                    );

                    ctx.stroke();
                }

                ctx.restore();
            }
        };

        const labelPlugin = {
            id:
                'waterfallLabels',

            afterDatasetsDraw(
                chart
            ) {
                const meta =
                    chart
                        .getDatasetMeta(
                            0
                        );

                const ctx =
                    chart.ctx;

                ctx.save();

                ctx.font =
                    '700 10.5px Inter, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif';

                ctx.textAlign =
                    'center';

                meta.data.forEach(
                    function (
                        bar,
                        index
                    ) {
                        const value =
                            effects[
                            index
                            ];

                        let y;

                        if (
                            index === 0 ||
                            index === 4
                        ) {
                            y =
                                bar.y -
                                10;
                        }
                        else {
                            const start =
                                floatingValues[
                                index
                                ][0];

                            const end =
                                floatingValues[
                                index
                                ][1];

                            const positive =
                                value >= 0;

                            const target =
                                positive
                                    ? end
                                    : start;

                            y =
                                chart
                                    .scales
                                    .y
                                    .getPixelForValue(
                                        target
                                    );

                            y +=
                                positive
                                    ? -10
                                    : 16;
                        }

                        ctx.fillStyle =
                            index === 0
                                ? '#4C5851'
                                : index === 4
                                    ? '#155C43'
                                    : value >=
                                        0
                                        ? '#187653'
                                        : '#A93430';

                        const label =
                            index === 0 ||
                                index === 4
                                ? formatShortNumber(
                                    value
                                )
                                : formatSignedShortNumber(
                                    value
                                );

                        ctx.fillText(
                            label,
                            bar.x,
                            y
                        );
                    }
                );

                ctx.restore();
            }
        };

        new Chart(
            canvas,
            {
                type: 'bar',

                data: {
                    labels:
                        labels,

                    datasets: [
                        {
                            label:
                                'RUC promena',

                            data:
                                floatingValues,

                            backgroundColor:
                                function (
                                    context
                                ) {
                                    const index =
                                        context
                                            .dataIndex;

                                    if (
                                        index ===
                                        0
                                    ) {
                                        return createVerticalGradient(
                                            context,
                                            'rgba(89, 101, 95, 0.99)',
                                            'rgba(131, 143, 137, 0.64)'
                                        );
                                    }

                                    if (
                                        index ===
                                        4
                                    ) {
                                        return createVerticalGradient(
                                            context,
                                            'rgba(16, 101, 70, 1)',
                                            'rgba(46, 153, 107, 0.72)'
                                        );
                                    }

                                    const value =
                                        effects[
                                        index
                                        ];

                                    if (
                                        value >=
                                        0
                                    ) {
                                        return createVerticalGradient(
                                            context,
                                            'rgba(35, 139, 96, 0.99)',
                                            'rgba(76, 190, 140, 0.67)'
                                        );
                                    }

                                    return createVerticalGradient(
                                        context,
                                        'rgba(183, 48, 44, 0.99)',
                                        'rgba(224, 91, 79, 0.70)'
                                    );
                                },

                            borderColor:
                                function (
                                    context
                                ) {
                                    const index =
                                        context
                                            .dataIndex;

                                    if (
                                        index ===
                                        0
                                    ) {
                                        return '#59655F';
                                    }

                                    if (
                                        index ===
                                        4
                                    ) {
                                        return '#106546';
                                    }

                                    return effects[
                                        index
                                    ] >=
                                        0
                                        ? '#1D825B'
                                        : '#B43631';
                                },

                            borderWidth:
                                1.5,

                            borderRadius: {
                                topLeft:
                                    10,
                                topRight:
                                    10,
                                bottomLeft:
                                    5,
                                bottomRight:
                                    5
                            },

                            borderSkipped:
                                false,

                            maxBarThickness:
                                58,

                            hoverBorderWidth:
                                2.2
                        }
                    ]
                },

                options: {
                    responsive:
                        true,

                    maintainAspectRatio:
                        false,

                    devicePixelRatio:
                        getChartPixelRatio(),

                    animation: {
                        duration:
                            1050,
                        easing:
                            'easeOutQuart'
                    },

                    interaction: {
                        mode:
                            'nearest',
                        intersect:
                            false
                    },

                    layout: {
                        padding: {
                            top: 34,
                            right: 12,
                            bottom: 2,
                            left: 8
                        }
                    },

                    plugins: {
                        legend: {
                            display:
                                false
                        },

                        tooltip: {
                            displayColors:
                                false,

                            backgroundColor:
                                'rgba(21, 29, 25, 0.98)',

                            titleColor:
                                '#FFFFFF',

                            bodyColor:
                                '#FFFFFF',

                            footerColor:
                                '#BFC9C3',

                            titleFont: {
                                size: 12,
                                weight:
                                    '600'
                            },

                            bodyFont: {
                                size: 13,
                                weight:
                                    '700'
                            },

                            footerFont: {
                                size: 11,
                                weight:
                                    '500'
                            },

                            padding:
                                13,

                            cornerRadius:
                                11,

                            callbacks: {
                                label:
                                    function (
                                        context
                                    ) {
                                        const index =
                                            context
                                                .dataIndex;

                                        const value =
                                            effects[
                                            index
                                            ];

                                        if (
                                            index ===
                                            0 ||
                                            index ===
                                            4
                                        ) {
                                            return formatRsd(
                                                value
                                            );
                                        }

                                        return formatSignedRsd(
                                            value
                                        );
                                    },

                                footer:
                                    function (
                                        items
                                    ) {
                                        const index =
                                            items[0]
                                                .dataIndex;

                                        if (
                                            index ===
                                            1
                                        ) {
                                            return (
                                                'Novi nivo: ' +
                                                formatRsd(
                                                    posleMargin
                                                )
                                            );
                                        }

                                        if (
                                            index ===
                                            2
                                        ) {
                                            return (
                                                'Novi nivo: ' +
                                                formatRsd(
                                                    posleVolume
                                                )
                                            );
                                        }

                                        if (
                                            index ===
                                            3
                                        ) {
                                            return (
                                                'Novi nivo: ' +
                                                formatRsd(
                                                    posleMix
                                                )
                                            );
                                        }

                                        return '';
                                    }
                            }
                        }
                    },

                    scales: {
                        x: {
                            border: {
                                display:
                                    false
                            },

                            grid: {
                                display:
                                    false
                            },

                            ticks: {
                                color:
                                    '#344039',

                                padding:
                                    9,

                                font: {
                                    size: 11,
                                    weight:
                                        '700'
                                }
                            }
                        },

                        y: {
                            beginAtZero:
                                false,

                            border: {
                                display:
                                    false
                            },

                            grid: {
                                color:
                                    function (
                                        context
                                    ) {
                                        if (
                                            context
                                                .tick
                                                .value ===
                                            0
                                        ) {
                                            return 'rgba(36, 52, 44, 0.62)';
                                        }

                                        return 'rgba(55, 70, 62, 0.22)';
                                    },

                                lineWidth:
                                    function (
                                        context
                                    ) {
                                        return context
                                            .tick
                                            .value ===
                                            0
                                            ? 1.7
                                            : 1.15;
                                    },

                                drawTicks:
                                    false
                            },

                            ticks: {
                                color:
                                    '#53605A',

                                padding:
                                    9,

                                font: {
                                    size: 11,
                                    weight:
                                        '600'
                                },

                                callback:
                                    function (
                                        value
                                    ) {
                                        return formatShortNumber(
                                            value
                                        );
                                    }
                            }
                        }
                    }
                },

                plugins: [
                    connectorPlugin,
                    labelPlugin
                ]
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

function formatRsd(
    value
) {
    const number =
        Number(value) ||
        0;

    return new Intl.NumberFormat(
        'sr-RS',
        {
            maximumFractionDigits:
                0
        }
    ).format(number) +
        ' RSD';
}

function formatSignedRsd(
    value
) {
    const number =
        Number(value) ||
        0;

    const sign =
        number > 0
            ? '+'
            : '';

    return (
        sign +
        new Intl.NumberFormat(
            'sr-RS',
            {
                maximumFractionDigits:
                    0
            }
        ).format(
            number
        ) +
        ' RSD'
    );
}

function formatShortNumber(
    value
) {
    const number =
        Number(value) ||
        0;

    const absolute =
        Math.abs(
            number
        );

    if (
        absolute >=
        1000000000
    ) {
        return (
            number /
            1000000000
        ).toFixed(1) +
            'B';
    }

    if (
        absolute >=
        1000000
    ) {
        return (
            number /
            1000000
        ).toFixed(1) +
            'M';
    }

    if (
        absolute >=
        1000
    ) {
        return (
            number /
            1000
        ).toFixed(0) +
            'K';
    }

    return number
        .toFixed(0);
}

function formatSignedShortNumber(
    value
) {
    const number =
        Number(value) ||
        0;

    const sign =
        number > 0
            ? '+'
            : '';

    return (
        sign +
        formatShortNumber(
            number
        )
    );
}