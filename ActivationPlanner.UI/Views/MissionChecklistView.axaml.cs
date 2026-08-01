using System;
using System.IO;
using ActivationPlanner.UI.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace ActivationPlanner.UI.Views;

public partial class MissionChecklistView : UserControl
{
    public MissionChecklistView()
    {
        InitializeComponent();
    }

    private async void OnPrintSelectedClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MissionChecklistViewModel vm || !vm.CanPrint)
            return;

        TopLevel? top = TopLevel.GetTopLevel(this);
        if (top is null)
            return;

        try
        {
            IStorageFile? file = await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Print selected gear as PDF",
                SuggestedFileName = vm.SuggestedPrintFileName,
                DefaultExtension = "pdf",
                FileTypeChoices = [new FilePickerFileType("PDF document") { Patterns = ["*.pdf"] }],
            });

            if (file is null)
                return;

            await using Stream stream = await file.OpenWriteAsync();
            await vm.PrintSelectedAsync(stream);
        }
        catch (Exception)
        {
            // Save/permission errors are surfaced by the OS file dialog; nothing to persist here.
        }
    }
}
