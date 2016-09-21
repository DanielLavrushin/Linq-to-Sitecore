﻿//------------------------------------------------------------------------------
// <copyright file="LinqToSitecoreImportWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

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

            var generator = new LinqToSitecoreFileGenerator(item, CodeDomProvider.CreateProvider("C#"));
            var code = generator.GenerateCode();
            var doc = Service.ItemOperations.NewFile(@"General\Visual C# Class", item.Name,
                Constants.vsProjectItemKindPhysicalFile);

            var txtSel = (TextSelection) Service.ActiveDocument.Selection;
            txtSel.SelectAll();
            txtSel.Delete();
            txtSel.Insert(code);
            txtSel.MoveTo(0, 0);
        }

        private void TreeViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var tvit = sender as TreeViewItem;
            var item = tvit.DataContext as Item;

            if (item != null)
            {
                var items = LinqToSitecoreFactory.GetChildren(item.Id);
                item.Children.Clear();
                foreach (var i in items)
                {
                    item.Children.Add(i);
                }
                tvit.IsExpanded = true;
                MessageBox.Show("found items : " + item.Children.Count);
            }

        }
    }
}