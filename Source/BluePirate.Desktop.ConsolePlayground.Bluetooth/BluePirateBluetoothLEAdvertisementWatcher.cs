using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace BluePirate.Desktop.ConsolePlayground.Bluetooth
{

    public class BluePirateBluetoothLEAdvertisementWatcher
    {
        #region events

        //fired when watcher stops listening
        public event Action StoppedListening = () => { };
        //fired when watcher starts listening
        public event Action StartedListening = () => { };
        //fired when a device is discovered
        public event Action<BluePirateBluetoothLEDevice> DeviceDiscovered = (device) => { };
        //fired when new divice is discovred
        public event Action<BluePirateBluetoothLEDevice> NewDeviceDiscovered = (device) => { };
        //fired when a device is removed for timing out
        public event Action<BluePirateBluetoothLEDevice> DeviceTimedout = (device) => { };

        //fired when a a new value is available
        public event Action<DroneAHRS> SubscribedValueChanged = (droneAHRS) => { };
        #endregion

        private static readonly Guid AHRSServiceGuid = new Guid("F000AB30-0451-4000-B000-000000000000");
        private static readonly Guid AHRSCharacteristicGuid = new Guid("F000AB31-0451-4000-B000-000000000000");
        private static readonly Guid AHRSSetPointCharacteristicGuid = new Guid("F000AB32-0451-4000-B000-000000000000");
        private static readonly Guid DronePIDConfigCharacteristicGuid = new Guid("F000AB33-0451-4000-B000-000000000000");

        private readonly BluetoothLEAdvertisementWatcher mWatcher;
        //list of our discovred devices ** need to make thread safe 
        private readonly Dictionary<string, BluePirateBluetoothLEDevice> mDiscoveredDevices = new Dictionary<string, BluePirateBluetoothLEDevice>();
        private readonly object mThreadLock = new object();

        public DroneAHRS droneAHRS = new DroneAHRS();
        public bool Listening => mWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;
        public int HeartBeatTimeout { get; set; } = 30;

        public GattCharacteristic Characteristic { get; set; }

        static public GattDeviceService mGattDeviceService;

        public GattCharacteristicsResult mGattCharacteristicsResult { get; set; }


        public IReadOnlyCollection<BluePirateBluetoothLEDevice> DiscoredDevices 
        {
            get 
            {
                //clean up any devices that has timedout
                CleanUpTimeouts();

                //thread safeety
                lock (mThreadLock) 
                {
                    return mDiscoveredDevices.Values.ToList().AsReadOnly();
                }
            }   
        }

        private void CleanUpTimeouts()
        {
            lock (mThreadLock)
            {
                var threshold = DateTime.UtcNow - TimeSpan.FromSeconds(HeartBeatTimeout);
                mDiscoveredDevices.Where(f => f.Value.BroadCastTime < threshold).ToList().ForEach(device =>
                {
                    mDiscoveredDevices.Remove(device.Key);
                    DeviceTimedout(device.Value);
                });
            }
        }

        public BluePirateBluetoothLEAdvertisementWatcher()
        {
            mWatcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            mWatcher.Received += WatcherAdvertisementReceivedAsync;
            mWatcher.Stopped += (watcher, e) =>
            {
                StoppedListening();
            };
            
        }

        private async void WatcherAdvertisementReceivedAsync(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            //Console.WriteLine("advert received");
            //clean up timeout 
            CleanUpTimeouts();

            //gets the ble device info
            var device = await GetBluetoothLEDeviceAsync(args.BluetoothAddress,args.Timestamp,args.RawSignalStrengthInDBm);

            if (device == null)
                return;

            //only want to find the drone test
            if (!device.Name.Equals("DroneTest")) 
            {
                //Console.WriteLine("Device Null could not get more information connect");
                return;
            }
                
            //TODO remove or reduce some logic here
            var newDiscovery = false;

            //lock for thread safety 
            lock (mThreadLock)
            {
                newDiscovery = !mDiscoveredDevices.ContainsKey(device.DeviceId);

                //if already in dic but name is changed/discovered
                if (!Listening)
                    return;

                //add or update the device to the dictionary
                mDiscoveredDevices[device.DeviceId] = device;
            }

            DeviceDiscovered(device);

            if ( newDiscovery )
            {
                //inform listeners new device has been added
                NewDeviceDiscovered(device);
            }

        }

        private async Task<BluePirateBluetoothLEDevice> GetBluetoothLEDeviceAsync(ulong address, DateTimeOffset broadCastTime, short signalStrenghtDB)
        {
            using var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);

            if (device == null)
                return null;

            if (device.DeviceInformation.Name.Contains("DroneTest")) 
            {
                Console.WriteLine("Found drone test");
                var access = await device.RequestAccessAsync();
                var test = await device.GetGattServicesAsync();
                Console.WriteLine("Found services");
            }

            return new BluePirateBluetoothLEDevice
                (
                    deviceId: device.DeviceId,
                    address: device.BluetoothAddress,
                    name: device.Name,
                    broadCastTime: broadCastTime,
                    signalStrenghtDB: signalStrenghtDB,
                    connected: device.ConnectionStatus == BluetoothConnectionStatus.Connected,
                    canPair: device.DeviceInformation.Pairing.CanPair,
                    paired: device.DeviceInformation.Pairing.IsPaired
                );
        }

        public void StartListening()
        {
            lock (mThreadLock)
            {
                if (Listening)
                    return;

                mWatcher.Start();
            }

            StartedListening();
        }

        public void StopsListening()
        {

            lock (mThreadLock) 
            {
                if (!Listening)
                    return;
                mWatcher.Stop();
                mDiscoveredDevices.Clear();
            }
            
        }

        //get  all service in a device
        public async Task<GattDeviceServicesResult> GetResultOfDeviceServicesAsync(string deviceId)
        {
            using var device = await BluetoothLEDevice.FromIdAsync(deviceId);
            if (device == null)
                throw new ArgumentNullException("Failed to get information from device");
            var serviceResults = await device.GetGattServicesAsync();
            return serviceResults;
        }

        public async Task WriteToCharacteristicPIDConfig(DronePIDConfig dronePIDConfig)
        {
            if (dronePIDConfig == null)
                throw new ArgumentNullException();
            if (mGattCharacteristicsResult == null)
                return;

            var characteristic = mGattCharacteristicsResult.Characteristics.FirstOrDefault(s => s.Uuid == DronePIDConfigCharacteristicGuid);
            if (characteristic == null)
                return;
            var rst = characteristic.CharacteristicProperties;

            if (rst.HasFlag(GattCharacteristicProperties.WriteWithoutResponse))
            {
                var writer = new DataWriter();
                writer.WriteBytes(getDronePIDConfigBytes(dronePIDConfig));
                GattCommunicationStatus result = await characteristic.WriteValueAsync(writer.DetachBuffer());
                if (result != GattCommunicationStatus.Success)
                {
                    Debug.WriteLine($"did not write to device sucssefully {result}");
                }
            }

        }

        //write to characteristic 
        public async Task WriteToCharacteristicSetPoint(AttitudeSetPoint droneAHRSSetPoint)
        {
            if(droneAHRSSetPoint == null)  
                throw new ArgumentNullException();
            if (mGattCharacteristicsResult == null)
                return;

            var characteristic = mGattCharacteristicsResult.Characteristics.FirstOrDefault(s => s.Uuid == AHRSSetPointCharacteristicGuid);
            var rst = characteristic.CharacteristicProperties;

            if (rst.HasFlag(GattCharacteristicProperties.WriteWithoutResponse))
            {
                var writer = new DataWriter();
                writer.WriteBytes(getAttitudeSetPointBytes(droneAHRSSetPoint));
                GattCommunicationStatus result = await characteristic.WriteValueAsync(writer.DetachBuffer());
                if (result != GattCommunicationStatus.Success)
                {
                    Debug.WriteLine($"did not write to device sucssefully {result}");
                }
            }
            
        }


        //TODO: break down and fix need to subscribe in a higher module
        public async Task SubscribeToCharacteristicsAsync(string deviceId)
        {
            using var device = await BluetoothLEDevice.FromIdAsync(deviceId);
            if (device == null)
                throw new ArgumentNullException("Failed to get information from device");
            device.ConnectionStatusChanged += Device_ConnectionStatusChanged;
            var serviceResults = await device.GetGattServicesAsync();
            //filter and find first service with the specified UUID
            GattDeviceService service = serviceResults.Services.FirstOrDefault(s => s.Uuid == AHRSServiceGuid);

            if (service == null)
                return;
            Debug.WriteLine($"Service found getting Chars{service.Uuid}");
            mGattCharacteristicsResult = await service.GetCharacteristicsAsync();
            Debug.WriteLine($"Char async stat results: {mGattCharacteristicsResult.Status}");

            if (mGattCharacteristicsResult.Status == GattCommunicationStatus.Success)
            {
                mGattDeviceService = service;
                mGattDeviceService.Session.MaintainConnection = true;
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
                        // Server has been informed of clients interest.
                    }
                }

            }
            
        }

        private void Device_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            Console.WriteLine($"Device status : {sender.ConnectionStatus}");
        }

        byte[] getDroneAHRSBytes(DroneAHRS str)
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

        byte[] getAttitudeSetPointBytes(AttitudeSetPoint str)
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

        byte[] getDronePIDConfigBytes(DronePIDConfig str)
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


        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            lock(mThreadLock)
            {
                //every time a characteristic changes
                var reader = DataReader.FromBuffer(args.CharacteristicValue);
                byte[] bff = new byte[32];
                reader.ReadBytes(bff);

                IntPtr ptPoit = IntPtr.Zero;
                try
                {
                    ptPoit = Marshal.AllocHGlobal(32);
                    Marshal.Copy(bff, 0, ptPoit, 32);
                    droneAHRS = (DroneAHRS)Marshal.PtrToStructure(ptPoit, typeof(DroneAHRS));
                }
                finally
                { 
                    Marshal.FreeHGlobal(ptPoit); 
                }

                if (droneAHRS == null)
                    return;
                SubscribedValueChanged(droneAHRS);
            }

        }
       
    }
    
   
}
