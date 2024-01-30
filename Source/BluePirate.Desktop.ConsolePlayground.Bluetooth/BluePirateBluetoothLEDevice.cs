using System;
using System.Collections.Generic;
using System.Text;

namespace BluePirate.Desktop.ConsolePlayground.Bluetooth
{
    public class BluePirateBluetoothLEDevice
    {
        public DateTimeOffset BroadCastTime { get; }

        public ulong Address { get; }
        public string Name { get; }
        public short SignalStrenghtDB { get; }
        public bool Connected { get; }
        public bool CanPair { get; }
        public bool Paired { get; }
        public string DeviceId { get; }

        public BluePirateBluetoothLEDevice(DateTimeOffset broadCastTime, ulong address, string name, short signalStrenghtDB, bool connected, bool canPair, bool paired, string deviceId)
        {
            BroadCastTime = broadCastTime;
            Address = address;
            Name = name;
            SignalStrenghtDB = signalStrenghtDB;
            Connected = connected;
            CanPair = canPair;
            Paired = paired;
            DeviceId = deviceId;
        }

        public override string ToString()
        {
            return $"{(string.IsNullOrEmpty(Name) ? "[No Name]" : Name) }[{DeviceId}]({SignalStrenghtDB})";
        }
    }
}
