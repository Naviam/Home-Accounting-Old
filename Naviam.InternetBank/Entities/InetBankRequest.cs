using System.Xml.Serialization;

namespace Naviam.InternetBank.Entities
{
    public class InetBankRequest
    {
        [XmlAttribute(AttributeName = "category")]
        public string Category { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "method")]
        public string Method { get; set; }
        [XmlElement(ElementName = "url", Type = typeof(string))]
        public string Url { get; set; }
        [XmlElement(ElementName = "referer", Type = typeof(string))]
        public string Referer { get; set; }
        [XmlElement(ElementName = "selector", Type = typeof(string))]
        public string Selector { get; set; }
        [XmlElement(ElementName = "postData")]
        public string PostData { get; set; }
        [XmlElement(ElementName = "setAuthCookies", Type = typeof(bool))]
        public bool SetAuthCookies { get; set; }
        [XmlArrayItem(ElementName = "cookie", Type = typeof(InetBankCookie))]
        [XmlArray(ElementName = "cookies")]
        public InetBankCookie[] CookieCollection { get; set; }
    }
}