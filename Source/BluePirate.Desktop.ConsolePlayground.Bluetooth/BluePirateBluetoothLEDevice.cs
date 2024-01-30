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

        public BluePirateBluetoothLEDevice(DateTimeOffset broadCastTime, ulong address, string name, short signalStrenghtDB)
        {
            BroadCastTime = broadCastTime;
            Address = address;
            Name = name;
            SignalStrenghtDB = signalStrenghtDB;
        }

        public override string ToString()
        {
            return $"{(string.IsNullOrEmpty(Name) ? "[No Name]" : Name) }\t{Address}\t({SignalStrenghtDB})";
        }
    }
}
