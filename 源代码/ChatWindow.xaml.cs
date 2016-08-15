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
    /// <summary>
    /// ChatWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChatWindow : Window
    {
        string localName;
        public string curDestName;
        ServerConnection serverConnection;
        P2PConnnection p2pConnection;
        GeneralTools generalTools;

        public ChatWindow(string localNameN)
        {
            InitializeComponent();
            localName = localNameN;
            AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)Enter_KeyPress);

            p2pConnection = new P2PConnnection();
            generalTools = new GeneralTools();

            AddFriendList(localName);
        }

        private void Query_Click(object sender, RoutedEventArgs e)
        {
            FriendQuery friendQuery = new FriendQuery(localName);
            friendQuery.Show();
        }

        private void File_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                if (filePath != "" || filePath != null)
                {
                    SendFile(filePath);
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            //查询服务器以获得目的地IP地址
            serverConnection = new ServerConnection();
            string result = serverConnection.ServerQuery("logout" + localName);
            serverConnection.ServerRelease();

            if (result == "loo")
            {
                MessageBox.Show("下线成功");
                this.Close();
            }
            else
            {
                MessageBox.Show("下线失败，请重新尝试");
            }
        }

        private void List_Click(object sender, RoutedEventArgs e)
        {
            Label List_Label = sender as Label;
            string sourceName = List_Label.Name.Substring(1, 10);
            curDestName = sourceName;
            List_Label.Background = new SolidColorBrush(Color.FromArgb(255, 248, 248, 248));
            List_Label.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));

            //查询好友在线状态
            serverConnection = new ServerConnection();
            string destIPStr = serverConnection.ServerQuery("q" + curDestName);
            serverConnection.ServerRelease();
            if (destIPStr == "n")
            {
                List_Label.Name += "（离线）";
            }

            ShowChatMessage();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void Enter_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)     //检测是否按Enter键
            {
                e.Handled = true;
                SendMessage();
            }
        }

        public void SendMessage()
        {
            //查询服务器以获得目的地IP地址
            serverConnection = new ServerConnection();
            string destIPStr = serverConnection.ServerQuery("q" + curDestName);
            serverConnection.ServerRelease();

            if (destIPStr == "n")
            {
                MessageBox.Show("好友不在线");
            }
            else
            {
                //封装需发送的数据
                byte[] infoByte = generalTools.InfoEncode(1, localName);
                byte[] dataByte = System.Text.Encoding.UTF8.GetBytes(Input_TextBox.Text);
                byte[] sendByte = new byte[infoByte.Length + dataByte.Length];
                infoByte.CopyTo(sendByte, 0);
                dataByte.CopyTo(sendByte, infoByte.Length);

                //存储数据
                //若txt不存在，则创建
                string fileName = "./" + localName + ".txt";
                FileStream file = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);//创建写入文件
                file.Close();
                //获取当前时间字符串
                string curTime = DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
                //写入当前数据
                StreamWriter fileWriter = new StreamWriter(fileName, true);
                fileWriter.WriteLine("S");
                fileWriter.WriteLine(curTime);
                fileWriter.WriteLine(localName);
                fileWriter.WriteLine(Input_TextBox.Text);
                fileWriter.Close();

                //发送数据并进行相关界面操作
                p2pConnection.SendP2P(destIPStr, sendByte);
                Input_TextBox.Clear();
                ShowChatMessage();
            }
        }

        private void SendFile(string filePath)
        {
            //查询服务器以获得目的地IP地址
            serverConnection = new ServerConnection();
            string destIPStr = serverConnection.ServerQuery("q" + curDestName);
            serverConnection.ServerRelease();

            if (destIPStr == "n")
            {
                MessageBox.Show("好友不在线");
            }
            else
            {
                //发送基本信息数据
                byte[] infoByte = generalTools.InfoEncode(8, localName);
                byte[] dataByte = System.Text.Encoding.UTF8.GetBytes(filePath);
                byte[] sendByte = new byte[infoByte.Length + dataByte.Length];
                infoByte.CopyTo(sendByte, 0);
                dataByte.CopyTo(sendByte, infoByte.Length);
                p2pConnection.SendP2P(destIPStr, sendByte);

                //分段发送文件
                FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                infoByte = generalTools.InfoEncode(9, localName);
                dataByte = new byte[1024 - 15];
                sendByte = new byte[infoByte.Length + dataByte.Length];
                int len = 0;
                while ((len = fileStream.Read(dataByte, 0, 1024 - 15)) != 0)
                {
                    //按实际的字节总量发送信息
                    infoByte.CopyTo(sendByte, 0);
                    dataByte.CopyTo(sendByte, infoByte.Length);
                    p2pConnection.SendP2P(destIPStr, sendByte);
                }

                //发送结束标识符
                infoByte = generalTools.InfoEncode(9, localName);
                dataByte = System.Text.Encoding.UTF8.GetBytes("END");
                sendByte = new byte[infoByte.Length + dataByte.Length];
                infoByte.CopyTo(sendByte, 0);
                dataByte.CopyTo(sendByte, infoByte.Length);
                p2pConnection.SendP2P(destIPStr, sendByte);

                fileStream.Close();
            }
        }

        public void AddFriendList(string sourceID)
        {
            Avatar_StackPanel.Height += 50;
            Name_StackPanel.Height += 50;
            string imageRoute = "pack://application:,,,/Images/" + sourceID + ".png";

            Image List_Image = new Image
            {
                Name = "_" + sourceID + "_Image",
                Source = new BitmapImage(new Uri(imageRoute)),
                Margin = new Thickness(2.5, 5, 0, 0),
                Height = 20,
                Width = 20,
            };
            Avatar_StackPanel.Children.Add(List_Image);

            Label List_Label = new Label
            {
                Name = "_" + sourceID + "_Label",
                Content = sourceID,
                FontSize = 10,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromArgb(255, 42, 42, 42)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                Height = 25,
                Width = 120,
            };
            List_Label.MouseDown += new MouseButtonEventHandler(List_Click);
            Name_StackPanel.Children.Add(List_Label);
        }

        public void ShowChatMessage()
        {
            string localImageRoute = "pack://application:,,,/Images/" + localName + ".png";
            string sourceImageRoute = "pack://application:,,,/Images/" + curDestName + ".png";

            ChatMessage_StackPanel.Children.Clear();
            LocalAvatar_StackPanel.Children.Clear();
            SourceAvatar_StackPanel.Children.Clear();

            //若txt不存在，则创建
            string fileName = "./" + curDestName + ".txt";
            FileStream file = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write);//创建写入文件
            file.Close();

            //读取txt存储的数据
            string[] historyLineArray = File.ReadAllLines(fileName);
            double lastMessageHeight = 10 - 30;
            double localMarginTop;
            double destMarginTop;
            for (int i = 0; i < historyLineArray.Length; i += 4)
            {
                ChatMessage_StackPanel.Height += 50;
                LocalAvatar_StackPanel.Height += 50;
                SourceAvatar_StackPanel.Height += 50;

                //建立时间标签
                Label Time_Label = new Label
                {
                    Height = 20,
                    Width = 384,
                    Content = historyLineArray[i + 1],
                    FontSize = 5,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)),
                };
                ChatMessage_StackPanel.Children.Add(Time_Label);

                //建立信息内容标签
                TextBlock Message_TextBlock = new TextBlock
                {
                    Height = 20,
                    MaxWidth = 360,
                    Text = " " + historyLineArray[i + 3] + " ",
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
                if (historyLineArray[i] == "S")
                {
                    Message_TextBlock.HorizontalAlignment = HorizontalAlignment.Right;
                    Message_TextBlock.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
                else
                {
                    Message_TextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    Message_TextBlock.Background = new SolidColorBrush(Color.FromArgb(255, 233, 242, 246));
                }
                ChatMessage_StackPanel.Children.Add(Message_TextBlock);

                //建立头像标签并根据情况进行布局处理
                if (historyLineArray[i] == "S")
                {
                    localMarginTop = 20 + lastMessageHeight + 20;
                    Image Avatar_Image = new Image
                    {
                        Source = new BitmapImage(new Uri(localImageRoute)),
                        Margin = new Thickness(0, localMarginTop, 10, 0),
                        Height = 20,
                        Width = 20,
                        Stretch = Stretch.UniformToFill,
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    LocalAvatar_StackPanel.Children.Add(Avatar_Image);
                }
                else
                {
                    destMarginTop = 20 + lastMessageHeight + 20;
                    Image Avatar_Image = new Image
                    {
                        Source = new BitmapImage(new Uri(sourceImageRoute)),
                        Margin = new Thickness(10, destMarginTop, 0, 0),
                        Height = 20,
                        Width = 20,
                        Stretch = Stretch.UniformToFill,
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    SourceAvatar_StackPanel.Children.Add(Avatar_Image);
                }

                //更新状态
                lastMessageHeight = 20;//Message_TextBlock.ActualHeight;
            }
        }
    }
}
