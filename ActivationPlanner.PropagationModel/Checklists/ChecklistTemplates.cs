using System.Collections.Generic;
using System.Linq;
using ActivationPlanner.PropagationModel.Missions;
using GC = ActivationPlanner.PropagationModel.Gear.GearCategory;

namespace ActivationPlanner.PropagationModel.Checklists;

/// <summary>
/// The built-in <see cref="ChecklistTemplate"/> catalog — a shared base go-kit (Item #2
/// categories) plus mission-specific additions. Templates are the standing starting point;
/// a specific activation's <see cref="ChecklistInstance"/> is built from one by the
/// <c>ChecklistService</c>, resolving each item against the operator's owned inventory.
/// </summary>
public static class ChecklistTemplates
{
    // The personal go-kit every mission starts from.
    private static readonly ChecklistTemplateItem[] Base =
    [
        new() { Category = ChecklistCategory.Power, Name = "Spare battery", Essential = true, GearCategory = GC.Power },
        new() { Category = ChecklistCategory.Power, Name = "Battery charger", GearCategory = GC.Power },
        new() { Category = ChecklistCategory.Power, Name = "DC power cables & fuses", GearCategory = GC.Power },

        new() { Category = ChecklistCategory.Communications, Name = "Backup / spare antenna", IsAntenna = true },
        new() { Category = ChecklistCategory.Communications, Name = "Spare coax & adapters", GearCategory = GC.Other },
        new() { Category = ChecklistCategory.Communications, Name = "Headset or hand mic", GearCategory = GC.Other },

        new() { Category = ChecklistCategory.Documentation, Name = "Logging device or pad", Essential = true },
        new() { Category = ChecklistCategory.Documentation, Name = "Notepad & pen/pencil" },

        new() { Category = ChecklistCategory.Safety, Name = "First aid kit" },
        new() { Category = ChecklistCategory.Safety, Name = "Water" },
        new() { Category = ChecklistCategory.Safety, Name = "Weather protection" },
        new() { Category = ChecklistCategory.Safety, Name = "Headlamp / flashlight" },

        new() { Category = ChecklistCategory.Identification, Name = "Copy of license", Essential = true },
        new() { Category = ChecklistCategory.Identification, Name = "ID & emergency contact card" },
    ];

    private static readonly IReadOnlyDictionary<MissionType, ChecklistTemplateItem[]> Extras =
        new Dictionary<MissionType, ChecklistTemplateItem[]>
        {
            [MissionType.Pota] =
            [
                new() { Category = ChecklistCategory.Power, Name = "Solar panel (optional top-up)", GearCategory = GC.Power },
                new() { Category = ChecklistCategory.Safety, Name = "Portable chair / small table" },
            ],
            [MissionType.Sota] =
            [
                new() { Category = ChecklistCategory.Documentation, Name = "Map & compass / GPS" },
                new() { Category = ChecklistCategory.Safety, Name = "Trekking poles / summit safety gear" },
                new() { Category = ChecklistCategory.Safety, Name = "Extra layers (summit exposure)" },
            ],
            [MissionType.FieldDay] =
            [
                new() { Category = ChecklistCategory.Communications, Name = "Digital-mode interface", GearCategory = GC.DigitalInterface },
                new() { Category = ChecklistCategory.Power, Name = "Extra power / generator fuel", GearCategory = GC.Power },
                new() { Category = ChecklistCategory.Documentation, Name = "Logging computer", GearCategory = GC.Computer },
                new() { Category = ChecklistCategory.Safety, Name = "Canopy / shelter" },
            ],
            [MissionType.Emcomm] =
            [
                new() { Category = ChecklistCategory.Communications, Name = "Digital-mode interface (Digirig / SignaLink / TNC)", Essential = true, GearCategory = GC.DigitalInterface },
                new() { Category = ChecklistCategory.Power, Name = "Redundant power source", Essential = true, GearCategory = GC.Power },
                new() { Category = ChecklistCategory.Documentation, Name = "ICS-213 / 214 forms", Essential = true },
                new() { Category = ChecklistCategory.Documentation, Name = "NTS radiogram pad" },
                new() { Category = ChecklistCategory.Communications, Name = "EMCOMM go-kit items", GearCategory = GC.Emcomm },
            ],
            [MissionType.General] = [],
        };

    private static readonly IReadOnlyDictionary<MissionType, string> Names =
        new Dictionary<MissionType, string>
        {
            [MissionType.Pota] = "POTA kit",
            [MissionType.Sota] = "SOTA kit",
            [MissionType.FieldDay] = "Field Day kit",
            [MissionType.Emcomm] = "EMCOMM go-kit",
            [MissionType.General] = "General kit",
        };

    /// <summary>The template for <paramref name="missionType"/> (base go-kit plus its additions).</summary>
    public static ChecklistTemplate For(MissionType missionType) => new()
    {
        MissionType = missionType,
        Name = Names[missionType],
        Items = [.. Base, .. Extras[missionType]],
    };
}
