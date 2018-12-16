using System;
using System.Windows;

namespace FTP_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UserTXT.GotFocus += RemoveText;
            UserTXT.LostFocus += AddText;

            PassTXT.GotFocus += RemovepText;
            PassTXT.LostFocus += AddpText;
        }
        public void RemoveText(object sender, EventArgs e)
        {
            UserTXT.Text = "";
        }

        public void AddText(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserTXT.Text))
                UserTXT.Text = "Enter Username";
        }
        public void RemovepText(object sender, EventArgs e)
        {
            PassTXT.Password = "";
        }

        public void AddpText(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PassTXT.Password))
                PassTXT.Password = "Enter Password";
        }

        private void LoginBTN_Click(object sender, RoutedEventArgs e)
        {
            //Login(UserTXT.Text, PassTXT.Password)                                             OVER RIDE ya abdoo
            if (true)
            {
                HomePage x = new HomePage();
                x.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("SAkltak omak ya 7omar");      //  OVER RIDE YA ABDOOOOOOOOOO
            }
        }

        private bool Login(string text, string password)
        {
            throw new NotImplementedException();
        }
    }
}
