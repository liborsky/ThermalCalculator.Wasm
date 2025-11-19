using ThermalCalculator.Wasm.Models;

namespace ThermalCalculator.Wasm.Services;

/// <summary>
/// Služba pro optimalizaci tloušťky izolace z ekonomického hlediska
/// </summary>
public class InsulationOptimizationService
{
    private const double MinThickness = 1.0; // cm
    private const double MaxThickness = 50.0; // cm
    private const double ThicknessStep = 0.5; // cm

    /// <summary>
    /// Vypočítá optimální tloušťku izolace na základě vstupních parametrů
    /// </summary>
    public InsulationOptimizationResult CalculateOptimization(InsulationOptimizationInput input)
    {
        var result = new InsulationOptimizationResult
        {
            Input = input
        };

        // Výpočet základní spotřeby bez izolace
        // Podle testování podle Gemini příkladu: baseline R ≈ 0.6 m²K/W
        // Standardní neizolovaná zeď (30cm cihla + omítky)
        const double baselineResistance = 0.6; // m²K/W
        const double baselineUValue = 1.0 / baselineResistance; // U = 1.67 W/m²K
        result.BaselineHeatLoss = CalculateAnnualHeatLoss(baselineUValue, input);
        result.BaselineHeatingCost = result.BaselineHeatLoss * input.EnergyCost;

        // Výpočet pro různé tloušťky izolace
        var dataPoints = new List<OptimizationDataPoint>();

        OptimizationDataPoint? previousPoint = null;

        for (double thickness = MinThickness; thickness <= MaxThickness; thickness += ThicknessStep)
        {
            var dataPoint = CalculateDataPoint(thickness, input, result.BaselineHeatingCost, baselineResistance);

            // Vypočítat přírůstkovou návratnost
            if (previousPoint != null)
            {
                double incrementalCost = dataPoint.InvestmentCost - previousPoint.InvestmentCost;
                double incrementalAnnualSavings = dataPoint.AnnualSavings - previousPoint.AnnualSavings;

                dataPoint.IncrementalPayback = incrementalAnnualSavings > 0
                    ? incrementalCost / incrementalAnnualSavings
                    : double.PositiveInfinity;
            }
            else
            {
                // První bod nemá s čím porovnávat
                dataPoint.IncrementalPayback = dataPoint.PaybackPeriod;
            }

            dataPoints.Add(dataPoint);
            previousPoint = dataPoint;
        }

        result.DataPoints = dataPoints;

        // Najít optimální tloušťku pomocí PŘÍRŮSTKOVÉ NÁVRATNOSTI (podle Gemini)
        // Princip: Porovnej PPR_přírůstkovou = (IN_d2 - IN_d1) / (RÚP_d2 - RÚP_d1)
        // STOP když přírůstková návratnost > životnost izolace
        // Pokud se další cm vrátí až po životnosti, už to nemá ekonomický smysl

        // Maximální přijatelná návratnost = cca 80-90% životnosti izolace
        // (chceme rezervu, ne čekat až do úplného konce životnosti)
        double maxAcceptableIncrementalPayback = input.LifetimeYears * 0.85;
        OptimizationDataPoint optimalPoint = dataPoints.First(); // Začneme s 1 cm

        for (int i = 1; i < dataPoints.Count; i++)
        {
            var current = dataPoints[i];
            var previous = dataPoints[i - 1];

            // Přírůstkové náklady (IN_d2 - IN_d1) [Kč]
            double incrementalCost = current.InvestmentCost - previous.InvestmentCost;

            // PŘÍRŮSTKOVÁ CELKOVÁ ÚSPORA ZA ŽIVOTNOST (s inflací pokud je zapnutá) [Kč]
            double incrementalLifetimeSavings = current.CumulativeSavings - previous.CumulativeSavings;

            // STOP podmínka: Když přírůstková celková úspora za životnost je menší než přírůstkové náklady
            // Tj. další 0.5 cm izolace se už za celou životnost nevyplatí
            if (incrementalLifetimeSavings < incrementalCost)
            {
                // Předchozí tloušťka byla optimální
                optimalPoint = previous;
                break;
            }

            // Pokračuj - aktuální tloušťka je zatím optimální
            optimalPoint = current;
        }

        // Pokud jsme prošli všechny body a pořád se to vyplatí,
        // optimalPoint bude poslední bod (MaxThickness)

        result.OptimalThickness = optimalPoint.Thickness;
        result.MaxNetProfit = optimalPoint.NetProfit;
        result.OptimalPaybackPeriod = optimalPoint.PaybackPeriod;
        result.OptimalAnnualSavings = optimalPoint.AnnualSavings;
        result.OptimalRValue = optimalPoint.RValue;
        result.OptimalUValue = optimalPoint.UValue;
        result.OptimalInvestmentCost = optimalPoint.InvestmentCost;

        // Generovat doporučení
        result.Recommendation = GenerateRecommendation(result);

        return result;
    }

    /// <summary>
    /// Vypočítá datový bod pro konkrétní tloušťku
    /// </summary>
    private OptimizationDataPoint CalculateDataPoint(double thickness, InsulationOptimizationInput input, double baselineHeatingCost, double baselineResistance)
    {
        var dataPoint = new OptimizationDataPoint
        {
            Thickness = thickness
        };

        // Tepelný odpor izolace [m²K/W]
        double insulationResistance = (thickness / 100.0) / input.Lambda;

        // Celkový tepelný odpor = původní zeď + izolace [m²K/W]
        double totalResistance = baselineResistance + insulationResistance;

        // Uložit R-value a U-value
        dataPoint.RValue = totalResistance;
        dataPoint.UValue = 1.0 / totalResistance;

        // Roční tepelné ztráty [kWh/rok]
        dataPoint.AnnualHeatLoss = CalculateAnnualHeatLoss(dataPoint.UValue, input);

        // Roční náklady na vytápění [Kč/rok]
        dataPoint.AnnualHeatingCost = dataPoint.AnnualHeatLoss * input.EnergyCost;

        // Roční úspora oproti bez izolace [Kč/rok]
        dataPoint.AnnualSavings = baselineHeatingCost - dataPoint.AnnualHeatingCost;

        // Investiční náklady [Kč] = FIXNÍ NÁKLADY + variabilní náklady na materiál
        // Podle Gemini: IN = fixní (lešení, práce, lepidlo, omítka) + materiál × tloušťka
        dataPoint.InvestmentCost = (input.FixedCost + input.InsulationCostPerCm * thickness) * input.Area;

        // Kumulativní úspora za celou životnost [Kč]
        // Pokud je zapnutá inflace, zohledníme růst cen energie v čase
        if (input.UseInflation && input.AnnualInflationRate > 0)
        {
            // Vypočítáme současnou hodnotu budoucích úspor s inflací
            // Každý rok úspora roste o inflaci (energie zdražuje)
            double cumulativeSavings = 0;
            for (int year = 1; year <= input.LifetimeYears; year++)
            {
                double inflationMultiplier = Math.Pow(1 + input.AnnualInflationRate / 100.0, year);
                double yearSavings = dataPoint.AnnualSavings * inflationMultiplier;
                cumulativeSavings += yearSavings;
            }
            dataPoint.CumulativeSavings = cumulativeSavings;
        }
        else
        {
            // Bez inflace: prostý součet (ceny energie konstantní)
            dataPoint.CumulativeSavings = dataPoint.AnnualSavings * input.LifetimeYears;
        }

        // NPV výpočty - pokud je zapnuté diskontování
        if (input.UseDiscounting && input.DiscountRate > 0)
        {
            // Diskontované kumulativní úspory (NPV úspor)
            // PV = Σ(úspora_N × (1+inflace)^N / (1+diskont)^N)
            double discountedSavings = 0;
            double cumulativeDiscountedSavings = 0;
            int discountedPaybackYear = 0;

            for (int year = 1; year <= input.LifetimeYears; year++)
            {
                // Roční úspora s inflací
                double inflationMultiplier = input.UseInflation && input.AnnualInflationRate > 0
                    ? Math.Pow(1 + input.AnnualInflationRate / 100.0, year)
                    : 1.0;

                double yearSavings = dataPoint.AnnualSavings * inflationMultiplier;

                // Diskontní faktor
                double discountFactor = Math.Pow(1 + input.DiscountRate / 100.0, year);

                // Současná hodnota roční úspory
                double presentValue = yearSavings / discountFactor;

                discountedSavings += presentValue;
                cumulativeDiscountedSavings += presentValue;

                // Najít rok kdy kumulativní diskontované úspory >= investice
                if (discountedPaybackYear == 0 && cumulativeDiscountedSavings >= dataPoint.InvestmentCost)
                {
                    discountedPaybackYear = year;
                }
            }

            dataPoint.DiscountedCumulativeSavings = discountedSavings;
            dataPoint.NetPresentValue = discountedSavings - dataPoint.InvestmentCost;
            dataPoint.DiscountedPaybackPeriod = discountedPaybackYear > 0
                ? discountedPaybackYear
                : double.PositiveInfinity;
        }
        else
        {
            // Bez diskontování: NPV = nominální hodnoty
            dataPoint.DiscountedCumulativeSavings = dataPoint.CumulativeSavings;
            dataPoint.NetPresentValue = dataPoint.NetProfit;
            dataPoint.DiscountedPaybackPeriod = dataPoint.PaybackPeriod;
        }

        // Čistý zisk (úspora - investice) [Kč]
        dataPoint.NetProfit = dataPoint.CumulativeSavings - dataPoint.InvestmentCost;

        // Návratnost investice [roky]
        // Pokud je inflace, musíme spočítat kolik let trvá vrátit investici
        // když roční úspora roste každý rok o inflaci
        if (dataPoint.AnnualSavings > 0)
        {
            if (input.UseInflation && input.AnnualInflationRate > 0)
            {
                // Iterativně najdeme rok, kdy kumulativní úspory překročí investici
                double cumulativeSavings = 0;
                int year = 0;
                while (cumulativeSavings < dataPoint.InvestmentCost && year < 100) // max 100 let
                {
                    year++;
                    double inflationMultiplier = Math.Pow(1 + input.AnnualInflationRate / 100.0, year);
                    double yearSavings = dataPoint.AnnualSavings * inflationMultiplier;
                    cumulativeSavings += yearSavings;
                }
                dataPoint.PaybackPeriod = year < 100 ? year : double.PositiveInfinity;
            }
            else
            {
                // Bez inflace: prostý výpočet
                dataPoint.PaybackPeriod = dataPoint.InvestmentCost / dataPoint.AnnualSavings;
            }
        }
        else
        {
            dataPoint.PaybackPeriod = double.PositiveInfinity;
        }

        return dataPoint;
    }

    /// <summary>
    /// Vypočítá roční tepelné ztráty
    /// </summary>
    /// <param name="uValue">Součinitel prostupu tepla [W/(m²·K)]</param>
    /// <param name="input">Vstupní parametry</param>
    /// <returns>Roční tepelné ztráty [kWh/rok]</returns>
    private double CalculateAnnualHeatLoss(double uValue, InsulationOptimizationInput input)
    {
        // Q = U × ΔT × A × 24h × počet_dnů [Wh]
        // Převod na kWh vydělením 1000
        double heatLossWh = uValue * input.TemperatureDifference * input.Area * 24 * input.HeatingDays;
        return heatLossWh / 1000.0; // kWh
    }

    /// <summary>
    /// Generuje doporučení na základě výsledků optimalizace
    /// </summary>
    private string GenerateRecommendation(InsulationOptimizationResult result)
    {
        var recommendations = new List<string>();

        // Zaokrouhlení na praktické hodnoty (5 cm)
        double practicalThickness = Math.Round(result.OptimalThickness / 5.0) * 5.0;

        if (practicalThickness != result.OptimalThickness)
        {
            var practicalPoint = result.GetDataPointForThickness(practicalThickness);
            if (practicalPoint != null)
            {
                recommendations.Add($"Doporučená tloušťka: {practicalThickness:F0} cm (zaokrouhleno z {result.OptimalThickness:F1} cm na běžně dostupnou tloušťku)");
            }
            else
            {
                recommendations.Add($"Doporučená tloušťka: {result.OptimalThickness:F1} cm");
            }
        }
        else
        {
            recommendations.Add($"Doporučená tloušťka: {result.OptimalThickness:F0} cm");
        }

        // Posouzení návratnosti
        if (result.OptimalPaybackPeriod < 5)
        {
            recommendations.Add("Výborná návratnost investice - vysoce doporučeno!");
        }
        else if (result.OptimalPaybackPeriod < 10)
        {
            recommendations.Add("Dobrá návratnost investice - doporučeno.");
        }
        else if (result.OptimalPaybackPeriod < 15)
        {
            recommendations.Add("Přijatelná návratnost investice.");
        }
        else
        {
            recommendations.Add("Dlouhá návratnost investice - zvažte jiné varianty nebo materiály.");
        }

        // Posouzení U-value podle ČSN
        if (result.OptimalUValue <= 0.15)
        {
            recommendations.Add("Dosahuje výborných tepelně-izolačních vlastností (pasivní dům).");
        }
        else if (result.OptimalUValue <= 0.22)
        {
            recommendations.Add("Dosahuje velmi dobrých tepelně-izolačních vlastností (nízkoenergetický dům).");
        }
        else if (result.OptimalUValue <= 0.30)
        {
            recommendations.Add("Splňuje doporučené hodnoty ČSN 73 0540-2.");
        }
        else
        {
            recommendations.Add("Upozornění: Optimální tloušťka nesplňuje doporučené hodnoty ČSN (U ≤ 0.30 W/m²K). Zvažte levnější materiál nebo vyšší tloušťku.");
        }

        // Informace o úsporách
        recommendations.Add($"Roční úspora: {result.OptimalAnnualSavings:F0} Kč/m²");

        if (result.Input.UseInflation && result.Input.AnnualInflationRate > 0)
        {
            recommendations.Add($"Úspora za {result.Input.LifetimeYears} let: {result.MaxNetProfit:F0} Kč/m² (po odečtení investice, zahrnuje inflaci {result.Input.AnnualInflationRate:F1}%)");
        }
        else
        {
            recommendations.Add($"Úspora za {result.Input.LifetimeYears} let: {result.MaxNetProfit:F0} Kč/m² (po odečtení investice)");
        }

        return string.Join(" ", recommendations);
    }

    /// <summary>
    /// Získá srovnávací data pro běžné tloušťky
    /// </summary>
    public List<OptimizationDataPoint> GetComparisonData(InsulationOptimizationResult result)
    {
        var commonThicknesses = new[] { 10.0, 15.0, 20.0, result.OptimalThickness };
        var comparisonPoints = new List<OptimizationDataPoint>();

        foreach (var thickness in commonThicknesses.Distinct().OrderBy(t => t))
        {
            var point = result.DataPoints.FirstOrDefault(dp => Math.Abs(dp.Thickness - thickness) < ThicknessStep);
            if (point != null)
            {
                comparisonPoints.Add(point);
            }
        }

        return comparisonPoints;
    }
}
