// Funkce pro práci s localStorage
window.saveToLocalStorage = function (key, value) {
    localStorage.setItem(key, value);
};

window.loadFromLocalStorage = function (key) {
    try {
        var value = localStorage.getItem(key);
        console.log('localStorage.getItem(' + key + ') returned:', value, 'type:', typeof value);

        // Vrátíme null pokud hodnota neexistuje nebo je prázdná
        if (value === null || value === "null" || value === "") {
            console.log('Returning null for key:', key);
            return null;
        }

        console.log('Returning value for key:', key, 'value:', value);
        return value;
    } catch (error) {
        console.error('Error in loadFromLocalStorage:', error);
        return null;
    }
};

// Funkce pro stažení souboru
window.downloadFile = function (base64String, contentType, fileName) {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = `data:${contentType};base64,${base64String}`;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Funkce pro zobrazení modalu
window.showModal = function (modalId, content) {
    // Získat nebo vytvořit modal
    let modal = document.getElementById(modalId);
    if (!modal) {
        modal = document.createElement('div');
        modal.id = modalId;
        modal.className = 'modal fade';
        modal.setAttribute('tabindex', '-1');
        modal.setAttribute('aria-hidden', 'true');
        document.body.appendChild(modal);
    }

    // Nastavit HTML obsah s bílým pozadím
    modal.innerHTML = `
        <div class="modal-dialog modal-dialog-scrollable modal-lg">
            <div class="modal-content" style="background-color: white;">
                ${content}
            </div>
        </div>
    `;

    // Zobrazit modal
    const bootstrapModal = new bootstrap.Modal(modal);
    bootstrapModal.show();
};

// Funkce pro vytvoření grafů
window.createChart = function (canvasId, config) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.error(`Canvas s ID '${canvasId}' nebyl nalezen`);
        return;
    }

    // Zničení existujícího grafu
    if (window.charts && window.charts[canvasId]) {
        window.charts[canvasId].destroy();
    }

    // Inicializace objektu pro grafy
    if (!window.charts) {
        window.charts = {};
    }

    // Vytvoření nového grafu
    const ctx = canvas.getContext('2d');
    window.charts[canvasId] = new Chart(ctx, config);
};

// Funkce pro prompt (náhrada za window.prompt)
window.blazorPrompt = function (message, defaultValue) {
    return new Promise((resolve) => {
        const modal = document.createElement('div');
        modal.className = 'modal fade';
        modal.setAttribute('tabindex', '-1');
        modal.setAttribute('aria-labelledby', 'promptModalLabel');
        modal.innerHTML = `
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="promptModalLabel">Zadejte název</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Zavřít"></button>
                    </div>
                    <div class="modal-body">
                        <label class="form-label">${message}</label>
                        <input type="text" class="form-control" id="promptInput" value="${defaultValue || ''}" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Zrušit</button>
                        <button type="button" class="btn btn-primary" onclick="window.confirmPrompt()">OK</button>
                    </div>
                </div>
            </div>
        `;
        document.body.appendChild(modal);

        const bootstrapModal = new bootstrap.Modal(modal);

        // Event listener pro opravu aria-hidden
        modal.addEventListener('shown.bs.modal', function () {
            modal.removeAttribute('aria-hidden');
            // Focus na input
            const input = document.getElementById('promptInput');
            if (input) {
                input.focus();
                input.select();
            }
        });

        modal.addEventListener('hidden.bs.modal', function () {
            modal.setAttribute('aria-hidden', 'true');
        });

        bootstrapModal.show();

        // Funkce pro potvrzení
        window.confirmPrompt = function () {
            const input = document.getElementById('promptInput');
            const value = input ? input.value : '';
            bootstrapModal.hide();
            document.body.removeChild(modal);
            resolve(value);
        };

        // Funkce pro zrušení
        modal.addEventListener('hidden.bs.modal', function () {
            document.body.removeChild(modal);
            resolve(null);
        });
    });
};

// Funkce pro alert (náhrada za window.alert)
window.blazorAlert = function (message) {
    return new Promise((resolve) => {
        const modal = document.createElement('div');
        modal.className = 'modal fade';
        modal.setAttribute('tabindex', '-1');
        modal.setAttribute('aria-labelledby', 'alertModalLabel');

        // Převést \n na <br> pro správné zobrazení nových řádků
        const formattedMessage = message.replace(/\n/g, '<br>');

        modal.innerHTML = `
            <div class="modal-dialog modal-dialog-scrollable" style="max-width: 650px;">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="alertModalLabel">Informace</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Zavřít"></button>
                    </div>
                    <div class="modal-body" style="white-space: pre-line; font-size: 0.9em; line-height: 1.5;">
                        ${formattedMessage}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-primary" data-bs-dismiss="modal">OK</button>
                    </div>
                </div>
            </div>
        `;
        document.body.appendChild(modal);

        const bootstrapModal = new bootstrap.Modal(modal);

        // Event listener pro opravu aria-hidden
        modal.addEventListener('shown.bs.modal', function () {
            modal.removeAttribute('aria-hidden');
        });

        modal.addEventListener('hidden.bs.modal', function () {
            modal.setAttribute('aria-hidden', 'true');
            document.body.removeChild(modal);
            resolve();
        });

        bootstrapModal.show();
    });
};

// Funkce pro bezpečné logování chyb (pro debugging)
window.safeConsoleError = function (message) {
    if (window.console && window.console.log) {
        console.log('ERROR:', message);
    }
};

// Funkce pro vyčištění poškozeného localStorage
window.clearLocalStorageIfCorrupted = function () {
    try {
        var keys = ['thermal_calculator_assemblies', 'custom_materials'];
        keys.forEach(function(key) {
            var value = localStorage.getItem(key);
            if (value && value !== 'null' && value !== '') {
                try {
                    JSON.parse(value);
                } catch (e) {
                    console.warn('Removing corrupted localStorage key:', key, 'value was:', value);
                    localStorage.removeItem(key);
                }
            }
        });
    } catch (error) {
        console.error('Error clearing localStorage:', error);
    }
};


// Funkce pro nastavení Blazor reference
window.setBlazorReference = function (dotNetRef) {
    window._blazorRef = dotNetRef;

    // Nastavit funkci pro načítání skladeb
    window.loadAssemblyInstance = function(name) {
        if (window._blazorRef) {
            window._blazorRef.invokeMethodAsync('LoadAssemblyByName', name);
        }
    };
};

// === COMBINED PAYBACK CHART ===
window.renderCombinedPaybackChart = function(chartData, annotations) {
    const canvasId = 'combinedPaybackChart';
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.error('Canvas element not found:', canvasId);
        return;
    }

    // Zničit existující graf
    if (window.combinedPaybackChartInstance) {
        window.combinedPaybackChartInstance.destroy();
    }

    const ctx = canvas.getContext('2d');
    window.combinedPaybackChartInstance = new Chart(ctx, {
        type: 'line',
        data: chartData,
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: true,
                    position: 'top',
                    labels: {
                        font: {
                            size: 14
                        },
                        padding: 20,
                        usePointStyle: true
                    }
                },
                title: {
                    display: true,
                    text: 'Analýza návratnosti investice do zateplení',
                    font: {
                        size: 17,
                        weight: 'bold'
                    },
                    padding: {
                        top: 10,
                        bottom: 25
                    }
                },
                tooltip: {
                    mode: 'index',
                    intersect: false,
                    backgroundColor: 'rgba(0, 0, 0, 0.9)',
                    padding: 15,
                    titleFont: {
                        size: 15,
                        weight: 'bold'
                    },
                    bodyFont: {
                        size: 14
                    },
                    callbacks: {
                        title: function(context) {
                            return 'Tloušťka: ' + context[0].parsed.x + ' cm';
                        },
                        label: function(context) {
                            const datasetLabel = context.dataset.label;
                            const value = context.parsed.y;

                            if (datasetLabel.includes('Celková')) {
                                return 'Celková návratnost: ' + value.toFixed(1) + ' let';
                            } else if (datasetLabel.includes('Přírůstková')) {
                                return 'Návratnost +1 cm: ' + value.toFixed(1) + ' let';
                            } else if (datasetLabel.includes('Čistý zisk')) {
                                return 'Čistý zisk: ' + value.toLocaleString('cs-CZ') + ' Kč';
                            } else if (datasetLabel.includes('ROI')) {
                                return 'ROI: ' + value.toFixed(0) + '%';
                            }
                            return context.dataset.label + ': ' + value;
                        },
                        afterBody: function(context) {
                            const lines = [];
                            context.forEach(item => {
                                const value = item.parsed.y;
                                const datasetLabel = item.dataset.label;

                                if (datasetLabel.includes('Celková')) {
                                    if (value < 5) {
                                        lines.push('\n✅ Velmi rychlá návratnost');
                                    } else if (value < 10) {
                                        lines.push('\n✅ Dobrá návratnost');
                                    } else if (value < 20) {
                                        lines.push('\n⚠️ Pomalejší návratnost');
                                    } else {
                                        lines.push('\n❌ Nevýhodná investice');
                                    }
                                } else if (datasetLabel.includes('Přírůstková')) {
                                    if (value < 5) {
                                        lines.push('\n✅ Přidání dalšího cm se velmi vyplatí');
                                    } else if (value < 10) {
                                        lines.push('\n✅ Přidání dalšího cm se vyplatí');
                                    } else if (value < 20) {
                                        lines.push('\n⚠️ Přidání dalšího cm má nižší efekt');
                                    } else {
                                        lines.push('\n❌ Další izolace už je neefektivní');
                                    }
                                } else if (datasetLabel.includes('Čistý zisk')) {
                                    if (value > 100000) {
                                        lines.push('\n✅ Výborný zisk za celou životnost');
                                    } else if (value > 50000) {
                                        lines.push('\n✅ Dobrý zisk za celou životnost');
                                    } else if (value > 0) {
                                        lines.push('\n⚠️ Nízký zisk');
                                    } else {
                                        lines.push('\n❌ Ztráta - investice se nevyplatí');
                                    }
                                } else if (datasetLabel.includes('ROI')) {
                                    if (value > 200) {
                                        lines.push('\n✅ Výjimečná návratnost (3x investice)');
                                    } else if (value > 100) {
                                        lines.push('\n✅ Výborná návratnost (2x investice)');
                                    } else if (value > 50) {
                                        lines.push('\n✅ Dobrá návratnost');
                                    } else if (value > 0) {
                                        lines.push('\n⚠️ Nízká návratnost');
                                    } else {
                                        lines.push('\n❌ Ztráta - investice se nevyplatí');
                                    }
                                }
                            });
                            return lines.join('');
                        }
                    }
                },
                annotation: {
                    annotations: annotations
                }
            },
            scales: {
                x: {
                    title: {
                        display: true,
                        text: 'Tloušťka izolace [cm]',
                        font: {
                            size: 14,
                            weight: 'bold'
                        }
                    },
                    grid: {
                        display: true,
                        color: 'rgba(0, 0, 0, 0.06)'
                    }
                },
                y: {
                    type: 'linear',
                    position: 'left',
                    title: {
                        display: true,
                        text: 'Doba návratnosti [roky]',
                        font: {
                            size: 14,
                            weight: 'bold'
                        }
                    },
                    beginAtZero: true,
                    grid: {
                        display: true,
                        color: 'rgba(0, 0, 0, 0.06)'
                    }
                },
                y1: {
                    type: 'linear',
                    position: 'right',
                    title: {
                        display: true,
                        text: 'Čistý zisk [Kč]',
                        font: {
                            size: 14,
                            weight: 'bold'
                        }
                    },
                    grid: {
                        display: false
                    },
                    ticks: {
                        callback: function(value) {
                            return value.toLocaleString('cs-CZ') + ' Kč';
                        }
                    }
                },
                y2: {
                    type: 'linear',
                    position: 'right',
                    title: {
                        display: true,
                        text: 'ROI [%]',
                        font: {
                            size: 14,
                            weight: 'bold'
                        }
                    },
                    grid: {
                        display: false
                    },
                    ticks: {
                        callback: function(value) {
                            return value.toFixed(0) + '%';
                        }
                    }
                }
            },
            interaction: {
                mode: 'nearest',
                axis: 'x',
                intersect: false
            }
        }
    });
};

// === DRAG & DROP FUNKCIONALITA ===

// Globální proměnné pro drag & drop
window.dragState = {
    draggedIndex: -1,
    dropZoneIndex: -1,
    blazorRef: null
};

// Inicializace drag & drop pro kontejner vrstev
window.initDragDrop = function(blazorRef) {
    window.dragState.blazorRef = blazorRef;
    console.log('Drag & Drop initialized');
};

// Začátek přetahování
window.onDragStart = function(event, index) {
    console.log('onDragStart called with index:', index);

    window.dragState.draggedIndex = index;

    if (event && event.dataTransfer) {
        event.dataTransfer.effectAllowed = 'move';
        event.dataTransfer.setData('text/plain', index.toString());
    }

    // Přidat CSS třídu pro vizuální feedback
    const target = event.target.closest('.layer-row');
    if (target) {
        target.classList.add('dragging');
    }

    console.log('Drag started for layer:', index);
};

// Konec přetahování
window.onDragEnd = function(event) {
    const target = event.target.closest('.layer-row');
    if (target) {
        target.classList.remove('dragging');
    }

    // Vyčistit všechny drop zone indikátory
    const dropZones = document.querySelectorAll('.layer-row');
    dropZones.forEach(zone => {
        zone.classList.remove('drag-over', 'drop-zone-active');
    });

    window.dragState.draggedIndex = -1;
    window.dragState.dropZoneIndex = -1;

    console.log('Drag ended');
};

// Vstup do drop zóny
window.onDragEnter = function(event, index) {
    event.preventDefault();
    window.dragState.dropZoneIndex = index;

    const target = event.currentTarget;
    target.classList.add('drag-over');

    console.log('Drag enter zone:', index);
};

// Přes drop zónu
window.onDragOver = function(event, index) {
    event.preventDefault();
    event.dataTransfer.dropEffect = 'move';

    const target = event.currentTarget;
    target.classList.add('drop-zone-active');
};

// Opuštění drop zóny
window.onDragLeave = function(event, index) {
    const target = event.currentTarget;
    target.classList.remove('drag-over', 'drop-zone-active');
};

// Pustupení do drop zóny
window.onDrop = function(event, index) {
    event.preventDefault();

    const target = event.currentTarget;
    target.classList.remove('drag-over', 'drop-zone-active');

    const fromIndex = window.dragState.draggedIndex;
    const toIndex = index;

    console.log('Drop: moving layer from', fromIndex, 'to', toIndex);

    // Zavolat Blazor metodu pro přesunutí vrstvy
    if (window.dragState.blazorRef && fromIndex !== -1 && toIndex !== -1 && fromIndex !== toIndex) {
        window.dragState.blazorRef.invokeMethodAsync('ReorderLayers', fromIndex, toIndex)
            .then(() => {
                console.log('Layer reordered successfully');
            })
            .catch(error => {
                console.error('Error reordering layers:', error);
            });
    }
};

// Vyčistit poškozený localStorage při načtení stránky
document.addEventListener('DOMContentLoaded', function() {
    clearLocalStorageIfCorrupted();
});
