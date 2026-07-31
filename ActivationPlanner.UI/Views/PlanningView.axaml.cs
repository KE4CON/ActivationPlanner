using System;
using System.IO;
using System.Threading.Tasks;
using ActivationPlanner.UI.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace ActivationPlanner.UI.Views;

public partial class PlanningView : UserControl
{
    public PlanningView()
    {
        InitializeComponent();
    }

    private async void OnExportClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not PlanningViewModel vm || !vm.CanExport)
            return;

        TopLevel? top = TopLevel.GetTopLevel(this);
        if (top is null)
            return;

        try
        {
            IStorageFile? file = await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export plan as PDF",
                SuggestedFileName = vm.SuggestedExportFileName,
                DefaultExtension = "pdf",
                FileTypeChoices = [new FilePickerFileType("PDF document") { Patterns = ["*.pdf"] }],
            });

            if (file is null)
                return;

            await using Stream stream = await file.OpenWriteAsync();
            await vm.ExportAsync(stream);
        }
        catch (Exception ex)
        {
            vm.StatusMessage = $"Export failed: {ex.Message}";
        }
    }
}
