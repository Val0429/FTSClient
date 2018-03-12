using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Tencent.DataSources;

namespace Tencent.Configurations {
    public class CameraInfoConfiguration : IConfigurationSectionHandler {
        public object Create(object parent, object configContext, XmlNode section) {
            var items = ConvertNode<List<Camera>>(section, new XmlRootAttribute("CameraInfo"));
            return items;
        }

        private static T ConvertNode<T>(XmlNode node, XmlRootAttribute root = null) where T : class {
            XmlSerializer ser = new XmlSerializer(typeof(T), root);
            T result = ser.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(node.OuterXml))) as T;
            return result;
        }
    }

}
