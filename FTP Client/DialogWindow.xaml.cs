using System.Windows;

namespace FTP_Client
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        private void SaveBTN_Click(object sender, RoutedEventArgs e)
        {
            HomePage.PromptData = DataTXT.Text;
            this.Close();
        }
    }
}
