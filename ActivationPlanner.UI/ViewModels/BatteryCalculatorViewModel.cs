using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Power;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// Field power-budget calculator: estimate how long a battery will run a radio, from capacity,
/// transmit power, and how much of the time you're transmitting. Prefills capacity and power from
/// owned gear where it can parse them; every value stays editable. Pure math lives in
/// <see cref="BatteryEstimator"/>; this VM just binds inputs and formats the result. Session-local.
/// </summary>
public sealed partial class BatteryCalculatorViewModel : ViewModelBase
{
    private const string ManualChoice = "(enter manually)";
    private static readonly Regex AhPattern = new(@"(\d+(?:\.\d+)?)\s*[Aa][Hh]", RegexOptions.Compiled);
    private static readonly Regex WattsPattern = new(@"(\d+(?:\.\d+)?)\s*[Ww]\b", RegexOptions.Compiled);

    private readonly Dictionary<string, GearItem> _batteriesByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, GearItem> _radiosByName = new(StringComparer.OrdinalIgnoreCase);

    public BatteryCalculatorViewModel(GearInventoryService inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        var batteries = new List<string> { ManualChoice };
        foreach (GearItem b in inventory.Current.ItemsIn(GearCategory.Power))
        {
            batteries.Add(b.Name);
            _batteriesByName[b.Name] = b;
        }
        Batteries = batteries;

        var radios = new List<string> { ManualChoice };
        foreach (GearItem r in inventory.Current.ItemsIn(GearCategory.Radio))
        {
            radios.Add(r.Name);
            _radiosByName[r.Name] = r;
        }
        Radios = radios;

        _selectedBattery = ManualChoice;
        _selectedRadio = ManualChoice;
        Recompute();
    }

    public IReadOnlyList<string> Batteries { get; }
    public IReadOnlyList<string> Radios { get; }

    [ObservableProperty] private string? _selectedBattery;
    [ObservableProperty] private string? _selectedRadio;

    // ---- inputs ----
    [ObservableProperty] private double _capacityAh = 20;
    [ObservableProperty] private double _usablePercent = 90;
    [ObservableProperty] private double _txPowerWatts = 100;
    [ObservableProperty] private double _supplyVoltage = 12.0;
    [ObservableProperty] private double _txEfficiencyPercent = 55;
    [ObservableProperty] private double _rxCurrentAmps = 1.0;
    [ObservableProperty] private double _txDutyPercent = 25;

    // ---- outputs ----
    [ObservableProperty] private string _txCurrentText = "";
    [ObservableProperty] private string _avgCurrentText = "";
    [ObservableProperty] private string _runtimeText = "";
    [ObservableProperty] private string _runtimeVerdict = "";

    partial void OnCapacityAhChanged(double value) => Recompute();
    partial void OnUsablePercentChanged(double value) => Recompute();
    partial void OnTxPowerWattsChanged(double value) => Recompute();
    partial void OnSupplyVoltageChanged(double value) => Recompute();
    partial void OnTxEfficiencyPercentChanged(double value) => Recompute();
    partial void OnRxCurrentAmpsChanged(double value) => Recompute();
    partial void OnTxDutyPercentChanged(double value) => Recompute();

    partial void OnSelectedBatteryChanged(string? value)
    {
        if (value is null || !_batteriesByName.TryGetValue(value, out GearItem? item))
            return;
        if (TryParse(AhPattern, $"{item.Name} {item.Notes}", out double ah))
            CapacityAh = ah;
        // A lithium pack can use ~90%+ of capacity; lead-acid should stop near 50%.
        if (Mentions(item, "lead") || Mentions(item, "agm") || Mentions(item, "sla"))
            UsablePercent = 50;
        else if (Mentions(item, "lifepo4") || Mentions(item, "lithium") || Mentions(item, "li-ion"))
            UsablePercent = 90;
    }

    partial void OnSelectedRadioChanged(string? value)
    {
        if (value is null || !_radiosByName.TryGetValue(value, out GearItem? item))
            return;
        if (TryParse(WattsPattern, $"{item.Name} {item.Notes}", out double watts))
            TxPowerWatts = watts;
    }

    private void Recompute()
    {
        PowerBudgetResult r = BatteryEstimator.Estimate(new PowerBudgetInput
        {
            CapacityAh = CapacityAh,
            UsableFraction = UsablePercent / 100.0,
            TxPowerWatts = TxPowerWatts,
            SupplyVoltage = SupplyVoltage,
            TxEfficiencyFraction = TxEfficiencyPercent / 100.0,
            RxCurrentAmps = RxCurrentAmps,
            TxDutyFraction = TxDutyPercent / 100.0,
        });

        TxCurrentText = $"{r.TxCurrentAmps:0.0} A on transmit";
        AvgCurrentText = $"{r.AverageCurrentAmps:0.0} A average draw";
        RuntimeText = FormatHours(r.RuntimeHours);
        RuntimeVerdict = Verdict(r.RuntimeHours);
    }

    private static string FormatHours(double hours)
    {
        if (hours <= 0)
            return "—";
        int h = (int)hours;
        int m = (int)Math.Round((hours - h) * 60);
        if (m == 60) { h++; m = 0; }
        return h > 0 ? $"{h} h {m} min" : $"{m} min";
    }

    private static string Verdict(double hours) => hours switch
    {
        <= 0 => "Enter your battery and radio to estimate runtime.",
        < 2 => "Short — plan a spare battery for anything beyond a quick activation.",
        < 4 => "Enough for a typical 2–4 hour activation.",
        < 8 => "Comfortable for a long outing.",
        _ => "Plenty — all-day capacity at this usage.",
    };

    private static bool Mentions(GearItem item, string term) =>
        (item.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
        || (item.Notes?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false);

    private static bool TryParse(Regex pattern, string text, out double value)
    {
        Match m = pattern.Match(text);
        if (m.Success && double.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;
        value = 0;
        return false;
    }
}
