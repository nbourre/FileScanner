using FileScanner.Commands;
using FileScanner.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace FileScanner.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string selectedFolder;
        private ObservableCollection<FolderItem> folderItems = new ObservableCollection<FolderItem>();
        private string nbElements;

        public DelegateCommand<string> OpenFolderCommand { get; private set; }
        public DelegateCommand<string> ScanFolderCommand { get; private set; }

        public ObservableCollection<FolderItem> FolderItems
        {
            get => folderItems;
            set
            {
                folderItems = value;
                OnPropertyChanged();
            }
        }

        public string SelectedFolder
        {
            get => selectedFolder;
            set
            {
                selectedFolder = value;
                OnPropertyChanged();
                ScanFolderCommand.RaiseCanExecuteChanged();
            }
        }

        public string NbElements
        {
            get => nbElements;
            set
            {
                nbElements = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            OpenFolderCommand = new DelegateCommand<string>(OpenFolder);
            ScanFolderCommand = new DelegateCommand<string>(ScanFolder, CanExecuteScanFolder);
        }

        private bool CanExecuteScanFolder(string obj)
        {
            return !string.IsNullOrEmpty(SelectedFolder);
        }

        private void OpenFolder(string obj)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    SelectedFolder = fbd.SelectedPath;
                }
            }
        }

        private async void ScanFolder(string dir)
        {
            FolderItems.Clear();

            Stopwatch sw = Stopwatch.StartNew();

            
            var dirTask = Task.Run(() => new ObservableCollection<string>(GetDirFiles(dir)));
            var fileTask = Task.Run(() => new ObservableCollection<string>(GetAllFiles(dir)));

            var results = await Task.WhenAll(dirTask, fileTask);
            
            foreach (var item in results[0])
            {
                FolderItems.Add(new FolderItem { Name = item, Image = "/Images/folder.png" });
            }

            foreach (var item in results[1])
            {
                FolderItems.Add(new FolderItem { Name = item, Image = "/Images/file.png" });
            }

            sw.Stop();

            NbElements = $" ({FolderItems.Count} elements) Execution time : {sw.ElapsedMilliseconds} ms";
            
        }

        List<string> GetDirFiles(string dir)
        {
            try { 
                return Directory.EnumerateDirectories(dir, "*", SearchOption.AllDirectories).ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return new List<string>() ;
            }
        }

        List<string> GetAllFiles(string dir)
        {
            try
            {
                return Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).ToList();
            } catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                return new List<string>();
            }
        }

        ///TODO : Tester avec un dossier avec beaucoup de fichier
        ///TODO : Rendre l'application asynchrone
        ///TODO : Ajouter un try/catch pour les dossiers sans permission


    }
}
