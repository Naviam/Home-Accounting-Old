using System.Xml.Serialization;

namespace Naviam.InternetBank.Entities
{
    public class LoginRequest : InetBankRequest
    {
        [XmlElement(ElementName = "setAuthCookies", Type = typeof(bool))]
        public bool SetAuthCookies { get; set; }
        [XmlArrayItem(ElementName = "cookie", Type = typeof(InetBankCookie))]
        [XmlArray(ElementName = "cookies")]
        public InetBankCookie[] CookieCollection { get; set; }
    }
}