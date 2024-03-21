using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BluePirate.Desktop.ConsolePlayground.Bluetooth
{
    [StructLayout(LayoutKind.Sequential)]
    public class DronePIDConfig
    {
        public float MKp { get; set; }      /* Motor Kp */
        public float MKi { get; set; }     /* Motor Ki */
        public float MKd { get; set; }       /* Motor Kd */
        public float FKp { get; set; }
        public float FKi { get; set; }  /* Mahony PI values */
    }
}
