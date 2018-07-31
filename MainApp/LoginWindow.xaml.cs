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
using System.Windows.Shapes;
using Tencent.DataSources;

namespace Tencent {
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window {
        public LoginWindow() {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {
            this.btn_Login.IsEnabled = false;
            FTSServerSource FTSServer = this.FindResource("FTSServerSource") as FTSServerSource;

            var ip = this.txt_IP.Text;
            var port = this.txt_Port.Text;
            var account = this.txt_Account.Text;
            var password = this.txt_Password.Text;

            try {
                await FTSServer.Login(ip, port, account, password);
            } catch(Exception) {
                this.btn_Login.IsEnabled = true;
                return;
            }

            var main = new MainWindow();
            main.Show();
            this.Close();
        }
    }
}
