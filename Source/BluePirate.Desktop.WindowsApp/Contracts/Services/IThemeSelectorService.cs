using BluePirate.Desktop.WindowsApp.Models;

namespace BluePirate.Desktop.WindowsApp.Contracts.Services;

public interface IThemeSelectorService
{
    void InitializeTheme();

    void SetTheme(AppTheme theme);

    AppTheme GetCurrentTheme();
}
