using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ActivationPlanner.UI.ViewModels;

namespace ActivationPlanner.UI;

/// <summary>
/// Resolves a view for a view model by naming convention:
/// <c>...ViewModels.Foo.BarViewModel</c> → <c>...Views.Foo.BarView</c>.
/// Registered as an application-level <c>DataTemplate</c> so any VM placed in a
/// <c>ContentControl</c> renders as its matching view.
/// </summary>
public sealed class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data is null)
            return new TextBlock { Text = "(null)" };

        var vmType = data.GetType();
        var viewName = vmType.FullName!
            .Replace(".ViewModels", ".Views", StringComparison.Ordinal)
            .Replace("ViewModel", "View", StringComparison.Ordinal);

        var viewType = vmType.Assembly.GetType(viewName);
        if (viewType is null)
            return new TextBlock { Text = $"View not found: {viewName}" };

        return (Control)Activator.CreateInstance(viewType)!;
    }

    public bool Match(object? data) => data is ViewModelBase;
}
