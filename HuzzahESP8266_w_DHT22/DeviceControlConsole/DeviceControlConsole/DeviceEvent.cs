using System;
using Newtonsoft.Json;

namespace DeviceControlConsole
{
    public class DeviceEvent
    {
        [JsonProperty("deviceid")]
        public string DeviceId { get; set; }

        [JsonProperty("readingtype")]
        public string ReadingType { get; set; }

        [JsonProperty("reading")]
        public double Reading { get; set; }

        [JsonProperty("threshold")]
        public double Threshold { get; set; }

        [JsonProperty("ruleoutput")]
        public string RuleOutput { get; set; }

        [JsonProperty("time")]
        public DateTime Time { get; set; }
    }
}
