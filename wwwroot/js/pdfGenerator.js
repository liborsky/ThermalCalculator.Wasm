// PDF Generator using pdfMake for Blazor WebAssembly
// pdfMake has better Czech character support than jsPDF

window.pdfGenerator = {
    generateThermalReport: function (assemblyData, projectName) {
        try {
            const docDefinition = {
                pageSize: 'A4',
                pageMargins: [40, 60, 40, 60],
                content: [],
                styles: {
                    header: {
                        fontSize: 18,
                        bold: true,
                        margin: [0, 0, 0, 10]
                    },
                    subheader: {
                        fontSize: 14,
                        bold: true,
                        margin: [0, 10, 0, 5]
                    },
                    tableHeader: {
                        bold: true,
                        fontSize: 12,
                        color: 'black',
                        fillColor: '#eeeeee'
                    }
                },
                defaultStyle: {
                    font: 'Roboto'
                }
            };

            // Header
            docDefinition.content.push({
                text: projectName || 'Tepelný výpočet',
                style: 'header'
            });

            docDefinition.content.push({
                text: `Datum: ${new Date().toLocaleDateString('cs-CZ')}`,
                margin: [0, 0, 0, 20]
            });

            // Assembly layers
            docDefinition.content.push({
                text: 'Skladba konstrukce',
                style: 'subheader'
            });

            if (assemblyData.layers && assemblyData.layers.length > 0) {
                const layersTable = {
                    table: {
                        headerRows: 1,
                        widths: ['auto', '*', 'auto', 'auto'],
                        body: [
                            ['#', 'Materiál', 'Tloušťka [mm]', 'R [m²K/W]']
                        ]
                    },
                    layout: 'lightHorizontalLines',
                    margin: [0, 5, 0, 15]
                };

                assemblyData.layers.forEach((layer, index) => {
                    layersTable.table.body.push([
                        (index + 1).toString(),
                        layer.materialName,
                        layer.thickness.toString(),
                        layer.resistance.toFixed(3)
                    ]);
                });

                docDefinition.content.push(layersTable);
            }

            // Thermal properties
            docDefinition.content.push({
                text: 'Tepelně technické vlastnosti',
                style: 'subheader'
            });

            docDefinition.content.push({
                ul: [
                    `Celková tloušťka: ${assemblyData.totalThickness} mm`,
                    `Tepelný odpor R: ${assemblyData.totalResistance.toFixed(3)} m²K/W`,
                    `Součinitel prostupu tepla U: ${assemblyData.uValue.toFixed(3)} W/m²K`
                ],
                margin: [0, 5, 0, 15]
            });

            // Condensation analysis
            if (assemblyData.dewPointAnalysis) {
                docDefinition.content.push({
                    text: 'Analýza kondenzace',
                    style: 'subheader'
                });

                const hasCondensation = assemblyData.dewPointAnalysis.hasCondensation;
                const statusText = hasCondensation ? 'VAROVÁNÍ: Riziko kondenzace' : 'OK: Bez rizika kondenzace';

                docDefinition.content.push({
                    text: statusText,
                    bold: true,
                    color: hasCondensation ? 'red' : 'green',
                    margin: [0, 5, 0, 10]
                });

                if (hasCondensation && assemblyData.dewPointAnalysis.condensationZones) {
                    const zones = assemblyData.dewPointAnalysis.condensationZones.map(zone =>
                        `${zone.location}: ${zone.description}`
                    );

                    docDefinition.content.push({
                        text: 'Zóny kondenzace:',
                        bold: true,
                        margin: [0, 0, 0, 5]
                    });

                    docDefinition.content.push({
                        ul: zones
                    });
                }
            }

            // Footer
            docDefinition.footer = function(currentPage, pageCount) {
                return {
                    text: `Strana ${currentPage} z ${pageCount} | Vygenerováno aplikací Výpočet izolace`,
                    alignment: 'center',
                    fontSize: 8,
                    margin: [0, 10, 0, 0]
                };
            };

            // Generate and download
            const fileName = `${projectName || 'tepelny-vypocet'}.pdf`.replace(/ /g, '_');
            pdfMake.createPdf(docDefinition).download(fileName);

            return true;
        } catch (error) {
            console.error('Chyba při generování PDF:', error);
            return false;
        }
    },

    generateOptimizationReport: function (optimizationData, projectName) {
        try {
            const docDefinition = {
                pageSize: 'A4',
                pageMargins: [40, 60, 40, 60],
                content: [],
                styles: {
                    header: {
                        fontSize: 18,
                        bold: true,
                        margin: [0, 0, 0, 10]
                    },
                    subheader: {
                        fontSize: 14,
                        bold: true,
                        margin: [0, 10, 0, 5]
                    }
                },
                defaultStyle: {
                    font: 'Roboto'
                }
            };

            // Header
            docDefinition.content.push({
                text: projectName || 'Ekonomická optimalizace izolace',
                style: 'header'
            });

            docDefinition.content.push({
                text: `Datum: ${new Date().toLocaleDateString('cs-CZ')}`,
                margin: [0, 0, 0, 20]
            });

            // Input parameters
            if (optimizationData.parameters) {
                docDefinition.content.push({
                    text: 'Vstupní parametry',
                    style: 'subheader'
                });

                docDefinition.content.push({
                    ul: [
                        `Cena energie: ${optimizationData.parameters.energyPrice} Kč/kWh`,
                        `Cena izolace: ${optimizationData.parameters.insulationCost} Kč/m²/cm`,
                        `Životnost izolace: ${optimizationData.parameters.lifespan} let`,
                        `Míra inflace: ${optimizationData.parameters.inflation}%`
                    ],
                    margin: [0, 5, 0, 15]
                });
            }

            // Optimal thickness
            docDefinition.content.push({
                text: 'Výsledky optimalizace',
                style: 'subheader'
            });

            docDefinition.content.push({
                text: `Optimální tloušťka: ${optimizationData.optimalThickness} mm`,
                fontSize: 12,
                bold: true,
                color: '#0066cc',
                margin: [0, 5, 0, 15]
            });

            // Results table
            if (optimizationData.results && optimizationData.results.length > 0) {
                const resultsTable = {
                    table: {
                        headerRows: 1,
                        widths: ['auto', 'auto', 'auto', 'auto'],
                        body: [
                            ['Tloušťka [mm]', 'U-hodnota [W/m²K]', 'Úspora [Kč/rok]', 'NPV [Kč]']
                        ]
                    },
                    layout: 'lightHorizontalLines',
                    margin: [0, 5, 0, 0]
                };

                optimizationData.results.forEach(result => {
                    resultsTable.table.body.push([
                        result.thickness.toString(),
                        result.uValue.toFixed(3),
                        result.savings.toFixed(0),
                        {
                            text: result.npv.toFixed(0),
                            color: result.npv >= 0 ? 'green' : 'red'
                        }
                    ]);
                });

                docDefinition.content.push(resultsTable);
            }

            // Footer
            docDefinition.footer = function(currentPage, pageCount) {
                return {
                    text: `Strana ${currentPage} z ${pageCount} | Vygenerováno aplikací Výpočet izolace`,
                    alignment: 'center',
                    fontSize: 8,
                    margin: [0, 10, 0, 0]
                };
            };

            // Generate and download
            const fileName = `${projectName || 'optimalizace-izolace'}.pdf`.replace(/ /g, '_');
            pdfMake.createPdf(docDefinition).download(fileName);

            return true;
        } catch (error) {
            console.error('Chyba při generování PDF optimalizace:', error);
            return false;
        }
    }
};
