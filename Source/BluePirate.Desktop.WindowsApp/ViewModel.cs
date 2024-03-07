using BluePirate.Desktop.ConsolePlayground.Bluetooth;
using BluePirate.Desktop.WindowsApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BluePirate.Desktop.WindowsApp
{
    public class ViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<KeyValuePairModel> _keyValuePairs;
        private KeyValuePairModel _selectedKeyValuePair;
        private ObservableCollection<GattServiceKVP> _gattServicesKVP;
        private GattServiceKVP _selectedGattServicesKVP;
        private ObservableCollection<GattCharacteristicKVP> _gattCharacteristicsKVP;
        private GattCharacteristicKVP _selectedGattCharacteristicsKVP;
        private DroneAttitude _droneAHRS;
        private bool _isWriteSetPointBtnEnabled = false;
        private bool _isWritePIDConstantsBtnEnabled = false;
        private bool _isConnectToDroneBtnEnabled = true;



        public ViewModel()
        {
            DroneAHRSSetPoint = new DroneAttitude();
            DronePIDConfigValue = new DronePIDConfig();
            DroneAHRSValue = new DroneAttitude();
            modelCenterX = 0;
            modelCenterY = 0;
            modelCenterZ = 0;
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
                if ( _gattCharacteristicsKVP != value)
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

        public GattServiceKVP SelectedGattServiceKVP
        {
            get { return _selectedGattServicesKVP; }
            set
            {
                if(_selectedGattServicesKVP != value)
                {
                    _selectedGattServicesKVP = value;
                    OnPropertyChanged(nameof(_selectedGattServicesKVP));
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

        public DroneAttitude DroneAHRSSetPoint { get; set; }

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

        public double modelCenterX { get; set; }
        public double modelCenterY { get; set; }
        public double modelCenterZ { get; set; }



        public void ClearLocalVariables()
        {
            _keyValuePairs.Clear();
            _gattCharacteristicsKVP.Clear();
            _gattServicesKVP.Clear();

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
