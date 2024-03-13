using ABI.System.Collections.Generic;
using BluePirate.Desktop.ConsolePlayground.Bluetooth;
using BluePirate.Desktop.WindowsApp.Models;
using BluePirate.Desktop.WindowsApp.MVVM.Model;
using ModerUiDesignDemo.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BluePirate.Desktop.WindowsApp.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {

        private ObservableCollection<KeyValuePairModel> _keyValuePairs;
        private KeyValuePairModel _selectedKeyValuePair;
        private BluePirateBluetoothLEDevice _selectedBluetoothDevice;
        private ObservableCollection<GattServiceKVP> _gattServicesKVP;
        private ObservableCollection<GattCharacteristicKVP> _gattCharacteristicsKVP;
        private GattCharacteristicKVP _selectedGattCharacteristicsKVP;
        private DroneAttitude _droneAHRS;
        private ObservableCollection<BluePirateBluetoothLEDevice> _bluetoothLEDevices;
        private bool _isWriteSetPointBtnEnabled = false;
        private bool _isWritePIDConstantsBtnEnabled = false;
        private bool _isConnectToDroneBtnEnabled = true;

        public double modelCenterX { get; set; }
        public double modelCenterY { get; set; }
        public double modelCenterZ { get; set; }

        public DroneViewModel DroneVM { get; set; }


        public ObservableCollection<BluePirateBluetoothLEDevice> DiscoveredDevices
        {
            get { return _bluetoothLEDevices; }
            set
            {
                if (_bluetoothLEDevices != value)
                {
                    _bluetoothLEDevices = value;
                    OnPropertyChanged(nameof(DiscoveredDevices));
                }
            }
        }


        public ObservableCollection<KeyValuePairModel> KeyValuePairs
        {
            get { return _keyValuePairs; }
            set
            {
                if (_keyValuePairs != value)
                {
                    _keyValuePairs = value;
                    OnPropertyChanged(nameof(KeyValuePairs));
                }
            }
        }

        public ObservableCollection<GattServiceKVP> GattServices
        {
            get { return _gattServicesKVP; }
            set
            {
                if (_gattServicesKVP != value)
                {
                    _gattServicesKVP = value;
                    OnPropertyChanged(nameof(GattServices));
                }
            }
        }

        public ObservableCollection<GattCharacteristicKVP> GattCharacteristics
        {
            get { return _gattCharacteristicsKVP; }
            set
            {
                if (_gattCharacteristicsKVP != value)
                {
                    _gattCharacteristicsKVP = value;
                    OnPropertyChanged(nameof(GattCharacteristics));
                }
            }

        }

        public bool IsConnectToDroneBtnEnabled
        {
            get { return _isConnectToDroneBtnEnabled; }
            set
            {
                if (_isConnectToDroneBtnEnabled != value)
                {
                    _isConnectToDroneBtnEnabled = value;
                    OnPropertyChanged(nameof(IsConnectToDroneBtnEnabled));
                }
            }
        }

        public bool IsWritePIDConstantsBtnEnabled
        {
            get { return _isWritePIDConstantsBtnEnabled; }
            set
            {
                if (_isWritePIDConstantsBtnEnabled != value)
                {
                    _isWritePIDConstantsBtnEnabled = value;
                    OnPropertyChanged(nameof(IsWritePIDConstantsBtnEnabled));
                }
            }
        }

        public bool IsWriteSetPointBtnEnabled
        {
            get { return _isWriteSetPointBtnEnabled; }
            set
            {
                if (_isWriteSetPointBtnEnabled != value)
                {
                    _isWriteSetPointBtnEnabled = value;
                    OnPropertyChanged(nameof(IsWriteSetPointBtnEnabled));
                }
            }
        }

        public GattCharacteristicKVP SelectedGattCharacteristicsKVP
        {
            get { return _selectedGattCharacteristicsKVP; }
            set
            {
                if (_selectedGattCharacteristicsKVP != value)
                {
                    _selectedGattCharacteristicsKVP = value;
                    OnPropertyChanged(nameof(SelectedGattCharacteristicsKVP));
                }
            }
        }


        public KeyValuePairModel SelectedKeyValuePair
        {
            get { return _selectedKeyValuePair; }
            set
            {
                if (_selectedKeyValuePair != value)
                {
                    _selectedKeyValuePair = value;
                    OnPropertyChanged(nameof(SelectedKeyValuePair));
                }
            }
        }

        public BluePirateBluetoothLEDevice SelectedDevice
        {
            get { return _selectedBluetoothDevice; }
            set
            {
                if (_selectedBluetoothDevice != value)
                {
                    _selectedBluetoothDevice = value;
                    OnPropertyChanged(nameof(SelectedDevice));
                }
            }
        }


        public DroneAttitude DroneAHRSSetPoint { get; set; }
        public DroneAttitude DroneAHRSSetPointTextbox { get; set; }

        public DroneAttitude DroneAHRSValue
        {
            get { return _droneAHRS; }
            set
            {
                if (_droneAHRS != value)
                {
                    _droneAHRS = value;
                    OnPropertyChanged(nameof(DroneAHRSValue));
                }
            }
        }

        public DronePIDConfig DronePIDConfigValue { get; set; }

        public MainViewModel()
        {
            DroneAHRSSetPoint = new DroneAttitude();
            DroneAHRSSetPointTextbox = new DroneAttitude();
            modelCenterX = 0;
            modelCenterY = 0;
            modelCenterZ = 0;
        }


        

    }
}
