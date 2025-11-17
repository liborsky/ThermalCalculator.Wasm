using Microsoft.JSInterop;
using ThermalCalculator.Wasm.Models;
using System.Text.Json;

namespace ThermalCalculator.Wasm.Services
{
    /// <summary>
    /// Service pro export PDF reportů pomocí jsPDF v Blazor WebAssembly
    /// Nahrazuje iText7 použité v Blazor Server verzi
    /// </summary>
    public class PdfExportService
    {
        private readonly IJSRuntime _jsRuntime;

        public PdfExportService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        /// <summary>
        /// Generuje PDF report tepelného výpočtu konstrukce
        /// </summary>
        public async Task<bool> GenerateThermalReportAsync(WallAssembly assembly, string projectName = "Tepelný výpočet")
        {
            try
            {
                if (assembly == null)
                    throw new ArgumentNullException(nameof(assembly), "WallAssembly nemůže být null");

                if (assembly.Layers == null || !assembly.Layers.Any())
                    throw new ArgumentException("WallAssembly musí obsahovat alespoň jednu vrstvu", nameof(assembly));

                // Připravíme data pro JavaScript
                var assemblyData = new
                {
                    layers = assembly.Layers.Select(l => new
                    {
                        materialName = l.Material?.Name ?? "Neznámý materiál",
                        thickness = l.Thickness,
                        lambda = l.Material?.ThermalConductivity ?? 0,
                        resistance = l.ThermalResistance
                    }).ToList(),
                    totalThickness = assembly.Layers.Sum(l => l.Thickness),
                    totalResistance = assembly.TotalThermalResistance,
                    uValue = 1.0 / (assembly.InternalSurfaceResistance + assembly.TotalThermalResistance + assembly.ExternalSurfaceResistance),
                    dewPointAnalysis = (object?)null
                };

                // Volání JavaScript funkce pro generování PDF
                var result = await _jsRuntime.InvokeAsync<bool>(
                    "pdfGenerator.generateThermalReport",
                    assemblyData,
                    projectName
                );

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba při generování PDF: {ex.Message}");
                throw new InvalidOperationException($"Chyba při exportu do PDF: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Synchronní wrapper pro GenerateThermalReportAsync (zachování kompatibility)
        /// POZNÁMKA: V Blazor WebAssembly preferujte async metody
        /// </summary>
        [Obsolete("Použijte async verzi GenerateThermalReportAsync()")]
        public byte[] GenerateThermalReport(WallAssembly assembly, string projectName = "Tepelný výpočet")
        {
            // V WebAssembly nelze použít synchronní JS interop
            // Vracíme prázdné pole - volající kód musí být upraven na async
            throw new NotSupportedException(
                "Synchronní generování PDF není podporováno v Blazor WebAssembly. " +
                "Použijte async metodu GenerateThermalReportAsync()."
            );
        }
    }
}
