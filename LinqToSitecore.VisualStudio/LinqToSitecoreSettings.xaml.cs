using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EnvDTE;
using LinqToSitecore.VisualStudio.Data;

namespace LinqToSitecore.VisualStudio
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    public partial class LinqToSitecoreSettings
    {
        public LinqToSitecoreSettings()
        {
            this.InitializeComponent();
        }

       
        public AppSettings Settings { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            Settings = AppSettings.Instance();
            DataContext = Settings;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.SitecoreLogin = SettingAccount.Text;
            Settings.SitecoreUrl = SettingSitecoreUrl.Text;
            Settings.SitecorePassword = SettingPassword.Text;
            Settings.Save();
            Close();
        }
    }


}