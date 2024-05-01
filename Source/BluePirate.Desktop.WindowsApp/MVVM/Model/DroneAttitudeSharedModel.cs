using BluePirate.Desktop.ConsolePlayground.Bluetooth;
using ModerUiDesignDemo.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePirate.Desktop.WindowsApp.MVVM.Model
{
    class DroneAttitudeSharedModel :ObservableObject
    {
        private DroneAttitude _droneAttitude;
        public DroneAttitude DroneAttitudeData
        {
            get { return _droneAttitude; }
            set
            {
                _droneAttitude = value;
                OnPropertyChanged(nameof(DroneAttitudeData));
            }
        }

    }
}
