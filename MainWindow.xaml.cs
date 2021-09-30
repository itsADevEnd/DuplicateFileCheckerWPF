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

namespace DuplicateFileCheckerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string LogFilePath { get; set; } = "";
        public string FileName { get; set; } = "duplicates.txt";
        public string LogFolderCaptionPrefix { get; } = "Log Folder: ";

        public MainWindow()
        {
            InitializeComponent();

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LogDirectory))
            {
                if (Directory.Exists(Properties.Settings.Default.LogDirectory))
                {
                    SelectedLogFolder.Text = LogFolderCaptionPrefix + Properties.Settings.Default.LogDirectory;
                    LogFilePath = Properties.Settings.Default.LogDirectory;
                }
                else
                {
                    SelectedLogFolder.Text = LogFolderCaptionPrefix + "No log folder selected";

                    if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LogDirectory))
                    {
                        if (MessageBox.Show("Previously remembered log folder " + Properties.Settings.Default.LogDirectory + " cannot be found. Would you like to clear this remembered folder?", "Log folder not found", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            Properties.Settings.Default.LogDirectory = "";
                        }
                    }
                }
            }
        }

        private void SelectLogFolder_Click(object sender, RoutedEventArgs e)
        {
            do
            {
                LogFilePath = GetFolderPath();

                if (LogFilePath == "") return;
                else if (LogFilePath != "You must select a folder.")
                {
                    SelectedLogFolder.Text = LogFolderCaptionPrefix + LogFilePath;
                    if (RememberLogFolder.IsChecked == true)
                    {
                        SelectedLogFolder.Text += " (this log folder has been remembered and will be used the next time you use this application)";
                        Properties.Settings.Default.LogDirectory = LogFilePath;
                        Properties.Settings.Default.Save();
                    }
                }
            } while (LogFilePath == "You must select a folder.");
        }

        private void SelectFolderToCheck_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LogFilePath))
            {
                MessageBox.Show("You must select a log folder before checking any files.", "Unable to check files");
                return;
            }

            string folderPathToSearch;

            do
            {
                folderPathToSearch = GetFolderPath();

                if (folderPathToSearch == "") return;
            } while (folderPathToSearch ==  "You must select a folder.");

            Spinner.Visibility = Visibility.Visible;
            FilesTextBlock.Text = "";
            string heading = "Possible duplicates in " + folderPathToSearch + Environment.NewLine;
            string possibleDuplicatesString = "";
            List<string> fileList = Directory.EnumerateFiles(folderPathToSearch).ToList();
            List<string> fileListClone = new List<string>(fileList);

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
                            FilesTextBlock.Text += fileName + Environment.NewLine;
                            possibleDuplicatesString += "   •" + fileName + Environment.NewLine;
                        }
                    }
                }

                for (int i = indicesToRemove.Count - 1; i > 0; i--)
                {
                    fileListClone.RemoveAt(indicesToRemove[i]);
                }
            }

            FilesTextBlock.Text = FilesTextBlock.Text.Remove(FilesTextBlock.Text.LastIndexOf(Environment.NewLine), Environment.NewLine.Length);
            string content = "--- START OF LOG (" + DateTime.Now.ToString("U") + ") ---" + Environment.NewLine + heading + possibleDuplicatesString + Environment.NewLine + "--- END OF LOG ---" + Environment.NewLine;
            File.AppendAllText(Path.Combine(LogFilePath + @"\", FileName), content);
            Spinner.Visibility = Visibility.Collapsed;

            if (MessageBox.Show("Do you want to view the log file now? If you click 'No' you can still view it later.", "View log file?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                Process.Start("notepad.exe", LogFilePath + @"\" + FileName);
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