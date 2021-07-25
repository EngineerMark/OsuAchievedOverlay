using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace OsuAchievedOverlay.Next
{
    public class Theme
    {
        /// <summary>
        /// The name of theme that is visible in-app
        /// </summary>
        public string PrettyName { get; set; }

        /// <summary>
        /// Which app version is this made for
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// Name used for internal handling of the theme
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Is this theme safe for work?
        /// </summary>
        public bool Safe { get; set; }

        public static Theme LoadFromFile(string path){
            string serializedData = File.ReadAllText(path);

            IXmlSerializerHelper xmlSerializerHelper = new XmlSerializerHelper();
            Theme deserializedData = xmlSerializerHelper.DeserializeFromXml<Theme>(serializedData);

            return deserializedData;
        }
    }

    public class ThemeData{
    }
}
