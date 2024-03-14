using BluePirate.Desktop.ConsolePlayground.Bluetooth;
using BluePirate.Desktop.WindowsApp.MVVM.Model;
using ModerUiDesignDemo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePirate.Desktop.WindowsApp.MVVM.ViewModel
{
    class DroneViewModel : ObservableObject
    {
        

        public double modelCenterX { get; set; }
        public double modelCenterY { get; set; }
        public double modelCenterZ { get; set; }

        private DroneAttitudeSharedModel _droneAttitudeModel;

        public DroneAttitude SharedAttitude
        {
            get { return _droneAttitudeModel.DroneAttitudeData; }
            set { _droneAttitudeModel.DroneAttitudeData = value; }
        }

        public DroneViewModel(DroneAttitudeSharedModel droneAttitudeModel) 
        {
           _droneAttitudeModel = droneAttitudeModel;
            modelCenterX = 0;
            modelCenterY = 0;
            modelCenterZ = 0;
        }
    }
}
