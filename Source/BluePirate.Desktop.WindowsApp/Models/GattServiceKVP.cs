using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BluePirate.Desktop.WindowsApp.Models
{
    public class GattServiceKVP
    {
        public string Key { get; set; }
        public GattDeviceService Value { get; set; }
    }
}
