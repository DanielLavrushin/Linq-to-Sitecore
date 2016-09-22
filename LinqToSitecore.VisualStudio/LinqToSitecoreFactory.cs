using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LinqToSitecore.VisualStudio.Data;
using LinqToSitecore.VisualStudio.SitecoreWebService;
using Microsoft.Internal.VisualStudio.PlatformUI;

namespace LinqToSitecore.VisualStudio
{
    public static class LinqToSitecoreFactory
    {
        private static VisualSitecoreServiceSoapClient _service;
        private static AppSettings _settings = AppSettings.Instance();

        public static VisualSitecoreServiceSoapClient Service
        {
            get
            {

                if (_service == null)
                {
                    var binding = new BasicHttpBinding
                    {
                        TransferMode = TransferMode.Buffered,
                        MaxBufferPoolSize = 524288,
                        MaxBufferSize = 16777216,
                        MaxReceivedMessageSize = 16777216,
                        ReaderQuotas = {MaxStringContentLength = 16777216}
                    };

                    var endpoint = new EndpointAddress($"{_settings.SitecoreUrl}/sitecore/shell/WebService/Service.asmx");

                    _service = new VisualSitecoreServiceSoapClient(binding, endpoint);
                }
                return _service;
            }
        }

        private static Credentials Credentials
        {
            get
            {
                var credentials = new Credentials
                {
                    UserName = _settings.SitecoreLogin,
                    Password = _settings.SitecorePassword
                };
                return credentials;
            }
        }

        public static Item GetItem(Guid id)
        {
            var xml = Service.GetXML(id.ToString("B"), false, "master", Credentials);

            var item = xml.ToItem();
            return item;
        }
        public static Item GetItem(Guid id, bool loadAll)
        {
            var xml = Service.GetXML(id.ToString("B"), loadAll, "master", Credentials);

            var item = xml.ToItem();
            return item;
        }
        public static Item GetRoot()
        {
            return GetItem(SitecoreGuids.Root);
        }

        public static ICollection<Item> GetChildren(Guid id)
        {
            var xml = Service.GetChildren(id.ToString("B"), "master", Credentials);
            var items = xml.ChildNodes.ToItems();

            return items;


        }
        public static ICollection<Field> GetFields(Guid id)
        {
            var xml = Service.GetXML(id.ToString("B"), true, "master", Credentials);
            var fields = new List<Field>();
            foreach (XmlNode x in xml.SelectNodes(@"//*[@template='template field']").Cast<XmlNode>().ToList())
            {
                    fields.Add(Field.Parse(x));
              
            }

            return fields;
        }
      
    }
}
