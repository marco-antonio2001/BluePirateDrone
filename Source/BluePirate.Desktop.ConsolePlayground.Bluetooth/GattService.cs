using System;
using System.Collections.Generic;
using System.Text;

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

        public GattService(string name, string uniformTypeID, ushort assignedNumber, string profileSpecification)
        {
            Name = name;
            UniformTypeID = uniformTypeID;
            AssignedNumber = assignedNumber;
            ProfileSpecification = profileSpecification;
        }
    }
}
