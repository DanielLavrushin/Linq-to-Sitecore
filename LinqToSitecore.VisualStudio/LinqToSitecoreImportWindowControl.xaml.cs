//------------------------------------------------------------------------------
// <copyright file="LinqToSitecoreImportWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using EnvDTE;
using LinqToSitecore.VisualStudio.Data;

namespace LinqToSitecore.VisualStudio
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for LinqToSitecoreImportWindowControl.
    /// </summary>
    public partial class LinqToSitecoreImportWindowControl
    {
        private const string unable_to_connect =
            "Unable to connect to a local Sitecore instance. Ensure you correctly entered your credentials in the Settings.";

      

        public LinqToSitecoreImportWindowControl()
        {
            this.InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        ItemTemplateControl.Visibility = Visibility.Hidden;
            FolderTemplateControl.Visibility = Visibility.Hidden;
            if (!LinqToSitecoreFactory.IsValidConnection())
            {
                MessageBox.Show(unable_to_connect);
                var settingsWindow = new LinqToSitecoreSettings();
                settingsWindow.Closed += SettingsWindow_Closed;
                settingsWindow.ShowDialog();
            }
            else
            {

                GetRoot();
            }
        }

        protected void SettingsWindow_Closed(object sender, EventArgs e)
        {
            if (LinqToSitecoreFactory.IsValidConnection())
            {
                GetRoot();
            }
            else
            {
                MessageBox.Show(unable_to_connect);
            }
        }

        private void GetRoot()
        {
            var root = LinqToSitecoreFactory.GetChildren(SitecoreGuids.Root);
            SitecoreItemsTree.ItemsSource = new ObservableCollection<Item>(root);
        }


        private void TreeViewItem_MouseClick(object sender, MouseButtonEventArgs e)
        {
            ItemTemplateControl.Visibility = Visibility.Hidden;
            FolderTemplateControl.Visibility = Visibility.Hidden;
            var tvit = sender as StackPanel;
            var item = tvit.DataContext as Item;

            if (item != null)
            {
                var detailedItem = LinqToSitecoreFactory.GetItem( item.Id );
                var templateItem = LinqToSitecoreFactory.GetItem(detailedItem.TemplateKey == "template" ? detailedItem.Id : detailedItem.TemplateId);
                var items = LinqToSitecoreFactory.GetChildren(item.Id);
                item.Children.Clear();
                foreach (var i in items)
                {
                    item.Children.Add(i);
                }

                item.IsExpanded = true;
                if (templateItem != null)
                {
                    templateItem.Namespace = LinqToSitecoreFactory.ProjectNamespace;
                    item.Namespace = LinqToSitecoreFactory.ProjectNamespace;


                    ItemTemplateControl.DataContext = templateItem;
                    ItemTemplateControl.Visibility = Visibility.Visible;
                }
            }

        }


        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new LinqToSitecoreSettings();
            settingsWindow.Closed += SettingsWindow_Closed;
            settingsWindow.ShowDialog();

        }

    


    }
}