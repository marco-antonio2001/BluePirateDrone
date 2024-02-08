using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BluePirate.Desktop.ConsolePlayground.Bluetooth
{
    //details about specific gatt service drone will use
    public class GattService
    {

        public string Name { get; }
        public string UniformTypeID { get; }

        //this number is contained in the uuid
        public ushort AssignedNumber { get; }

        public string ProfileSpecification {  get; }

        private readonly Dictionary<string, GattCharacteristic> mCharacteristics = new Dictionary<string, GattCharacteristic>();
        public IReadOnlyCollection<GattCharacteristic> Characteristics
        {
            get
            {
                return mCharacteristics.Values.ToList().AsReadOnly();
            }
        }

        public GattService(string name, string uniformTypeID, ushort assignedNumber, string profileSpecification)
        {
            Name = name;
            UniformTypeID = uniformTypeID;
            AssignedNumber = assignedNumber;
            ProfileSpecification = profileSpecification;
        }

        public override string ToString()
        {
            //TODO: fix this string
            return $"{(string.IsNullOrEmpty(Name) ? "[No Name]" : Name)})";
        }
    }
}
