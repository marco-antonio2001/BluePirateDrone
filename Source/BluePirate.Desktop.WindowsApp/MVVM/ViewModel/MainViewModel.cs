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

        private DroneAttitude _droneAHRS;
        private ulong _bluetoothLEDeviceAddress;
        private bool _isWriteSetPointBtnEnabled = false;
        private bool _isWritePIDConstantsBtnEnabled = false;
        private bool _isConnectToDroneBtnEnabled = true;

        public double modelCenterX { get; set; }
        public double modelCenterY { get; set; }
        public double modelCenterZ { get; set; }

        public DroneViewModel DroneVM { get; set; }


        public ulong BluetoothLEDeviceAddress
        {
            get { return _bluetoothLEDeviceAddress; }
            set
            {
                if (_bluetoothLEDeviceAddress != value)
                {
                    _bluetoothLEDeviceAddress = value;
                    OnPropertyChanged(nameof(BluetoothLEDeviceAddress));
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
            DronePIDConfigValue = new DronePIDConfig();
            IsConnectToDroneBtnEnabled = false;
            modelCenterX = 0;
            modelCenterY = 0;
            modelCenterZ = 0;
        }


        

    }
}
