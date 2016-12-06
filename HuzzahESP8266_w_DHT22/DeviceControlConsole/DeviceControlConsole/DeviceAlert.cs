using System;

namespace DeviceControlConsole
{
    public class DeviceAlert
    {
        public string DeviceId { get; set; }
        public DateTime LastUpdate { get; set; }
        public bool FanOn { get; set; }
    }
}
