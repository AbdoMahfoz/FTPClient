using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace FTP_Client
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Window
    {
        public string Filepath { get; set; }
        public HomePage()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //gets the Root (Base) Folder and Fills the first View 
            foreach (var name in GetRootDirectory())
            {
                //Creates New Empty item
                var Dir = new TreeViewItem();

                //Sets the File Name 
                Dir.Header = name;
                //Sets the File Path 
                Dir.Tag = name;

                //adds dummy item  
                if(!name.Contains("."))
                    Dir.Items.Add(null);

                //listen For the Expantion event 
                Dir.Expanded += Folder_Expanded;
                // Sets the mouse Click to select 
                Dir.MouseLeftButtonUp += FolderClicked;

                //adds the item to the tree view 
                FileExplorer.Items.Add(Dir);
            }
        }

        private void FolderClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var TreeViewitem = (TreeViewItem)sender;
            //gets the file path 
            Filepath = (string)TreeViewitem.Tag;
        }

        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            var TreeItem = (TreeViewItem)sender;
            //checks if the tree view item contains any items 
            if (TreeItem.Items.Count != 1 || TreeItem.Items[0] != null)
                return;

            //Cleans The Dummy data
            TreeItem.Items.Clear();

            //gets the folder name
            var FullPath = (string)TreeItem.Tag;

            //gets everything in the folder 
            List<string> FolderList = GetDirectories(FullPath);

            if (FolderList.Count <= 0)
            {
                return;
            }

            foreach (var Folder in FolderList)
            {
                //creates a new item for every folder
                var SubItems = new TreeViewItem();
                //sets the name of the folder
                SubItems.Header = Folder;
                //sets the path of the folder 
                SubItems.Tag = FullPath + "/" + Folder;
                //Dummy item
                if(!Folder.Contains("."))
                    SubItems.Items.Add(null);

                SubItems.Expanded += Folder_Expanded;
                SubItems.MouseLeftButtonUp += FolderClicked;

                //adds this to the parrent
                TreeItem.Items.Add(SubItems);
            }
        }

        
        public List<string> GetDirectories(string fullPath)
        {
            List<string> s = null;
            Task.Run(async () =>
            {
                s = new List<string>(await Gateway.GetDirectoriesAndFiles(fullPath));
            }).Wait();
            return s;
        }
        
        public List<string> GetRootDirectory()
        {
            List<string> s = null;
            Task.Run(async () =>
            {
                s = new List<string>(await Gateway.GetDirectoriesAndFiles(""));
            }).Wait();
            return s;
        }

        private async void RenameBTN_Click(object sender, RoutedEventArgs e)
        {
            await Gateway.Rename(Filepath, "Dummy");
            RefreshBTN_Click(null, null);
        }

        private async void DeleteBTN_Click(object sender, RoutedEventArgs e)
        {
            await Gateway.Delete(Filepath);
            RefreshBTN_Click(null, null);
        }

        private void RefreshBTN_Click(object sender, RoutedEventArgs e)
        {
            FileExplorer.Items.Clear();
            Window_Loaded(null, null);
        }

        private async void NewDirectoryBTN_Click(object sender, RoutedEventArgs e)
        {
            await Gateway.CreateDirectory(Filepath, "Dummy");
            RefreshBTN_Click(null, null);
        }

        private void DownloadBTN_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () => await Gateway.DownloadFile(Filepath)).Wait();
            MessageBox.Show("File Downloaded Successfully");
        }

        private void UploadBTN_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () => await Gateway.UploadFile(Filepath)).Wait();
            MessageBox.Show("File Uploaded Successfully");
            RefreshBTN_Click(null, null);
        }
    }
}
