using System.Windows.Controls;

namespace BluePirate.Desktop.WindowsApp.Contracts.Views;

public interface IShellWindow
{
    Frame GetNavigationFrame();

    void ShowWindow();

    void CloseWindow();
}
