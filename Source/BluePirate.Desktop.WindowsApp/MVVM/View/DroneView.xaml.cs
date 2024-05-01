using BluePirate.Desktop.WindowsApp.MVVM.Model;
using BluePirate.Desktop.WindowsApp.MVVM.ViewModel;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BluePirate.Desktop.WindowsApp.MVVM.View
{
    /// <summary>
    /// Interaction logic for DroneView.xaml
    /// </summary>
    public partial class DroneView : UserControl
    {
        DroneViewModel droneViewModel; 
        private const string MODEL_PATH = @"C:\Users\marco\source\repos\BluePirate\Source\BluePirate.Desktop.WindowsApp\Models\arduinoBle3dModel.obj";
        public DroneView()
        {
            InitializeComponent();
            Loaded += (e, a) => { Setup3dModelScene(); };
            
        }

        public void Setup3dModelScene()
        {
            ModelImporter import = new ModelImporter();
            Model3DGroup model1 = import.Load(MODEL_PATH);

            model.Content = model1;
            //sets the model centre for rotation // wiring for the viewmodel for binding
            droneViewModel.modelCenterX = model.Content.Bounds.GetCenter().X;
            droneViewModel.modelCenterY = model.Content.Bounds.GetCenter().Y;
            droneViewModel.modelCenterZ = model.Content.Bounds.GetCenter().Z;

            helixViewPort.FixedRotationPointEnabled = true;
            helixViewPort.FixedRotationPoint = new Point3D(droneViewModel.modelCenterX, droneViewModel.modelCenterY, droneViewModel.modelCenterZ);
            helixViewPort.Camera.LookDirection = new Vector3D(droneViewModel.modelCenterX, droneViewModel.modelCenterY, droneViewModel.modelCenterZ);
            helixViewPort.ShowCameraTarget = true;
            helixViewPort.CameraController.CameraTarget = new Point3D(droneViewModel.modelCenterX, droneViewModel.modelCenterY, droneViewModel.modelCenterZ);
        }
    }
}
