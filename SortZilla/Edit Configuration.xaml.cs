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
using System.Windows.Shapes;

namespace SortZilla
{
    /// <summary>
    /// Interaction logic for Edit_Configuration.xaml
    /// </summary>
    public partial class Edit_Configuration : Window
    {
        public Edit_Configuration()
        {
            InitializeComponent();
        }

        //
        // THIS WINDOW IS NOT IN USE ANYMORE
        //

        private void buttonSubmitConfig_Click(object sender, RoutedEventArgs e)
        {
            ZillaConfig zCFG = new ZillaConfig(textBoxFolderName.Text,
                                               comboBoxMapFrom.SelectedIndex,
                                               comboBoxMapFrom.Text,
                                               int.Parse(textBoxNumberOfFiles.Text),
                                               0);

            ((MainWindow)Application.Current.MainWindow).listBoxCurrentConfig.Items.Add("Map " + zCFG.ComboBoxString + " to " + zCFG.FolderName + " for " + zCFG.Amount + " elements");

            //((MainWindow)Application.Current.MainWindow).zCFGList.Add(zCFG);

            this.Close();
        }
    }
}
