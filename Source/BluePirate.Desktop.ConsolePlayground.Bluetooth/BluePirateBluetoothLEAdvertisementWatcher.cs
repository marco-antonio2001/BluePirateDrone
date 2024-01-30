using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

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

        private readonly BluetoothLEAdvertisementWatcher mWatcher;
        //list of our discovred devices ** need to make thread safe 
        private readonly Dictionary<string, BluePirateBluetoothLEDevice> mDiscoveredDevices = new Dictionary<string, BluePirateBluetoothLEDevice>();
        private readonly object mThreadLock = new object();
        private readonly GattServiceIDs mGattServices;

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

            //lock for thread safety 
            lock (mThreadLock)
            {
                newDiscovery = !mDiscoveredDevices.ContainsKey(device.DeviceId);

                if (!newDiscovery)
                {
                    existingName = mDiscoveredDevices[device.DeviceId].Name;
                }
            }

            //if already in dic but name is changed/discovered
            var nameChanged = !newDiscovery && !string.IsNullOrEmpty(device.Name) && existingName != device.Name;

            lock (mThreadLock)
            {
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
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address);

            if (device == null)
                return null;
            

            //get gatt services that are available 
            var gatt = await device.GetGattServicesAsync();

            //if we have any services
            if (gatt.Status == GattCommunicationStatus.Success)
            {
                //loop wach gatt service
                foreach(var service in gatt.Services)
                {
                    //contains the GATT service ID we can use to connect/ extract data
                    //TODO connect to device
                    var gattServiceId = service.Uuid;
                }
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
    }
}
