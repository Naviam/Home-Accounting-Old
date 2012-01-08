using System;
using System.Xml.Serialization;

namespace Naviam.InternetBank.Entities
{
    [Serializable]
    [XmlRoot(ElementName = "internetBankSettings")]
    public class InetBankSettings
    {
        [XmlElement(ElementName = "baseUrl", Type = typeof(string))]
        public string BaseUrl { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "maxDaysPeriod", Type = typeof(int))]
        public int MaxDaysPeriod { get; set; }
        [XmlElement(ElementName = "commonHeaders", Type = typeof(RequestHeaders))]
        public RequestHeaders RequestHeaders { get; set; }
        [XmlArrayItem(ElementName = "request", Type = typeof(InetBankRequest))]
        [XmlArray(ElementName = "requests")]
        public InetBankRequest[] Requests { get; set; }
    }
}