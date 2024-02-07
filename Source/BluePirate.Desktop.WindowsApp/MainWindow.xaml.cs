using BluePirate.Desktop.ConsolePlayground.Bluetooth;
using BluePirate.Desktop.WindowsApp.Models;
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
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BluePirate.Desktop.WindowsApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static public BluePirateBluetoothLEAdvertisementWatcher watcher;
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
                    //return all services for device
                    var rst = await watcher.GetResultOfDeviceServicesAsync(devicekvp.DeviceId);

                    if (rst != null)
                        viewModel.GattServices = new ObservableCollection<GattServiceKVP>(rst.Services.Select(kvp => new GattServiceKVP { Key = kvp.Uuid.ToString(), Value = kvp }));


                    Debug.WriteLine($"Attempting to connect to device {devicekvp.DeviceId}");
                    await watcher.SubscribeToCharacteristicsAsync(devicekvp.DeviceId);
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

        private void listViewDeviceGattServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //when the user selects a service uuid from list get all of the service char 

            //get service Chars
            if (viewModel.SelectedGattServiceKVP == null)
                return;

            var tcs = new TaskCompletionSource<bool>();

            Task.Run(async () =>
            {
                try
                {
                    //return all char for device
                    var rst = await watcher.GetResultOfServiceCharacteristicsAsync(viewModel.SelectedGattServiceKVP.Value);
                    if (rst.Status == GattCommunicationStatus.Success)
                    {
                        viewModel.GattCharacteristics = new ObservableCollection<GattCharacteristicKVP>(rst.Characteristics.Select(kvp => new GattCharacteristicKVP { Key = kvp.Uuid.ToString(), Value = kvp }));
                    }
                    Debug.WriteLine($"testing the connection");
                }
                catch (Exception e) 
                {
                    Debug.WriteLine(e);
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

        private void btnWriteSetPointToDrone_Click(object sender, RoutedEventArgs e)
        {
            var tcs = new TaskCompletionSource<bool>();

            Task.Run(async () =>
            {
                try
                {
                    //return all char for device
                    await watcher.WriteToCharacteristicSetPoint();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
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
