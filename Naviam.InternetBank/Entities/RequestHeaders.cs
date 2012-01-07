using System;
using System.Xml.Serialization;

namespace Naviam.InternetBank.Entities
{
    [Serializable]
    public class RequestHeaders
    {
        [XmlElement(ElementName = "contentType")]
        public string ContentType { get; set; }
        [XmlElement(ElementName = "preAuthenticate")]
        public bool PreAuthenticate { get; set; }
        [XmlElement(ElementName = "host")]
        public string Host { get; set; }
        [XmlElement(ElementName = "userAgent")]
        public string UserAgent { get; set; }
        [XmlElement(ElementName = "accept")]
        public string Accept { get; set; }
        [XmlElement(ElementName = "acceptLanguage")]
        public string AcceptLanguage { get; set; }
        [XmlElement(ElementName = "acceptEncoding")]
        public string AcceptEncoding { get; set; }
        [XmlElement(ElementName = "acceptCharset")]
        public string AcceptCharset { get; set; }
    }
}