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
using System.IO;

namespace VChat
{
    class MainProcess
    {
        string localName;
        int localPort;
              
        string fileName;
        string fileFormat;

        Byte[] receByte;// = new Byte[80];

        IPAddress localIP;
        IPEndPoint localEndPoint;
        Socket listenSocket, receiveSocket;
        Thread listenThread;

        ChatWindow chatWindow;
        GeneralTools generalTools;

        public MainProcess(string localNameN)
        {
            localName = localNameN;

            chatWindow = new ChatWindow(localName);
            chatWindow.Show();

            generalTools = new GeneralTools();

            localIP = IPAddress.Parse(generalTools.GetLocalIP());
            localPort = 16000;
            localEndPoint = new IPEndPoint(localIP, localPort);

            listenThread = new Thread(Listen);
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        public void Listen()
        {
            //设置侦听套接字
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listenSocket.Bind(localEndPoint);
            }
            catch (SocketException se)
            {
                MessageBox.Show("异常：" + se.Message);
                return;
            }
            listenSocket.Listen(10);

            //接收信息
            while (true)
            {
                receiveSocket = listenSocket.Accept();
                receByte = new Byte[1024];
                int i = receiveSocket.Receive(receByte);
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                                                        new ReceiveP2PDelegate(ReceiveP2P));
            }
        }

        private delegate void ReceiveP2PDelegate();

        private void ReceiveP2P()
        {
            //数据类型
            int infoType = receByte[0];
            //源IP
            string srcIPStr = receByte[1].ToString();
            for (int i = 0; i < 3; i++)
            {
                srcIPStr = srcIPStr + "." + receByte[i + 2].ToString();
            }
            //源ID
            byte[] receIDByte = new byte[10];
            Array.Copy(receByte, 5, receIDByte, 0, 10);
            string sourceID = System.Text.Encoding.UTF8.GetString(receIDByte);
            //数据
            byte[] receDataByte = new byte[receByte.Length - 15];
            Array.Copy(receByte, 15, receDataByte, 0, receByte.Length - 15);
            string dataText = System.Text.Encoding.UTF8.GetString(receDataByte);
            //数据信息
            int end = 0;
            for (; end < dataText.Length - 1 && dataText[end] != 0; end++)
            {
            }
            string dataTextTrim = dataText.Substring(0, end);

            //根据数据类型进行相应的处理
            switch (infoType)
            {
                case 1:
                    //若txt不存在，则创建
                    fileName = "./" + sourceID + ".txt";
                    FileStream file = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);//创建写入文件
                    file.Close();
                    //获取当前时间字符串
                    string curTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
                    //写入当前数据
                    StreamWriter fileWriter = new StreamWriter(fileName, true);
                    fileWriter.WriteLine("R");
                    fileWriter.WriteLine(curTime);
                    fileWriter.WriteLine(sourceID);
                    fileWriter.WriteLine(dataTextTrim);
                    fileWriter.Close();
                    //判断是否显示数据
                    if (sourceID == chatWindow.curDestName)
                    {
                        chatWindow.ShowChatMessage();
                    }
                    break;
                case 2:
                    P2PConnnection p2pConnection = new P2PConnnection();
                    if (MessageBox.Show(sourceID + "想添加您为好友：\n  " + dataTextTrim,
                        "好友申请", MessageBoxButton.YesNo, MessageBoxImage.None) == MessageBoxResult.Yes)
                    {
                        chatWindow.AddFriendList(sourceID);
                        byte[] infoByte = generalTools.InfoEncode(3, localName);
                        p2pConnection.SendP2P(srcIPStr, infoByte);
                    }
                    else
                    {
                        byte[] infoByte = generalTools.InfoEncode(4, localName);
                        p2pConnection.SendP2P(srcIPStr, infoByte);
                    }
                    break;
                case 3:
                    chatWindow.AddFriendList(sourceID);
                    break;
                case 4:
                    MessageBox.Show(sourceID + "拒绝了您的好友请求");
                    break;
                case 8:
                    if (MessageBox.Show(sourceID + "想向您传输文件", "文件发送",
                        MessageBoxButton.YesNo, MessageBoxImage.None) == MessageBoxResult.Yes)
                    {
                        System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                        saveFileDialog.Title = "文件保存在";
                        saveFileDialog.Filter = "文件(*.*)|*.*";
                        saveFileDialog.InitialDirectory = @"C:\";
                        saveFileDialog.FileName = "save";
                        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            fileName = saveFileDialog.FileName;
                            fileFormat = dataTextTrim.Substring(dataTextTrim.LastIndexOf('.'));
                        }
                    }
                    break;
                case 9:
                    StreamWriter fileWriterN = new StreamWriter(fileName + fileFormat, true);
                    if (dataTextTrim != "END")
                    {
                        fileWriterN.Write(dataTextTrim);
                    }
                    fileWriterN.Close();
                    break;
                default:
                    MessageBox.Show("数据类型错误");
                    break;
            }
        }
    }
}
