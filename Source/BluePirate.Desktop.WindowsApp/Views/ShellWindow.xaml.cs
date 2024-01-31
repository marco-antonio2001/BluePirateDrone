using System.Windows.Controls;

using BluePirate.Desktop.WindowsApp.Contracts.Views;
using BluePirate.Desktop.WindowsApp.ViewModels;

using MahApps.Metro.Controls;

namespace BluePirate.Desktop.WindowsApp.Views;

public partial class ShellWindow : MetroWindow, IShellWindow
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public Frame GetNavigationFrame()
        => shellFrame;

    public void ShowWindow()
        => Show();

    public void CloseWindow()
        => Close();
}
