using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using EnvDTE;
using LinqToSitecore.VisualStudio.Data;

namespace LinqToSitecore.VisualStudio.Controls
{
    /// <summary>
    /// Interaction logic for ItemTemplateControl.xaml
    /// </summary>
    public partial class ItemTemplateControl: UserControl
    {
        public ItemTemplateControl()
        {
            InitializeComponent();
        }


        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (Item)DataContext;

            var generator = new LinqToSitecoreFileGenerator(item);
            if (generator.SaveToFile())
            {
                MessageBox.Show("Wow! Generation was... perfect! Time to check this out!");
            }
            else
            {
                MessageBox.Show(
             "Unable to add generated template to the current project. Ensure that the file does not exists physically on your drive.");
            }

        }
    }
}
