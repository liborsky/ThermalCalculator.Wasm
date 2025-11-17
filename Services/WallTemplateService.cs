using ThermalCalculator.Wasm.Models;

namespace ThermalCalculator.Wasm.Services;

public class WallTemplateService
{
    /// <summary>
    /// Získá všechny přednastavené šablony
    /// </summary>
    public List<WallTemplate> GetAllTemplates()
    {
        return new List<WallTemplate>
        {
            // === ETICS SYSTÉMY ===
            CreateEticsTemplate(),
            CreateEticsThickTemplate(),
            CreateEticsWoodFiberTemplate(),
            
            // === SENDVIČOVÉ PANELY ===
            CreateSandwichPanelTemplate(),
            CreateInsulatedSteelPanelTemplate(),
            
            // === JEDNOPLÁŠŤOVÉ ZDI ===
            CreatePorotermWallTemplate(),
            CreateAAGWallTemplate(),
            CreateBrickWallTemplate(),
            
            // === PŘÍČKY ===
            CreateInteriorPartitionTemplate(),
            CreateAcousticPartitionTemplate(),
            
            // === DŘEVOSTAVBY ===
            CreateWoodFrameWallTemplate(),
            CreateCrossLaminatedTimberTemplate(),
            
            // === PRŮMYSLOVÉ ===
            CreateIndustrialWallTemplate(),
            CreateColdStorageWallTemplate()
        };
    }

    /// <summary>
    /// Získá šablony podle kategorie
    /// </summary>
    public List<WallTemplate> GetTemplatesByCategory(string category)
    {
        return GetAllTemplates().Where(t => t.Category == category).ToList();
    }

    /// <summary>
    /// Získá populární šablony
    /// </summary>
    public List<WallTemplate> GetPopularTemplates()
    {
        return GetAllTemplates().Where(t => t.IsPopular).ToList();
    }

    /// <summary>
    /// Převede šablonu na WallAssembly
    /// </summary>
    public WallAssembly ConvertToWallAssembly(SelectedTemplate selectedTemplate, MaterialService materialService)
    {
        var assembly = new WallAssembly
        {
            Name = selectedTemplate.Template.Name
        };

        var availableMaterials = materialService.GetCommonMaterials();
        availableMaterials.AddRange(materialService.GetCustomMaterials());

        foreach (var layer in selectedTemplate.Layers.Where(l => l.IsEnabled).OrderBy(l => l.TemplateLayer.Order))
        {
            var material = availableMaterials.FirstOrDefault(m => m.Name == layer.TemplateLayer.MaterialName);
            if (material != null)
            {
                assembly.Layers.Add(new WallLayer
                {
                    Material = material,
                    Thickness = layer.SelectedThickness
                });
            }
        }

        return assembly;
    }

    // === IMPLEMENTACE ŠABLON ===

    private WallTemplate CreateEticsTemplate()
    {
        return new WallTemplate
        {
            Name = "ETICS - Standardní zateplení",
            Description = "Vnější zateplovací systém s polystyrenem",
            Category = "ETICS",
            Icon = "bi-house-fill",
            IsPopular = true,
            EstimatedCost = 1800,
            Notes = "Nejčastěji používaný zateplovací systém pro rodinné domy",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Vápenná omítka", DefaultThickness = 15, MinThickness = 10, MaxThickness = 25, Order = 1, Description = "Vnitřní omítka" },
                new() { MaterialName = "Cihla plná", DefaultThickness = 300, MinThickness = 200, MaxThickness = 500, Order = 2, Description = "Nosná konstrukce" },
                new() { MaterialName = "Polystyren EPS 20 kg/m³", DefaultThickness = 120, MinThickness = 80, MaxThickness = 200, Order = 3, Description = "Tepelná izolace" },
                new() { MaterialName = "Cementová omítka", DefaultThickness = 8, MinThickness = 5, MaxThickness = 15, Order = 4, Description = "Vnější omítka", IsAdjustable = false }
            }
        };
    }

    private WallTemplate CreateEticsThickTemplate()
    {
        return new WallTemplate
        {
            Name = "ETICS - Silné zateplení",
            Description = "Vnější zateplení pro pasivní domy (180mm)",
            Category = "ETICS", 
            Icon = "bi-shield-check",
            IsPopular = false,
            EstimatedCost = 2200,
            Notes = "Vhodné pro pasivní a nízkoenergetické domy",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Vápenná omítka", DefaultThickness = 15, MinThickness = 10, MaxThickness = 25, Order = 1 },
                new() { MaterialName = "Pórobeton 400 kg/m³", DefaultThickness = 300, MinThickness = 250, MaxThickness = 400, Order = 2 },
                new() { MaterialName = "Polystyren EPS 15 kg/m³", DefaultThickness = 180, MinThickness = 150, MaxThickness = 250, Order = 3 },
                new() { MaterialName = "Cementová omítka", DefaultThickness = 8, IsAdjustable = false, Order = 4 }
            }
        };
    }

    private WallTemplate CreateEticsWoodFiberTemplate()
    {
        return new WallTemplate
        {
            Name = "ETICS - Dřevovlákno",
            Description = "Ekologické zateplení s dřevovláknitými deskami",
            Category = "ETICS",
            Icon = "bi-tree-fill",
            IsPopular = false,
            EstimatedCost = 2800,
            Notes = "Přírodní materiály, dobrá paropropustnost",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Vápenná omítka", DefaultThickness = 15, Order = 1 },
                new() { MaterialName = "Cihla děrovaná", DefaultThickness = 300, Order = 2 },
                new() { MaterialName = "Dřevovláknitá deska", DefaultThickness = 140, MinThickness = 100, MaxThickness = 200, Order = 3 },
                new() { MaterialName = "Vápenná omítka", DefaultThickness = 10, IsAdjustable = false, Order = 4 }
            }
        };
    }

    private WallTemplate CreateSandwichPanelTemplate()
    {
        return new WallTemplate
        {
            Name = "Sendvičový panel - Minerální vata",
            Description = "Průmyslový sendvičový panel s ocelovými plášti",
            Category = "Sandwich",
            Icon = "bi-layers-fill",
            IsPopular = true,
            EstimatedCost = 2000,
            Notes = "Rychlá montáž, vhodné pro haly a sklady",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Polystyren EPS 20 kg/m³", DefaultThickness = 0.7, IsAdjustable = false, Order = 1, Description = "Vnitřní plech 0.7mm" },
                new() { MaterialName = "Minerální vata 60 kg/m³", DefaultThickness = 100, MinThickness = 60, MaxThickness = 200, Order = 2, Description = "Izolační výplň" },
                new() { MaterialName = "Polystyren EPS 20 kg/m³", DefaultThickness = 0.7, IsAdjustable = false, Order = 3, Description = "Vnější plech 0.7mm" }
            }
        };
    }

    private WallTemplate CreateInsulatedSteelPanelTemplate()
    {
        return new WallTemplate
        {
            Name = "Sendvičový panel - PUR jádro",
            Description = "Vysoce izolační panel s polyuretanovou výplní",
            Category = "Sandwich",
            Icon = "bi-layers-half",
            IsPopular = false,
            EstimatedCost = 2500,
            Notes = "Výborné tepelné vlastnosti, tenčí konstrukce",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Polystyren EPS 20 kg/m³", DefaultThickness = 0.7, IsAdjustable = false, Order = 1 },
                new() { MaterialName = "Pěnový polyuretan", DefaultThickness = 80, MinThickness = 60, MaxThickness = 150, Order = 2 },
                new() { MaterialName = "Polystyren EPS 20 kg/m³", DefaultThickness = 0.7, IsAdjustable = false, Order = 3 }
            }
        };
    }

    private WallTemplate CreatePorotermWallTemplate()
    {
        return new WallTemplate
        {
            Name = "Porotherm - Jednoplášť",
            Description = "Moderní jednoplášťová stěna z izolačních cihel",
            Category = "Monolithic",
            Icon = "bi-bricks",
            IsPopular = true,
            EstimatedCost = 1600,
            Notes = "Dobrá tepelná izolace, snadná výstavba",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Vápenná omítka", DefaultThickness = 15, Order = 1 },
                new() { MaterialName = "Porotherm 38 T Profi", DefaultThickness = 380, MinThickness = 300, MaxThickness = 500, Order = 2, IsAdjustable = false },
                new() { MaterialName = "Cementová omítka", DefaultThickness = 20, Order = 3 }
            }
        };
    }

    private WallTemplate CreateAAGWallTemplate()
    {
        return new WallTemplate
        {
            Name = "Pórobeton - AAC stěna",
            Description = "Jednoplášťová stěna z autoklávovaného pórobetonu",
            Category = "Monolithic",
            Icon = "bi-building",
            IsPopular = true,
            EstimatedCost = 1400,
            Notes = "Lehká konstrukce, dobrá tepelná izolace",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Vápenná omítka", DefaultThickness = 15, Order = 1 },
                new() { MaterialName = "Pórobeton 400 kg/m³", DefaultThickness = 375, MinThickness = 300, MaxThickness = 500, Order = 2 },
                new() { MaterialName = "Cementová omítka", DefaultThickness = 20, Order = 3 }
            }
        };
    }

    private WallTemplate CreateBrickWallTemplate()
    {
        return new WallTemplate
        {
            Name = "Klasická cihlová zeď",
            Description = "Tradiční cihlová konstrukce s omítkami",
            Category = "Monolithic",
            Icon = "bi-house-door",
            IsPopular = false,
            EstimatedCost = 1200,
            Notes = "Osvědčená technologie, vysoká tepelná kapacita",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Vápenná omítka", DefaultThickness = 20, Order = 1 },
                new() { MaterialName = "Cihla plná", DefaultThickness = 450, MinThickness = 300, MaxThickness = 600, Order = 2 },
                new() { MaterialName = "Cementová omítka", DefaultThickness = 25, Order = 3 }
            }
        };
    }

    private WallTemplate CreateInteriorPartitionTemplate()
    {
        return new WallTemplate
        {
            Name = "Vnitřní příčka - Standardní",
            Description = "Běžná vnitřní příčka z cihlových bloků",
            Category = "Partition",
            Icon = "bi-distribute-vertical",
            IsPopular = true,
            EstimatedCost = 800,
            Notes = "Vhodné pro dělení místností v bytech",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Vápenná omítka", DefaultThickness = 15, Order = 1 },
                new() { MaterialName = "Cihla děrovaná", DefaultThickness = 115, MinThickness = 80, MaxThickness = 200, Order = 2 },
                new() { MaterialName = "Vápenná omítka", DefaultThickness = 15, Order = 3 }
            }
        };
    }

    private WallTemplate CreateAcousticPartitionTemplate()
    {
        return new WallTemplate
        {
            Name = "Akustická příčka",
            Description = "Zvukově izolační příčka s dvojitou konstrukcí",
            Category = "Partition", 
            Icon = "bi-soundwave",
            IsPopular = false,
            EstimatedCost = 1500,
            Notes = "Vysoká zvuková izolace, vhodné mezi byty",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Vápenná omítka", DefaultThickness = 15, Order = 1 },
                new() { MaterialName = "Cihla děrovaná", DefaultThickness = 115, Order = 2 },
                new() { MaterialName = "Minerální vata 30 kg/m³", DefaultThickness = 60, MinThickness = 40, MaxThickness = 100, Order = 3 },
                new() { MaterialName = "Cihla děrovaná", DefaultThickness = 115, Order = 4 },
                new() { MaterialName = "Vápenná omítka", DefaultThickness = 15, Order = 5 }
            }
        };
    }

    private WallTemplate CreateWoodFrameWallTemplate()
    {
        return new WallTemplate
        {
            Name = "Dřevostavba - Standardní",
            Description = "Rámová dřevostavba s minerální vatou", 
            Category = "WoodFrame",
            Icon = "bi-tree",
            IsPopular = true,
            EstimatedCost = 2000,
            Notes = "Rychlá výstavba, ekologické materiály",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Polystyren EPS 20 kg/m³", DefaultThickness = 12.5, IsAdjustable = false, Order = 1, Description = "Sádrokarton" },
                new() { MaterialName = "Polystyren EPS 20 kg/m³", DefaultThickness = 0.2, IsAdjustable = false, Order = 2, Description = "Parozábrana" },
                new() { MaterialName = "Minerální vata 30 kg/m³", DefaultThickness = 160, MinThickness = 120, MaxThickness = 200, Order = 3, Description = "Izolace v rámu" },
                new() { MaterialName = "Dřevovláknitá deska", DefaultThickness = 15, IsAdjustable = false, Order = 4, Description = "OSB deska" },
                new() { MaterialName = "Polystyren EPS 20 kg/m³", DefaultThickness = 0.2, IsAdjustable = false, Order = 5, Description = "Difuzní folie" },
                new() { MaterialName = "Minerální vata 30 kg/m³", DefaultThickness = 60, MinThickness = 40, MaxThickness = 100, Order = 6, Description = "Kontaktní izolace" },
                new() { MaterialName = "Cementová omítka", DefaultThickness = 20, Order = 7, Description = "Vnější omítka" }
            }
        };
    }

    private WallTemplate CreateCrossLaminatedTimberTemplate()
    {
        return new WallTemplate
        {
            Name = "CLT - Masivní dřevo",
            Description = "Cross-laminated timber s dodatečnou izolací",
            Category = "WoodFrame",
            Icon = "bi-stack",
            IsPopular = false,
            EstimatedCost = 3500,
            Notes = "Moderní dřevostavba, vysoká nosnost",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Polystyren EPS 20 kg/m³", DefaultThickness = 12.5, IsAdjustable = false, Order = 1, Description = "Sádrokarton" },
                new() { MaterialName = "Dřevo smrkové", DefaultThickness = 120, MinThickness = 80, MaxThickness = 200, Order = 2, Description = "CLT panel", IsAdjustable = false },
                new() { MaterialName = "Dřevovláknitá deska", DefaultThickness = 140, MinThickness = 100, MaxThickness = 200, Order = 3, Description = "Vnější izolace" },
                new() { MaterialName = "Vápenná omítka", DefaultThickness = 15, Order = 4, Description = "Vnější omítka" }
            }
        };
    }

    private WallTemplate CreateIndustrialWallTemplate()
    {
        return new WallTemplate
        {
            Name = "Průmyslová hala",
            Description = "Stěna pro průmyslové budovy",
            Category = "Industrial",
            Icon = "bi-building-gear", 
            IsPopular = false,
            EstimatedCost = 1800,
            Notes = "Odolná konstrukce pro průmyslové využití",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Cementová omítka", DefaultThickness = 20, Order = 1 },
                new() { MaterialName = "Cihla plná", DefaultThickness = 300, MinThickness = 200, MaxThickness = 400, Order = 2 },
                new() { MaterialName = "Minerální vata 60 kg/m³", DefaultThickness = 100, MinThickness = 80, MaxThickness = 150, Order = 3 },
                new() { MaterialName = "Cementová omítka", DefaultThickness = 25, Order = 4 }
            }
        };
    }

    private WallTemplate CreateColdStorageWallTemplate()
    {
        return new WallTemplate
        {
            Name = "Chladírenská stěna",
            Description = "Silně izolovaná stěna pro chladírny",
            Category = "Industrial",
            Icon = "bi-snow",
            IsPopular = false,
            EstimatedCost = 3000,
            Notes = "Extrémní tepelná izolace, parotěsnost",
            Layers = new List<TemplateLayer>
            {
                new() { MaterialName = "Polystyren EPS 20 kg/m³", DefaultThickness = 0.5, IsAdjustable = false, Order = 1, Description = "Nerezový plech" },
                new() { MaterialName = "Pěnový polyuretan", DefaultThickness = 200, MinThickness = 150, MaxThickness = 300, Order = 2, Description = "PUR izolace" },
                new() { MaterialName = "Polystyren EPS 20 kg/m³", DefaultThickness = 0.5, IsAdjustable = false, Order = 3, Description = "Vnější plech" }
            }
        };
    }

    /// <summary>
    /// Získá dostupné kategorie šablon
    /// </summary>
    public List<string> GetCategories()
    {
        return new List<string>
        {
            "ETICS",
            "Sandwich", 
            "Monolithic",
            "Partition",
            "WoodFrame",
            "Industrial"
        };
    }

    /// <summary>
    /// Získá český název kategorie
    /// </summary>
    public string GetCategoryDisplayName(string category)
    {
        return category switch
        {
            "ETICS" => "Zateplovací systémy",
            "Sandwich" => "Sendvičové panely",
            "Monolithic" => "Jednoplášťové zdi", 
            "Partition" => "Příčky",
            "WoodFrame" => "Dřevostavby",
            "Industrial" => "Průmyslové",
            _ => category
        };
    }
}