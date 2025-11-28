using ThermalCalculator.Wasm.Models;

namespace ThermalCalculator.Wasm.Services;

public class MaterialService
{
    private List<Material> _customMaterials = new();

    public List<Material> GetCommonMaterials()
    {
        return new List<Material>
        {
            // Zdivo
            new Material { Name = "Cihla plná", Category = "Zdivo", ThermalConductivity = 0.8, Density = 1800, SpecificHeatCapacity = 880, Manufacturer = "Standard", PricePerM3 = 2500, DiffusionResistanceFactor = 5.0 },
            new Material { Name = "Cihla děrovaná", Category = "Zdivo", ThermalConductivity = 0.4, Density = 1400, SpecificHeatCapacity = 880, Manufacturer = "Standard", PricePerM3 = 2200, DiffusionResistanceFactor = 4.0 },
            new Material { Name = "Pórobeton 400 kg/m³", Category = "Zdivo", ThermalConductivity = 0.11, Density = 400, SpecificHeatCapacity = 1000, Manufacturer = "Ytong", PricePerM3 = 1800, DiffusionResistanceFactor = 3.0 },
            new Material { Name = "Pórobeton 600 kg/m³", Category = "Zdivo", ThermalConductivity = 0.15, Density = 600, SpecificHeatCapacity = 1000, Manufacturer = "Ytong", PricePerM3 = 2000, DiffusionResistanceFactor = 3.0 },
            new Material { Name = "Vápenopískové tvárnice", Category = "Zdivo", ThermalConductivity = 0.25, Density = 1200, SpecificHeatCapacity = 880, Manufacturer = "Standard", PricePerM3 = 1900, DiffusionResistanceFactor = 4.0 },
            
            // Zateplené cihly
            new Material { Name = "Porotherm 30 T Profi", Category = "Zdivo", ThermalConductivity = 0.075, Density = 680, SpecificHeatCapacity = 880, Manufacturer = "Wienerberger", PricePerM3 = 3500, DiffusionResistanceFactor = 4.0 },
            new Material { Name = "Porotherm 38 T Profi", Category = "Zdivo", ThermalConductivity = 0.075, Density = 750, SpecificHeatCapacity = 880, Manufacturer = "Wienerberger", PricePerM3 = 3800, DiffusionResistanceFactor = 4.0 },
            new Material { Name = "Porotherm 44 T Profi", Category = "Zdivo", ThermalConductivity = 0.075, Density = 820, SpecificHeatCapacity = 880, Manufacturer = "Wienerberger", PricePerM3 = 4200, DiffusionResistanceFactor = 4.0 },
            new Material { Name = "Porotherm 50 T Profi", Category = "Zdivo", ThermalConductivity = 0.075, Density = 900, SpecificHeatCapacity = 880, Manufacturer = "Wienerberger", PricePerM3 = 4500, DiffusionResistanceFactor = 4.0 },
            new Material { Name = "HELUZ Family 50 2in1", Category = "Zdivo", ThermalConductivity = 0.058, Density = 650, SpecificHeatCapacity = 880, Manufacturer = "HELUZ", PricePerM3 = 4000, DiffusionResistanceFactor = 3.5 },
            new Material { Name = "Ytong Multipor", Category = "Izolace", ThermalConductivity = 0.045, Density = 115, SpecificHeatCapacity = 1000, Manufacturer = "Ytong", PricePerM3 = 3200, DiffusionResistanceFactor = 3.0 },
            
            // Kamenné zdivo
            new Material { Name = "Žula", Category = "Zdivo", ThermalConductivity = 3.1, Density = 2500, SpecificHeatCapacity = 800, Manufacturer = "Přírodní kámen", PricePerM3 = 6000, DiffusionResistanceFactor = 200.0 },
            new Material { Name = "Pískovec", Category = "Zdivo", ThermalConductivity = 1.3, Density = 2200, SpecificHeatCapacity = 900, Manufacturer = "Přírodní kámen", PricePerM3 = 5000, DiffusionResistanceFactor = 30.0 },
            new Material { Name = "Vápenec", Category = "Zdivo", ThermalConductivity = 1.3, Density = 2250, SpecificHeatCapacity = 900, Manufacturer = "Přírodní kámen", PricePerM3 = 4500, DiffusionResistanceFactor = 20.0 },
            new Material { Name = "Břidlice", Category = "Zdivo", ThermalConductivity = 1.7, Density = 2800, SpecificHeatCapacity = 800, Manufacturer = "Přírodní kámen", PricePerM3 = 5500, DiffusionResistanceFactor = 100.0 },
            
            // Dřevo
            new Material { Name = "Dřevo smrkové", Category = "Dřevo", ThermalConductivity = 0.14, Density = 450, SpecificHeatCapacity = 2300, Manufacturer = "Standard", PricePerM3 = 3500, DiffusionResistanceFactor = 2.0 },
            new Material { Name = "Dřevo dubové", Category = "Dřevo", ThermalConductivity = 0.18, Density = 650, SpecificHeatCapacity = 2300, Manufacturer = "Standard", PricePerM3 = 4500, DiffusionResistanceFactor = 2.0 },
            new Material { Name = "Dřevovláknitá deska", Category = "Dřevo", ThermalConductivity = 0.04, Density = 200, SpecificHeatCapacity = 2100, Manufacturer = "Standard", PricePerM3 = 2800, DiffusionResistanceFactor = 1.5 },
            
            // Izolace
            new Material { Name = "Minerální vata 30 kg/m³", Category = "Izolace", ThermalConductivity = 0.035, Density = 30, SpecificHeatCapacity = 1030, Manufacturer = "Isover", PricePerM3 = 1200, DiffusionResistanceFactor = 1.0 },
            new Material { Name = "Minerální vata 60 kg/m³", Category = "Izolace", ThermalConductivity = 0.040, Density = 60, SpecificHeatCapacity = 1030, Manufacturer = "Isover", PricePerM3 = 1400, DiffusionResistanceFactor = 1.0 },
            new Material { Name = "Polystyren EPS 15 kg/m³", Category = "Izolace", ThermalConductivity = 0.035, Density = 15, SpecificHeatCapacity = 1500, Manufacturer = "Styropor", PricePerM3 = 800, DiffusionResistanceFactor = 30.0 },
            new Material { Name = "Polystyren EPS 20 kg/m³", Category = "Izolace", ThermalConductivity = 0.040, Density = 20, SpecificHeatCapacity = 1500, Manufacturer = "Styropor", PricePerM3 = 900, DiffusionResistanceFactor = 30.0 },
            new Material { Name = "Pěnový polyuretan", Category = "Izolace", ThermalConductivity = 0.025, Density = 30, SpecificHeatCapacity = 1400, Manufacturer = "Standard", PricePerM3 = 2500, DiffusionResistanceFactor = 50.0 },
            new Material { Name = "Celulózová izolace", Category = "Izolace", ThermalConductivity = 0.040, Density = 35, SpecificHeatCapacity = 2100, Manufacturer = "Standard", PricePerM3 = 1100, DiffusionResistanceFactor = 1.0 },
            new Material { Name = "NewTherm PIR", Category = "Izolace", ThermalConductivity = 0.023, Density = 38, SpecificHeatCapacity = 1400, Manufacturer = "NewTherm", PricePerM3 = 3500, DiffusionResistanceFactor = 95.0 },
            
            // Omítky
            new Material { Name = "Vápenná omítka", Category = "Omítky", ThermalConductivity = 0.7, Density = 1600, SpecificHeatCapacity = 880, Manufacturer = "Standard", PricePerM3 = 800, DiffusionResistanceFactor = 8.0 },
            new Material { Name = "Cementová omítka", Category = "Omítky", ThermalConductivity = 1.0, Density = 1800, SpecificHeatCapacity = 880, Manufacturer = "Standard", PricePerM3 = 900, DiffusionResistanceFactor = 10.0 },
            new Material { Name = "Sádrová omítka", Category = "Omítky", ThermalConductivity = 0.4, Density = 1200, SpecificHeatCapacity = 1000, Manufacturer = "Standard", PricePerM3 = 1200, DiffusionResistanceFactor = 6.0 },
            
            // Folie a zábrany
            new Material { Name = "Parozábrana PE 0,2 mm", Category = "Folie a zábrany", ThermalConductivity = 0.2, Density = 920, SpecificHeatCapacity = 2300, Manufacturer = "Standard", PricePerM3 = 25000, DiffusionResistanceFactor = 100000.0 },
            new Material { Name = "Parozábrana AL 0,1 mm", Category = "Folie a zábrany", ThermalConductivity = 0.25, Density = 2700, SpecificHeatCapacity = 896, Manufacturer = "Standard", PricePerM3 = 45000, DiffusionResistanceFactor = 200000.0 },
            new Material { Name = "Paropropustná folie", Category = "Folie a zábrany", ThermalConductivity = 0.15, Density = 800, SpecificHeatCapacity = 1800, Manufacturer = "Standard", PricePerM3 = 35000, DiffusionResistanceFactor = 0.1 },
            new Material { Name = "Difúzní fólie s výztuhou", Category = "Folie a zábrany", ThermalConductivity = 0.18, Density = 850, SpecificHeatCapacity = 1800, Manufacturer = "Standard", PricePerM3 = 42000, DiffusionResistanceFactor = 0.2 },
            new Material { Name = "Parozábrana s hliníkovou vrstvou", Category = "Folie a zábrany", ThermalConductivity = 0.3, Density = 950, SpecificHeatCapacity = 1900, Manufacturer = "Standard", PricePerM3 = 55000, DiffusionResistanceFactor = 150000.0 },
            new Material { Name = "Reflexní fólie", Category = "Folie a zábrany", ThermalConductivity = 0.25, Density = 900, SpecificHeatCapacity = 1700, Manufacturer = "Standard", PricePerM3 = 38000, DiffusionResistanceFactor = 80000.0 },
            new Material { Name = "Windstop fólie", Category = "Folie a zábrany", ThermalConductivity = 0.16, Density = 780, SpecificHeatCapacity = 1800, Manufacturer = "Standard", PricePerM3 = 32000, DiffusionResistanceFactor = 5.0 },
            new Material { Name = "Tyvek Housewrap", Category = "Folie a zábrany", ThermalConductivity = 0.14, Density = 750, SpecificHeatCapacity = 1800, Manufacturer = "DuPont", PricePerM3 = 48000, DiffusionResistanceFactor = 0.08 },
            new Material { Name = "Delta-Vent N", Category = "Folie a zábrany", ThermalConductivity = 0.15, Density = 820, SpecificHeatCapacity = 1800, Manufacturer = "Dörken", PricePerM3 = 44000, DiffusionResistanceFactor = 0.15 },
            new Material { Name = "Jutafol N 110 Special", Category = "Folie a zábrany", ThermalConductivity = 0.17, Density = 840, SpecificHeatCapacity = 1800, Manufacturer = "Juta", PricePerM3 = 40000, DiffusionResistanceFactor = 0.12 },

            // Vzduchové mezery neprovětrávané - s fixními R-hodnotami podle ČSN EN ISO 6946
            new Material { Name = "Vzduchová mezera 0-5 mm", Category = "Vzduchové mezery neprovětrávané", ThermalConductivity = 0.024, Density = 1.2, SpecificHeatCapacity = 1005, Manufacturer = "Standard", PricePerM3 = 0.0, DiffusionResistanceFactor = 1.0, IsAirGap = true, FixedThermalResistance = 0.00 },
            new Material { Name = "Vzduchová mezera 5-10 mm", Category = "Vzduchové mezery neprovětrávané", ThermalConductivity = 0.024, Density = 1.2, SpecificHeatCapacity = 1005, Manufacturer = "Standard", PricePerM3 = 0.0, DiffusionResistanceFactor = 1.0, IsAirGap = true, FixedThermalResistance = 0.11 },
            new Material { Name = "Vzduchová mezera 10-15 mm", Category = "Vzduchové mezery neprovětrávané", ThermalConductivity = 0.024, Density = 1.2, SpecificHeatCapacity = 1005, Manufacturer = "Standard", PricePerM3 = 0.0, DiffusionResistanceFactor = 1.0, IsAirGap = true, FixedThermalResistance = 0.13 },
            new Material { Name = "Vzduchová mezera 15-25 mm", Category = "Vzduchové mezery neprovětrávané", ThermalConductivity = 0.024, Density = 1.2, SpecificHeatCapacity = 1005, Manufacturer = "Standard", PricePerM3 = 0.0, DiffusionResistanceFactor = 1.0, IsAirGap = true, FixedThermalResistance = 0.14 },
            new Material { Name = "Vzduchová mezera 25-50 mm", Category = "Vzduchové mezery neprovětrávané", ThermalConductivity = 0.024, Density = 1.2, SpecificHeatCapacity = 1005, Manufacturer = "Standard", PricePerM3 = 0.0, DiffusionResistanceFactor = 1.0, IsAirGap = true, FixedThermalResistance = 0.16 },
            new Material { Name = "Vzduchová mezera 50-300 mm", Category = "Vzduchové mezery neprovětrávané", ThermalConductivity = 0.024, Density = 1.2, SpecificHeatCapacity = 1005, Manufacturer = "Standard", PricePerM3 = 0.0, DiffusionResistanceFactor = 1.0, IsAirGap = true, FixedThermalResistance = 0.17 },

            // Ostatní
            new Material { Name = "Beton C25/30", Category = "Ostatní", ThermalConductivity = 2.0, Density = 2400, SpecificHeatCapacity = 880, Manufacturer = "Standard", PricePerM3 = 1500, DiffusionResistanceFactor = 15.0 },
            new Material { Name = "Železobeton", Category = "Ostatní", ThermalConductivity = 2.3, Density = 2500, SpecificHeatCapacity = 880, Manufacturer = "Standard", PricePerM3 = 2000, DiffusionResistanceFactor = 15.0 },
            new Material { Name = "Sklo", Category = "Ostatní", ThermalConductivity = 1.0, Density = 2500, SpecificHeatCapacity = 800, Manufacturer = "Standard", PricePerM3 = 5000, DiffusionResistanceFactor = 100.0 },
            new Material { Name = "Kov (ocel)", Category = "Ostatní", ThermalConductivity = 50.0, Density = 7850, SpecificHeatCapacity = 460, Manufacturer = "Standard", PricePerM3 = 8000, DiffusionResistanceFactor = 1000.0 }
        };
    }
    
    public List<string> GetCategories()
    {
        return new List<string> { "Zdivo", "Dřevo", "Izolace", "Omítky", "Folie a zábrany", "Vzduchové mezery neprovětrávané", "Ostatní", "Jiné" };
    }

    public List<Material> GetAllMaterials()
    {
        var commonMaterials = GetCommonMaterials();
        var allMaterials = new List<Material>(commonMaterials);
        allMaterials.AddRange(_customMaterials);
        return allMaterials;
    }

    public void SetCustomMaterials(List<Material> customMaterials)
    {
        _customMaterials = customMaterials ?? new List<Material>();
    }

    public List<Material> GetCustomMaterials()
    {
        return _customMaterials.ToList();
    }
} 