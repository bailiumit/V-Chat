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
    class GeneralTools
    {
        public GeneralTools()
        {

        }

        public byte[] InfoEncode(byte type, string name)
        {
            byte[] infoByte = new byte[15];
            string[] IPSpilt = GetLocalIP().ToString().Split('.');

            infoByte[0] = type;
            for (int i = 0; i < 4; i++)
            {
                infoByte[i + 1] = (byte)(int.Parse(IPSpilt[i]));
            }

            for (int i = 5; i < 15; i++)
            {
                infoByte[i] = (byte)name[i - 5];
            }

            return infoByte;
        }

        public string GetLocalIP()
        {
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return IpEntry.AddressList[i].ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取本机IP出错:" + ex.Message);
                return "";
            }
        }
    }
}
