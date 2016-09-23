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

        private string _projectNamespace;
        private Project _project;

        public LinqToSitecoreImportWindowControl(DTE service)
        {
            Service = service;
            this.InitializeComponent();
        }

        public DTE Service { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanelItem.Visibility = Visibility.Hidden;
            if (!LinqToSitecoreFactory.IsValidConnection())
            {
                MessageBox.Show(unable_to_connect);
                var settingsWindow = new LinqToSitecoreSettings();
                settingsWindow.Closed += SettingsWindow_Closed;
                settingsWindow.ShowDialog();
            }
            else
            {

                _project = ((object[]) Service.ActiveSolutionProjects)[0] as Project;
                var properties = _project.Properties.Cast<Property>().ToDictionary(x => x.Name);
                _projectNamespace = properties["DefaultNamespace"].Value.ToString();

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
            StackPanelItem.Visibility = Visibility.Hidden;
            var tvit = sender as StackPanel;
            var item = tvit.DataContext as Item;

            if (item != null)
            {
                var detailedItem = LinqToSitecoreFactory.GetItem(item.Id);
                var items = LinqToSitecoreFactory.GetChildren(item.Id);
                item.Children.Clear();
                detailedItem.Namespace = _projectNamespace;
                foreach (var i in items)
                {
                    item.Children.Add(i);
                }

                item.IsExpanded = true;
                item.Namespace = _projectNamespace;
                if (detailedItem.TemplateKey == "template")
                {
                    StackPanelItem.DataContext = detailedItem;
                    StackPanelItem.Visibility = Visibility.Visible;
                }
            }

        }


        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new LinqToSitecoreSettings();
            settingsWindow.Closed += SettingsWindow_Closed;
            settingsWindow.ShowDialog();

        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (Item) StackPanelItem.DataContext;


            item.IsSystemIncluded = GenerateSystemProperties.IsChecked.GetValueOrDefault();

            var generator = new LinqToSitecoreFileGenerator(item, CodeDomProvider.CreateProvider("C#"));
            var code = generator.GenerateCode();
            Service.ItemOperations.NewFile(@"General\Visual C# Class", item.DisplayName,
                Constants.vsProjectItemKindPhysicalFile);

            var txtSel = (TextSelection) Service.ActiveDocument.Selection;
            txtSel.SelectAll();
            txtSel.Delete();
            txtSel.Insert(code);
            txtSel.MoveTo(1, 1);

            var projectDir = new FileInfo(_project.FullName).Directory.FullName;
            var namespacepath = item.Namespace.EndsWith(".") ? item.Namespace : item.Namespace + '.';


            string pattern = $@"^(?<project>{_projectNamespace}\.)(?<sub>.*)";
            var regexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant;
            var regex = new Regex(pattern, regexOptions);

            namespacepath = regex.Replace(namespacepath, @"${sub}");
            namespacepath = namespacepath.Replace('.', '\\');
           

            if (!Directory.Exists(projectDir + namespacepath))
            {
                Directory.CreateDirectory(projectDir + namespacepath);
            }

            var codeFilePath = $@"{projectDir}\{namespacepath}{item.DisplayName}.cs";

            try
            {

                Service.ActiveDocument.Save(codeFilePath);
                _project.ProjectItems.AddFromFile(codeFilePath);
                MessageBox.Show("Wow! Generation was... perfect! Time to check this out!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unable to add generated template to the current project. Ensure that the file does not exists physically on your drive.");
            }

        }


    }
}