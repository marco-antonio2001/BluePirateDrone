using System.Windows.Controls;

using BluePirate.Desktop.WindowsApp.ViewModels;

namespace BluePirate.Desktop.WindowsApp.Views;

public partial class ListDetailsPage : Page
{
    public ListDetailsPage(ListDetailsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
