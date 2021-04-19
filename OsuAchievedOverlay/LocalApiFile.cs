using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuAchievedOverlay
{
    public class LocalApiFile : ICloneable
    {
        [JsonProperty("FileName")]
        public string FileName { get; set; }

        [JsonProperty("StringData")]
        public string StringData { get; set; }

        [JsonProperty("PositivePrefix")]
        public string PositivePrefix { get; set; } = "+";

        [JsonProperty("NegativePrefix")]
        public string NegativePrefix { get; set; } = "-";

        public object Clone()
        {
            return new LocalApiFile()
            {
                FileName = FileName,
                StringData = StringData,
                PositivePrefix = PositivePrefix,
                NegativePrefix = NegativePrefix
            };
        }
    }
}
