using BluePirate.Desktop.ConsolePlayground.Bluetooth;
using System;

namespace BluePirate.Desktop.Playground
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var watcher = new BluePirateBluetoothLEAdvertisementWatcher();

            //this sets us into the events
            watcher.StartedListening += () =>
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Started Listining");
            };

            watcher.StoppedListening += () =>
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("Stopped Listining");
            };

            watcher.NewDeviceDiscovered += (device) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"new device discovered {device}");
            };

            watcher.DeviceNameChanged += (device) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Device Name Changed {device}");
            };

            watcher.DeviceTimedout += (device) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Device Timed out {device}");
            };

            watcher.StartListening();

            while(true) 
            {
                Console.ReadLine();

                var devices = watcher.DiscoredDevices;
                Console.WriteLine($"{devices.Count} devices....");
                foreach( var device in devices )
                {
                    Console.WriteLine(device);
                }
            }

            
        }
    }
}
