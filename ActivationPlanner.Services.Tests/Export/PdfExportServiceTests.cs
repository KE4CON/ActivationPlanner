using System.Text;
using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Checklists;
using ActivationPlanner.PropagationModel.Missions;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.Services.Export;
using ActivationPlanner.Services.Planning;

namespace ActivationPlanner.Services.Tests.Export;

public sealed class PdfExportServiceTests
{
    private static BandRecommendation Band() => new()
    {
        Band = HamBand.M20,
        FrequencyMhz = 14.1,
        OwnedAntennas = [],
        Prediction = new BandPrediction
        {
            Band = HamBand.M20,
            FrequencyMhz = 14.1,
            Hours = [new BandHourSample { HourUtc = 14, Reliability = 0.8 }],
        },
    };

    private static ChecklistInstance Checklist() => new()
    {
        MissionType = MissionType.Pota,
        TemplateName = "POTA kit",
        Items =
        [
            new ChecklistInstanceItem
            {
                Category = ChecklistCategory.Power, Name = "Spare battery",
                Essential = true, Status = ChecklistItemStatus.Owned,
            },
            new ChecklistInstanceItem
            {
                Category = ChecklistCategory.Communications, Name = "Digital interface",
                Essential = false, Status = ChecklistItemStatus.Acquire,
            },
        ],
    };

    [Fact]
    public void Produces_a_valid_pdf()
    {
        var request = new PdfExportRequest
        {
            Title = "Test Plan",
            Subtitle = "Denver • 2026-07-31",
            IsSampleData = true,
            Bands = [Band()],
            Checklist = Checklist(),
        };

        byte[] pdf = new PdfExportService().BuildBytes(request);

        Assert.True(pdf.Length > 0);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4)); // PDF magic header
    }

    [Fact]
    public void HasContent_reflects_selected_sections()
    {
        var full = new PdfExportRequest { Bands = [Band()], Checklist = Checklist() };
        Assert.True(full.HasContent);

        var nothingSelected = full with { IncludeBands = false, IncludeAntennas = false, IncludeChecklist = false };
        Assert.False(nothingSelected.HasContent);
    }

    [Fact]
    public async Task WriteAsync_writes_to_a_stream()
    {
        using var ms = new MemoryStream();
        await new PdfExportService().WriteAsync(new PdfExportRequest { Bands = [Band()] }, ms);
        Assert.True(ms.Length > 0);
    }

    [Fact]
    public void Gear_list_print_produces_a_valid_pdf()
    {
        var request = new GearListPrintRequest
        {
            Title = "POTA kit — packing list",
            Subtitle = "POTA • 2026-07-31",
            PackingTip = "Balanced portable kit.",
            Items =
            [
                new GearPrintItem { Name = "Icom IC-705", Group = "Radio", Essential = false },
                new GearPrintItem { Name = "Spare battery", Group = "Power", Essential = true },
            ],
        };

        byte[] pdf = new PdfExportService().BuildGearListBytes(request);

        Assert.True(pdf.Length > 0);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public async Task Gear_list_print_writes_even_with_no_items_selected()
    {
        // Guards the "nothing selected" branch — should still render a (near-empty) valid sheet.
        using var ms = new MemoryStream();
        await new PdfExportService().WriteGearListAsync(
            new GearListPrintRequest { Title = "Empty", Items = [] }, ms);
        Assert.True(ms.Length > 0);
    }
}
