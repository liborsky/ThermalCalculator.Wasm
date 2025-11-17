using ThermalCalculator.Wasm.Models;

namespace ThermalCalculator.Wasm.Services;

public static class ValidationHelper
{
    // České normy ČSN 73 0540-2 - požadavky na tepelný odpor
    public static class CzechStandards
    {
        // Požadované hodnoty U [W/(m²·K)]
        public const double RequiredU = 0.30; // Požadovaná hodnota dle ČSN
        public const double RecommendedU = 0.25; // Doporučená hodnota dle ČSN
        public const double PassiveHouseU = 0.15; // Pasivní dům
        public const double NearlyZeroEnergyU = 0.20; // Téměř nulová energetická spotřeba
        
        // Požadované hodnoty pro různé typy konstrukcí
        public const double ExternalWallU = 0.30;
        public const double RoofU = 0.24;
        public const double FloorU = 0.45;
        public const double WindowU = 1.50;
        
        // Minimální tloušťky izolace [mm]
        public const int MinInsulationThickness = 50;
        public const int RecommendedInsulationThickness = 100;
        public const int PassiveHouseInsulationThickness = 200;
    }
    
    // Fyzikální limity materiálů
    public static class PhysicalLimits
    {
        // Tloušťka vrstev [mm]
        public const int MinLayerThickness = 1;
        public const int MaxLayerThickness = 1000;
        public const int MinInsulationLayer = 20;
        public const int MaxStructuralLayer = 500;
        
        // Tepelná vodivost [W/(m·K)]
        public const double MaxInsulationLambda = 0.065; // Nad touto hodnotou to už není izolace
        public const double MinStructuralLambda = 0.08; // Pod touto hodnotou to není nosný materiál
        
        // Difuzní odpor
        public const double MaxDiffusionResistance = 20.0; // Kritický limit paropropustnosti
        public const double RecommendedMaxDiffusion = 10.0; // Doporučený limit
        
        // Hustota [kg/m³]
        public const double MinDensity = 10;
        public const double MaxDensity = 3000;
    }
    
    // Validační výsledky
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.Info;
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
        public ValidationCategory Category { get; set; } = ValidationCategory.General;
    }
    
    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
    
    public enum ValidationCategory
    {
        General,
        CzechStandards,
        PhysicalLimits,
        MaterialCombination,
        Economic,
        Condensation
    }
    
    // Hlavní validační metody
    public static List<ValidationResult> ValidateWallAssembly(WallAssembly assembly)
    {
        var results = new List<ValidationResult>();
        
        if (assembly.Layers.Count == 0)
        {
            results.Add(new ValidationResult
            {
                IsValid = false,
                Severity = ValidationSeverity.Warning,
                Message = "Skladba neobsahuje žádné vrstvy",
                Category = ValidationCategory.General
            });
            return results;
        }
        
        // Kontrola U-hodnoty podle ČSN
        results.AddRange(ValidateUValue(assembly.ThermalTransmittance));
        
        // Kontrola jednotlivých vrstev
        results.AddRange(ValidateLayers(assembly.Layers));
        
        // Kontrola kombinací materiálů
        results.AddRange(ValidateMaterialCombinations(assembly.Layers));
        
        // Kontrola difuzní propustnosti
        results.AddRange(ValidateDiffusionProperties(assembly));
        
        // Ekonomická analýza
        results.AddRange(ValidateEconomicEfficiency(assembly));
        
        return results;
    }
    
    public static List<ValidationResult> ValidateUValue(double uValue)
    {
        var results = new List<ValidationResult>();
        
        if (uValue > CzechStandards.RequiredU)
        {
            results.Add(new ValidationResult
            {
                IsValid = false,
                Severity = ValidationSeverity.Error,
                Message = $"Konstrukce nesplňuje požadavky ČSN 73 0540-2",
                Details = $"U = {uValue:F3} W/(m²·K) > {CzechStandards.RequiredU:F2} W/(m²·K)",
                Suggestion = "Zvyšte tloušťku izolace nebo použijte materiál s nižší tepelnou vodivostí",
                Category = ValidationCategory.CzechStandards
            });
        }
        else if (uValue > CzechStandards.RecommendedU)
        {
            results.Add(new ValidationResult
            {
                Severity = ValidationSeverity.Warning,
                Message = "Konstrukce splňuje požadavky, ale nesplňuje doporučené hodnoty ČSN",
                Details = $"U = {uValue:F3} W/(m²·K) > {CzechStandards.RecommendedU:F2} W/(m²·K)",
                Suggestion = "Pro lepší energetickou efektivitu zvažte zvýšení izolace",
                Category = ValidationCategory.CzechStandards
            });
        }
        else if (uValue <= CzechStandards.PassiveHouseU)
        {
            results.Add(new ValidationResult
            {
                Severity = ValidationSeverity.Info,
                Message = "Výborné! Konstrukce splňuje standard pasivního domu",
                Details = $"U = {uValue:F3} W/(m²·K) ≤ {CzechStandards.PassiveHouseU:F2} W/(m²·K)",
                Category = ValidationCategory.CzechStandards
            });
        }
        
        return results;
    }
    
    public static List<ValidationResult> ValidateLayerThickness(WallLayer layer)
    {
        var results = new List<ValidationResult>();
        
        if (layer.Thickness < PhysicalLimits.MinLayerThickness)
        {
            results.Add(new ValidationResult
            {
                IsValid = false,
                Severity = ValidationSeverity.Error,
                Message = $"Vrstva '{layer.Material.Name}' je příliš tenká",
                Details = $"Tloušťka {layer.Thickness} mm < minimální {PhysicalLimits.MinLayerThickness} mm",
                Category = ValidationCategory.PhysicalLimits
            });
        }
        else if (layer.Thickness > PhysicalLimits.MaxLayerThickness)
        {
            results.Add(new ValidationResult
            {
                IsValid = false,
                Severity = ValidationSeverity.Error,
                Message = $"Vrstva '{layer.Material.Name}' je příliš tlustá",
                Details = $"Tloušťka {layer.Thickness} mm > maximální {PhysicalLimits.MaxLayerThickness} mm",
                Category = ValidationCategory.PhysicalLimits
            });
        }
        
        // Specifické kontroly podle kategorie materiálu
        if (layer.Material.Category == "Izolace")
        {
            if (layer.Thickness < PhysicalLimits.MinInsulationLayer)
            {
                results.Add(new ValidationResult
                {
                    Severity = ValidationSeverity.Warning,
                    Message = $"Tenká vrstva izolace '{layer.Material.Name}'",
                    Details = $"Tloušťka {layer.Thickness} mm < doporučené minimum {PhysicalLimits.MinInsulationLayer} mm",
                    Suggestion = "Zvažte zvýšení tloušťky izolace pro lepší účinnost",
                    Category = ValidationCategory.PhysicalLimits
                });
            }
        }
        
        return results;
    }
    
    private static List<ValidationResult> ValidateLayers(List<WallLayer> layers)
    {
        var results = new List<ValidationResult>();
        
        foreach (var layer in layers)
        {
            results.AddRange(ValidateLayerThickness(layer));
        }
        
        return results;
    }
    
    private static List<ValidationResult> ValidateMaterialCombinations(List<WallLayer> layers)
    {
        var results = new List<ValidationResult>();
        
        // Kontrola pozice parozábrany
        var vaporBarriers = layers.Where(l => l.Material.DiffusionResistanceFactor > 100).ToList();
        if (vaporBarriers.Count > 1)
        {
            results.Add(new ValidationResult
            {
                Severity = ValidationSeverity.Warning,
                Message = "Více parozábran v konstrukci",
                Details = "Více vrstev s vysokým difuzním odporem může způsobit problémy",
                Suggestion = "Použijte pouze jednu parozábranu na vnitřní straně",
                Category = ValidationCategory.MaterialCombination
            });
        }
        
        // Kontrola pozice izolace
        var insulationLayers = layers.Where(l => l.Material.Category == "Izolace").ToList();
        if (insulationLayers.Count == 0)
        {
            results.Add(new ValidationResult
            {
                Severity = ValidationSeverity.Error,
                Message = "Konstrukce neobsahuje tepelnou izolaci",
                Suggestion = "Přidejte vrstvu tepelné izolace",
                Category = ValidationCategory.MaterialCombination
            });
        }
        
        return results;
    }
    
    private static List<ValidationResult> ValidateDiffusionProperties(WallAssembly assembly)
    {
        var results = new List<ValidationResult>();
        
        var totalDiffusion = assembly.TotalDiffusionResistance;
        
        if (totalDiffusion > PhysicalLimits.MaxDiffusionResistance)
        {
            results.Add(new ValidationResult
            {
                Severity = ValidationSeverity.Warning,
                Message = "Vysoký celkový difuzní odpor",
                Details = $"μd = {totalDiffusion:F1} m > {PhysicalLimits.MaxDiffusionResistance:F0} m",
                Suggestion = "Zvažte použití paropropustnějších materiálů",
                Category = ValidationCategory.Condensation
            });
        }
        
        return results;
    }
    
    private static List<ValidationResult> ValidateEconomicEfficiency(WallAssembly assembly)
    {
        var results = new List<ValidationResult>();
        
        // Kontrola předimenzované izolace
        if (assembly.ThermalTransmittance < 0.10)
        {
            var cost = assembly.TotalCost;
            results.Add(new ValidationResult
            {
                Severity = ValidationSeverity.Info,
                Message = "Možná předimenzovaná izolace",
                Details = $"U = {assembly.ThermalTransmittance:F3} W/(m²·K), náklady {cost:F0} Kč/m²",
                Suggestion = "Zvažte optimalizaci mezi náklady a úsporami energie",
                Category = ValidationCategory.Economic
            });
        }
        
        return results;
    }
    
    // Pomocné metody pro získání CSS tříd podle severity
    public static string GetSeverityClass(ValidationSeverity severity)
    {
        return severity switch
        {
            ValidationSeverity.Info => "text-info",
            ValidationSeverity.Warning => "text-warning",
            ValidationSeverity.Error => "text-danger",
            ValidationSeverity.Critical => "text-danger fw-bold",
            _ => "text-muted"
        };
    }
    
    public static string GetSeverityIcon(ValidationSeverity severity)
    {
        return severity switch
        {
            ValidationSeverity.Info => "bi-info-circle",
            ValidationSeverity.Warning => "bi-exclamation-triangle",
            ValidationSeverity.Error => "bi-x-circle",
            ValidationSeverity.Critical => "bi-x-octagon",
            _ => "bi-question-circle"
        };
    }
}