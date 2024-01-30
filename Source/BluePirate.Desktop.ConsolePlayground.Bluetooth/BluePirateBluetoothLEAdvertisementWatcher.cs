using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Bluetooth.Advertisement;

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
        private readonly Dictionary<ulong, BluePirateBluetoothLEDevice> mDiscoveredDevices = new Dictionary<ulong, BluePirateBluetoothLEDevice>();
        private readonly object mThreadLock = new object();
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

        public BluePirateBluetoothLEAdvertisementWatcher()
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
        }

        private void WatcherAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            //clean up timeout 
            CleanUpTimeouts();

            BluePirateBluetoothLEDevice device = null;

            var newDiscovery = !mDiscoveredDevices.ContainsKey(args.BluetoothAddress);
            //if already in dic but name is changed/discovered
            var nameChanged = !newDiscovery && !string.IsNullOrEmpty(args.Advertisement.LocalName) && mDiscoveredDevices[args.BluetoothAddress].Name != args.Advertisement.LocalName;

            lock (mThreadLock)
            {
                var name = args.Advertisement.LocalName;

                //if the new name is blank and we have a device
                if(string.IsNullOrEmpty(name) && !newDiscovery)
                {
                    //prevent overriding actual name with null/blank
                    name = mDiscoveredDevices[args.BluetoothAddress].Name;
                }

                //creating a new class off the device
                device = new BluePirateBluetoothLEDevice
                    (
                        address: args.BluetoothAddress,
                        name: name,
                        broadCastTime: args.Timestamp,
                        signalStrenghtDB: args.RawSignalStrengthInDBm
                    );

                //add or update the device to the dictionary
                mDiscoveredDevices[args.BluetoothAddress] = device;
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
