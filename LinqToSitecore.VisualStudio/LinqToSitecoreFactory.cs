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
        public static VisualSitecoreServiceSoapClient Service
        {
            get
            {

                if (_service == null)
                {
                    var binding = new BasicHttpBinding();
                    binding.TransferMode = TransferMode.Buffered;
                    binding.MaxBufferPoolSize = 524288;
                    binding.MaxBufferSize = 16777216;
                    binding.MaxReceivedMessageSize = 16777216;
                    binding.ReaderQuotas.MaxStringContentLength = 16777216;

                    var endpoint = new EndpointAddress("http://scdev8/sitecore/shell/WebService/Service.asmx");

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
                    UserName = @"sitecore\admin",
                    Password = "b"
                };
                return credentials;
            }
        }

        public static Item GetItem(Guid id)
        {
            var xml = Service.GetXML(id.ToString("B"), false, "master", Credentials);

            var item = xml.ToItem();
            item.Fields = GetFields(id);
            return item;
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
