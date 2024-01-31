using BluePirate.Desktop.WindowsApp.Core.Models;

namespace BluePirate.Desktop.WindowsApp.Core.Contracts.Services;

public interface ISampleDataService
{
    Task<IEnumerable<SampleOrder>> GetGridDataAsync();

    Task<IEnumerable<SampleOrder>> GetListDetailsDataAsync();
}
