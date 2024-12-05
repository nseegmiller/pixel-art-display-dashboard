using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Art_Display_Dashboard
{
    public class DeviceResponse
    {
        public int ReturnCode { get; set; }
        public required string ReturnMessage { get; set; }
        public required List<Device> DeviceList { get; set; }
    }

    public class Device
    {
        public required string DeviceName { get; set; }
        public long DeviceId { get; set; }
        public required string DevicePrivateIP { get; set; }
        public required string DeviceMac { get; set; }
        public int Hardware { get; set; }
    }
}
