namespace ThermalCalculator.Wasm.Models;

public class Material
{
    public string Name { get; set; } = string.Empty;
    public double ThermalConductivity { get; set; } // λ (lambda) v W/(m·K)
    public double Density { get; set; } = 0; // hustota v kg/m³
    public double SpecificHeatCapacity { get; set; } = 0; // měrná tepelná kapacita v J/(kg·K)
    public string Category { get; set; } = string.Empty; // kategorie materiálu
    public string Manufacturer { get; set; } = string.Empty; // výrobce
    public string ProductCode { get; set; } = string.Empty; // kód výrobku
    public double PricePerM3 { get; set; } = 0; // cena za m³ v Kč
    public double Thickness { get; set; } // tloušťka v metrech
    public string Unit { get; set; } = "m"; // jednotka tloušťky
    public double DiffusionResistanceFactor { get; set; } = 0; // faktor difúzního odporu μ (mí)

    // Vlastnosti pro vzduchové mezery
    public bool IsAirGap { get; set; } = false; // indikuje, zda se jedná o vzduchovou mezeru
    public double? FixedThermalResistance { get; set; } = null; // fixní R-hodnota pro vzduchové mezery v m²·K/W

    public double ThermalResistance => Thickness / ThermalConductivity; // R = d/λ v m²·K/W
}

public class WallLayer
{
    public Material Material { get; set; } = new();
    public double Thickness { get; set; } // v milimetrech

    public double ThermalResistance
    {
        get
        {
            // Pokud je to vzduchová mezera s fixní R-hodnotou, použij ji
            if (Material.IsAirGap && Material.FixedThermalResistance.HasValue)
            {
                return Material.FixedThermalResistance.Value;
            }

            // Jinak standardní výpočet R = d/λ
            return (Thickness / 1000.0) / Material.ThermalConductivity; // převod mm na m
        }
    }

    public double DiffusionResistance => (Thickness / 1000.0) * Material.DiffusionResistanceFactor; // difúzní odpor v m
}

public class WallAssembly
{
    public string Name { get; set; } = string.Empty;
    public List<WallLayer> Layers { get; set; } = new();
    public double InternalSurfaceResistance { get; set; } = 0.13; // Rsi v m²·K/W
    public double ExternalSurfaceResistance { get; set; } = 0.04; // Rse v m²·K/W
    
    // Klimatické podmínky pro analýzu kondenzace
    public double InternalTemperature { get; set; } = 20.0; // °C
    public double ExternalTemperature { get; set; } = -15.0; // °C
    public double InternalHumidity { get; set; } = 50.0; // %
    public double ExternalHumidity { get; set; } = 80.0; // %
    
    public double TotalThermalResistance
    {
        get
        {
            double total = InternalSurfaceResistance + ExternalSurfaceResistance;
            foreach (var layer in Layers)
            {
                total += layer.ThermalResistance;
            }
            return total;
        }
    }
    
    public double ThermalTransmittance => 1.0 / TotalThermalResistance; // U = 1/R v W/(m²·K)
    
    // Tepelná kapacita skladby v J/(m²·K)
    public double ThermalCapacity => Layers.Sum(l => (l.Thickness / 1000.0) * l.Material.Density * l.Material.SpecificHeatCapacity);
    
    // Fázový posun v hodinách (čas, za který teplo projde skladbou)
    public double PhaseShift => ThermalCapacity / (ThermalTransmittance * 3600);
    
    // Celková cena skladby za m² v Kč
    public double TotalCost => Layers.Sum(l => (l.Thickness / 1000.0) * l.Material.PricePerM3);
    
    // Celkový difúzní odpor skladby v m
    public double TotalDiffusionResistance => Layers.Sum(l => l.DiffusionResistance);
    
    // === DYNAMICKÉ TEPELNÉ VLASTNOSTI ===
    
    // Teplotní útlum ν (nu) - bezrozměrné číslo podle ČSN 73 0540-4
    public double TemperatureDamping => CalculateTemperatureDamping();
    
    // Amplituda dekremetu v procentech
    public double AmplitudeDecrement => (1.0 - 1.0 / Math.Max(TemperatureDamping, 1.0)) * 100;
    
    // Dynamický fázový posun v hodinách podle komplexní tepelné admitance
    public double DynamicPhaseShift => CalculateDynamicPhaseShift();
    
    // Periodická tepelná penetrace [m]
    public double ThermalPenetrationDepth => CalculateThermalPenetrationDepth();
    
    // Hodnocení tepelné setrvačnosti
    public ThermalInertiaRating GetThermalInertiaRating()
    {
        return TemperatureDamping switch
        {
            >= 15 => ThermalInertiaRating.VeryHigh,
            >= 10 => ThermalInertiaRating.High,
            >= 5 => ThermalInertiaRating.Medium,
            >= 2 => ThermalInertiaRating.Low,
            _ => ThermalInertiaRating.VeryLow
        };
    }
    
    // Teplotní stabilita v létě
    public SummerComfortRating GetSummerComfortRating()
    {
        return (TemperatureDamping, DynamicPhaseShift) switch
        {
            ( >= 10, >= 8) => SummerComfortRating.Excellent,
            ( >= 5, >= 6) => SummerComfortRating.Good,
            ( >= 3, >= 4) => SummerComfortRating.Adequate,
            ( >= 2, >= 2) => SummerComfortRating.Poor,
            _ => SummerComfortRating.Inadequate
        };
    }
    
    // Metody pro výpočet rosného bodu
    public class DewPointCalculation
    {
        public double InternalTemperature { get; set; } = 20.0; // °C
        public double ExternalTemperature { get; set; } = -15.0; // °C
        public double InternalHumidity { get; set; } = 50.0; // %
        public double ExternalHumidity { get; set; } = 80.0; // %
        
        public List<DewPointLayer> LayerCalculations { get; set; } = new();
        public bool HasCondensation { get; set; } = false;
        public double CondensationDepth { get; set; } = 0.0; // mm od vnitřního povrchu
        public List<CondensationZone> CondensationZones { get; set; } = new();
    }
    
    public class CondensationZone
    {
        public string MaterialName { get; set; } = string.Empty;
        public double StartDepth { get; set; } = 0.0; // mm od vnitřního povrchu
        public double EndDepth { get; set; } = 0.0; // mm od vnitřního povrchu
        public double CondensationAmount { get; set; } = 0.0; // kg/(m²·rok)
        public string Severity { get; set; } = string.Empty; // "Nízké", "Střední", "Vysoké"
    }
    
    public class DewPointLayer
    {
        public string MaterialName { get; set; } = string.Empty;
        public double Thickness { get; set; } = 0.0; // mm
        public double TemperatureAtStart { get; set; } = 0.0; // °C
        public double TemperatureAtEnd { get; set; } = 0.0; // °C
        public double VaporPressureAtStart { get; set; } = 0.0; // Pa
        public double VaporPressureAtEnd { get; set; } = 0.0; // Pa
        public double DewPointTemperatureAtStart { get; set; } = 0.0; // °C
        public double DewPointTemperatureAtEnd { get; set; } = 0.0; // °C
        public bool HasCondensation { get; set; } = false;
    }
    
    public DewPointCalculation CalculateDewPoint()
    {
        var calculation = new DewPointCalculation
        {
            InternalTemperature = this.InternalTemperature,
            ExternalTemperature = this.ExternalTemperature,
            InternalHumidity = this.InternalHumidity,
            ExternalHumidity = this.ExternalHumidity
        };
        
        if (Layers.Count == 0)
            return calculation;
        
        // Výpočet tepelného gradientu s vlastními teplotami
        var temperatureProfile = CalculateTemperatureProfile();
        
        // Výpočet tlakového gradientu vodní páry s vlastními podmínkami
        var vaporPressureProfile = CalculateVaporPressureProfile();
        
        // Porovnání teplot a tlaků pro nalezení rosného bodu
        calculation.LayerCalculations = CompareTemperatureAndVaporPressure(temperatureProfile, vaporPressureProfile);
        
        // Kontrola, zda dochází ke kondenzaci
        calculation.HasCondensation = calculation.LayerCalculations.Any(l => l.HasCondensation);
        
        // Výpočet přesných zón kondenzace
        calculation.CondensationZones = CalculateCondensationZones(temperatureProfile, vaporPressureProfile);
        
        return calculation;
    }
    
    private List<(double depth, double temperature)> CalculateTemperatureProfile()
    {
        var profile = new List<(double depth, double temperature)>();
        double currentDepth = 0.0; // mm od vnitřního povrchu
        double currentTemperature = InternalTemperature; // vlastní vnitřní teplota
        
        // Celkový tepelný odpor skladby (bez povrchových odporů)
        double totalThermalResistance = TotalThermalResistance - InternalSurfaceResistance - ExternalSurfaceResistance;
        
        // Tepelný tok (W/m²)
        double heatFlux = (InternalTemperature - ExternalTemperature) / TotalThermalResistance;
        
        // Přidat vnitřní hranici
        profile.Add((currentDepth, currentTemperature));
        
        // Výpočet teplotního profilu přes jednotlivé vrstvy
        foreach (var layer in Layers)
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
    
    private List<(double depth, double vaporPressure)> CalculateVaporPressureProfile()
    {
        var profile = new List<(double depth, double vaporPressure)>();
        double currentDepth = 0.0; // mm od vnitřního povrchu
        
        // Převod vlhkosti z % na desetinné číslo
        double internalHumidityDecimal = InternalHumidity / 100.0;
        double externalHumidityDecimal = ExternalHumidity / 100.0;
        
        // Saturační tlaky vodní páry na hranicích
        double internalSaturationPressure = CalculateSaturationVaporPressure(InternalTemperature);
        double externalSaturationPressure = CalculateSaturationVaporPressure(ExternalTemperature);
        
        // Skutečné tlaky vodní páry na hranicích
        double internalVaporPressure = internalSaturationPressure * internalHumidityDecimal;
        double externalVaporPressure = externalSaturationPressure * externalHumidityDecimal;
        
        // Celkový difúzní odpor skladby (bez povrchových odporů)
        double totalDiffusionResistance = TotalDiffusionResistance;
        
        // Difúzní tok vodní páry (kg/(m²·s))
        double vaporFlux = (internalVaporPressure - externalVaporPressure) / totalDiffusionResistance;
        
        // Přidat vnitřní hranici
        profile.Add((currentDepth, internalVaporPressure));
        
        // Výpočet tlakového profilu přes jednotlivé vrstvy
        double currentVaporPressure = internalVaporPressure;
        
        foreach (var layer in Layers)
        {
            // Difúzní odpor vrstvy
            double layerDiffusionResistance = layer.DiffusionResistance;
            
            // Pokles tlaku vodní páry přes vrstvu
            double vaporPressureDrop = vaporFlux * layerDiffusionResistance;
            
            // Nový tlak na konci vrstvy
            currentVaporPressure -= vaporPressureDrop;
            currentDepth += layer.Thickness;
            
            profile.Add((currentDepth, currentVaporPressure));
        }
        
        return profile;
    }
    
    private double CalculateSaturationVaporPressure(double temperature)
    {
        // Vylepšený Magnusův vzorec pro výpočet saturačního tlaku vodní páry
        // Platí pro teploty od -40°C do +50°C
        if (temperature >= 0)
        {
            // Pro teploty ≥ 0°C
            return 610.78 * Math.Exp((17.2694 * temperature) / (temperature + 238.3));
        }
        else
        {
            // Pro teploty < 0°C (nad ledem)
            return 610.78 * Math.Exp((21.875 * temperature) / (temperature + 265.5));
        }
    }
    
    private List<DewPointLayer> CompareTemperatureAndVaporPressure(
        List<(double depth, double temperature)> tempProfile,
        List<(double depth, double vaporPressure)> vaporProfile)
    {
        var layerCalculations = new List<DewPointLayer>();
        
        for (int i = 0; i < Layers.Count; i++)
        {
            var layer = Layers[i];
            var calculation = new DewPointLayer
            {
                MaterialName = layer.Material.Name,
                Thickness = layer.Thickness,
                TemperatureAtStart = tempProfile[i].temperature,
                TemperatureAtEnd = tempProfile[i + 1].temperature,
                VaporPressureAtStart = vaporProfile[i].vaporPressure,
                VaporPressureAtEnd = vaporProfile[i + 1].vaporPressure
            };
            
            // Výpočet teploty rosného bodu pro tlaky na začátku a konci vrstvy
            calculation.DewPointTemperatureAtStart = CalculateDewPointTemperature(calculation.VaporPressureAtStart);
            calculation.DewPointTemperatureAtEnd = CalculateDewPointTemperature(calculation.VaporPressureAtEnd);
            
            // Kontrola kondenzace - pokud je teplota nižší než teplota rosného bodu
            calculation.HasCondensation = calculation.TemperatureAtStart < calculation.DewPointTemperatureAtStart ||
                                        calculation.TemperatureAtEnd < calculation.DewPointTemperatureAtEnd;
            
            layerCalculations.Add(calculation);
        }
        
        return layerCalculations;
    }
    
    private double CalculateDewPointTemperature(double vaporPressure)
    {
        // Vylepšený výpočet teploty rosného bodu
        // Pro tlaky odpovídající teplotám ≥ 0°C
        if (vaporPressure >= 610.78)
        {
            double a = 17.2694;
            double b = 238.3;
            return (b * Math.Log(vaporPressure / 610.78)) / (a - Math.Log(vaporPressure / 610.78));
        }
        else
        {
            // Pro tlaky odpovídající teplotám < 0°C
            double a = 21.875;
            double b = 265.5;
            return (b * Math.Log(vaporPressure / 610.78)) / (a - Math.Log(vaporPressure / 610.78));
        }
    }
    
    private List<CondensationZone> CalculateCondensationZones(
        List<(double depth, double temperature)> tempProfile,
        List<(double depth, double vaporPressure)> vaporProfile)
    {
        var zones = new List<CondensationZone>();
        
        // Počet bodů pro detailní analýzu v každé vrstvě
        const int pointsPerLayer = 10;
        
        for (int layerIndex = 0; layerIndex < Layers.Count; layerIndex++)
        {
            var layer = Layers[layerIndex];
            double layerStartDepth = tempProfile[layerIndex].depth;
            double layerEndDepth = tempProfile[layerIndex + 1].depth;
            double layerThickness = layerEndDepth - layerStartDepth;
            
            bool hasCondensationInLayer = false;
            double condensationStart = 0.0;
            double condensationEnd = 0.0;
            double totalCondensation = 0.0;
            
            // Detailní analýza vrstvy
            for (int i = 0; i <= pointsPerLayer; i++)
            {
                double relativePosition = (double)i / pointsPerLayer;
                double currentDepth = layerStartDepth + relativePosition * layerThickness;
                
                // Interpolace teploty a tlaku páry
                double temperature = tempProfile[layerIndex].temperature + 
                                  relativePosition * (tempProfile[layerIndex + 1].temperature - tempProfile[layerIndex].temperature);
                double vaporPressure = vaporProfile[layerIndex].vaporPressure + 
                                     relativePosition * (vaporProfile[layerIndex + 1].vaporPressure - vaporProfile[layerIndex].vaporPressure);
                
                // Výpočet teploty rosného bodu
                double dewPointTemperature = CalculateDewPointTemperature(vaporPressure);
                
                // Kontrola kondenzace
                bool isCondensing = temperature < dewPointTemperature;
                
                if (isCondensing)
                {
                    if (!hasCondensationInLayer)
                    {
                        condensationStart = currentDepth;
                        hasCondensationInLayer = true;
                    }
                    condensationEnd = currentDepth;
                    
                    // Výpočet množství kondenzace (zjednodušeně)
                    double condensationRate = (dewPointTemperature - temperature) * 0.001; // kg/(m²·den)
                    totalCondensation += condensationRate;
                }
            }
            
            // Přidat zónu kondenzace, pokud existuje
            if (hasCondensationInLayer)
            {
                var zone = new CondensationZone
                {
                    MaterialName = layer.Material.Name,
                    StartDepth = condensationStart,
                    EndDepth = condensationEnd,
                    CondensationAmount = totalCondensation * 365, // kg/(m²·rok)
                    Severity = GetCondensationSeverity(totalCondensation * 365)
                };
                zones.Add(zone);
            }
        }
        
        return zones;
    }
    
    private string GetCondensationSeverity(double annualCondensation)
    {
        return annualCondensation switch
        {
            < 0.5 => "Nízké",
            < 2.0 => "Střední",
            _ => "Vysoké"
        };
    }
    
    // === IMPLEMENTACE DYNAMICKÝCH VÝPOČTŮ ===
    
    private double CalculateTemperatureDamping()
    {
        if (Layers.Count == 0) return 1.0;
        
        // Výpočet podle ČSN 73 0540-4
        // Zjednodušený výpočet pro 24h periodu
        const double period = 24 * 3600; // 24 hodin v sekundách
        const double omega = 2 * Math.PI / period; // úhlová frekvence
        
        double totalAdmittance = 0.0;
        double totalPhase = 0.0;
        
        foreach (var layer in Layers)
        {
            var thickness = layer.Thickness / 1000.0; // převod na metry
            var lambda = layer.Material.ThermalConductivity;
            var rho = layer.Material.Density;
            var c = layer.Material.SpecificHeatCapacity;
            
            // Tepelná difuzivita
            var a = lambda / (rho * c);
            
            // Penetrační hloubka
            var delta = Math.Sqrt(2 * a / omega);
            
            // Komplexní tepelná admitance vrstvy
            var beta = thickness / delta;
            var admittanceAmp = lambda / delta * Math.Sqrt(2) / (Math.Cosh(beta) + Math.Cos(beta));
            var phaseShift = Math.Atan2(Math.Sinh(beta) - Math.Sin(beta), Math.Cosh(beta) + Math.Cos(beta));
            
            totalAdmittance += admittanceAmp;
            totalPhase += phaseShift;
        }
        
        // Teplotní útlum jako poměr vnější a vnitřní amplitudy
        var dampingFactor = Math.Max(1.0, totalAdmittance * TotalThermalResistance);
        
        return Math.Min(dampingFactor, 100.0); // omezení na rozumnou hodnotu
    }
    
    private double CalculateDynamicPhaseShift()
    {
        if (Layers.Count == 0) return 0.0;
        
        const double period = 24 * 3600; // 24 hodin
        const double omega = 2 * Math.PI / period;
        
        double totalPhase = 0.0;
        
        foreach (var layer in Layers)
        {
            var thickness = layer.Thickness / 1000.0;
            var lambda = layer.Material.ThermalConductivity;
            var rho = layer.Material.Density;
            var c = layer.Material.SpecificHeatCapacity;
            
            var a = lambda / (rho * c);
            var delta = Math.Sqrt(2 * a / omega);
            var beta = thickness / delta;
            
            // Fázový posun vrstvy
            var layerPhase = Math.Atan2(Math.Sinh(beta) - Math.Sin(beta), Math.Cosh(beta) + Math.Cos(beta));
            totalPhase += layerPhase;
        }
        
        // Převod na hodiny
        return totalPhase / omega * 3600 / (2 * Math.PI);
    }
    
    private double CalculateThermalPenetrationDepth()
    {
        if (Layers.Count == 0) return 0.0;
        
        const double period = 24 * 3600;
        const double omega = 2 * Math.PI / period;
        
        // Průměrná penetrační hloubka konstrukce
        double totalPenetration = 0.0;
        double totalThickness = 0.0;
        
        foreach (var layer in Layers)
        {
            var thickness = layer.Thickness / 1000.0;
            var lambda = layer.Material.ThermalConductivity;
            var rho = layer.Material.Density;
            var c = layer.Material.SpecificHeatCapacity;
            
            var a = lambda / (rho * c);
            var delta = Math.Sqrt(2 * a / omega);
            
            totalPenetration += delta * thickness;
            totalThickness += thickness;
        }
        
        return totalThickness > 0 ? totalPenetration / totalThickness : 0.0;
    }
}

// === ENUM TYPY PRO HODNOCENÍ ===

public enum ThermalInertiaRating
{
    VeryLow,    // < 2
    Low,        // 2-5
    Medium,     // 5-10
    High,       // 10-15
    VeryHigh    // > 15
}

public enum SummerComfortRating
{
    Inadequate, // Nedostatečné - riziko přehřívání
    Poor,       // Slabé
    Adequate,   // Přijatelné
    Good,       // Dobré
    Excellent   // Výborné - optimální letní komfort
} 