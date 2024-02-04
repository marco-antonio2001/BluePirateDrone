using BluePirate.Desktop.ConsolePlayground.Bluetooth;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BluePirate.Desktop.WindowsApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public BluePirateBluetoothLEAdvertisementWatcher watcher;
        ViewModel viewModel = new ViewModel();
        public MainWindow()
        {
            watcher = new BluePirateBluetoothLEAdvertisementWatcher(new GattServiceIDs());
            watcher.NewDeviceDiscovered += (device) =>
            {
                viewModel.KeyValuePairs = new ObservableCollection<KeyValuePairModel>(watcher.DiscoredDevices.Select(kvp => new KeyValuePairModel { Key = kvp.Name, Value = kvp }));
            };
            watcher.StoppedListening += () => 
            {
                viewModel.KeyValuePairs = new ObservableCollection<KeyValuePairModel>(watcher.DiscoredDevices.Select(kvp => new KeyValuePairModel { Key = kvp.Name, Value = kvp }));
            };
            watcher.DeviceTimedout += (device) => { viewModel.KeyValuePairs = new ObservableCollection<KeyValuePairModel>(watcher.DiscoredDevices.Select(kvp => new KeyValuePairModel { Key = kvp.Name, Value = kvp })); };
            watcher.SubscribedValueChanged += (ahrs) => { viewModel.DronePitch = watcher.droneAHRS.pitch; viewModel.DroneRoll = watcher.droneAHRS.roll; };

            watcher.StartListening();
            InitializeComponent();

            viewModel.KeyValuePairs = new ObservableCollection<KeyValuePairModel>(watcher.DiscoredDevices.Select(kvp => new KeyValuePairModel { Key = kvp.Name, Value = kvp }));

            DataContext = viewModel;
        }

        private void btnBLEStartListening_Click(object sender, RoutedEventArgs e)
        {
            watcher.StartListening();
        }

        private void listViewDiscoveredDrones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnBLEStopListening_Click(object sender, RoutedEventArgs e)
        {
            watcher.StopsListening();
        }

        private void btnConnectToSelectedBLEDevice_Click(object sender, RoutedEventArgs e)
        {
            //Connect/get Gatt services of whatever the selected device is
            if (viewModel.SelectedKeyValuePair == null)
                return;

            BluePirateBluetoothLEDevice devicekvp = viewModel.SelectedKeyValuePair.Value;

            //null gaurd
            if (devicekvp == null)
                return;

            var tcs = new TaskCompletionSource<bool>();

            Task.Run(async () =>
            {
                try
                {
                    Debug.WriteLine($"Attempting to connect to device {devicekvp.DeviceId}");
                    await watcher.SubscribeToCharacteristicsAsync(devicekvp.DeviceId, "ab30", "ab31");
                    Debug.WriteLine($"Device connected: {devicekvp.Connected}");
                }
                finally
                {
                    //anything goes wrong exit task
                    tcs.SetResult(false);

                }
                tcs.TrySetResult(true);
            });

            tcs.Task.Wait();
        }
    }
}
