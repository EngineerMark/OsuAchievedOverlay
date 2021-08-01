using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OsuAchievedOverlay.Next
{
    public class Theme
    {
        /// <summary>
        /// The name of theme that is visible in-app
        /// </summary>
        [JsonProperty("PrettyName")]
        public string PrettyName { get; set; }

        /// <summary>
        /// Which app version is this made for
        /// </summary>
        [JsonProperty("AppVersion")]
        public string AppVersion { get; set; }

        /// <summary>
        /// Name used for internal handling of the theme
        /// </summary>
        [JsonProperty("InternalName")]
        public string InternalName { get; set; }

        /// <summary>
        /// Is this theme safe for work?
        /// </summary>
        [JsonProperty("Safe")]
        public bool Safe { get; set; }

        [JsonProperty("Data")]
        public ThemeData ThemeData { get; set; }

        public static Theme LoadFromFile(string path){
            if(File.Exists(path)){
                string serializedData = File.ReadAllText(path);
                Theme deserializedData = null;
                try {
                    deserializedData = JsonConvert.DeserializeObject<Theme>(serializedData);
                }
                catch (Exception){ }
                return deserializedData;
            }
            return null;
        }
    }

    public class ThemeData{
        [JsonProperty("Background")]
        public string Background { get; set; }

        [JsonProperty("BackgroundType")]
        public string BackgroundType{ get; set; }

        [JsonProperty("TextColor")]
        public string TextColor { get; set; }
    }
}
