# Plán: Implementace neprovětrávaných vzduchových mezer

## Datum: 2025-11-28
## Branch: grafy

## Cíl
Přidat do tepelné kalkulačky možnost vkládat neprovětrávaná vzduchové mezery jako vrstvu v konstrukci s fixními hodnotami tepelného odporu podle ČSN EN ISO 6946.

---

## 1. Rozšíření datového modelu Material

**Soubor:** `Models/Material.cs`

### Změny:
- Přidat property `IsAirGap` (bool) do třídy `Material`
  - Indikuje, zda se jedná o vzduchovou mezeru
  - Default: `false`

- Přidat property `FixedThermalResistance` (double?) do třídy `Material`
  - Uchovává fixní hodnotu R pro vzduchové mezery
  - Pro běžné materiály: `null`
  - Pro vzduchové mezery: příslušná R-hodnota z normy

---

## 2. Úprava výpočtu tepelného odporu v WallLayer

**Soubor:** `Models/Material.cs`

### Změny v třídě `WallLayer`:
- Modifikovat property `ThermalResistance` (aktuálně řádek ~25)

**Nová logika:**
```csharp
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
        return (Thickness / 1000.0) / Material.ThermalConductivity;
    }
}
```

**Poznámka:** Tloušťka zůstává editovatelná, ale u vzduchových mezer neovlivňuje R-hodnotu.

---

## 3. Přidání vzduchových mezer do MaterialService

**Soubor:** `Services/MaterialService.cs`

### Nová kategorie materiálů:
**Kategorie:** "Vzduchové mezery neprovětrávané"

### 6 nových materiálů podle ČSN EN ISO 6946:

1. **Vzduchová mezera 0-5 mm**
   - Name: "Vzduchová mezera 0-5 mm"
   - Category: "Vzduchové mezery neprovětrávané"
   - ThermalConductivity: 0.024 (W/m·K) - pro zobrazení, nepoužívá se ve výpočtu
   - FixedThermalResistance: 0.00 (m²·K/W)
   - IsAirGap: true
   - Thickness (default): 2.5 mm
   - DiffusionResistanceFactor: 1.0
   - Density: 1.2 kg/m³
   - SpecificHeatCapacity: 1005 J/(kg·K)
   - PricePerM3: 0.0 Kč

2. **Vzduchová mezera 5-10 mm**
   - FixedThermalResistance: 0.11 (m²·K/W)
   - Thickness (default): 7.5 mm
   - Ostatní parametry stejné jako výše

3. **Vzduchová mezera 10-15 mm**
   - FixedThermalResistance: 0.13 (m²·K/W)
   - Thickness (default): 12.5 mm

4. **Vzduchová mezera 15-25 mm**
   - FixedThermalResistance: 0.14 (m²·K/W)
   - Thickness (default): 20 mm

5. **Vzduchová mezera 25-50 mm**
   - FixedThermalResistance: 0.16 (m²·K/W)
   - Thickness (default): 37.5 mm

6. **Vzduchová mezera 50-300 mm**
   - FixedThermalResistance: 0.17 (m²·K/W)
   - Thickness (default): 100 mm

### Umístění v kódu:
Přidat po sekci "Folie a zábrany" (řádky 53-63) v metodě `GetCommonMaterials()`

---

## 4. Aktualizace UI pro zobrazení vzduchových mezer

**Soubor:** `Pages/ThermalResistance.razor`

### Ověření:
- Dropdown seznam materiálů (řádky 113-142) automaticky zobrazí novou kategorii
- Kategorie "Vzduchové mezery neprovětrávané" se zobrazí v sekci pro výběr materiálu
- Tloušťka vrstvy zůstane editovatelná (input field na řádcích ~135)
- Vypočtená R-hodnota se zobrazuje v tabulce (řádek ~157) - bude zobrazovat fixní hodnotu

### Případné UI vylepšení (volitelné):
- Přidat tooltip k vzduchové mezeře vysvětlující, že R-hodnota je fixní podle normy
- Vizuálně zvýraznit, že změna tloušťky u vzduchové mezery neovlivní R-hodnotu

---

## 5. Testování

### Test scenáře:

1. **Test přidání vzduchové mezery:**
   - Otevřít kalkulačku tepelného odporu
   - Přidat novou vrstvu
   - Vybrat kategorii "Vzduchové mezery neprovětrávané"
   - Vybrat materiál např. "Vzduchová mezera 15-25 mm"
   - Ověřit, že se vrstva přidala s tloušťkou 20 mm

2. **Test fixní R-hodnoty:**
   - U vzduchové mezery 15-25 mm změnit tloušťku z 20 mm na 10 mm
   - Ověřit, že zobrazená R-hodnota zůstává **0.14 m²·K/W** (NEMĚNÍ se!)
   - Změnit tloušťku na 40 mm
   - Ověřit, že R-hodnota stále zobrazuje **0.14 m²·K/W**

3. **Test celkového U-value:**
   - Vytvořit skladbu: Omítka (10mm) + Zdivo (300mm) + Vzduchová mezera 25-50mm (37.5mm) + Izolace (150mm) + Omítka (10mm)
   - Ověřit správný výpočet celkového R a U-value
   - Ručně přepočítat a porovnat

4. **Test difuzního odporu:**
   - Ověřit, že vzduchová mezera má μ = 1.0
   - Zkontrolovat výpočet difuzního odporu v tabulce

5. **Test uložení a načtení:**
   - Vytvořit skladbu se vzduchovou mezerou
   - Uložit do localStorage
   - Reload stránky
   - Ověřit, že vzduchová mezera se načetla správně s fixní R-hodnotou

---

## Soubory k úpravě - Souhrn

1. **Models/Material.cs**
   - Přidat `IsAirGap` property
   - Přidat `FixedThermalResistance` property
   - Upravit `WallLayer.ThermalResistance` výpočet

2. **Services/MaterialService.cs**
   - Přidat 6 nových materiálů vzduchových mezer
   - Kategorie: "Vzduchové mezery neprovětrávané"

3. **Pages/ThermalResistance.razor**
   - Ověřit správné zobrazení (pravděpodobně bez úprav)
   - Případně přidat tooltip/vysvětlení

---

## Technické poznámky

### Difuzní odpor vzduchových mezer:
- μ = 1.0 (vzduch je referenční médium)
- sd = d · μ = d · 1.0 = d (v metrech)
- Velmi nízký difuzní odpor - páry snadno prostupují

### Zobrazení v UI:
- Název materiálu jasně identifikuje rozsah tloušťky (např. "15-25 mm")
- Uživatel může editovat tloušťku, ale měla by odpovídat vybranému rozsahu
- R-hodnota zůstává fixní bez ohledu na zadanou tloušťku

### Možná budoucí vylepšení:
- Validace: upozornit uživatele, pokud zadá tloušťku mimo rozsah materiálu
- Automatický výběr správného materiálu při změně tloušťky vzduchové mezery
- Tooltip s odkazem na ČSN EN ISO 6946

---

## Očekávané výsledky

Po implementaci bude uživatel moci:
1. Vybrat vzduchovou mezeru z kategorie "Vzduchové mezery neprovětrávané"
2. Přidat ji do skladby konstrukce
3. Editovat tloušťku (pro dokumentační účely)
4. Získat správný výpočet tepelného odporu podle normy ČSN EN ISO 6946
5. Vidět správný celkový U-value konstrukce se zahrnutím vzduchové mezery

---

**Konec plánu**
