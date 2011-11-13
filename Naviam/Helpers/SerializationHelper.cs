using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml.Serialization;

namespace Naviam.WebUI.Helpers
{
    public class SerializationHelper
    {
        /// <summary>
        /// Serialize any object to Xml file.
        /// </summary>
        /// <param name="obj">Serializable object.</param>
        /// <param name="fileName">Xml file name.</param>
        public static void ToXml(object obj, string fileName)
        {
            try
            {
                using (TextWriter output = new StreamWriter(fileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
                    xmlSerializer.Serialize(output, obj);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Serialize any object to Xml string.
        /// </summary>
        /// <param name="obj">Serializable object.</param>
        /// <returns>Xml string.</returns>
        public static string ToXml(object obj)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            try
            {
                using (TextWriter output = new StringWriter(sb))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
                    xmlSerializer.Serialize(output, obj);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return sb.ToString();
        }
        /// <summary>
        /// Deserialize object from xml file.
        /// </summary>
        /// <param name="type">Type of object.</param>
        /// <param name="fileName">Xml file name.</param>
        public static object FromXml(Type type, string fileName)
        {
            try
            {
                using (TextReader reader = new StreamReader(fileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(type);
                    return xmlSerializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Deserialize object from xml file.
        /// </summary>
        /// <param name="type">Type of object.</param>
        /// <param name="fileName">Xml file name.</param>
        public static object FromXmlString(Type type, string xmlString)
        {
            try
            {
                using (TextReader reader = new StringReader(xmlString))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(type);
                    return xmlSerializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}