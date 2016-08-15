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

namespace VChat
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string localName;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            localName = UserName_TextBox.Text;
            string logInStr = UserName_TextBox.Text + "_" + Password_PasswordBox.Password;
            LogInProcess(logInStr);
        }

        private void LogInProcess(string logInStr)
        {
            //查询服务器
            ServerConnection serverConnection = new ServerConnection();
            string resultStr = serverConnection.ServerQuery(logInStr);

            //根据查询结果进行操作
            if (resultStr == "lol")    //登陆成功
            {
                MainProcess mainProcess = new MainProcess(localName);
                this.Close();
            }
            else if (resultStr != "lol")   //登录失败
            {
                MessageBox.Show(" 用户名或密码错误");
            }
            //sockClient.Close();
        }
    }
}
