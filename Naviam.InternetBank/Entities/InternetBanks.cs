using System;
using System.Xml.Serialization;

namespace Naviam.InternetBank.Entities
{
    [Serializable]
    [XmlRoot(ElementName = "internetBanks")]
    public class InternetBanks
    {
        [XmlElement(ElementName = "bank")]
        public InetBank[] Banks { get; set; }
    }
}