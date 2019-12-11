using System;

namespace Mehyaa.Arduino.OTAServer.Abstractions.Entities
{
    public class Firmware
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string Path { get; set; }
        public string Filename { get; set; }
        public string Hash { get; set; }
        public string Note { get; set; }
        public DateTime ReleasedAt { get; set; }

        public virtual Device Device { get; set; }
    }
}