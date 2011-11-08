using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using System.Configuration;
using Naviam.WebUI.Resources;
using System.Collections.Specialized;

//using Resources;

namespace Naviam.WebUI.Models
{
    public class MenuModel
    {
        #region .fields
        private MainMenuItem _rootNode = new MainMenuItem();
        private List<MainMenuItem> _internalMenuItemsList = new List<MainMenuItem>();
        private NameValueCollection _queryString;
        #endregion .fields

        #region .properties
        public List<MainMenuItem> TopMenu
        {
            get
            {
                return _rootNode != null? _rootNode.GetRange(0, _rootNode.Count) : null;
            }
        }
        public List<MainMenuItem> SubMenu
        {
            get
            {
                return GetSubMenuItems();
            }
        }
        public List<MainMenuItem> MenuItems
        {
            get
            {
                return _internalMenuItemsList;
            }
        }
        #endregion .properties

        #region .ctors

        public MenuModel() :
            this(string.Empty, string.Empty, null)
        { }
        public MenuModel(string controller, string action) :
            this(controller, action, null)
        { }
        public MenuModel(string controller, string action, NameValueCollection queryString, string sitemapConfigName = "siteMapFile")
        {
            _queryString = queryString;
            Initialize(sitemapConfigName, controller, action);
            SetActiveMenuItems(controller, action);
        }

        #endregion .ctors

        #region .metods
        private void Initialize(string sitemapConfigName, string controller, string action)
        {
            _internalMenuItemsList = new List<MainMenuItem>();
            string pathAbsolute = HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings[sitemapConfigName]);
            _rootNode = (MainMenuItem)MainMenuItem.FromXml(typeof(MainMenuItem), pathAbsolute);
            
            //check restrictions
            if (_rootNode.Restrictions != null && _rootNode.Restrictions.Equals(action, StringComparison.InvariantCultureIgnoreCase)) 
                _rootNode.Clear();
            
            GetInternalMenuItemsList(_rootNode, controller, action);
        }

        private string ToQueryString(NameValueCollection nvc)
        {
            return (nvc.Count > 0 ? "?" : "") + string.Join("&", Array.ConvertAll(nvc.AllKeys, key => string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(nvc[key]))));
        }

        private void GetInternalMenuItemsList(MainMenuItem mainMenuItem, string controller, string action)
        {
            if (mainMenuItem.Restrictions!=null && mainMenuItem.Restrictions.Equals(action, StringComparison.InvariantCultureIgnoreCase))
                return;
            _internalMenuItemsList.Add(mainMenuItem);
            if (mainMenuItem.Url != null && _queryString != null)
                mainMenuItem.Url = mainMenuItem.Url + ToQueryString(_queryString);
            foreach (MainMenuItem itm in mainMenuItem)
            {
                GetInternalMenuItemsList(itm, controller, action);
            }
        }
        private void SetActiveMenuItems(string controller, string action)
        {
            foreach (MainMenuItem item in MenuItems)
            {
                if (!string.IsNullOrEmpty(item.Controller)
                    && !string.IsNullOrEmpty(item.Action)
                    && item.Controller.Equals(controller, StringComparison.OrdinalIgnoreCase)
                    && HasAction(action, item))
                {
                    item.Selected = true;
                }
            }
        }

        private bool HasAction(string action, MainMenuItem item)
        {
            foreach (string act in item.Action.Split(','))
            {
                if (action.Equals(act, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        private List<MainMenuItem> GetSubMenuItems()
        {
            foreach (MainMenuItem item in TopMenu)
            {
                if (item.Selected)
                {
                    return item.ToList();
                }
            }
            return new List<MainMenuItem>();

        }
        #endregion .metods
    }

    [Serializable, XmlRoot("tree")]
    public class MainMenuItem : List<MainMenuItem>, IXmlSerializable
    {
        #region .constants
        const string ATTR_TITLE = "title";
        const string ATTR_DESCRIPTION = "description";
        const string ATTR_URL = "url";
        const string ATTR_ENABLELOCALIZATION = "enableLocalization";
        const string ATTR_ROLES = "roles";
        const string ATTR_STYLE = "style";
        const string ATTR_CONTROLLER = "controller";
        const string ATTR_ACTION = "action";
        const string ATTR_RESTRICTIONS = "restrictions";
        #endregion .constants

        #region .fields
        private MainMenuItem _parent = null;
        private string _url;
        private string _title;
        private string _description;
        private bool _selected;
        #endregion .fields

        #region .ctors
        public MainMenuItem()
        { }
        #endregion .ctors

        #region .properties
        [XmlAttribute(AttributeName = ATTR_TITLE)]
        public string Title
        {
            get
            {
                return EnableLocalization ? GetFromRes(_title) : _title;
            }

            set
            {
                _title = value;
            }
        }

        [XmlAttribute(AttributeName = ATTR_DESCRIPTION)]
        public string Description
        {
            get { return _description; }

            set { _description = value; }
        }

        [XmlAttribute(AttributeName = ATTR_URL)]
        public string Url
        {
            get { return _url; }

            set { _url = value; }
        }

        [XmlAttribute(AttributeName = ATTR_ENABLELOCALIZATION)]
        public bool EnableLocalization { get; set; }

        [XmlAttribute(AttributeName = ATTR_STYLE)]
        public string Style { get; set; }

        [XmlAttribute(AttributeName = ATTR_CONTROLLER)]
        public string Controller { get; set; }

        [XmlAttribute(AttributeName = ATTR_ACTION)]
        public string Action { get; set; }

        [XmlAttribute(AttributeName = ATTR_RESTRICTIONS)]
        public string Restrictions { get; set; }

        [XmlAttribute(AttributeName = ATTR_ROLES)]
        public List<String> Roles { get; set; }

        [XmlIgnore]
        public bool Selected
        {
            get
            {
                return _selected;

            }
            set
            {
                _selected = value;
                if (Parent != null) Parent.Selected = value;
            }
        }

        [XmlIgnore]
        public MainMenuItem Parent { get { return _parent; } }

        #endregion .properties

        #region .methods
        public string GetFromRes(string resourceName)
        {
            string result = string.Empty;
            CultureInfo ci;
            try
            {
                ci = CultureInfo.CurrentCulture;// CreateSpecificCulture("en");
            }
            catch (Exception)
            {
                ci = CultureInfo.InvariantCulture;
            }
            result = SharedStrings.ResourceManager.GetString(resourceName, ci);
            return string.IsNullOrEmpty(result) ? resourceName : result;
        }
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
        #endregion .methods

        #region .IXmlSerializable
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            //initialize root element
            FillItem(reader, this);

            //initialize body
            ReadXml(reader, this);
        }

        private void ReadXml(XmlReader reader, MainMenuItem current)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement) return;
                MainMenuItem itm = new MainMenuItem();
                itm._parent = current;
                current.Add(itm);
                FillItem(reader, itm);
                if (!reader.IsEmptyElement)
                {
                    ReadXml(reader, itm);
                }
            }
        }

        private void FillItem(XmlReader reader, MainMenuItem item)
        {
            item.Title = reader[ATTR_TITLE];
            item.Description = reader[ATTR_DESCRIPTION];
            item.Url = reader[ATTR_URL];
            item.EnableLocalization = bool.Parse(reader[ATTR_ENABLELOCALIZATION] != null ? reader[ATTR_ENABLELOCALIZATION] : "false");
            item.Style = reader[ATTR_STYLE];
            item.Controller = reader[ATTR_CONTROLLER];
            item.Action = reader[ATTR_ACTION];
            item.Restrictions = reader[ATTR_RESTRICTIONS];
            string roles = reader[ATTR_ROLES];
            item.Roles = new List<string>();
            if (!string.IsNullOrEmpty(roles))
            {
                item.Roles.AddRange(reader[ATTR_ROLES].Split(';'));
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            WriteItem(writer, this);
            WriteXml(writer, this);
        }

        private void WriteXml(XmlWriter writer, MainMenuItem current)
        {
            for (int i = 0; i < current.Count; i++)
            {
                writer.WriteStartElement("node");
                WriteItem(writer, current[i]);
                if (current[i].Count > 0)
                    WriteXml(writer, current[i]);
                writer.WriteEndElement();
            }
        }
        private void WriteItem(XmlWriter writer, MainMenuItem current)
        {
            writer.WriteAttributeString(ATTR_TITLE, current.Title);
            writer.WriteAttributeString(ATTR_DESCRIPTION, current.Description);
            writer.WriteAttributeString(ATTR_URL, current.Url);
            writer.WriteAttributeString(ATTR_STYLE, current.Style);
            writer.WriteAttributeString(ATTR_CONTROLLER, current.Controller);
            writer.WriteAttributeString(ATTR_ACTION, current.Action);
            writer.WriteAttributeString(ATTR_ENABLELOCALIZATION, current.EnableLocalization.ToString());
            writer.WriteAttributeString(ATTR_RESTRICTIONS, current.Restrictions);
        }
        #endregion .IXmlSerializable

    }


}
