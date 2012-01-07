using System;
using System.Xml.Serialization;

namespace Naviam.InternetBank.Entities
{
    [Serializable]
    public class InetBank
    {
        /// <summary>
        /// Bank ID in Naviam database
        /// </summary>
        [XmlAttribute(AttributeName = "naviamId")]
        public string NaviamId { get; set; }
        /// <summary>
        /// Internet Bank ID
        /// </summary>
        [XmlAttribute(AttributeName = "iBankId")]
        public string BankId { get; set; }
        /// <summary>
        /// Internet Bank Settings
        /// </summary>
        [XmlAttribute(AttributeName = "iBankSettings")]
        public string BankSettings { get; set; }
    }
}