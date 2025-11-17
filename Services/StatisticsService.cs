using Microsoft.JSInterop;
using System.Text.Json;
using ThermalCalculator.Wasm.Models;

namespace ThermalCalculator.Wasm.Services;

/// <summary>
/// Service pro správu statistik použití aplikace
/// </summary>
public class StatisticsService
{
    private const string STATISTICS_KEY = "usage_statistics";
    private readonly IJSRuntime _jsRuntime;
    private UsageStatistics? _cachedStatistics;

    public StatisticsService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Načte statistiky z localStorage
    /// </summary>
    public async Task<UsageStatistics> GetStatisticsAsync()
    {
        if (_cachedStatistics != null)
            return _cachedStatistics;

        try
        {
            var statisticsJson = await _jsRuntime.InvokeAsync<string>("eval", 
                $"localStorage.getItem('{STATISTICS_KEY}') || '{{}}'");

            if (string.IsNullOrEmpty(statisticsJson) || statisticsJson == "{}")
            {
                _cachedStatistics = new UsageStatistics();
            }
            else
            {
                _cachedStatistics = JsonSerializer.Deserialize<UsageStatistics>(statisticsJson) ?? new UsageStatistics();
            }

            return _cachedStatistics;
        }
        catch (Exception ex)
        {
            // Při chybě deserializace vytvoř nové statistiky
            await _jsRuntime.InvokeVoidAsync("console.error", $"Chyba při načítání statistik: {ex.Message}");
            _cachedStatistics = new UsageStatistics();
            return _cachedStatistics;
        }
    }

    /// <summary>
    /// Uloží statistiky do localStorage
    /// </summary>
    public async Task SaveStatisticsAsync(UsageStatistics statistics)
    {
        try
        {
            var statisticsJson = JsonSerializer.Serialize(statistics, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await _jsRuntime.InvokeVoidAsync("eval", 
                $"localStorage.setItem('{STATISTICS_KEY}', {JsonSerializer.Serialize(statisticsJson)})");

            _cachedStatistics = statistics;
        }
        catch (Exception ex)
        {
            await _jsRuntime.InvokeVoidAsync("console.error", $"Chyba při ukládání statistik: {ex.Message}");
        }
    }

    /// <summary>
    /// Zaznamená výpočet tepelného odporu
    /// </summary>
    public async Task TrackCalculationAsync(WallAssembly assembly)
    {
        var statistics = await GetStatisticsAsync();
        
        statistics.TotalCalculationsPerformed++;
        statistics.TotalLayersCreated += assembly.Layers.Count;
        statistics.UpdateLastUsage();

        // Zaznamenej použité materiály a kategorie
        foreach (var layer in assembly.Layers)
        {
            statistics.IncrementMaterialUsage(layer.Material.Name);
            statistics.IncrementCategoryUsage(layer.Material.Category);
        }

        await SaveStatisticsAsync(statistics);
    }

    /// <summary>
    /// Zaznamená přidání vrstvy
    /// </summary>
    public async Task TrackLayerAddedAsync(string materialName, string categoryName)
    {
        var statistics = await GetStatisticsAsync();
        
        statistics.TotalLayersCreated++;
        statistics.UpdateLastUsage();
        
        statistics.IncrementMaterialUsage(materialName);
        statistics.IncrementCategoryUsage(categoryName);

        await SaveStatisticsAsync(statistics);
    }

    /// <summary>
    /// Zaznamená uložení skladby
    /// </summary>
    public async Task TrackAssemblySavedAsync()
    {
        var statistics = await GetStatisticsAsync();
        
        statistics.AssembliesSaved++;
        statistics.UpdateLastUsage();

        await SaveStatisticsAsync(statistics);
    }

    /// <summary>
    /// Zaznamená načtení skladby
    /// </summary>
    public async Task TrackAssemblyLoadedAsync()
    {
        var statistics = await GetStatisticsAsync();
        
        statistics.AssembliesLoaded++;
        statistics.UpdateLastUsage();

        await SaveStatisticsAsync(statistics);
    }

    /// <summary>
    /// Zaznamená použití příkladu
    /// </summary>
    public async Task TrackExampleUsedAsync()
    {
        var statistics = await GetStatisticsAsync();
        
        statistics.ExamplesUsed++;
        statistics.UpdateLastUsage();

        await SaveStatisticsAsync(statistics);
    }

    /// <summary>
    /// Zaznamená export PDF
    /// </summary>
    public async Task TrackPdfExportAsync()
    {
        var statistics = await GetStatisticsAsync();
        
        statistics.PdfExportsGenerated++;
        statistics.UpdateLastUsage();

        await SaveStatisticsAsync(statistics);
    }

    /// <summary>
    /// Vymaže všechny statistiky
    /// </summary>
    public async Task ClearStatisticsAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("eval", $"localStorage.removeItem('{STATISTICS_KEY}')");
            _cachedStatistics = null;
        }
        catch (Exception ex)
        {
            await _jsRuntime.InvokeVoidAsync("console.error", $"Chyba při mazání statistik: {ex.Message}");
        }
    }

    /// <summary>
    /// Exportuje statistiky jako JSON string
    /// </summary>
    public async Task<string> ExportStatisticsAsync()
    {
        var statistics = await GetStatisticsAsync();
        
        return JsonSerializer.Serialize(statistics, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Získá nejpoužívanější materiály (top 10)
    /// </summary>
    public async Task<List<(string Material, int Count)>> GetTopMaterialsAsync(int count = 10)
    {
        var statistics = await GetStatisticsAsync();
        
        return statistics.MaterialUsageCount
            .OrderByDescending(x => x.Value)
            .Take(count)
            .Select(x => (x.Key, x.Value))
            .ToList();
    }

    /// <summary>
    /// Získá nejpoužívanější kategorie
    /// </summary>
    public async Task<List<(string Category, int Count)>> GetTopCategoriesAsync()
    {
        var statistics = await GetStatisticsAsync();
        
        return statistics.CategoryUsageCount
            .OrderByDescending(x => x.Value)
            .Select(x => (x.Key, x.Value))
            .ToList();
    }
}