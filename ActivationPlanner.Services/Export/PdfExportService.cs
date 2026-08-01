using ActivationPlanner.PropagationModel.Checklists;
using ActivationPlanner.Services.Planning;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ActivationPlanner.Services.Export;

/// <summary>
/// Renders an activation plan to PDF with operator-selectable sections (bands / antennas /
/// checklist). Uses QuestPDF under its Community license. Layer-3 service: consumes planner
/// domain types; the UI supplies the request and handles the save-file dialog.
/// </summary>
public sealed class PdfExportService
{
    static PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <summary>Render the plan to <paramref name="output"/> as PDF (off the UI thread).</summary>
    public Task WriteAsync(PdfExportRequest request, Stream output, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(output);
        return Task.Run(() => Build(request).GeneratePdf(output), ct);
    }

    /// <summary>Render the plan to a PDF byte array.</summary>
    public byte[] BuildBytes(PdfExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Build(request).GeneratePdf();
    }

    /// <summary>Render a selected-only gear pack list to <paramref name="output"/> (off the UI thread).</summary>
    public Task WriteGearListAsync(GearListPrintRequest request, Stream output, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(output);
        return Task.Run(() => BuildGearList(request).GeneratePdf(output), ct);
    }

    /// <summary>Render a selected-only gear pack list to a PDF byte array.</summary>
    public byte[] BuildGearListBytes(GearListPrintRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return BuildGearList(request).GeneratePdf();
    }

    private static IDocument Build(PdfExportRequest request) => Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Margin(36);
            page.Size(PageSizes.A4);
            page.DefaultTextStyle(t => t.FontSize(10));

            page.Header().Column(col =>
            {
                col.Item().Text(request.Title).FontSize(20).SemiBold();
                if (!string.IsNullOrWhiteSpace(request.Subtitle))
                    col.Item().Text(request.Subtitle).FontSize(9).FontColor(Colors.Grey.Darken1);
                if (request.IsSampleData)
                    col.Item().PaddingTop(2).Text("SAMPLE DATA — configure VOACAP for real predictions")
                        .FontSize(9).FontColor(Colors.Orange.Darken2);
            });

            page.Content().PaddingVertical(12).Column(col =>
            {
                col.Spacing(16);
                if (request.IncludeBands && request.Bands.Count > 0)
                    col.Item().Element(e => BandsSection(e, request.Bands));
                if (request.IncludeAntennas && request.Bands.Any(b => b.OwnedAntennas.Count > 0))
                    col.Item().Element(e => AntennasSection(e, request.Bands));
                if (request.IncludeChecklist && request.Checklist is { } checklist)
                    col.Item().Element(e => ChecklistSection(e, checklist));
            });

            page.Footer().AlignRight().Text(t =>
            {
                t.Span("Activation Planner • ").FontSize(8).FontColor(Colors.Grey.Medium);
                t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                t.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                t.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    });

    // A standalone packing sheet of only the items the operator selected on the gear-list screen,
    // grouped by kind, each with a tick box to check off as it goes in the bag.
    private static IDocument BuildGearList(GearListPrintRequest request) => Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Margin(36);
            page.Size(PageSizes.A4);
            page.DefaultTextStyle(t => t.FontSize(11));

            page.Header().Column(col =>
            {
                col.Item().Text(request.Title).FontSize(20).SemiBold();
                if (!string.IsNullOrWhiteSpace(request.Subtitle))
                    col.Item().Text(request.Subtitle).FontSize(9).FontColor(Colors.Grey.Darken1);
                if (!string.IsNullOrWhiteSpace(request.PackingTip))
                    col.Item().PaddingTop(4).Text(request.PackingTip).FontSize(9).Italic().FontColor(Colors.Blue.Darken2);
            });

            page.Content().PaddingVertical(12).Column(col =>
            {
                col.Spacing(10);
                if (request.Items.Count == 0)
                {
                    col.Item().Text("No items selected.").Italic().FontColor(Colors.Grey.Darken1);
                    return;
                }

                foreach (var group in request.Items.GroupBy(i => i.Group))
                {
                    col.Item().PaddingTop(4).Text(group.Key).FontSize(13).SemiBold();
                    foreach (var item in group)
                        col.Item().Row(row =>
                        {
                            row.AutoItem().Text("☐  ");
                            row.RelativeItem().Text(t =>
                            {
                                t.Span(item.Name);
                                if (item.Essential)
                                    t.Span("  (essential)").FontSize(9).FontColor(Colors.Green.Darken2);
                            });
                        });
                }
            });

            page.Footer().AlignRight().Text(t =>
            {
                t.Span("Activation Planner • ").FontSize(8).FontColor(Colors.Grey.Medium);
                t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                t.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                t.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    });

    private static void BandsSection(IContainer container, IReadOnlyList<BandRecommendation> bands) =>
        container.Column(col =>
        {
            col.Item().Text("Band recommendations").FontSize(13).SemiBold();
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(3); c.RelativeColumn(3); });
                table.Header(h =>
                {
                    HeaderCell(h.Cell(), "Band");
                    HeaderCell(h.Cell(), "Freq");
                    HeaderCell(h.Cell(), "Avg reliability");
                    HeaderCell(h.Cell(), "Best hour");
                });
                foreach (var band in bands)
                {
                    BodyCell(table.Cell(), band.BandName);
                    BodyCell(table.Cell(), $"{band.FrequencyMhz:0.0##} MHz");
                    BodyCell(table.Cell(), $"{band.AverageReliability * 100:0}%");
                    BodyCell(table.Cell(), band.BestHourUtc is { } h
                        ? $"{h:00}:00 UTC ({band.BestReliability * 100:0}%)"
                        : "no opening");
                }
            });
        });

    private static void AntennasSection(IContainer container, IReadOnlyList<BandRecommendation> bands) =>
        container.Column(col =>
        {
            col.Item().Text("Antennas by band").FontSize(13).SemiBold();
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(4); c.RelativeColumn(3); });
                table.Header(h =>
                {
                    HeaderCell(h.Cell(), "Band");
                    HeaderCell(h.Cell(), "Antenna");
                    HeaderCell(h.Cell(), "Modeling");
                });
                foreach (var band in bands.Where(b => b.OwnedAntennas.Count > 0))
                {
                    foreach (var antenna in band.OwnedAntennas)
                    {
                        BodyCell(table.Cell(), band.BandName);
                        BodyCell(table.Cell(), antenna.Antenna.Name);
                        BodyCell(table.Cell(), antenna.IsLibraryReady ? "Library-ready" : "Needs modeling");
                    }
                }
            });
        });

    private static void ChecklistSection(IContainer container, ChecklistInstance checklist) =>
        container.Column(col =>
        {
            col.Item().Text($"Packing checklist — {checklist.TemplateName}").FontSize(13).SemiBold();

            foreach (var group in checklist.PackItems.GroupBy(i => i.Category))
            {
                col.Item().PaddingTop(6).Text(group.Key.ToString()).SemiBold();
                foreach (var item in group)
                {
                    string mark = item.Essential ? "☑ (essential)" : "☐";
                    col.Item().Text($"{mark}  {item.Name}");
                }
            }

            var acquire = checklist.AcquireItems.ToList();
            if (acquire.Count > 0)
            {
                col.Item().PaddingTop(8).Text("Consider acquiring").SemiBold().FontColor(Colors.Orange.Darken2);
                foreach (var item in acquire)
                    col.Item().Text($"•  {item.Name}").FontColor(Colors.Grey.Darken1);
            }
        });

    private static void HeaderCell(IContainer cell, string text) =>
        cell.BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3)
            .Text(text).SemiBold().FontSize(9);

    private static void BodyCell(IContainer cell, string text) =>
        cell.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).PaddingVertical(3).Text(text);
}
