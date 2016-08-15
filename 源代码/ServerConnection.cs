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
    class ServerConnection
    {
        IPAddress serverIP;
        IPEndPoint serverEndPoint;
        Socket serverSocket;

        public ServerConnection()
        {
            serverIP = IPAddress.Parse("166.111.140.14");
            serverEndPoint = new IPEndPoint(serverIP, 8000);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public string ServerQuery(string queryStr)
        {
            byte[] arrLogInStr = System.Text.Encoding.UTF8.GetBytes(queryStr);
            byte[] arrResultStr = new byte[1024 * 1024 * 2];
            int length = -1;

            //发送登录信息
            try
            {
                serverSocket.Connect(serverEndPoint);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
                return "0";
            }
            serverSocket.Send(arrLogInStr);

            //接收服务器反馈的信息
            try
            {
                length = serverSocket.Receive(arrResultStr);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
                return "0";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return "0";
            }

            //查询结果字符串
            string resultStr = System.Text.Encoding.UTF8.GetString(arrResultStr, 0, length);
            return resultStr;
        }

        public void ServerRelease()
        {
            serverSocket.Close();
        }
    }
}
