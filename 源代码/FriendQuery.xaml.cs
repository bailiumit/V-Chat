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

namespace VChat
{
    /// <summary>
    /// InvitationSend.xaml 的交互逻辑
    /// </summary>
    public partial class FriendQuery : Window
    {
        string localName;

        public FriendQuery(string localNameN)
        {
            InitializeComponent();

            localName = localNameN;
        }

        private void QueryConfirm_Click(object sender, RoutedEventArgs e)
        {
            //查询服务器
            ServerConnection serverConnection = new ServerConnection();
            string resultStr = serverConnection.ServerQuery("q" + IP_TextBox.Text);

            if (resultStr == "n")
            {
                MessageBox.Show("好友不在线");
            }
            else
            {
                FriendInvitation friendInvitation = new FriendInvitation(resultStr, localName);
                friendInvitation.Show();
            }

            this.Close();
        }

        private void QueryCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
