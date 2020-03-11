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
using System.IO;
using System.ComponentModel;
using System.Threading;
using Microsoft.Win32;

namespace SortZilla
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Variables

        private BackgroundWorker bgWorker = new BackgroundWorker();
        private string globalDataSetPath;
        private string globallabelRealPath;
        private string globallabelRealOutputFolder;

        private List<ZillaConfig> zCFGList = new List<ZillaConfig>();
        private List<int> candidateMappings = new List<int>();
        private bool processingFinished = false;
        private bool loadedConfig = false;
        private bool uiEnabled = true;
        private bool forcedStop = false;
        private int filesToBeProcessed = 0;
        private int sortedFiles = 0;
        private int processedFiles = 0;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            InitializebgWorkers();
        }

        #endregion

        #region Events

        private void buttonAddConfig_Click(object sender, RoutedEventArgs e)
        {
            ZillaConfig zCFG;

            if ((zCFG = CreateZillaConfig()) != null)
            {
                if (listBoxCurrentConfig.SelectedIndex < 0)
                    PopulateListBox(zCFG);
                else
                {
                    EditZillaConfig(zCFG, listBoxCurrentConfig.SelectedIndex);

                    UpdateListBox(zCFG, listBoxCurrentConfig.SelectedIndex);
                }

                ClearConfigFields();
            }
        }

        private void buttonSetPath_Click(object sender, RoutedEventArgs e)
        {
            HandleSetDataPath();
        }

        private void buttonSetLabelsPath_Click(object sender, RoutedEventArgs e)
        {
            HandleSetLabelsPath();
        }

        private void buttonSetOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            HandleSetOutputPath();
        }

        private void buttonDeleteConfig_Click(object sender, RoutedEventArgs e)
        {
            DeleteItemFromListBox();
        }

        private void buttonLoadConfig_Click(object sender, RoutedEventArgs e)
        {
            string filePath = OpenFileDialog();

            LoadFromFile(filePath);
        }

        private void buttonSaveConfig_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog svd = new SaveFileDialog();

            svd.Filter = "Text file (*.txt)|*.txt";

            if (svd.ShowDialog() == true)
                SaveToFile(svd.FileName);
        }

        private void buttonWorkZilla_Click(object sender, RoutedEventArgs e)
        {
            if (uiEnabled == true)
            {
                DisableUI();

                FindFilesToBeProcessed();

                CreateCandidateMappings();

                if (CreateOutputFolders())
                    bgWorker.RunWorkerAsync();
            }
            else
            {
                EnableUI();

                bgWorker.CancelAsync();
            }

        }

        private void listBoxCurrentConfig_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxCurrentConfig.SelectedIndex != -1)
            {
                int tempIndex = listBoxCurrentConfig.SelectedIndex;

                textBoxFolderName.Text = zCFGList[tempIndex].FolderName;
                comboBoxMapFrom.SelectedIndex = zCFGList[tempIndex].ComboBoxIndex;
                textBoxNumberOfFiles.Text = zCFGList[tempIndex].Amount.ToString();
            }
        }

        private void listBoxCurrentConfig_MouseDown(object sender, MouseButtonEventArgs e)
        {
            listBoxCurrentConfig.SelectedIndex = -1;

            ClearConfigFields();
        }

        #endregion

        #region Background Worker

        private void InitializebgWorkers()
        {
            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);
            bgWorker.WorkerReportsProgress = true;
            bgWorker.WorkerSupportsCancellation = true;
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ReadLabelsFile(e);
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Dispatcher.Invoke(delegate
            {
                progressBarStatus.Value = sortedFiles;
            });
        }

        #endregion

        #region Functions

        private void DisableUI()
        {
            uiEnabled = false;
            forcedStop = false;

            progressBarStatus.IsIndeterminate = false;
            progressBarStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF09E696"));
           
            SetStatusImage(imageLogo, "img/SortZillaLogo_2_working.png");

            foreach (UIElement element in MainGrid.Children)
                if (element.Uid != "workZilla")
                    element.IsEnabled = false;

            labelFour.Content = "STOP HIM";
        }

        private void EnableUI()
        {
            uiEnabled = true;

            imageLogo.Source = new BitmapImage(new Uri(@"img/SortZillaLogo_2.png", UriKind.Relative));

            foreach (UIElement element in MainGrid.Children)
                element.IsEnabled = true;

            labelFour.Content = "UNLEASH";
        }

        private string OpenFileDialog()
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();

            //dialog.InitialDirectory = @"C:";
            dialog.InitialDirectory = @"E:\Download\rvl-cdip";
            dialog.DefaultExt = ".txt";
            dialog.Filter = ".TXT Files (*.txt)|*.txt";

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true)
                return dialog.FileName;
            else
            {
                SetStatusImage(imgPathLabels, "img/alert.png");
                return "Error browsing for file!";
            }
        }

        private string OpenFolderBrowserDialog(string source)
        {
            WPFFolderBrowser.WPFFolderBrowserDialog wpffbd = new WPFFolderBrowser.WPFFolderBrowserDialog();

            //wpffbd.InitialDirectory = @"C:";
            wpffbd.InitialDirectory = @"E:\Download\rvl-cdip";

            bool? pathWasSelected = wpffbd.ShowDialog();

            if (pathWasSelected == true)
                return wpffbd.FileName;
            else
            {
                if (source == "data")
                    SetStatusImage(imgPathDataSet, "img/alert.png");
                else if (source == "output")
                    SetStatusImage(imgPathOutput, "img/alert.png");

                return "Error browsing for folder!";
            }
        }

        private void FindFilesToBeProcessed()
        {
            foreach (ZillaConfig zCFG in zCFGList)
                filesToBeProcessed += zCFG.Amount;

            progressBarStatus.Maximum = filesToBeProcessed;
        }

        private void CreateCandidateMappings()
        {
            candidateMappings = new List<int>();

            foreach (ZillaConfig zCGF in zCFGList)
                candidateMappings.Add(zCGF.ComboBoxIndex);
        }

        private ZillaConfig CreateZillaConfig()
        {
            try
            {
                if (textBoxFolderName.Text == "")
                    MessageBox.Show("Name the folder to which you mant to map to. SortZilla can't guess what you want :(.");

                else if (comboBoxMapFrom.SelectedIndex < 0)
                    MessageBox.Show("Select category from where to map from. Don't confuse SortZilla :(.");

                else if (int.Parse(textBoxNumberOfFiles.Text) == 0 || int.Parse(textBoxNumberOfFiles.Text) < -1)
                    MessageBox.Show("Enter -1 if you want SortZilla to process all files. Other negative or non-zero numbers are not accepted.");

                else
                {
                    ZillaConfig zCFG;

                    zCFG = new ZillaConfig(textBoxFolderName.Text,
                                                          comboBoxMapFrom.SelectedIndex,
                                                          comboBoxMapFrom.Text,
                                                          int.Parse(textBoxNumberOfFiles.Text),
                                                          0);

                    return zCFG;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

            return null;
        }

        private void EditZillaConfig(ZillaConfig zCFG, int index)
        {
            zCFGList[index].Amount = zCFG.Amount;
            zCFGList[index].AmountDummy = zCFG.AmountDummy;
            zCFGList[index].ComboBoxIndex = zCFG.ComboBoxIndex;
            zCFGList[index].ComboBoxString = zCFG.ComboBoxString;
            zCFGList[index].FolderName = zCFG.FolderName;
        }

        private Boolean CreateOutputFolders()
        {
            foreach (ZillaConfig zCFG in zCFGList)
                if (Directory.Exists(labelRealOutputFolder.Content + "\\" + zCFG.FolderName) == false)
                    try
                    {
                        Directory.CreateDirectory(labelRealOutputFolder.Content + "\\" + zCFG.FolderName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString() + " (in new folder name)");
                        return false;
                    }
            return true;
        }

        private void ReadLabelsFile(DoWorkEventArgs e)
        {
            using (StreamReader sr = new StreamReader(globallabelRealPath))
            {
                String line;

                while ((line = sr.ReadLine()) != null && processingFinished == false)
                {
                    if (bgWorker.CancellationPending)
                    {
                        this.Dispatcher.Invoke(delegate
                        {
                            forcedStop = true;
                            labelStatus.Content = "Status: Whoops. SortZilla's abandoned your files. Some have been sorted, though.";
                            progressBarStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFF0046"));
                            progressBarStatus.IsIndeterminate = true;
                        });
                        break;
                    }
                    else
                    {
                        string[] tokenizedLine = new string[2];

                        tokenizedLine = line.Split(' ');

                        // If images<char> folder has been selected
                        if (globalDataSetPath.Split('\\').Last() == tokenizedLine[0].Split('\\').First())
                        {
                            string newToken = tokenizedLine[0].Replace(tokenizedLine[0].Split('\\').First(), "");

                            if (candidateMappings.Contains(int.Parse(tokenizedLine[1])))
                                MapFile(new ZillaLabel(newToken, int.Parse(tokenizedLine[1])));

                            processedFiles++;
                        }

                        // If global images folder has been selected
                        else if (globalDataSetPath.Split('\\').Last() == "images")
                        {
                            if (candidateMappings.Contains(int.Parse(tokenizedLine[1])))
                                MapFile(new ZillaLabel(tokenizedLine[0], int.Parse(tokenizedLine[1])));

                            processedFiles++;
                        }

                        // All lines from labels.txt that have been read
                        else
                        {
                            processedFiles++;
                            continue;
                        }
                    }
                }

                ResetConfig();
            }
        }

        private void ProcessLine(string[] tokLine)
        {
            ZillaLabel zLabel = new ZillaLabel(tokLine[0], int.Parse(tokLine[1]));

            MapFile(zLabel);
        }

        private void MapFile(ZillaLabel zLabel)
        {
            foreach (ZillaConfig zCFG in zCFGList)
                if (zLabel.Mapping == zCFG.ComboBoxIndex && zCFG.AmountDummy != zCFG.Amount)
                {
                    // If file already exists, it won't be overwritten, and there is no prompt for overwriting
                    try
                    {
                        File.Copy(globalDataSetPath + "\\" +
                                  zLabel.Path, globallabelRealOutputFolder +
                                  "\\" + zCFG.FolderName + "\\" +
                                  zLabel.Path.Split('\\').Last());

                        sortedFiles++;
                        zCFG.AmountDummy++;
                    }
                    catch (Exception ex) { Console.WriteLine(ex); }

                    bgWorker.ReportProgress(sortedFiles);
                }

            CheckIsProcessingFinished();
        }

        private void CheckIsProcessingFinished()
        {
            processingFinished = true;

            foreach (ZillaConfig zCFG in zCFGList)
                if (zCFG.AmountDummy != zCFG.Amount)
                {
                    processingFinished = false;
                    break;
                }

            this.Dispatcher.Invoke(delegate
            {
                labelStatus.Content = "Status: SortZilla's hard at work! He went through " +
                                      processedFiles.ToString() + " files, of which he sorted " +
                                      sortedFiles.ToString() + "/" + filesToBeProcessed.ToString();
            });
        }

        private void UpdateListBox(ZillaConfig zCFG, int index)
        {
            listBoxCurrentConfig.Items[index] = "Map " + zCFG.ComboBoxString + " to " +
                                                zCFG.FolderName + " for " + zCFG.Amount + " elements";
        }

        private void PopulateListBox(ZillaConfig zCFG)
        {
            listBoxCurrentConfig.Items.Add("Map " + zCFG.ComboBoxString + " to " +
                                            zCFG.FolderName + " for " + zCFG.Amount + " elements");

            zCFGList.Add(zCFG);
        }

        private void DeleteItemFromListBox()
        {
            if (listBoxCurrentConfig.SelectedItem != null)
            {
                zCFGList.RemoveAt(listBoxCurrentConfig.SelectedIndex);
                listBoxCurrentConfig.Items.Remove(listBoxCurrentConfig.SelectedItem);
            }
        }

        private void SetStatusImage(Image img, string imgName)
        {
            img.Source = new BitmapImage(new Uri(imgName, UriKind.Relative));
        }

        private void SaveToFile(string filePath)
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                List<Object> elementsToSave = new List<Object>();

                elementsToSave.Add(labelRealPath.Content);
                elementsToSave.Add(labelRealLabelsPath.Content);
                elementsToSave.Add(labelRealOutputFolder.Content);

                foreach (ZillaConfig zCFG in zCFGList)
                    elementsToSave.Add(zCFG.ToString());

                foreach (Object obj in elementsToSave)
                    sw.WriteLine(obj);
            }
        }

        private void LoadFromFile(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                String line;

                line = sr.ReadLine();
                labelRealPath.Content = line;

                line = sr.ReadLine();
                labelRealLabelsPath.Content = line;

                line = sr.ReadLine();
                labelRealOutputFolder.Content = line;

                loadedConfig = true;
                HandleAllPaths();
                loadedConfig = false;

                zCFGList = new List<ZillaConfig>();
                listBoxCurrentConfig.Items.Clear();

                while ((line = sr.ReadLine()) != null)
                {
                    string[] tokLine = new string[5];

                    ZillaConfig zCFG = LoadZillaConfig(tokLine = line.Split('~'));

                    PopulateListBox(zCFG);
                }
            }
        }

        private ZillaConfig LoadZillaConfig(string[] tokLine)
        {
            ZillaConfig zCFG = new ZillaConfig(tokLine[0],
                                               int.Parse(tokLine[1]),
                                               tokLine[2],
                                               int.Parse(tokLine[3]),
                                               int.Parse(tokLine[4]));

            return zCFG;
        }

        private void HandleAllPaths()
        {
            HandleSetDataPath();
            HandleSetLabelsPath();
            HandleSetOutputPath();
        }

        private void HandleSetDataPath()
        {
            if (loadedConfig == false)
                labelRealPath.Content = OpenFolderBrowserDialog("data");

            if (labelRealPath.Content.ToString() != "Error browsing for folder!")
                SetStatusImage(imgPathDataSet, "img/tick.png");
            else
                SetStatusImage(imgPathDataSet, "img/alert.png");

            globalDataSetPath = labelRealPath.Content.ToString();
        }

        private void HandleSetLabelsPath()
        {
            if (loadedConfig == false)
                labelRealLabelsPath.Content = OpenFileDialog();

            if (labelRealLabelsPath.Content.ToString() != "Error browsing for file!")
                SetStatusImage(imgPathLabels, "img/tick.png");
            else
                SetStatusImage(imgPathDataSet, "img/alert.png");

            globallabelRealPath = labelRealLabelsPath.Content.ToString();
        }

        private void HandleSetOutputPath()
        {
            if (loadedConfig == false)
                labelRealOutputFolder.Content = OpenFolderBrowserDialog("output");

            if (labelRealOutputFolder.Content.ToString() != "Error browsing for folder!")
                SetStatusImage(imgPathOutput, "img/tick.png");
            else
                SetStatusImage(imgPathDataSet, "img/alert.png");

            globallabelRealOutputFolder = labelRealOutputFolder.Content.ToString();
        }

        private void ClearConfigFields()
        {
            textBoxFolderName.Text = "";
            textBoxNumberOfFiles.Text = "";
            comboBoxMapFrom.SelectedIndex = -1;
        }

        private void ResetConfig()
        {
            foreach (ZillaConfig zCFG in zCFGList)
                zCFG.AmountDummy = 0;

            sortedFiles = 0;

            processedFiles = 0;
            filesToBeProcessed = 0;
            processingFinished = false;

            this.Dispatcher.Invoke(delegate
            {
                if (forcedStop == false)
                    labelStatus.Content = "Status: Your files are sorted! SortZilla's going back to sleep.";

                progressBarStatus.Value = 0;

                EnableUI();
            });

        }

        #endregion

    }
}

