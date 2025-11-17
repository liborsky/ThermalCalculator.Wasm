using ThermalCalculator.Wasm.Models;

namespace ThermalCalculator.Wasm.Models;

// Lineární tepelný mostek podle ČSN EN ISO 14683
public class LinearThermalBridge
{
    public string Name { get; set; } = string.Empty;
    public BridgeType Type { get; set; }
    public double PsiValue { get; set; } // ψ [W/(mK)]
    public double Length { get; set; } = 1.0; // délka mostu [m]
    public string Description { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public WallConfiguration WallConfig { get; set; } = WallConfiguration.Insulated;
    
    // Tepelná ztráta mostku [W/K]
    public double HeatLoss => PsiValue * Length;
}

// Kolekce tepelných mostků pro konstrukci
public class ThermalBridgeCollection
{
    public List<LinearThermalBridge> Bridges { get; set; } = new();
    public double BuildingPerimeter { get; set; } = 40.0; // obvod budovy [m]
    public double FloorArea { get; set; } = 100.0; // podlahová plocha [m²]
    
    // Celková tepelná ztráta mostky [W/K]
    public double TotalBridgeHeatLoss => Bridges.Where(b => b.IsEnabled).Sum(b => b.HeatLoss);
    
    // Korekce U-hodnoty vlivem tepelných mostků [W/(m²K)]
    public double UThermalBridgeCorrection => TotalBridgeHeatLoss / FloorArea;
    
    // Přidá automaticky detekované typické mostky
    public void AddTypicalBridges(WallAssembly assembly)
    {
        var wallConfig = GetWallConfiguration(assembly);
        Bridges.Clear();
        
        // Automaticky přidej typické mostky podle konfigurace
        Bridges.AddRange(GetStandardBridges(wallConfig));
    }
    
    private WallConfiguration GetWallConfiguration(WallAssembly assembly)
    {
        var hasInsulation = assembly.Layers.Any(l => l.Material.Category == "Izolace");
        var maxInsulationThickness = assembly.Layers
            .Where(l => l.Material.Category == "Izolace")
            .DefaultIfEmpty()
            .Max(l => l?.Thickness ?? 0);
            
        return (hasInsulation, maxInsulationThickness) switch
        {
            (false, _) => WallConfiguration.Uninsulated,
            (true, >= 150) => WallConfiguration.ThickInsulation,
            (true, >= 80) => WallConfiguration.Insulated,
            _ => WallConfiguration.ThinInsulation
        };
    }
    
    private List<LinearThermalBridge> GetStandardBridges(WallConfiguration config)
    {
        var bridges = new List<LinearThermalBridge>();
        var catalog = ThermalBridgeCatalog.GetStandardCatalog();
        
        // Přidej typické mostky pro obvod budovy
        foreach (var bridgeType in new[] { BridgeType.ExternalCorner, BridgeType.Foundation, BridgeType.RoofConnection })
        {
            var bridgeData = catalog.GetBridgeData(bridgeType, config);
            if (bridgeData != null)
            {
                bridges.Add(new LinearThermalBridge
                {
                    Name = bridgeData.Name,
                    Type = bridgeType,
                    PsiValue = bridgeData.PsiValue,
                    Length = GetTypicalLength(bridgeType),
                    Description = bridgeData.Description,
                    WallConfig = config
                });
            }
        }
        
        return bridges;
    }
    
    private double GetTypicalLength(BridgeType type)
    {
        return type switch
        {
            BridgeType.ExternalCorner => BuildingPerimeter * 0.1, // cca 10% obvodu
            BridgeType.Foundation => BuildingPerimeter,
            BridgeType.RoofConnection => BuildingPerimeter,
            BridgeType.WindowSill => BuildingPerimeter * 0.3, // cca 30% obvodu
            BridgeType.WindowLintel => BuildingPerimeter * 0.3,
            BridgeType.Balcony => 10.0, // typická délka balkonu
            _ => 1.0
        };
    }
}

// Katalog standardních ψ hodnot podle ČSN EN ISO 14683
public class ThermalBridgeCatalog
{
    private static readonly Dictionary<(BridgeType, WallConfiguration), BridgeData> _catalog = new()
    {
        // Vnější rohy
        { (BridgeType.ExternalCorner, WallConfiguration.Uninsulated), new BridgeData("Vnější roh - neizolaná stěna", 0.20, "Typický roh bez izolace") },
        { (BridgeType.ExternalCorner, WallConfiguration.ThinInsulation), new BridgeData("Vnější roh - tenká izolace", 0.15, "Roh s izolací do 80mm") },
        { (BridgeType.ExternalCorner, WallConfiguration.Insulated), new BridgeData("Vnější roh - standardní izolace", 0.10, "Roh s izolací 80-150mm") },
        { (BridgeType.ExternalCorner, WallConfiguration.ThickInsulation), new BridgeData("Vnější roh - silná izolace", 0.05, "Roh s izolací nad 150mm") },
        
        // Základ
        { (BridgeType.Foundation, WallConfiguration.Uninsulated), new BridgeData("Základ - neizolaná stěna", 0.80, "Styč stěny se základem bez izolace") },
        { (BridgeType.Foundation, WallConfiguration.ThinInsulation), new BridgeData("Základ - tenká izolace", 0.65, "Styč s tenkou izolací") },
        { (BridgeType.Foundation, WallConfiguration.Insulated), new BridgeData("Základ - standardní izolace", 0.50, "Styč se standardní izolací") },
        { (BridgeType.Foundation, WallConfiguration.ThickInsulation), new BridgeData("Základ - silná izolace", 0.35, "Styč se silnou izolací") },
        
        // Střešní konstrukce
        { (BridgeType.RoofConnection, WallConfiguration.Uninsulated), new BridgeData("Atika - neizolaná", 0.60, "Atika bez izolace") },
        { (BridgeType.RoofConnection, WallConfiguration.ThinInsulation), new BridgeData("Atika - tenká izolace", 0.45, "Atika s tenkou izolací") },
        { (BridgeType.RoofConnection, WallConfiguration.Insulated), new BridgeData("Atika - standardní izolace", 0.30, "Atika se standardní izolací") },
        { (BridgeType.RoofConnection, WallConfiguration.ThickInsulation), new BridgeData("Atika - silná izolace", 0.20, "Atika se silnou izolací") },
        
        // Okenní otvory
        { (BridgeType.WindowSill, WallConfiguration.Uninsulated), new BridgeData("Parapet - neizolaná stěna", 0.30, "Parapet okna bez izolace") },
        { (BridgeType.WindowSill, WallConfiguration.ThinInsulation), new BridgeData("Parapet - tenká izolace", 0.25, "Parapet s tenkou izolací") },
        { (BridgeType.WindowSill, WallConfiguration.Insulated), new BridgeData("Parapet - standardní izolace", 0.20, "Parapet se standardní izolací") },
        { (BridgeType.WindowSill, WallConfiguration.ThickInsulation), new BridgeData("Parapet - silná izolace", 0.15, "Parapet se silnou izolací") },
        
        { (BridgeType.WindowLintel, WallConfiguration.Uninsulated), new BridgeData("Nadpraží - neizolaná stěna", 0.25, "Nadpraží okna bez izolace") },
        { (BridgeType.WindowLintel, WallConfiguration.ThinInsulation), new BridgeData("Nadpraží - tenká izolace", 0.20, "Nadpraží s tenkou izolací") },
        { (BridgeType.WindowLintel, WallConfiguration.Insulated), new BridgeData("Nadpraží - standardní izolace", 0.15, "Nadpraží se standardní izolací") },
        { (BridgeType.WindowLintel, WallConfiguration.ThickInsulation), new BridgeData("Nadpraží - silná izolace", 0.10, "Nadpraží se silnou izolací") },
        
        // Balkony a lodžie
        { (BridgeType.Balcony, WallConfiguration.Uninsulated), new BridgeData("Balkon - neizolaná stěna", 1.20, "Balkon bez tepelné izolace") },
        { (BridgeType.Balcony, WallConfiguration.ThinInsulation), new BridgeData("Balkon - tenká izolace", 1.00, "Balkon s přerušením mostku") },
        { (BridgeType.Balcony, WallConfiguration.Insulated), new BridgeData("Balkon - standardní izolace", 0.80, "Balkon s izolačními prvky") },
        { (BridgeType.Balcony, WallConfiguration.ThickInsulation), new BridgeData("Balkon - silná izolace", 0.60, "Balkon s kvalitním přerušením") },
        
        // Stropní konstrukce
        { (BridgeType.FloorSlab, WallConfiguration.Uninsulated), new BridgeData("Stropní deska - neizolaná", 0.70, "Železobetonová deska bez izolace") },
        { (BridgeType.FloorSlab, WallConfiguration.ThinInsulation), new BridgeData("Stropní deska - tenká izolace", 0.55, "Deska s tenkou izolací") },
        { (BridgeType.FloorSlab, WallConfiguration.Insulated), new BridgeData("Stropní deska - standardní izolace", 0.40, "Deska se standardní izolací") },
        { (BridgeType.FloorSlab, WallConfiguration.ThickInsulation), new BridgeData("Stropní deska - silná izolace", 0.25, "Deska se silnou izolací") },
    };
    
    public static ThermalBridgeCatalog GetStandardCatalog()
    {
        return new ThermalBridgeCatalog();
    }
    
    public BridgeData? GetBridgeData(BridgeType type, WallConfiguration config)
    {
        return _catalog.TryGetValue((type, config), out var data) ? data : null;
    }
    
    public List<BridgeData> GetAllBridgeTypes(WallConfiguration config)
    {
        return _catalog
            .Where(kvp => kvp.Key.Item2 == config)
            .Select(kvp => kvp.Value)
            .ToList();
    }
}

// Data pro tepelný mostek
public record BridgeData(string Name, double PsiValue, string Description);

// Typy tepelných mostků
public enum BridgeType
{
    ExternalCorner,    // Vnější roh
    InternalCorner,    // Vnitřní roh
    Foundation,        // Základ
    RoofConnection,    // Atika/střecha
    WindowSill,        // Parapet
    WindowLintel,      // Nadpraží
    FloorSlab,         // Stropní deska
    Balcony,           // Balkon/lodžie
    Pillar,            // Sloup
    Beam,              // Průvlak
    Other              // Ostatní
}

// Konfigurace stěny podle tloušťky izolace
public enum WallConfiguration
{
    Uninsulated,       // Bez izolace
    ThinInsulation,    // Tenká izolace (< 80mm)
    Insulated,         // Standardní izolace (80-150mm)
    ThickInsulation    // Silná izolace (> 150mm)
}

// Rozšíření WallAssembly o tepelné mostky
public static class WallAssemblyThermalBridgeExtensions
{
    // Přidá podporu pro tepelné mostky do existující třídy
    public static double GetUValueWithThermalBridges(this WallAssembly assembly, ThermalBridgeCollection bridges)
    {
        return assembly.ThermalTransmittance + bridges.UThermalBridgeCorrection;
    }
    
    // Získá doporučené tepelné mostky pro konstrukci
    public static ThermalBridgeCollection GetRecommendedThermalBridges(this WallAssembly assembly)
    {
        var bridges = new ThermalBridgeCollection();
        bridges.AddTypicalBridges(assembly);
        return bridges;
    }
    
    // Hodnotí kritičnost tepelných mostků
    public static ThermalBridgeCriticality GetThermalBridgeCriticality(this WallAssembly assembly, ThermalBridgeCollection bridges)
    {
        var correction = bridges.UThermalBridgeCorrection;
        var relativeIncrease = correction / assembly.ThermalTransmittance * 100;
        
        return relativeIncrease switch
        {
            > 50 => ThermalBridgeCriticality.Critical,
            > 30 => ThermalBridgeCriticality.High,
            > 15 => ThermalBridgeCriticality.Medium,
            > 5 => ThermalBridgeCriticality.Low,
            _ => ThermalBridgeCriticality.Negligible
        };
    }
}

public enum ThermalBridgeCriticality
{
    Negligible, // Zanedbatelné (< 5% zvýšení U)
    Low,        // Nízké (5-15% zvýšení U)  
    Medium,     // Střední (15-30% zvýšení U)
    High,       // Vysoké (30-50% zvýšení U)
    Critical    // Kritické (> 50% zvýšení U)
}