using System.Windows.Controls;

using BluePirate.Desktop.WindowsApp.ViewModels;

namespace BluePirate.Desktop.WindowsApp.Views;

public partial class DataGridPage : Page
{
    public DataGridPage(DataGridViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
