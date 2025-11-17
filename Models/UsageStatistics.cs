using System.Text.Json.Serialization;

namespace ThermalCalculator.Wasm.Models;

/// <summary>
/// Model pro sledování statistik použití aplikace
/// </summary>
public class UsageStatistics
{
    public int TotalCalculationsPerformed { get; set; } = 0;
    public int TotalLayersCreated { get; set; } = 0;
    public int AssembliesSaved { get; set; } = 0;
    public int AssembliesLoaded { get; set; } = 0;
    public int ExamplesUsed { get; set; } = 0;
    public int PdfExportsGenerated { get; set; } = 0;
    public DateTime FirstUsage { get; set; } = DateTime.MinValue;
    public DateTime LastUsage { get; set; } = DateTime.MinValue;
    public Dictionary<string, int> MaterialUsageCount { get; set; } = new();
    public Dictionary<string, int> CategoryUsageCount { get; set; } = new();

    /// <summary>
    /// Celkový počet dnů používání aplikace
    /// </summary>
    [JsonIgnore]
    public int TotalUsageDays => FirstUsage == DateTime.MinValue ? 0 : 
        Math.Max(1, (DateTime.Now - FirstUsage).Days + 1);

    /// <summary>
    /// Průměrný počet výpočtů za den
    /// </summary>
    [JsonIgnore]
    public double AverageCalculationsPerDay => TotalUsageDays > 0 ? 
        (double)TotalCalculationsPerformed / TotalUsageDays : 0;

    /// <summary>
    /// Průměrný počet vrstev na výpočet
    /// </summary>
    [JsonIgnore]
    public double AverageLayersPerCalculation => TotalCalculationsPerformed > 0 ? 
        (double)TotalLayersCreated / TotalCalculationsPerformed : 0;

    /// <summary>
    /// Nejpoužívanější materiál
    /// </summary>
    [JsonIgnore]
    public string? MostUsedMaterial => MaterialUsageCount.Count > 0 ? 
        MaterialUsageCount.OrderByDescending(x => x.Value).First().Key : null;

    /// <summary>
    /// Nejpoužívanější kategorie
    /// </summary>
    [JsonIgnore]
    public string? MostUsedCategory => CategoryUsageCount.Count > 0 ? 
        CategoryUsageCount.OrderByDescending(x => x.Value).First().Key : null;

    /// <summary>
    /// Aktualizuje čas posledního použití
    /// </summary>
    public void UpdateLastUsage()
    {
        var now = DateTime.Now;
        
        if (FirstUsage == DateTime.MinValue)
        {
            FirstUsage = now;
        }
        
        LastUsage = now;
    }

    /// <summary>
    /// Přidá použití materiálu do statistik
    /// </summary>
    public void IncrementMaterialUsage(string materialName)
    {
        if (!string.IsNullOrEmpty(materialName))
        {
            MaterialUsageCount[materialName] = MaterialUsageCount.GetValueOrDefault(materialName, 0) + 1;
        }
    }

    /// <summary>
    /// Přidá použití kategorie do statistik
    /// </summary>
    public void IncrementCategoryUsage(string categoryName)
    {
        if (!string.IsNullOrEmpty(categoryName))
        {
            CategoryUsageCount[categoryName] = CategoryUsageCount.GetValueOrDefault(categoryName, 0) + 1;
        }
    }
}

/// <summary>
/// Detailní statistiky pro konkrétní výpočet
/// </summary>
public class CalculationSnapshot
{
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public int LayerCount { get; set; }
    public double TotalThickness { get; set; }
    public double ThermalResistance { get; set; }
    public double ThermalTransmittance { get; set; }
    public bool HasCondensation { get; set; }
    public List<string> MaterialsUsed { get; set; } = new();
    public List<string> CategoriesUsed { get; set; } = new();
}