using BluePirate.Desktop.ConsolePlayground.Bluetooth;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BluePirate.Desktop.WindowsApp.Models
{
    public class GattCharacteristicKVP
    {
        public string Key { get; set; }
        public GattCharacteristic Value { get; set; }
    }
}
