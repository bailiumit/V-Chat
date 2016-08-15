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
    /// FriendInvitation.xaml 的交互逻辑
    /// </summary>
    public partial class FriendInvitation : Window
    {
        string localName;
        string destIPStr;
        GeneralTools generalTools;
        P2PConnnection p2pConnection;

        public FriendInvitation(string destIPStrN, string localNameN)
        {
            InitializeComponent();

            localName = localNameN;
            destIPStr = destIPStrN;
            generalTools = new GeneralTools();
            p2pConnection = new P2PConnnection();
        }

        public void InvitationConfirm_Click(object sender, RoutedEventArgs e)
        {
            //封装需发送的数据
            byte[] dataByte = System.Text.Encoding.UTF8.GetBytes(Invitation_TextBox.Text);
            byte[] infoByte = generalTools.InfoEncode(2, localName);
            byte[] sendByte = new byte[infoByte.Length + dataByte.Length];
            infoByte.CopyTo(sendByte, 0);
            dataByte.CopyTo(sendByte, infoByte.Length);

            //发送数据
            p2pConnection.SendP2P(destIPStr, sendByte);

            this.Close();
        }

        public void InvitationCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
