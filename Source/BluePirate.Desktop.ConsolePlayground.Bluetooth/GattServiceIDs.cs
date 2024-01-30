using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BluePirate.Desktop.ConsolePlayground.Bluetooth
{
    public class GattServiceIDs : IReadOnlyCollection<GattService>
    {

        //the backing store for the list of services
        private readonly IReadOnlyCollection<GattService> mCollections;

        public int Count => mCollections.Count;

        public GattServiceIDs()
        {
            mCollections = new List<GattService>(new[] 
            {
                new GattService("Location and Navigation","id: org.bluetooth.service.location_and_navigation",0x1819,"GSS"),
                new GattService("User Data","org.bluetooth.service.user_data",0x181C,"GSS"),
                new GattService("Generic Access","org.bluetooth.service.generic_access",0x1800,"GSS"),
                new GattService("Generic Attribute","org.bluetooth.service.generic_attribute",0x1801,"GSS")
            });
        }

        public IEnumerator<GattService> GetEnumerator() => mCollections.GetEnumerator();
        

        IEnumerator IEnumerable.GetEnumerator() => mCollections.GetEnumerator();
    }
}
