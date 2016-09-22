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
using System.Linq;
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
        public LinqToSitecoreImportWindowControl(DTE service)
        {
            Service = service;
            this.InitializeComponent();
        }

        public DTE Service { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var root = LinqToSitecoreFactory.GetRoot();

            SitecoreItemsTree.ItemsSource = new ObservableCollection<Item>() {root};

        }


        private void LoadItemData()
        {
            var item = LinqToSitecoreFactory.GetItem(SitecoreGuids.Site8MyLinqToSitecore);


        }

        private void TreeViewItem_MouseClick(object sender, MouseButtonEventArgs e)
        {
            StackPanelItem.Visibility = Visibility.Hidden;
            var tvit = sender as StackPanel;
            var item = tvit.DataContext as Item;

            if (item != null)
            {
                var detailedItem = LinqToSitecoreFactory.GetItem(item.Id);
                var items = LinqToSitecoreFactory.GetChildren(item.Id);
                item.Children.Clear();
                foreach (var i in items)
                {
                    item.Children.Add(i);
                }

             item.IsExpanded = true;
                if (detailedItem.TemplateKey == "template")
                {
                    StackPanelItem.DataContext = detailedItem;
                    StackPanelItem.Visibility = Visibility.Visible;
                }
            }

        }

        private void SettingsButton_OnMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new LinqToSitecoreSettings();
            settingsWindow.ShowDialog();

        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (Item) StackPanelItem.DataContext;

            var generator = new LinqToSitecoreFileGenerator(item, CodeDomProvider.CreateProvider("C#"));
            var code = generator.GenerateCode();
            var doc = Service.ItemOperations.NewFile(@"General\Visual C# Class", item.Name,
                Constants.vsProjectItemKindPhysicalFile);

            var txtSel = (TextSelection) Service.ActiveDocument.Selection;
            txtSel.SelectAll();
            txtSel.Delete();
            txtSel.Insert(code);
            txtSel.MoveTo(1, 1);
        }

    
    }
}