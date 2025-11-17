# ThermalCalculator - Blazor WebAssembly

## 🎯 Co je ThermalCalculator

**Webová aplikace pro výpočet tepelného odporu a ekonomickou optimalizaci zateplení stavebních konstrukcí.**

Aplikace pomáhá projektantům, stavebním inženýrům a běžným uživatelům:
- Vypočítat tepelný odpor (R-hodnotu) a součinitel prostupu tepla (U-hodnotu) vícevrstvých konstrukcí
- Analyzovat kondenzaci vodní páry v konstrukci (rosný bod)
- Najít ekonomicky optimální tloušťku tepelné izolace s ohledem na cenu materiálu, cenu energie a životnost
- Generovat PDF reporty s výsledky
- Spravovat vlastní materiálovou databázi

## 🚀 Proč Blazor WebAssembly?

Tato verze běží **100% v prohlížeči** - není potřeba server!

✅ **Funguje offline** - po prvním načtení funguje bez internetu
✅ **Cross-platform** - Windows, Mac, Linux, iPhone, Android
✅ **PWA** - Instalovatelná jako mobilní/desktop aplikace
✅ **Žádné náklady na hosting** - statické soubory lze hostovat kdekoli
✅ **Bezpečné** - všechna data pouze v prohlížeči (localStorage)

### Rozdíly oproti Blazor Server verzi:
- ❌ **Žádný .NET runtime na serveru** - vše běží v browseru přes WebAssembly
- ⚠️ **Pomalejší první načtení** (~5-10 MB download) - ale pak bleskově rychlé
- ✅ **Offline režim** - funguje bez připojení
- ⚠️ **PDF export jinak** - používá jsPDF (JavaScript knihovna) místo iText7

## 📱 Hlavní funkce

### 1. **Výpočet tepelného odporu** (stránka: Tepelný odpor)
- Přidávání vrstev konstrukce (materiál + tloušťka)
- Automatický výpočet:
  - **R-hodnota** (m²·K/W) - tepelný odpor
  - **U-hodnota** (W/(m²·K)) - součinitel prostupu tepla
  - Splnění české normy ČSN 73 0540-2
- **Kondenzační analýza**:
  - Teplotní profil napříč konstrukcí
  - Profil tlaku vodní páry
  - Detekce kondenzačních zón (rosný bod)
  - Grafická vizualizace

### 2. **Optimalizace tloušťky izolace** (stránka: Optimalizace izolace)
**Ekonomická analýza** - Jaká tloušťka izolace se nejvíce vyplatí?

Vstupy:
- Materiál izolace (λ - tepelná vodivost)
- Cena materiálu (Kč/m²/cm)
- Fixní náklady (lešení, práce, omítka)
- Cena energie (Kč/kWh)
- **Roční míra inflace energie** (%)
- Životnost izolace (roky)
- Rozdíl teplot, počet otopných dnů

Výstupy:
- **Optimální tloušťka** izolace
- Graf: Čistý zisk vs. tloušťka
- Graf: Roční úspora vs. tloušťka
- Tabulka: Investice, úspora, návratnost, čistý zisk pro každou tloušťku
- **Zohlednění inflace** - přírůstková analýza podle metodiky Gemini AI

### 3. **Příklady** (stránka: Příklady)
Předpřipravené skladby zdi:
- Nevyhovující konstrukce (0.80 W/(m²·K))
- Minerální vata (0.30, 0.20, 0.15 W/(m²·K))
- Polystyren EPS (0.30, 0.20, 0.15 W/(m²·K))
- Dřevostavba (0.15 W/(m²·K))

Kliknutím se načte příklad do kalkulačky.

### 4. **Vlastní materiály** (stránka: Vlastní materiály)
CRUD operace pro materiály:
- Přidání vlastního materiálu (název, kategorie, λ, hustota, cena, μ-faktor)
- Úprava existujícího
- Mazání
- **Úložiště: localStorage** (data v prohlížeči)

### 5. **Statistiky** (stránka: Statistiky)
Tracking využití aplikace:
- Počet výpočtů
- Počet vytvořených vrstev
- Nejpoužívanější materiály
- Exporty do PDF
- **Úložiště: localStorage**

### 6. **3D Vizualizace** (komponenta)
Interaktivní 3D řez stěnou s teplotními gradienty:
- Barevné schéma (Modrá-Červená, Duha, Tepelná kamera, Černobílé)
- Zobrazení teplot v každé vrstvě
- Legenda s popisem materiálů

### 7. **PDF Export**
Generování reportů s výsledky:
- Přehled vrstev
- Klimatické podmínky
- Výsledky (R, U hodnota)
- Hodnocení podle normy
- **Technologie: jsPDF** (JavaScript knihovna)

### 8. **PWA Podpora**
Progressive Web App:
- Instalace na plochu/home screen
- Offline režim
- Service Worker cache
- Manifest s ikonami (192x192, 512x512)

## 🏗️ Architektura

### Technologie:
- **Frontend**: Blazor WebAssembly (.NET 8)
- **UI Framework**: Radzen Blazor (Material theme)
- **PDF**: jsPDF (JavaScript)
- **Storage**: Browser localStorage
- **PWA**: Service Worker

### Struktura projektu:

```
ThermalCalculator.Wasm/
├── Models/
│   ├── Material.cs                    # Stavební materiál (λ, ρ, μ, cena)
│   ├── WallLayer.cs                   # Vrstva konstrukce (materiál + tloušťka)
│   ├── WallAssembly.cs                # Kompletní skladba + výpočty
│   ├── InsulationOptimizationInput.cs # Parametry pro ekonomickou optimalizaci
│   └── ...
│
├── Services/
│   ├── MaterialService.cs             # Databáze materiálů + localStorage
│   ├── WallAssemblyService.cs         # Správa skladeb
│   ├── InsulationOptimizationService.cs # Ekonomická optimalizace (přírůstková analýza)
│   ├── PdfExportService.cs            # PDF generování (jsPDF přes JS Interop)
│   ├── StatisticsService.cs           # Tracking využití
│   └── WallTemplateService.cs         # Předpřipravené příklady
│
├── Pages/
│   ├── Index.razor                    # Úvodní stránka
│   ├── ThermalResistance.razor        # Hlavní výpočet R/U hodnoty
│   ├── InsulationOptimization.razor   # Ekonomická optimalizace
│   ├── Examples.razor                 # Předpřipravené skladby
│   ├── CustomMaterials.razor          # Správa vlastních materiálů
│   ├── Statistics.razor               # Statistiky použití
│   └── Error.razor                    # Chybová stránka
│
├── Components/
│   ├── WallVisualization3D.razor      # 3D vizualizace řezu
│   ├── ThermalGradientControls.razor  # Ovládání vizualizace
│   ├── VisualizationLegend.razor      # Legenda k 3D vizualizaci
│   └── ...
│
├── Shared/
│   ├── MainLayout.razor               # Hlavní layout (Radzen)
│   └── NavMenu.razor                  # Boční menu
│
└── wwwroot/
    ├── js/
    │   └── pdfGenerator.js            # jsPDF implementace
    ├── css/
    │   └── site.css                   # Custom CSS
    ├── icons/
    │   ├── icon-192.png               # PWA ikony
    │   └── icon-512.png
    ├── fonts/
    │   └── DejaVu*.ttf                # Fonty pro PDF (české znaky)
    ├── manifest.json                  # PWA manifest
    └── service-worker.js              # Service Worker (auto-generovaný)
```

## 🔬 Klíčové algoritmy

### 1. Tepelný odpor (R-hodnota)
```
R = d / λ
d = tloušťka vrstvy [m]
λ = tepelná vodivost [W/(m·K)]

R_total = R_si + Σ(R_vrstvy) + R_se
R_si = 0.13 m²·K/W (vnitřní povrchový odpor)
R_se = 0.04 m²·K/W (vnější povrchový odpor)
```

### 2. Součinitel prostupu tepla (U-hodnota)
```
U = 1 / R_total [W/(m²·K)]
```

### 3. Kondenzační analýza
- **Teplotní profil**: Výpočet teploty v každém bodě konstrukce pomocí tepelného toku
- **Tlak vodní páry**: Magnusova rovnice pro výpočet saturovaného tlaku
- **Kondenzace**: Porovnání skutečného tlaku s nasyceným tlakem (rosný bod)

### 4. Ekonomická optimalizace (GEMINI metodika)
**Přírůstková analýza** - porovnávání přírůstků mezi sousedními tloušťkami:

```
IN(d) = FC + d * MC    [Kč/m²]
FC = fixní náklady (lešení, práce, omítka)
MC = cena materiálu na 1 cm
d = tloušťka [cm]

AS(d) = 1000 * ΔT * HD * (U_bez - U(d)) * EC [Kč/m²/rok]
ΔT = rozdíl teplot [°C]
HD = počet otopných dnů [dny]
U_bez = U-hodnota bez izolace
U(d) = U-hodnota s izolací tloušťky d
EC = cena energie [Kč/kWh]

CLS(d) = Σ(AS(d) * (1 + inf)^rok) pro rok=1..LT [Kč/m²]
inf = roční míra inflace energie
LT = životnost izolace [roky]

STOP podmínka:
  Δ IN > Δ CLS
  kde:
    Δ IN = IN(d+0.5) - IN(d)
    Δ CLS = CLS(d+0.5) - CLS(d)

  Přidávání izolace zastavíme když přírůstek nákladů
  překročí přírůstek úspor za celou životnost.
```

## 🎨 UI Design

- **Téma**: Radzen Material (FREE)
- **Barvy**: Čitelné, kontrastní (černý text, bílé pozadí formulářů)
- **Responsivita**: Bootstrap grid (Radzen komponenty)
- **Ikony**: Material Icons
- **Loga**: KABE Farben (hlavička), PUR-therm, Steico, Styrcon (menu)

## 💾 Úložiště dat

**localStorage** klíče:
- `thermal_calculator_assemblies` - uložené skladby
- `custom_materials` - vlastní materiály
- `thermal_calculator_stats` - statistiky použití

## 🧪 Testování

### Lokální vývoj:
```bash
cd ThermalCalculator.Wasm
dotnet watch run
```

Aplikace poběží na: http://localhost:5000

### Build pro produkci:
```bash
dotnet publish -c Release -o publish
```

Výstup v `publish/wwwroot/` - statické soubory pro hosting.

### Test offline:
1. Otevřít aplikaci v browseru
2. DevTools → Application → Service Workers → "Offline"
3. Refreshnout stránku → aplikace musí fungovat

### Test PWA instalace (iPhone):
1. Otevřít Safari
2. Navigovat na aplikaci
3. Share → Add to Home Screen
4. Otevřít z home screenu → musí fungovat offline

## 📝 Changelog

### Blazor WebAssembly verze (2025-11-16)
- ✅ Migrace z Blazor Server na Blazor WebAssembly
- ✅ PWA podpora s offline režimem
- ✅ PDF export přes jsPDF místo iText7
- ✅ Všechny výpočty zachovány (R/U hodnota, kondenzace, optimalizace)
- ✅ Radzen Material téma
- ✅ Kompletní dokumentace v README.md

### Blazor Server verze (historie)
- Inflační parametr v optimalizaci
- 3D vizualizace řezu stěnou
- Kondenzační analýza
- Ekonomická optimalizace
- Vlastní materiály
- Statistiky využití
- PDF export (iText7)

## 🔐 Bezpečnost a Omezení

### Co je bezpečné:
✅ Všechna data pouze v prohlížeči (localStorage)
✅ Žádné API volání
✅ Žádné osobní údaje
✅ Open source (lze auditovat kód)

### Omezení:
⚠️ **Orientační výsledky** - pro přesný projekt konzultujte odborného projektanta
⚠️ **České normy** - výpočty podle ČSN 73 0540-2
⚠️ **Jednoduchá geometrie** - předpokládá se rovinná stěna
⚠️ **Ustálený stav** - nezohledňuje dynamické změny (akumulace tepla)

## 🤝 Pro vývojáře

### Jak přidat nový materiál do databáze:
Upravit `MaterialService.cs` → metoda `GetCommonMaterials()`

### Jak změnit PWA ikony:
Nahradit soubory v `wwwroot/icons/` (192x192, 512x512 PNG)

### Jak upravit PDF export:
Upravit `wwwroot/js/pdfGenerator.js` a `Services/PdfExportService.cs`

### Debugging:
```bash
dotnet watch run
# Browser DevTools → Console
```

## 📄 License

MIT License - Aplikace volně k použití a modifikaci.

## 🙋 Podpora

Pro technické problémy nebo dotazy:
- GitHub Issues (pokud je projekt na GitHubu)
- Email: [kontakt]

## 📚 Reference

- **ČSN 73 0540-2** - Tepelná ochrana budov
- **Radzen Blazor** - https://blazor.radzen.com
- **jsPDF** - https://github.com/parallax/jsPDF
- **Blazor WebAssembly** - https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor

---

**Verze**: 2.0 (WebAssembly)
**Datum**: 16.11.2025
**Framework**: .NET 8.0 + Blazor WebAssembly
