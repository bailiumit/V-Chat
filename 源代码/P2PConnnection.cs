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
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace VChat
{
    class P2PConnnection
    {
        public P2PConnnection()
        {
            
        }

        public void SendP2P(string destIPN, byte[] sendByte)
        {
            IPAddress destIP = IPAddress.Parse(destIPN);
            int destPort = 16000;
            IPEndPoint destEndPoint = new IPEndPoint(destIP, destPort);
            Socket destSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //发送信息
            try
            {
                destSocket.Connect(destEndPoint);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
                return;
            }
            destSocket.Send(sendByte);

            destSocket.Close();
        }
    }
}
