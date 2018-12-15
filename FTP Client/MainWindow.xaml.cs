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
                UserTXT.Text = "Enter UserName";
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
    }
}
