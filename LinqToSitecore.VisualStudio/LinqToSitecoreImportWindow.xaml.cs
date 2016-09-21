using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LinqToSitecore.VisualStudio.SitecoreWebService;

namespace LinqToSitecore.VisualStudio
{
    /// <summary>
    /// Interaction logic for LinqToSitecoreImportWindow.xaml
    /// </summary>
    public partial class LinqToSitecoreImportWindow
    {
        public LinqToSitecoreImportWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var credentials = new Credentials();
            credentials.UserName = "admin";
            credentials.Password = "b";

            var service = new SitecoreWebService.SitecoreWebService2SoapClient();
            service.GetXML("{11111111-1111-1111-1111-111111111111}", false, "master", credentials);
        }
    }
}
