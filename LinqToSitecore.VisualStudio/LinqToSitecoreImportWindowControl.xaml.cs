//------------------------------------------------------------------------------
// <copyright file="LinqToSitecoreImportWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

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
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqToSitecoreImportWindowControl"/> class.
        /// </summary>
        public LinqToSitecoreImportWindowControl()
        {
            this.InitializeComponent();
        }

        public DTE Service { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


            var item = LinqToSitecoreFactory.GetItem(SitecoreGuids.Site8MyLinqToSitecore);
            var solutionDir = System.IO.Path.GetDirectoryName(Service.Solution.FullName);


            Service.ItemOperations.NewFile(@"General\Visual C# Class", item.Name, Constants.vsViewKindTextView);

            var txtSel = (TextSelection)Service.ActiveDocument.Selection;
            var txtDoc = (TextDocument)Service.ActiveDocument.Object("");

            txtSel.SelectAll();
            txtSel.Delete();
            txtSel.Insert("using System;" + System.Environment.NewLine);
            txtSel.Insert(System.Environment.NewLine);
            txtSel.Insert(System.Environment.NewLine);

            txtSel.Insert($"public class {item.Name}{{" + System.Environment.NewLine);
            txtSel.Insert("}");

        }

      
    }
}