using BluePirate.Desktop.WindowsApp.BluetoothLeHelper;
using BluePirate.Desktop.WindowsApp.Models;
using BluePirate.Desktop.WindowsApp.MVVM.Model;
using BluePirate.Desktop.WindowsApp.MVVM.ViewModel;
using HelixToolkit.Wpf;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Media3D;


[assembly: log4net.Config.XmlConfigurator(Watch =true)]

namespace BluePirate.Desktop.WindowsApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string MODEL_PATH = @"C:\Users\marco\source\repos\BluePirate\Source\BluePirate.Desktop.WindowsApp\Models\arduinoBle3dModel.obj";
        private static string flightDataFilePath = "C:/Users/marco/source/repos/BluePirate/output/flightData.txt";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WindowsApp.cs");
        static public BluePirateBluetoothLEAdvertisementWatcher watcher;
        MainViewModel viewModel;

        public delegate void MyDelagate(string message);

        bool loggerEnabled = false;
        public object FilDia { get; private set; }

        public MainWindow()
        {
            viewModel = new MainViewModel();
            //sets python dll path
            Runtime.PythonDLL = "C:\\Users\\marco\\AppData\\Local\\Programs\\Python\\Python311\\python311.dll";
            //initializes python engine
            PythonEngine.Initialize();
            //on window close even we need to shutdown the python engine
            Closed += (obj, e) =>
            {
                Console.WriteLine("Window is closing! shutting down python engine!");
                PythonEngine.Shutdown();
            };

            watcher = new BluePirateBluetoothLEAdvertisementWatcher("BluePirateDrone");
            watcher.DeviceDiscovered += () =>
            {
                viewModel.IsConnectToDroneBtnEnabled = true;
                viewModel.BluetoothLEDeviceAddress = watcher.BluetoothDeviceAddress;
            };
            watcher.StoppedListening += () => 
            {
                //viewModel.ClearLocalVariables();
            };
            watcher.SubscribedValueChanged += (ahrs) =>
            {
                viewModel.DroneAHRSValue = watcher.droneAHRS;
                if (loggerEnabled)
                    log.Info($"{watcher.droneAHRS},{viewModel.DroneAHRSSetPoint}");
            };
            this.DataContext = viewModel;
            watcher.StartListening();
            InitializeComponent();

            Loaded += (e, a) => { Setup3dModelScene(); };
            Debug.WriteLine($"test is starting");
        }

        private void Setup3dModelScene()
        {
            ModelImporter import = new ModelImporter();
            Model3DGroup model1 = import.Load(MODEL_PATH);

            model.Content = model1;
            //sets the model centre for rotation // wiring for the viewmodel for binding
            viewModel.modelCenterX = model.Content.Bounds.GetCenter().X;
            viewModel.modelCenterY = model.Content.Bounds.GetCenter().Y;
            viewModel.modelCenterZ = model.Content.Bounds.GetCenter().Z; 

            helixViewPort.ShowFrameRate = true;
            helixViewPort.LimitFPS = true;
            
            helixViewPort.FixedRotationPointEnabled = true;
            helixViewPort.FixedRotationPoint = new Point3D(viewModel.modelCenterX, viewModel.modelCenterY, viewModel.modelCenterZ);
            helixViewPort.Camera.LookDirection = new Vector3D(viewModel.modelCenterX, viewModel.modelCenterY, viewModel.modelCenterZ);
            helixViewPort.CameraController.CameraTarget = new Point3D(viewModel.modelCenterX, viewModel.modelCenterY, viewModel.modelCenterZ);
        }

        private void btnConnectToSelectedBLEDevice_Click(object sender, RoutedEventArgs e)
        {
            var tcs = new TaskCompletionSource<bool>();

            Task.Run(async () =>
            {
                try
                {
                    await watcher.ConnectToDeviceAsync();
                    await watcher.SubscribeToCharacteristicsAsync();
                    await watcher.ReadAllCharacteristicsAsync();
                }
                finally
                {
                    tcs.SetResult(false);
                }
                //set task true
                tcs.TrySetResult(true);
            });

            tcs.Task.Wait();
        }


        private void btnWriteSetPointToDrone_Click(object sender, RoutedEventArgs e)
        {
            var tcs = new TaskCompletionSource<bool>();
            viewModel.DroneAHRSSetPoint.roll = viewModel.DroneAHRSSetPointTextbox.roll;
            viewModel.DroneAHRSSetPoint.pitch = viewModel.DroneAHRSSetPointTextbox.pitch;

            Task.Run(async () =>
            {
                try
                {
                    await watcher.WriteToDeviceCharacteristic(viewModel.DroneAHRSSetPoint);
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
                        await watcher.WriteToDeviceCharacteristic(viewModel.DronePIDConfigValue);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
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

        private void toggleArmEsc_Checked(object sender, RoutedEventArgs e)
        {
            writeToArmEsc(1);
        }

        private void toggleArmEsc_Unchecked(object sender, RoutedEventArgs e)
        {
            writeToArmEsc(0);
        }


        private void writeToArmEsc(byte armEsc) 
        {
            var tcs = new TaskCompletionSource<bool>();
            Task.Run(async () =>
            {
                try
                {
                    await watcher.WriteToDeviceCharacteristic(armEsc);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
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
            //delete old data in file
            string filePath = @"C:\Users\marco\source\repos\BluePirate\output\flightData.txt"; // Replace with the actual file path

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Console.WriteLine("File deleted successfully.");
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Error deleting file: {0}", ex.Message);
                }
            }
            else
            {
                Console.WriteLine("File does not exist.");
            }
            //start logger... enable 
            loggerEnabled = true;
            log.Info("Roll,Pitch,Yaw,Heading,Roll Setpoint,Pitch Setpoint");
        }

        private void cBoxLogData_UnChecked(object sender, RoutedEventArgs e)
        {
            //stop logger... disable logger
            loggerEnabled = false;
        }

        private void btnPlotFlightData_Click(object sender, RoutedEventArgs e)
        {
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                sys.path.append(@"C:\Users\marco\source\repos\BluePirate\Source\BluePirate.Desktop.WindowsApp");
                var fligthDataFilePathParam = new PyString(flightDataFilePath);
                var pythonScript = Py.Import("flightDataVisualizer");
                try
                {
                    pythonScript.InvokeMethod("analyseData", new PyObject[] { fligthDataFilePathParam });
                }catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                
            }

        }

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, int lParam);
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


    }
}
