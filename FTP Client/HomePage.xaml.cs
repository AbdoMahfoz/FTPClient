using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
            MessageBox.Show("Item Selected");
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
                SubItems.Items.Add(null);

                SubItems.Expanded += Folder_Expanded;

                //adds this to the parrent
                TreeItem.Items.Add(SubItems);
            }
        }

        //Over Ride ya Abdo
        public List<string> GetDirectories(string fullPath)
        {
            List<string> Root = new List<string>();
            Root.Add("Taban Lak Part 2  ");
            Root.Add("Ya 3rbeed Ya Sakeeeeer");
            Root.Add("Sakalatak Omak we 3ametak ");
            return Root;
        }
        //override ya abdo
        public List<string> GetRootDirectory()
        {
            List<string> Root = new List<string>();
            Root.Add("Taban Lak");
            Root.Add("Ya 3rbeed");
            Root.Add("Sakalatak Omak ");
            return Root;
        }

        private void RenameBTN_Click(object sender, RoutedEventArgs e)
        {
            //use filePath string ya abdo
        }

        private void DeleteBTN_Click(object sender, RoutedEventArgs e)
        {
            //use filePath string ya abdo
        }
    }
}
