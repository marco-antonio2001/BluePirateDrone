using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BluePirate.Desktop.ConsolePlayground.Bluetooth
{

    public class BluePirateBluetoothLEAdvertisementWatcher
    {
        //fired when watcher stops listening
        public event Action StoppedListening = () => { };
        //fired when watcher starts listening
        public event Action StartedListening = () => { };
        //fired when a device is discovered
        public event Action<BluePirateBluetoothLEDevice> DeviceDiscovered = (device) => { };
        //fired when new divice is discovred
        public event Action<BluePirateBluetoothLEDevice> NewDeviceDiscovered = (device) => { };
        //fired when a known device's name changes
        public event Action<BluePirateBluetoothLEDevice> DeviceNameChanged = (device) => { };
        //fired when a device is removed for timing out
        public event Action<BluePirateBluetoothLEDevice> DeviceTimedout = (device) => { };

        //fired when a device is removed for timing out
        public event Action<DroneAHRS> SubscribedValueChanged = (droneAHRS) => { };

        private readonly BluetoothLEAdvertisementWatcher mWatcher;
        //list of our discovred devices ** need to make thread safe 
        private readonly Dictionary<string, BluePirateBluetoothLEDevice> mDiscoveredDevices = new Dictionary<string, BluePirateBluetoothLEDevice>();
        private readonly object mThreadLock = new object();
        private readonly GattServiceIDs mGattServices;

        public DroneAHRS droneAHRS = new DroneAHRS();
        public bool Listening => mWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;
        public int HeartBeatTimeout { get; set; } = 30;

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

        public BluePirateBluetoothLEAdvertisementWatcher(GattServiceIDs gattIds)
        {
            mGattServices = gattIds ?? throw new ArgumentNullException(nameof(gattIds)); 

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
            //clean up timeout 
            CleanUpTimeouts();


            //gets the ble device info
            var device = await GetBluetoothLEDeviceAsync(args.BluetoothAddress,args.Timestamp,args.RawSignalStrengthInDBm);

            if (device == null)
                return;



            var newDiscovery = false;
            string existingName = default(string);
            bool nameChanged;

            //lock for thread safety 
            lock (mThreadLock)
            {
                newDiscovery = !mDiscoveredDevices.ContainsKey(device.DeviceId);

                if (!newDiscovery)
                {
                    existingName = mDiscoveredDevices[device.DeviceId].Name;
                }


                //if already in dic but name is changed/discovered
                nameChanged = !newDiscovery && !string.IsNullOrEmpty(device.Name) && existingName != device.Name;
                if (!Listening)
                    return;

                //add or update the device to the dictionary
                mDiscoveredDevices[device.DeviceId] = device;
            }

            DeviceDiscovered(device);

            if(nameChanged)
            {
                DeviceNameChanged(device);
            }

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
            

            //get gatt services that are available 
            var gatt = await device.GetGattServicesAsync();
            IReadOnlyList<GattDeviceService> gattService = null;
            //if we have any services
            if (gatt.Status == GattCommunicationStatus.Success)
            {
                gattService = gatt.Services;
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
                    paired: device.DeviceInformation.Pairing.IsPaired,
                    gattServices: gattService
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
        public async Task PairToDeviceAsync(string deviceId)
        {
            using var device = await BluetoothLEDevice.FromIdAsync(deviceId);

            if (device == null)
                throw new ArgumentNullException("Failed to get information from device") ;

            device.DeviceInformation.Pairing.Custom.PairingRequested += (sender, args) =>
            {
                var paringKind = args.PairingKind;
                args.Accept();
            };

            var result = await device.DeviceInformation.Pairing.Custom.PairAsync(Windows.Devices.Enumeration.DevicePairingKinds.None);
        }

        public async Task SubscribeToCharacteristicsAsync(string deviceId, string serviceUuid, string characteristicUuid)
        {
            using var device = await BluetoothLEDevice.FromIdAsync(deviceId);
            if (device == null)
                throw new ArgumentNullException("Failed to get information from device");
            var serviceResults = await device.GetGattServicesAsync();
            //filter and find first service with the specified UUID
            GattDeviceService service = serviceResults.Services.FirstOrDefault(s => s.Uuid.ToString().Substring(4, 4) == serviceUuid);

            if (service == null)
                return;

            GattCharacteristicsResult characteristicsResult = await service.GetCharacteristicsAsync();
            if (characteristicsResult.Status == GattCommunicationStatus.Success)
            {
                var characteristic = characteristicsResult.Characteristics.FirstOrDefault(c => c.Uuid.ToString().Substring(4, 4) == characteristicUuid);
                if (characteristic == null) return;
                GattCharacteristicProperties properties = characteristic.CharacteristicProperties;

                if (properties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    Console.WriteLine("This characteristic has notify");
                    GattCommunicationStatus status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    if (status == GattCommunicationStatus.Success)
                    {
                        characteristic.ValueChanged += Characteristic_ValueChanged;
                        // Server has been informed of clients interest.
                    }
                }

            }

        }

        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            //every time a characteristic changes
            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            byte[] bff = new byte[32];
            reader.ReadBytes(bff);

            IntPtr ptPoit = Marshal.AllocHGlobal(32);
            Marshal.Copy(bff, 0, ptPoit, 32);
            droneAHRS = (DroneAHRS)Marshal.PtrToStructure(ptPoit, typeof(DroneAHRS));
            Marshal.FreeHGlobal(ptPoit);
            if (droneAHRS == null)
                return;
            SubscribedValueChanged(droneAHRS);
            


        }

    }
    

}
