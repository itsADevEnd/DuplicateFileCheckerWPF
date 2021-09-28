using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using Ookii.Dialogs.Wpf;
using Path = System.IO.Path;
using Newtonsoft.Json;

namespace DuplicateFileCheckerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _logFilePath = "";
        private string _fileName = "duplicates.txt";
        private string _selectedLogFolderCaption = "Log Folder: ";
        private Settings AppSettings;
        private bool _savedLogFolderExists = true;

        public MainWindow()
        {
            InitializeComponent();
            string json = File.ReadAllText(Directory.GetCurrentDirectory() + @"\settings.json");
            AppSettings = DeserializeJsonSettings(json);

            if (!string.IsNullOrWhiteSpace(AppSettings.LogDirectory))
            {
                if (Directory.Exists(AppSettings.LogDirectory))
                {
                    SelectedLogFolder.Text = _selectedLogFolderCaption + AppSettings.LogDirectory;
                    _savedLogFolderExists = true;
                }
                else
                {
                    _savedLogFolderExists = false;
                    SelectedLogFolder.Text = _selectedLogFolderCaption + "No log folder selected";
                    MessageBox.Show("Previously remembered log folder " + AppSettings.LogDirectory + " cannot be found.");
                }
            }
        }

        private Settings DeserializeJsonSettings(string json)
        {
            return JsonConvert.DeserializeObject<Settings>(json);
        }

        private void SelectFolder_Click(object sender, EventArgs e)
        {
            do
            {
                if (!_savedLogFolderExists) _logFilePath = GetFolderPath();
                else break;

                if (_logFilePath == "") return;
            } while (_logFilePath == "You must select a folder.");

            string folderPathToSearch;

            do
            {
                folderPathToSearch = GetFolderPath();

                if (folderPathToSearch == "") return;
            } while (folderPathToSearch ==  "You must select a folder.");

            string heading = "Possible duplicates in " + folderPathToSearch + Environment.NewLine;
            string possibleDuplicatesString = "";
            List<string> fileList = Directory.EnumerateFiles(folderPathToSearch).ToList();
            List<string> fileListClone = new List<string>(fileList);
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = fileList.Count;

            foreach (string file in fileList)
            {
                int fileMatch = 0;
                FileInfo fileInfo = new FileInfo(file);
                int clonedFileListIndex = 0;
                List<int> indicesToRemove = new List<int>();

                foreach (string clonedFile in fileListClone)
                {
                    FileInfo clonedFileInfo = new FileInfo(clonedFile);

                    if (fileInfo.Length.Equals(clonedFileInfo.Length))
                    {
                        fileMatch++;
                        indicesToRemove.Add(clonedFileListIndex);
                        clonedFileListIndex++;

                        if (fileMatch > 1)
                        {
                            string fileName = FileNameOnly(clonedFile);
                            FilesListBox.Items.Add(fileName);
                            possibleDuplicatesString += "   •" + fileName + Environment.NewLine;
                        }
                    }
                }

                for (int i = indicesToRemove.Count - 1; i > 0; i--)
                {
                    fileListClone.RemoveAt(indicesToRemove[i]);
                }

                Thread.Sleep(100);
                ProgressBar.Value++;
            }

            string content = "--- START OF LOG (" + DateTime.Now.ToString("U") + ") ---" + Environment.NewLine + heading + possibleDuplicatesString + Environment.NewLine + "--- END OF LOG ---" + Environment.NewLine;
            File.AppendAllText(Path.Combine(_logFilePath + @"\", _fileName), content);
            
            if (MessageBox.Show("Do you want to view the log file now? If you click 'No' you can still view it later.", "View log file?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Process.Start("notepad.exe", _logFilePath + @"\" + _fileName);
            }
        }

        private static string FileNameOnly(string filePath)
        {
            return filePath.Remove(0, filePath.LastIndexOf(@"\") + 1);
        }

        private void Hint_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("When you click \"Select Folder\" File Explorer will appear — select the folder where you want to store the log file. The log file will contain the possible duplicate files that are found." + Environment.NewLine + Environment.NewLine + "The second File Explorer window will appear after this — select the folder you want to search duplicates for." + Environment.NewLine + Environment.NewLine + "Select 'Remember Log Folder' before selecting the log folder to remember the log folder selected. This only needs to be done once and you will see your selected log folder at the top of the application.", "Help");
        }

        private string GetFolderPath()
        {
            VistaFolderBrowserDialog browseForLogDirectory = new VistaFolderBrowserDialog();

            if (browseForLogDirectory.ShowDialog() == false)
            {
                return "";
            }

            if (browseForLogDirectory.SelectedPath == "")
            {
                return "You must select a folder.";
            }
            else
            {
                return browseForLogDirectory.SelectedPath;
            }
        }
    }
}