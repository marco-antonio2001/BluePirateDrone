using System.Collections.ObjectModel;

using BluePirate.Desktop.WindowsApp.Contracts.ViewModels;
using BluePirate.Desktop.WindowsApp.Core.Contracts.Services;
using BluePirate.Desktop.WindowsApp.Core.Models;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BluePirate.Desktop.WindowsApp.ViewModels;

public class ListDetailsViewModel : ObservableObject, INavigationAware
{
    private readonly ISampleDataService _sampleDataService;
    private SampleOrder _selected;

    public SampleOrder Selected
    {
        get { return _selected; }
        set { SetProperty(ref _selected, value); }
    }

    public ObservableCollection<SampleOrder> SampleItems { get; private set; } = new ObservableCollection<SampleOrder>();

    public ListDetailsViewModel(ISampleDataService sampleDataService)
    {
        _sampleDataService = sampleDataService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        SampleItems.Clear();

        var data = await _sampleDataService.GetListDetailsDataAsync();

        foreach (var item in data)
        {
            SampleItems.Add(item);
        }

        Selected = SampleItems.First();
    }

    public void OnNavigatedFrom()
    {
    }
}
