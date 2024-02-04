using BluePirate.Desktop.ConsolePlayground.Bluetooth;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace BluePirate.Desktop.WindowsApp
{
    public class ViewModel : INotifyPropertyChanged
    {
       

        private ObservableCollection<KeyValuePairModel> _keyValuePairs;
        private KeyValuePairModel _selectedKeyValuePair;
        private DroneAHRS _droneAHRS;
        private float _roll;
        private float _pitch;

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


        public float DroneRoll
        {
            get { return _roll; }
            set
            {
                if (_roll != value)
                {
                    _roll = value;
                    OnPropertyChanged(nameof(DroneRoll));
                }
            }
        }

        public float DronePitch
        {
            get { return _pitch; }
            set
            {
                if (_pitch != value)
                {
                    _pitch = value;
                    OnPropertyChanged(nameof(DronePitch));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
