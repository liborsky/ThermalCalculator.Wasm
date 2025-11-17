using System.Text.Json;
using ThermalCalculator.Wasm.Models;

namespace ThermalCalculator.Wasm.Services;

public class WallAssemblyService
{
    private const string STORAGE_KEY = "thermal_calculator_assemblies";
    
    public List<SavedWallAssembly> GetSavedAssemblies()
    {
        try
        {
            // Načíst z localStorage pomocí JavaScript
            var assembliesJson = GetFromLocalStorage(STORAGE_KEY);
            if (!string.IsNullOrEmpty(assembliesJson))
            {
                var assemblies = JsonSerializer.Deserialize<List<SavedWallAssembly>>(assembliesJson);
                return assemblies ?? new List<SavedWallAssembly>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při načítání skladeb: {ex.Message}");
        }
        
        return new List<SavedWallAssembly>();
    }
    
    public void SaveAssembly(WallAssembly assembly, string name)
    {
        try
        {
            var assemblies = GetSavedAssemblies();
            
            // Odstranit existující skladbu se stejným názvem
            assemblies.RemoveAll(a => a.Name == name);
            
            // Přidat novou skladbu
            var savedAssembly = new SavedWallAssembly
            {
                Name = name,
                CreatedAt = DateTime.Now,
                Assembly = assembly
            };
            
            assemblies.Add(savedAssembly);
            
            // Uložit do localStorage
            SaveToLocalStorage(STORAGE_KEY, JsonSerializer.Serialize(assemblies));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při ukládání skladby: {ex.Message}");
        }
    }
    
    public void DeleteAssembly(string name)
    {
        try
        {
            var assemblies = GetSavedAssemblies();
            assemblies.RemoveAll(a => a.Name == name);
            SaveToLocalStorage(STORAGE_KEY, JsonSerializer.Serialize(assemblies));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při mazání skladby: {ex.Message}");
        }
    }
    
    public WallAssembly? LoadAssembly(string name)
    {
        try
        {
            var assemblies = GetSavedAssemblies();
            var savedAssembly = assemblies.FirstOrDefault(a => a.Name == name);
            return savedAssembly?.Assembly;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při načítání skladby: {ex.Message}");
            return null;
        }
    }
    
    // Pomocné metody pro práci s localStorage
    private string GetFromLocalStorage(string key)
    {
        // Tato metoda bude volána z Blazor komponenty pomocí IJSRuntime
        // Prozatím vrátíme prázdný string, skutečná implementace bude v komponentě
        return string.Empty;
    }
    
    private void SaveToLocalStorage(string key, string data)
    {
        // Tato metoda bude volána z Blazor komponenty pomocí IJSRuntime
        // Prozatím prázdná implementace
    }
}

public class SavedWallAssembly
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public WallAssembly Assembly { get; set; } = new();
} 