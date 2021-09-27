using System;
using System.Collections.Generic;
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
        private string _logFilePath = "";
        private string _fileName = "duplicates.txt";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectFolder_Click(object sender, EventArgs e)
        {
            VistaFolderBrowserDialog browseForLogDirectory = new VistaFolderBrowserDialog();
            bool pathIsSelected;

            do
            {
                if (browseForLogDirectory.ShowDialog() == false)
                {
                    return;
                }

                if (browseForLogDirectory.SelectedPath == "")
                {
                    pathIsSelected = false;
                    MessageBox.Show("You must select a folder.", "Error");
                }
                else
                {
                    pathIsSelected = true;
                }
            } while (pathIsSelected == false);

            _logFilePath = browseForLogDirectory.SelectedPath;

            VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog();

            do
            {
                if (folderBrowser.ShowDialog() == false)
                {
                    return;
                }

                if (folderBrowser.SelectedPath == "")
                {
                    pathIsSelected = false;
                    MessageBox.Show("You must select a folder.", "Error");
                }
                else
                {
                    pathIsSelected = true;
                }
            } while (pathIsSelected == false);

            
            string heading = "Possible duplicates in " + folderBrowser.SelectedPath + Environment.NewLine;
            string possibleDuplicatesString = "";
            List<string> fileList = Directory.EnumerateFiles(folderBrowser.SelectedPath).ToList();
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
            MessageBox.Show("When you click \"Select Folder\" File Explorer will appear — select the folder where you want to store the log file. The log file will contain the possible duplicate files that are found." + Environment.NewLine + Environment.NewLine + "The second File Explorer window will appear after this — select the folder you want to search duplicates for.", "Help");
        }
    }
}