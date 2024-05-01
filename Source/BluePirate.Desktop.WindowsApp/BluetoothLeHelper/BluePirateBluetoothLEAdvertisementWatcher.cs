using BluePirate.Desktop.ConsolePlayground.Bluetooth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BluePirate.Desktop.WindowsApp.BluetoothLeHelper
{
    /// <summary>
    /// Bluetooth LE Windows API Wrapper class
    /// This class will handle , finding, connecting, subscribing, writing and reading characteristics for the Specific Device
    /// </summary>
    public class BluePirateBluetoothLEAdvertisementWatcher
    {
        private static readonly Guid AHRSServiceGuid = new Guid("F000AB30-0451-4000-B000-000000000000");
        private static readonly Guid AHRSCharacteristicGuid = new Guid("F000AB31-0451-4000-B000-000000000000");
        private static readonly Guid AHRSSetPointCharacteristicGuid = new Guid("F000AB32-0451-4000-B000-000000000000");
        private static readonly Guid DronePIDConfigCharacteristicGuid = new Guid("F000AB33-0451-4000-B000-000000000000");
        private static readonly Guid armEscCharacteristicGuid = new Guid("F000AB34-0451-4000-B000-000000000000");

        #region events
        //fired when watcher stops listening
        public event Action StoppedListening = () => { };
        //fired when watcher starts listening
        public event Action StartedListening = () => { };
        //fired when watcher starts listening
        public event Action DeviceConnectionStatusChanged = () => { };
        //fired when we subscribe to the DroneAttitude Characteristic notification
        public event Action<DroneAttitude> SubscribedValueChanged = (droneAHRS) => { };
        //fired when a device is discovered
        public event Action DeviceDiscovered = () => { };

        #endregion

        private readonly BluetoothLEAdvertisementWatcher mWatcher;
        static private BluetoothLEDevice mDevice ;
        public ulong BluetoothDeviceAddress;

        public DroneAttitude droneAHRS = new DroneAttitude();
        public GattCharacteristicsResult mGattCharacteristicsResult { get; set; }
        public GattCharacteristic Characteristic { get; set; }
        public List<string> stringArr = new List<string>();

        public bool DeviceConnected => mDevice != null? mDevice.ConnectionStatus == BluetoothConnectionStatus.Connected:false ;


        int sizeOfDroneAttitudeClass;
        DataReader reader = null;
        byte[] bff;

        /// <summary>
        /// Class Constructor, Initialises BluetoothLEAdvertisementWatcher
        /// hooks into the watchers events
        /// add a advertisement filter
        /// </summary>
        /// <param name="deviceName">The device Name for the watcher to filter by</param>
        public BluePirateBluetoothLEAdvertisementWatcher(string deviceName)
        {
            mWatcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            mWatcher.Received += WatcherAdvertisementReceived;
            mWatcher.Stopped += (watcher, e) =>
            {
                StoppedListening();
            };
            BluetoothLEAdvertisementFilter filter = new BluetoothLEAdvertisementFilter();
            filter.Advertisement.LocalName = deviceName;
            mWatcher.AdvertisementFilter = filter;

            sizeOfDroneAttitudeClass = Marshal.SizeOf(droneAHRS);
            bff = new byte[sizeOfDroneAttitudeClass];
        }

        /// <summary>
        /// This Method is called whenever the watcher receives a new advertisement (Filtered)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void WatcherAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            Debug.WriteLine("Device Found");
            BluetoothDeviceAddress = args.BluetoothAddress;
            DeviceDiscovered();
        }

        /// <summary>
        /// This class will connect to the device async
        /// </summary>
        /// <returns></returns>
        public async Task ConnectToDeviceAsync()
        {
            //get device and get characteristics 
            try
            {
                mDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(BluetoothDeviceAddress);
                mDevice.ConnectionStatusChanged += Device_ConnectionStatusChanged;
                Debug.WriteLine($"connected succesfully: {mDevice.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }


            if (mDevice == null)
            {
                Debug.WriteLine($"Connecting to Device Failed address used {BluetoothDeviceAddress}");
                return;
            }

            //get service
            var serviceResults = await mDevice.GetGattServicesForUuidAsync(AHRSServiceGuid);
            if (serviceResults == null)
            {
                Debug.WriteLine($"Device Service results status for service uuid {AHRSServiceGuid} Returned null");
                return;
            }
            if (serviceResults.Status != GattCommunicationStatus.Success)
            {
                Debug.WriteLine($"Device Service results status for service uuid {AHRSServiceGuid} {serviceResults.Status}");
                return;
            }

            var AHRSGattService = serviceResults.Services.FirstOrDefault(s => s.Uuid == AHRSServiceGuid);
            try
            {
                AHRSGattService.Session.MaintainConnection = true;
                mGattCharacteristicsResult = await AHRSGattService.GetCharacteristicsAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Failed to get characteristes from service {AHRSGattService.Uuid}......{e}");
            }

            if (mGattCharacteristicsResult.Status != GattCommunicationStatus.Success)
            {
                Debug.WriteLine($"Failed to get Gatt characteristics ... Status {mGattCharacteristicsResult.Status}");
            }

        }

        /// <summary>
        /// This Method is called whenever the device Connection status changes (Disconnects/Connects)
        /// Will start watcher when device is disconected
        /// will stop watcher when device is connected (improve performance)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Device_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {   
            Debug.WriteLine($"Sender.. Device Connection Status {sender.ConnectionStatus} ");
            DeviceConnectionStatusChanged();
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Disconnected) 
            {
                mWatcher.Start();
                mDevice.Dispose();
            }
            else
            {
                StopListening();
            }

        }

        /// <summary>
        /// Start the watcher listener
        /// </summary>
        public void StartListening()
        {
            mWatcher.Start();
            StartedListening();
        }

        /// <summary>
        /// Stop the wather Listener
        /// </summary>
        public void StopListening()
        {
            mWatcher.Stop();
        }

        /// <summary>
        /// This async method will subscribe to the AHRS characteristic (can use the await Keyword)
        /// </summary>
        /// <returns>return the async Task </returns>
        public async Task SubscribeToCharacteristicsAsync()
        {
            if (mGattCharacteristicsResult == null)
            {
                Debug.WriteLine("Gatt Chars are null ... connect to device first");
                return;
            }
            if (mGattCharacteristicsResult.Status == GattCommunicationStatus.Success)
            {
                Characteristic = mGattCharacteristicsResult.Characteristics.FirstOrDefault(c => c.Uuid == AHRSCharacteristicGuid);
                if (Characteristic == null) return;
                GattCharacteristicProperties properties = Characteristic.CharacteristicProperties;

                if (properties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    Console.WriteLine("This characteristic has notify");
                    GattCommunicationStatus status = await Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.Notify);

                    if (status == GattCommunicationStatus.Success)
                    {
                        Characteristic.ValueChanged += Characteristic_ValueChanged;

                    }
                }

            }

        }

        /// <summary>
        /// This async method will subscribe to the AHRS characteristic (can use the await Keyword)
        /// </summary>
        /// <returns>return the async Task </returns>
        public async Task ReadAllCharacteristicsAsync()
        {
            if (mGattCharacteristicsResult == null)
            {
                Debug.WriteLine("Gatt Chars are null ... connect to device first");
                return;
            }
            if (mGattCharacteristicsResult.Status == GattCommunicationStatus.Success)
            {
                Characteristic = mGattCharacteristicsResult.Characteristics.FirstOrDefault(c => c.Uuid == DronePIDConfigCharacteristicGuid);
                if (Characteristic == null) return;
                GattCharacteristicProperties properties = Characteristic.CharacteristicProperties;

                if (properties.HasFlag(GattCharacteristicProperties.Read))
                {
                    Console.WriteLine("This characteristic has notify");
                    var readResults = await Characteristic.ReadValueAsync();
                    if (readResults.Status == GattCommunicationStatus.Success)
                    {
                        reader = DataReader.FromBuffer(readResults.Value);
                        reader.ReadBytes(new byte[Marshal.SizeOf(new DronePIDConfig())]);


                        // Server has been informed of clients interest.
                    }
                }

            }

        }

        /// <summary>
        /// This method is called every time the AHRS characteristic value 
        /// Reads the characteristic value from the buffer
        /// using the Marshal library we can cast the Bytes array ptr to the DroneAttitude structure
        /// fires the SubscribedValueChanged to notify other modules of the cahnge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            //every time a characteristic changes
            reader = DataReader.FromBuffer(args.CharacteristicValue);
            reader.ReadBytes(bff);

            IntPtr ptPoit = IntPtr.Zero;
            try
            {
                ptPoit = Marshal.AllocHGlobal(12);
                Marshal.Copy(bff, 0, ptPoit, 12);
                droneAHRS = (DroneAttitude)Marshal.PtrToStructure(ptPoit, typeof(DroneAttitude));
            }
            finally
            {
                Marshal.FreeHGlobal(ptPoit);
            }

            if (droneAHRS == null)
                return;
/*            string s = $"{droneAHRS} : TimeStamp ms {args.Timestamp.} : Timestamp microsecond {args.Timestamp.Microsecond}";
            stringArr.Add(s);*/
            SubscribedValueChanged(droneAHRS);
        }


        /// <summary>
        /// Writes to the correct characteristic async
        /// </summary>
        /// <param name="structureToWrite">Struture to write to the characteristic (DroneAttitude) or (DronePIDConfig)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task WriteToDeviceCharacteristic(object structureToWrite)
        {
            bool isArmEsc = false;
            if (structureToWrite == null)
                throw new ArgumentNullException();
            if (mGattCharacteristicsResult == null)
            {
                Debug.WriteLine("characteristics results are null, connect to device first");
                return;
            }

            GattCharacteristic characteristic = null;

            if (structureToWrite is DroneAttitude)
            {
                Debug.WriteLine("Writting to the drone attitude characteristic");
                structureToWrite = (DroneAttitude)structureToWrite;
                characteristic = mGattCharacteristicsResult.Characteristics.FirstOrDefault(s => s.Uuid == AHRSSetPointCharacteristicGuid);

            }
            else if (structureToWrite is DronePIDConfig)
            {
                Debug.WriteLine("Writting to the drone attitude characteristic");
                structureToWrite = (DronePIDConfig)structureToWrite;
                characteristic = mGattCharacteristicsResult.Characteristics.FirstOrDefault(s => s.Uuid == DronePIDConfigCharacteristicGuid);
            }
            else if (structureToWrite is byte)
            {
                Debug.WriteLine($"Writting to the arm ESC characteristic {(byte)structureToWrite}");
                characteristic = mGattCharacteristicsResult.Characteristics.FirstOrDefault(s => s.Uuid == armEscCharacteristicGuid);
                isArmEsc = true;
            }
            else 
            {
                Debug.WriteLine($"characteristic is null investiagte!!! structure to write Type {structureToWrite.GetType()}");
                return ;
            }

            var rst = characteristic.CharacteristicProperties;
            if (rst.HasFlag(GattCharacteristicProperties.WriteWithoutResponse))
            {
                var writer = new DataWriter();
                if(isArmEsc)
                {
                    writer.WriteByte((byte)structureToWrite);
                }
                else
                {
                    writer.WriteBytes(getStructureToBytes(structureToWrite));
                }
                
                GattCommunicationStatus result = await characteristic.WriteValueAsync(writer.DetachBuffer());
                if (result != GattCommunicationStatus.Success)
                {
                    Debug.WriteLine($"did not write to device sucssefully {result}");
                }
            }

        }

        /// <summary>
        /// uising the Marshal Library Turns the structure into a byte array so we can write to the characteristic
        /// </summary>
        /// <param name="str">Structure to be retuened as a byte array</param>
        /// <returns>The Byte array of the Structure</returns>
        byte[] getStructureToBytes(object str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(str, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
            return arr;
        }

    }
}
