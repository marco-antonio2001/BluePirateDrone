using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BluePirate.Desktop.ConsolePlayground.Bluetooth
{
    [StructLayout(LayoutKind.Sequential)]
    public class DroneAttitude
    {
        public float roll { get; set; }      /* rotation around x axis in degrees */
        public float pitch { get; set; }    /* rotation around y axis in degrees */
        public float yaw { get; set; }       /* rotation around z axis in degrees */

        public override string ToString()
        {
            return $"{roll},{pitch}";
        }
    }

}
