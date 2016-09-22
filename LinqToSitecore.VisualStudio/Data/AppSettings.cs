using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace LinqToSitecore.VisualStudio.Data
{
    public class AppSettings
    {

        public static AppSettings Instance()
        {
            return _settings ?? Load();
        }

        private static AppSettings _settings;
          
        public string SitecoreUrl { get; set; }
        public string SitecoreLogin { get; set; }
        public string SitecorePassword { get; set; }

        public static string Path
        {
            get
            {

                var folder =
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "LinqToSitecore.VisualStudio");

                if (!System.IO.Directory.Exists(folder))
                {
                    System.IO.Directory.CreateDirectory(folder);
                }

                var settingsFile = System.IO.Path.Combine(folder, "settings.xml");
                return settingsFile;
            }
        }


        public static AppSettings Load()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(AppSettings));
                using (var reader = XmlReader.Create(Path))
                {
                    _settings= (AppSettings) serializer.Deserialize(reader);
                }
            }
            catch (Exception)
            {
                _settings = new AppSettings();
            }

            if (_settings.SitecoreUrl == string.Empty)
            {
                _settings.SitecoreUrl = "http://";
            }
            if (_settings.SitecoreLogin == string.Empty)
            {
                _settings.SitecoreLogin = @"sitecore\admin";
            }

            return _settings;
        }

        public void Save()
        {
            if (!string.IsNullOrEmpty(SitecoreUrl) && SitecoreUrl.EndsWith("/"))
            {
                SitecoreUrl = SitecoreUrl.Substring(0, SitecoreUrl.Length - 1);
            }

            var serializer = new XmlSerializer(typeof(AppSettings));
            using (var writer = XmlWriter.Create(Path))
            {
                serializer.Serialize(writer, this);
            }
            _settings = this;
        }
    }

}
