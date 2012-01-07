using System.Xml.Serialization;

namespace Naviam.InternetBank.Entities
{
    public class InetBankCookie
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
        [XmlAttribute(AttributeName = "path")]
        public string Path { get; set; }
        [XmlAttribute(AttributeName = "domain")]
        public string Domain { get; set; }
    }
}