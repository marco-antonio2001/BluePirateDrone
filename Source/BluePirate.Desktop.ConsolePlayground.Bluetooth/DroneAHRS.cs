using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace BluePirate.Desktop.ConsolePlayground.Bluetooth
{
    [StructLayout(LayoutKind.Sequential)]
    public class DroneAHRS
    {
        public float roll { get; set; }     /* rotation around x axis in degrees */
        public float pitch { get; set; }    /* rotation around y axis in degrees */
        public float yaw { get; set; }       /* rotation around z axis in degrees */
        public float heading { get; set; }   /* rotation relative to magnetic north */
        public float rollRadians, pitchRadians, yawRadians;
        public uint timeStamp;

        public override string ToString()
        {
            return $"Roll: {roll} Pitch: {pitch} Yaw: {yaw} Heading: {heading} TimeStamp: {timeStamp}";
        }
    }

}
