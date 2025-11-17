namespace ThermalCalculator.Wasm.Models;

/// <summary>
/// Vstupní parametry pro optimalizaci tloušťky izolace
/// </summary>
public class InsulationOptimizationInput
{
    /// <summary>
    /// Tepelná vodivost izolačního materiálu [W/(m·K)]
    /// </summary>
    public double Lambda { get; set; } = 0.035; // Default EPS

    /// <summary>
    /// Cena energie [Kč/kWh]
    /// </summary>
    public double EnergyCost { get; set; } = 1.2; // Testováno podle Gemini příkladu

    /// <summary>
    /// Cena izolace [Kč/m²/cm] - pouze materiál
    /// </summary>
    public double InsulationCostPerCm { get; set; } = 20.0;

    /// <summary>
    /// Fixní náklady na zateplení [Kč/m²] - lešení, práce, lepidlo, omítka
    /// Tyto náklady jsou stejné bez ohledu na tloušťku izolace
    /// </summary>
    public double FixedCost { get; set; } = 1200.0;

    /// <summary>
    /// Rozdíl teplot mezi interiérem a exteriérem [°C]
    /// </summary>
    public double TemperatureDifference { get; set; } = 18.0; // Testováno podle Gemini

    /// <summary>
    /// Počet otopných dnů v roce [dny]
    /// </summary>
    public int HeatingDays { get; set; } = 150; // Testováno podle Gemini

    /// <summary>
    /// Uvažovaná životnost izolace [roky]
    /// </summary>
    public int LifetimeYears { get; set; } = 20;

    /// <summary>
    /// Plocha konstrukce [m²] - default 1 m² pro specifické výpočty
    /// </summary>
    public double Area { get; set; } = 1.0;

    /// <summary>
    /// Název zvoleného materiálu (pokud byl vybrán z databáze)
    /// </summary>
    public string MaterialName { get; set; } = string.Empty;

    /// <summary>
    /// Zohlednit roční míru inflace při výpočtu úspor
    /// </summary>
    public bool UseInflation { get; set; } = false;

    /// <summary>
    /// Roční míra inflace [%]
    /// </summary>
    public double AnnualInflationRate { get; set; } = 2.0;
}

/// <summary>
/// Datový bod pro graf optimalizace
/// </summary>
public class OptimizationDataPoint
{
    /// <summary>
    /// Tloušťka izolace [cm]
    /// </summary>
    public double Thickness { get; set; }

    /// <summary>
    /// Součinitel prostupu tepla U [W/(m²·K)]
    /// </summary>
    public double UValue { get; set; }

    /// <summary>
    /// Tepelný odpor R [m²K/W]
    /// </summary>
    public double RValue { get; set; }

    /// <summary>
    /// Roční tepelné ztráty [kWh/rok]
    /// </summary>
    public double AnnualHeatLoss { get; set; }

    /// <summary>
    /// Roční náklady na vytápění [Kč/rok]
    /// </summary>
    public double AnnualHeatingCost { get; set; }

    /// <summary>
    /// Roční úspora oproti bez izolace [Kč/rok]
    /// </summary>
    public double AnnualSavings { get; set; }

    /// <summary>
    /// Investiční náklady [Kč]
    /// </summary>
    public double InvestmentCost { get; set; }

    /// <summary>
    /// Kumulativní úspora za celou životnost [Kč]
    /// </summary>
    public double CumulativeSavings { get; set; }

    /// <summary>
    /// Čistý zisk (úspora - investice) [Kč]
    /// </summary>
    public double NetProfit { get; set; }

    /// <summary>
    /// Návratnost investice [roky]
    /// </summary>
    public double PaybackPeriod { get; set; }

    /// <summary>
    /// Přírůstková návratnost (oproti předchozímu kroku) [roky]
    /// Použito pro určení ekonomického optima
    /// </summary>
    public double IncrementalPayback { get; set; }
}

/// <summary>
/// Výsledek optimalizace tloušťky izolace
/// </summary>
public class InsulationOptimizationResult
{
    /// <summary>
    /// Vstupní parametry použité pro výpočet
    /// </summary>
    public InsulationOptimizationInput Input { get; set; } = new();

    /// <summary>
    /// Seznam datových bodů pro graf
    /// </summary>
    public List<OptimizationDataPoint> DataPoints { get; set; } = new();

    /// <summary>
    /// Optimální tloušťka izolace [cm]
    /// </summary>
    public double OptimalThickness { get; set; }

    /// <summary>
    /// Maximální čistý zisk při optimální tloušťce [Kč]
    /// </summary>
    public double MaxNetProfit { get; set; }

    /// <summary>
    /// Návratnost investice při optimální tloušťce [roky]
    /// </summary>
    public double OptimalPaybackPeriod { get; set; }

    /// <summary>
    /// Roční úspora při optimální tloušťce [Kč/rok]
    /// </summary>
    public double OptimalAnnualSavings { get; set; }

    /// <summary>
    /// U-value při optimální tloušťce [W/(m²·K)]
    /// </summary>
    public double OptimalUValue { get; set; }

    /// <summary>
    /// R-value při optimální tloušťce [m²K/W]
    /// </summary>
    public double OptimalRValue { get; set; }

    /// <summary>
    /// Investiční náklady při optimální tloušťce [Kč]
    /// </summary>
    public double OptimalInvestmentCost { get; set; }

    /// <summary>
    /// Roční tepelné ztráty bez izolace [kWh/rok]
    /// </summary>
    public double BaselineHeatLoss { get; set; }

    /// <summary>
    /// Roční náklady na vytápění bez izolace [Kč/rok]
    /// </summary>
    public double BaselineHeatingCost { get; set; }

    /// <summary>
    /// Doporučení na základě výsledků
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;

    /// <summary>
    /// Získat datový bod pro konkrétní tloušťku
    /// </summary>
    public OptimizationDataPoint? GetDataPointForThickness(double thickness)
    {
        return DataPoints.FirstOrDefault(dp => Math.Abs(dp.Thickness - thickness) < 0.1);
    }
}

/// <summary>
/// Předdefinované scénáře pro rychlý výběr
/// </summary>
public class OptimizationPreset
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Lambda { get; set; }
    public double InsulationCostPerCm { get; set; }
    public string MaterialCategory { get; set; } = string.Empty;

    public static List<OptimizationPreset> GetPresets()
    {
        return new List<OptimizationPreset>
        {
            new OptimizationPreset
            {
                Name = "Polystyren EPS",
                Description = "Běžný fasádní polystyren",
                Lambda = 0.035,
                InsulationCostPerCm = 20.0, // Pouze materiál, fixní náklady se přidají zvlášť
                MaterialCategory = "Izolace"
            },
            new OptimizationPreset
            {
                Name = "PUR deska",
                Description = "Polyuretanová deska - nejlepší izolace",
                Lambda = 0.023,
                InsulationCostPerCm = 30.0, // Dražší materiál
                MaterialCategory = "Izolace"
            },
            new OptimizationPreset
            {
                Name = "Minerální vata",
                Description = "Běžná fasádní minerální vata",
                Lambda = 0.040,
                InsulationCostPerCm = 22.0,
                MaterialCategory = "Izolace"
            },
            new OptimizationPreset
            {
                Name = "Pěnový polyuretan (PUR pěna)",
                Description = "Stříkaná PUR pěna, vysoký výkon",
                Lambda = 0.025,
                InsulationCostPerCm = 28.0,
                MaterialCategory = "Izolace"
            },
            new OptimizationPreset
            {
                Name = "Dřevovláknitá deska",
                Description = "Ekologická izolace",
                Lambda = 0.040,
                InsulationCostPerCm = 35.0, // Dražší ekologický materiál
                MaterialCategory = "Dřevo"
            }
        };
    }
}
