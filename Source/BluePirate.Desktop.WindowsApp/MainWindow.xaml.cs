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

[assembly: log4net.Config.XmlConfigurator(Watch =true)]

namespace BluePirate.Desktop.WindowsApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WindowsApp.cs");
        static public BluePirateBluetoothLEAdvertisementWatcher watcher;
        ViewModel viewModel = new ViewModel();
        bool loggerEnabled = false;
        public MainWindow()
        {
            watcher = new BluePirateBluetoothLEAdvertisementWatcher();
            watcher.DeviceDiscovered += (device) =>
            {
                viewModel.KeyValuePairs = new ObservableCollection<KeyValuePairModel>(watcher.DiscoredDevices.Select(kvp => new KeyValuePairModel { Key = kvp.Name, Value = kvp }));
            };
            watcher.StoppedListening += () => 
            {
                //viewModel.ClearLocalVariables();
            };
            watcher.DeviceTimedout += (device) => { viewModel.KeyValuePairs = new ObservableCollection<KeyValuePairModel>(watcher.DiscoredDevices.Select(kvp => new KeyValuePairModel { Key = kvp.Name, Value = kvp })); };
            watcher.SubscribedValueChanged += (ahrs) =>
            {
                viewModel.DroneAHRSValue = watcher.droneAHRS;
                if (loggerEnabled)
                    log.Info(watcher.droneAHRS);
            };

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



        private void btnWriteSetPointToDrone_Click(object sender, RoutedEventArgs e)
        {
            var tcs = new TaskCompletionSource<bool>();

            Task.Run(async () =>
            {
                try
                {
                    //return all char for device
                    await watcher.WriteToCharacteristicSetPoint(viewModel.DroneAHRSSetPoint);
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

        private void btnWritePidValues_Click(object sender, RoutedEventArgs e)
        {
            var tcs = new TaskCompletionSource<bool>();

                Task.Run(async () =>
                {
                    try
                    {
                        //return all char for device
                        await watcher.WriteToCharacteristicPIDConfig(viewModel.DronePIDConfigValue);
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

        private void cBoxLogData_Checked(object sender, RoutedEventArgs e)
        {
            //start logger... enable 
            loggerEnabled = true;
            log.Info("Roll,Pitch,Yaw,Heading");
        }

        private void cBoxLogData_UnChecked(object sender, RoutedEventArgs e)
        {
            //stop logger... disable logger
            loggerEnabled = false;
        }
    }
}
