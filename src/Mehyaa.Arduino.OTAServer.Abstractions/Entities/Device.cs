using System.Collections.Generic;

namespace Mehyaa.Arduino.OTAServer.Abstractions.Entities
{
    public class Device
    {
        public int Id { get; set; }
        public string MacAddress { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }
        
        public ICollection<Firmware> Firmwares { get; set; }
    }
}