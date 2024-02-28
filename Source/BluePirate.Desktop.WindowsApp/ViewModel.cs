﻿using BluePirate.Desktop.ConsolePlayground.Bluetooth;
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
        private DroneAHRS _droneAHRS;

        public ViewModel()
        {
            DroneAHRSSetPoint = new AttitudeSetPoint();
            DronePIDConfigValue = new DronePIDConfig();
            DroneAHRSValue = new DroneAHRS();
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

        public AttitudeSetPoint DroneAHRSSetPoint { get; set; }

        public DroneAHRS DroneAHRSValue
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
