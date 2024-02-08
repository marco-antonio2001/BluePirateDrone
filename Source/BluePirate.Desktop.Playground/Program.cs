using BluePirate.Desktop.ConsolePlayground.Bluetooth;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BluePirate.Desktop.Playground
{
    internal class Program
    {
        
        static void Main(string[] args)
        {

            var tcs = new TaskCompletionSource<bool>();

            Task.Run(async () => 
            {
                try
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

                    watcher.DeviceTimedout += (device) =>
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Device Timed out {device}");
                    };

                    watcher.SubscribedValueChanged += (droneAHRS) =>
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($" trigger: {droneAHRS}");
                    };

                    watcher.StartListening();

                    while (true)
                    {
                        var command = Console.ReadLine()?.ToLower().Trim();

                        if (string.IsNullOrEmpty(command))
                        {
                            var devices = watcher.DiscoredDevices;
                            Console.WriteLine($"{devices.Count} devices....");
                            foreach (var device in devices)
                            {
                                Console.WriteLine(device);
                            }
                        }
                        else if (command == "c")
                        {
                            //try to connect
                            Console.WriteLine("connecting to drone");
                            var droneDevice = watcher.DiscoredDevices.FirstOrDefault(f => f.Name.Contains("DroneTest"));

                            if (droneDevice == null)
                            {
                                Console.WriteLine("No DroneTest Device Found");
                                continue;
                            }

                            Console.WriteLine("Connecting to DroneTest Device...");

                            //try and pair
                            try
                            {
                                //await watcher.PairToDeviceAsync(droneDevice.DeviceId);
                                await watcher.SubscribeToCharacteristicsAsync(droneDevice.DeviceId);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed to subscribe to dronetest characteristic notification....");
                                Console.WriteLine(e);
                            }


                        }else if (command == "q") { break; }

                    }
                }
                finally
                {
                    //anything goes wrong exit task
                    tcs.SetResult(false);
                    
                }
                tcs.TrySetResult(true);
            });

            tcs.Task.Wait();

            
        }
    }
}
