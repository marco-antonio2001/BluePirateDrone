using System.Windows.Controls;

namespace BluePirate.Desktop.WindowsApp.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);

    Page GetPage(string key);
}
