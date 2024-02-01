using System.Windows.Controls;

using BluePirate.Desktop.WindowsApp.ViewModels;

namespace BluePirate.Desktop.WindowsApp.Views;

public partial class MainPage : Page
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void button_Click(object sender, System.Windows.RoutedEventArgs e)
    {

    }
}
