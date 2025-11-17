namespace ThermalCalculator.Wasm.Models;

/// <summary>
/// Šablona zdi s přednastavenými vrstvami materiálů
/// </summary>
public class WallTemplate
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<TemplateLayer> Layers { get; set; } = new();
    public string Icon { get; set; } = "bi-layers"; // Bootstrap icon
    public bool IsPopular { get; set; } = false;
    public double EstimatedCost { get; set; } = 0; // Orientační cena za m²
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Vrstva v šabloně zdi
/// </summary>
public class TemplateLayer
{
    public string MaterialName { get; set; } = string.Empty;
    public double DefaultThickness { get; set; } = 0; // mm
    public double MinThickness { get; set; } = 0; // mm
    public double MaxThickness { get; set; } = 0; // mm
    public bool IsAdjustable { get; set; } = true;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; } = 0; // Pořadí od vnitřní strany
}

/// <summary>
/// Kategorie šablon
/// </summary>
public enum TemplateCategory
{
    ETICS,          // Zateplovací systémy
    Sandwich,       // Sendvičové panely  
    Monolithic,     // Jednoplášťové zdi
    Partition,      // Příčky
    WoodFrame,      // Dřevostavby
    GreenRoof,      // Zelené střechy
    Industrial,     // Průmyslové konstrukce
    Historic        // Historické konstrukce
}

/// <summary>
/// Vybraná šablona s možností úpravy tlouštěk
/// </summary>
public class SelectedTemplate
{
    public WallTemplate Template { get; set; } = new();
    public List<SelectedTemplateLayer> Layers { get; set; } = new();
    public bool AllowCustomThickness { get; set; } = true;
}

/// <summary>
/// Vrstva vybrané šablony s možností úpravy
/// </summary>
public class SelectedTemplateLayer
{
    public TemplateLayer TemplateLayer { get; set; } = new();
    public double SelectedThickness { get; set; } = 0; // mm
    public bool IsEnabled { get; set; } = true;
    public string CustomNotes { get; set; } = string.Empty;
}

/// <summary>
/// Pomocná třída pro JavaScript komunikaci
/// </summary>
public class LayerSetting
{
    public double Thickness { get; set; }
    public bool Enabled { get; set; }
}