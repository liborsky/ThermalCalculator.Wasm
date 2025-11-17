namespace ThermalCalculator.Wasm.Models;

/// <summary>
/// Data pro 3D vizualizaci tepelných vlastností zdi
/// </summary>
public class WallVisualizationData
{
    public List<Layer3D> Layers { get; set; } = new();
    public TemperatureGradient TemperatureData { get; set; } = new();
    public List<CondensationZone3D> CondensationZones { get; set; } = new();
    public double TotalThickness { get; set; } = 0.0; // mm
    public double WallHeight { get; set; } = 2800.0; // mm - standardní výška 2.8m
    public double WallWidth { get; set; } = 1000.0; // mm - standardní šířka 1m pro výpočty
    public ClimateConditions Climate { get; set; } = new();
}

/// <summary>
/// 3D vrstva s vizualizačními vlastnostmi
/// </summary>
public class Layer3D
{
    public string MaterialName { get; set; } = string.Empty;
    public double Thickness { get; set; } = 0.0; // mm
    public double StartPosition { get; set; } = 0.0; // mm od vnitřní strany
    public double EndPosition { get; set; } = 0.0; // mm od vnitřní strany
    public MaterialVisualizationProperties VisualProperties { get; set; } = new();
    public List<TemperaturePoint> TemperaturePoints { get; set; } = new();
    public double ThermalConductivity { get; set; } = 0.0; // λ
    public bool HasCondensation { get; set; } = false;
}

/// <summary>
/// Vizualizační vlastnosti materiálu
/// </summary>
public class MaterialVisualizationProperties
{
    public string BaseColor { get; set; } = "#888888"; // Hex barva
    public double Opacity { get; set; } = 0.8;
    public string TextureType { get; set; } = "solid"; // solid, brick, concrete, insulation, wood
    public double Roughness { get; set; } = 0.5; // 0-1 pro Three.js materiály
    public double Metalness { get; set; } = 0.0; // 0-1 pro Three.js materiály
}

/// <summary>
/// Teplotní gradient přes celou konstrukci
/// </summary>
public class TemperatureGradient
{
    public List<TemperatureProfile> Profiles { get; set; } = new();
    public double MinTemperature { get; set; } = 0.0; // °C
    public double MaxTemperature { get; set; } = 0.0; // °C
    public double TemperatureRange => MaxTemperature - MinTemperature;
}

/// <summary>
/// Teplotní profil pro konkrétní pozici
/// </summary>
public class TemperatureProfile
{
    public double Position { get; set; } = 0.0; // mm od vnitřní strany
    public double Temperature { get; set; } = 0.0; // °C
    public double VaporPressure { get; set; } = 0.0; // Pa
    public double DewPointTemperature { get; set; } = 0.0; // °C
    public string LayerName { get; set; } = string.Empty;
}

/// <summary>
/// Teplotní bod pro detailní gradient
/// </summary>
public class TemperaturePoint
{
    public double RelativePosition { get; set; } = 0.0; // 0-1 v rámci vrstvy
    public double Temperature { get; set; } = 0.0; // °C
    public string ColorHex { get; set; } = "#000000"; // Barva pro gradient
    public bool IsCondensationRisk { get; set; } = false;
}

/// <summary>
/// 3D kondenzační zóna s vizualizačními vlastnostmi
/// </summary>
public class CondensationZone3D
{
    public string MaterialName { get; set; } = string.Empty;
    public double StartDepth { get; set; } = 0.0; // mm od vnitřní strany
    public double EndDepth { get; set; } = 0.0; // mm od vnitřní strany
    public double CondensationAmount { get; set; } = 0.0; // kg/(m²·rok)
    public CondensationSeverity Severity { get; set; } = CondensationSeverity.Low;
    public string VisualizationColor { get; set; } = "#FF0000"; // Barva pro zobrazení
    public double IntensityAlpha { get; set; } = 0.3; // Průhlednost 0-1
}

/// <summary>
/// Klimatické podmínky pro vizualizaci
/// </summary>
public class ClimateConditions
{
    public double InternalTemperature { get; set; } = 20.0; // °C
    public double ExternalTemperature { get; set; } = -15.0; // °C
    public double InternalHumidity { get; set; } = 50.0; // %
    public double ExternalHumidity { get; set; } = 80.0; // %
    public string Season { get; set; } = "winter"; // winter, summer
}

/// <summary>
/// Konfigurace 3D vizualizace
/// </summary>
public class VisualizationConfig
{
    public VisualizationMode Mode { get; set; } = VisualizationMode.Temperature;
    public bool ShowCondensationZones { get; set; } = true;
    public bool ShowTemperatureLabels { get; set; } = true;
    public bool ShowMaterialLabels { get; set; } = true;
    public bool EnableAnimation { get; set; } = false;
    public double AnimationSpeed { get; set; } = 1.0; // Rychlost animace
    public ColorScheme TemperatureColorScheme { get; set; } = ColorScheme.BlueRedScale;
}

/// <summary>
/// Módy vizualizace
/// </summary>
public enum VisualizationMode
{
    Temperature,        // Teplotní gradient
    Materials,         // Materiálové vrstvy
    Condensation,      // Kondenzační zóny
    HeatFlow,         // Tepelný tok
    Dimensional       // Rozměrové zobrazení
}

/// <summary>
/// Barevné schéma pro teploty
/// </summary>
public enum ColorScheme
{
    BlueRedScale,     // Modra → Červená
    Rainbow,          // Duha spektrum
    Thermal,          // Tepelná kamera
    Monochrome       // Černobílé
}

/// <summary>
/// Závažnost kondenzace
/// </summary>
public enum CondensationSeverity
{
    None,      // Žádná kondenzace
    Low,       // Nízké riziko
    Medium,    // Střední riziko  
    High,      // Vysoké riziko
    Critical   // Kritické riziko
}

/// <summary>
/// Rozšíření WallAssembly pro 3D vizualizaci
/// </summary>
public static class WallAssemblyVisualizationExtensions
{
    /// <summary>
    /// Získá kompletní data pro 3D vizualizaci
    /// </summary>
    public static WallVisualizationData GetVisualizationData(this WallAssembly assembly, VisualizationConfig? config = null)
    {
        config ??= new VisualizationConfig();
        
        var data = new WallVisualizationData
        {
            TotalThickness = assembly.Layers.Sum(l => l.Thickness),
            Climate = new ClimateConditions
            {
                InternalTemperature = assembly.InternalTemperature,
                ExternalTemperature = assembly.ExternalTemperature,
                InternalHumidity = assembly.InternalHumidity,
                ExternalHumidity = assembly.ExternalHumidity
            }
        };

        // Vytvoření 3D vrstev
        data.Layers = CreateLayers3D(assembly);
        
        // Výpočet teplotního gradientu
        data.TemperatureData = CalculateDetailedTemperatureGradient(assembly);
        
        // Kondenzační zóny
        data.CondensationZones = CreateCondensationZones3D(assembly);
        
        return data;
    }

    /// <summary>
    /// Vytvoří 3D vrstvy s vizualizačními vlastnostmi
    /// </summary>
    private static List<Layer3D> CreateLayers3D(WallAssembly assembly)
    {
        var layers3D = new List<Layer3D>();
        double currentPosition = 0.0;

        foreach (var layer in assembly.Layers)
        {
            var layer3D = new Layer3D
            {
                MaterialName = layer.Material.Name,
                Thickness = layer.Thickness,
                StartPosition = currentPosition,
                EndPosition = currentPosition + layer.Thickness,
                ThermalConductivity = layer.Material.ThermalConductivity,
                VisualProperties = GetMaterialVisualizationProperties(layer.Material)
            };

            layers3D.Add(layer3D);
            currentPosition += layer.Thickness;
        }

        return layers3D;
    }

    /// <summary>
    /// Získá vizualizační vlastnosti podle typu materiálu
    /// </summary>
    private static MaterialVisualizationProperties GetMaterialVisualizationProperties(Material material)
    {
        return material.Category.ToLower() switch
        {
            "izolace" or "tepelná izolace" => new MaterialVisualizationProperties
            {
                BaseColor = "#FFE4B5", // Světle žlutá
                TextureType = "insulation",
                Roughness = 0.9,
                Opacity = 0.9 // Zvýšeno z 0.7 na 0.9
            },
            "zdivo" or "cihla" => new MaterialVisualizationProperties
            {
                BaseColor = "#CD853F", // Cihlová
                TextureType = "brick",
                Roughness = 0.8,
                Opacity = 0.9
            },
            "beton" or "pórobeton" => new MaterialVisualizationProperties
            {
                BaseColor = "#A9A9A9", // Šedá
                TextureType = "concrete",
                Roughness = 0.6,
                Opacity = 0.9
            },
            "dřevo" => new MaterialVisualizationProperties
            {
                BaseColor = "#DEB887", // Dřevěná
                TextureType = "wood",
                Roughness = 0.7,
                Opacity = 0.9
            },
            "omítka" => new MaterialVisualizationProperties
            {
                BaseColor = "#F5F5DC", // Béžová
                TextureType = "solid",
                Roughness = 0.4,
                Opacity = 0.9
            },
            "folie a zábrany" => new MaterialVisualizationProperties
            {
                BaseColor = "#FF6B6B", // Červeno-růžová pro viditelnost
                TextureType = "membrane",
                Roughness = 0.2,
                Opacity = 1.0 // Plná neprůhlednost pro tenké folie
            },
            _ => new MaterialVisualizationProperties
            {
                BaseColor = "#808080", // Výchozí šedá
                TextureType = "solid",
                Roughness = 0.5,
                Opacity = 0.9 // Zvýšeno z 0.8 na 0.9
            }
        };
    }

    /// <summary>
    /// Vypočítá detailní teplotní gradient pro 3D vizualizaci
    /// </summary>
    private static TemperatureGradient CalculateDetailedTemperatureGradient(WallAssembly assembly)
    {
        const int pointsPerLayer = 20; // Hustota bodů pro hladký gradient
        
        var gradient = new TemperatureGradient();
        var profiles = new List<TemperatureProfile>();

        if (assembly.Layers.Count == 0) return gradient;

        // Základní výpočet teplotního profilu
        var temperatures = CalculateBasicTemperatureProfile(assembly);
        if (temperatures.Count == 0) return gradient;

        gradient.MinTemperature = temperatures.Min(t => t.temperature);
        gradient.MaxTemperature = temperatures.Max(t => t.temperature);

        // Detailní interpolace pro hladký gradient
        for (int layerIndex = 0; layerIndex < assembly.Layers.Count; layerIndex++)
        {
            var layer = assembly.Layers[layerIndex];
            var startTemp = temperatures[layerIndex].temperature;
            var endTemp = temperatures[layerIndex + 1].temperature;
            var startDepth = temperatures[layerIndex].depth;
            
            for (int i = 0; i <= pointsPerLayer; i++)
            {
                var relativePos = (double)i / pointsPerLayer;
                var currentDepth = startDepth + relativePos * layer.Thickness;
                var currentTemp = startTemp + relativePos * (endTemp - startTemp);
                
                profiles.Add(new TemperatureProfile
                {
                    Position = currentDepth,
                    Temperature = currentTemp,
                    DewPointTemperature = 0.0, // Bude doplněno později
                    LayerName = layer.Material.Name
                });
            }
        }

        gradient.Profiles = profiles;
        return gradient;
    }

    /// <summary>
    /// Základní výpočet teplotního profilu
    /// </summary>
    private static List<(double depth, double temperature)> CalculateBasicTemperatureProfile(WallAssembly assembly)
    {
        var profile = new List<(double depth, double temperature)>();
        double currentDepth = 0.0; // mm od vnitřního povrchu
        double currentTemperature = assembly.InternalTemperature;
        
        // Celkový tepelný odpor skladby (bez povrchových odporů)
        double totalThermalResistance = assembly.TotalThermalResistance - assembly.InternalSurfaceResistance - assembly.ExternalSurfaceResistance;
        
        // Tepelný tok (W/m²)
        double heatFlux = (assembly.InternalTemperature - assembly.ExternalTemperature) / assembly.TotalThermalResistance;
        
        // Přidat vnitřní hranici
        profile.Add((currentDepth, currentTemperature));
        
        // Výpočet teplotního profilu přes jednotlivé vrstvy
        foreach (var layer in assembly.Layers)
        {
            // Tepelný odpor vrstvy
            double layerThermalResistance = layer.ThermalResistance;
            
            // Pokles teploty přes vrstvu
            double temperatureDrop = heatFlux * layerThermalResistance;
            
            // Nová teplota na konci vrstvy
            currentTemperature -= temperatureDrop;
            currentDepth += layer.Thickness;
            
            profile.Add((currentDepth, currentTemperature));
        }
        
        return profile;
    }

    /// <summary>
    /// Vytvoří 3D kondenzační zóny
    /// </summary>
    private static List<CondensationZone3D> CreateCondensationZones3D(WallAssembly assembly)
    {
        var dewPointCalculation = assembly.CalculateDewPoint();
        var zones3D = new List<CondensationZone3D>();

        foreach (var zone in dewPointCalculation.CondensationZones)
        {
            var severity = GetCondensationSeverity(zone.CondensationAmount);
            var zone3D = new CondensationZone3D
            {
                MaterialName = zone.MaterialName,
                StartDepth = zone.StartDepth,
                EndDepth = zone.EndDepth,
                CondensationAmount = zone.CondensationAmount,
                Severity = severity,
                VisualizationColor = GetCondensationColor(severity),
                IntensityAlpha = GetCondensationAlpha(zone.CondensationAmount)
            };

            zones3D.Add(zone3D);
        }

        return zones3D;
    }

    /// <summary>
    /// Určí závažnost kondenzace
    /// </summary>
    private static CondensationSeverity GetCondensationSeverity(double annualCondensation)
    {
        return annualCondensation switch
        {
            0 => CondensationSeverity.None,
            < 0.5 => CondensationSeverity.Low,
            < 2.0 => CondensationSeverity.Medium,
            < 5.0 => CondensationSeverity.High,
            _ => CondensationSeverity.Critical
        };
    }

    /// <summary>
    /// Získá barvu podle závažnosti kondenzace
    /// </summary>
    private static string GetCondensationColor(CondensationSeverity severity)
    {
        return severity switch
        {
            CondensationSeverity.None => "#00FF00",      // Zelená
            CondensationSeverity.Low => "#FFFF00",       // Žlutá
            CondensationSeverity.Medium => "#FFA500",    // Oranžová
            CondensationSeverity.High => "#FF4500",      // Červeno-oranžová
            CondensationSeverity.Critical => "#FF0000",  // Červená
            _ => "#808080"                               // Šedá
        };
    }

    /// <summary>
    /// Získá průhlednost podle množství kondenzace
    /// </summary>
    private static double GetCondensationAlpha(double condensationAmount)
    {
        return Math.Min(0.8, Math.Max(0.2, condensationAmount / 5.0));
    }

    /// <summary>
    /// Převede teplotu na barvu podle barevného schématu
    /// </summary>
    public static string TemperatureToColor(double temperature, double minTemp, double maxTemp, ColorScheme scheme = ColorScheme.BlueRedScale)
    {
        if (maxTemp <= minTemp) return "#888888";
        
        var normalized = Math.Max(0, Math.Min(1, (temperature - minTemp) / (maxTemp - minTemp)));
        
        return scheme switch
        {
            ColorScheme.BlueRedScale => InterpolateBlueRed(normalized),
            ColorScheme.Rainbow => InterpolateRainbow(normalized),
            ColorScheme.Thermal => InterpolateThermal(normalized),
            ColorScheme.Monochrome => InterpolateMonochrome(normalized),
            _ => InterpolateBlueRed(normalized)
        };
    }

    /// <summary>
    /// Interpolace modra-červená
    /// </summary>
    private static string InterpolateBlueRed(double t)
    {
        var r = (int)(255 * t);
        var g = (int)(255 * (1 - Math.Abs(2 * t - 1)));
        var b = (int)(255 * (1 - t));
        
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    /// <summary>
    /// Interpolace duha spektrum
    /// </summary>
    private static string InterpolateRainbow(double t)
    {
        var hue = (int)(240 * (1 - t)); // 240° (modrá) → 0° (červená)
        return $"hsl({hue}, 100%, 50%)";
    }

    /// <summary>
    /// Interpolace tepelná kamera
    /// </summary>
    private static string InterpolateThermal(double t)
    {
        if (t < 0.25)
        {
            var r = 0;
            var g = 0;
            var b = (int)(255 * (4 * t));
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        else if (t < 0.5)
        {
            var r = 0;
            var g = (int)(255 * (4 * (t - 0.25)));
            var b = 255;
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        else if (t < 0.75)
        {
            var r = (int)(255 * (4 * (t - 0.5)));
            var g = 255;
            var b = (int)(255 * (1 - 4 * (t - 0.5)));
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        else
        {
            var r = 255;
            var g = (int)(255 * (1 - 4 * (t - 0.75)));
            var b = 0;
            return $"#{r:X2}{g:X2}{b:X2}";
        }
    }

    /// <summary>
    /// Interpolace černobílá
    /// </summary>
    private static string InterpolateMonochrome(double t)
    {
        var intensity = (int)(255 * t);
        return $"#{intensity:X2}{intensity:X2}{intensity:X2}";
    }
}