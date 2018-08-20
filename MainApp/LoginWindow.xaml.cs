using Infralution.Localization.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            /// make default
            CultureManager.UICulture = new System.Globalization.CultureInfo("en");
        }

        private async void Button_Click(object sender, RoutedEventArgs e) {
            this.btn_Login.IsEnabled = false;
            FTSServerSource FTSServer = this.FindResource("FTSServerSource") as FTSServerSource;
            FaceListenerSource FaceListener = this.FindResource("FaceListenerSource") as FaceListenerSource;


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

            /// Init config map back to old one.
            FaceListener.InitConfig(FTSServer);
            /// And then start FRS server.
            FaceListener.StartServer();

            var main = new MainWindow();
            main.Show();
            this.Close();
        }

        #region "Dependency Properties"


        public string Version {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Version.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(string), typeof(LoginWindow), new PropertyMetadata("v11"));


        #endregion "Dependency Properties"

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            CultureManager.UICulture = new System.Globalization.CultureInfo((this.LanguageBox.SelectedItem as FrameworkElement).Tag as string);
        }
    }
}
