using ActivationPlanner.UI.ViewModels;
using Avalonia.Controls;

namespace ActivationPlanner.UI.Views;

public partial class AntennaPatternView : UserControl
{
    public AntennaPatternView()
    {
        InitializeComponent();

        // If the 3D surface can't start (no Vulkan GPU), fall back to the 2D plot automatically.
        FarField3D.GpuUnavailable += (_, _) =>
        {
            if (DataContext is AntennaPatternViewModel vm)
                vm.Show3D = false;
        };
    }
}
