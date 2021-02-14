using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Custom_Communiations
{
    public class TCP
    {
        private TcpListener Listener_Handle=null;//实例化一个侦听器
        private TcpClient Client_handle = null;//实例化一个公共的TCP句柄
        private NetworkStream Stream=null;//实例化一个数据流类

        public void TCP_Listener_Create(string ip, int port)//创建TCP侦听器
        {
            Listener_Handle = new TcpListener(IPAddress.Parse(ip), port);//绑定本地的IP（多个网卡中的某一个）和尝试连接的远程端口号
            Listener_Handle.Start();
        }
        public int TCP_Listener_Wait(int time_out)
        {
            int i = 0;
            int result = 0;
            bool wait = true;
            while (wait)
            {
                if (Listener_Handle.Pending())//侦听器正在挂起，无客户端接入
                {
                    if (i >= time_out)//等待已超时
                    {
                        wait = false;
                        result = 0;
                    }
                    i++;
                }
                else//有客户端接入
                {
                    Client_handle = Listener_Handle.AcceptTcpClient();
                    Stream = Client_handle.GetStream();
                    wait = false;
                    result = 1;
                }
                Thread.Sleep(1);
            }
            return result;
        }//等待客户端接入
        public void TCP_Connect(string ip_remote, int port_remote, int port_local)//创建客户端连接
        {
            IPEndPoint Clinet_EndPoint = new IPEndPoint(IPAddress.Any, port_local);//指定本地端口号
            Client_handle = new TcpClient(Clinet_EndPoint);//重新实例化客户端
            try
            {
                Client_handle.Connect(ip_remote, port_remote);//绑定远程端口
                Stream = Client_handle.GetStream();
            }
            catch
            {
                throw (new CustomException("Do not find a Server"));
            }
            
        }

        public void TCP_Write(string message)//发送数据
        {
            if (Stream != null)
            {
                Byte[] data_Send = Encoding.Default.GetBytes(message);
                int data_length = data_Send.Length;
                Stream.Write(data_Send, 0, data_length);
            }
        }
        public string TCP_Read(int lenght, int time_out)//接收数据
        {
            string message = "";
            int i = 0;
            bool read = true;
            while (read)
            {
                if (Client_handle != null)
                {
                    //利用Available属性可以使阻塞式IO当做不阻塞使用
                    if (i >= time_out || Client_handle.Available >= lenght)//如果超时 或者 数据长度满足，则将现有的数据读出，停止读取
                    {
                        if (Client_handle.Available > 0)
                        {
                            Byte[] data = new Byte[lenght];
                            Int32 bytes = Stream.Read(data, 0, data.Length);
                            message = Encoding.Default.GetString(data, 0, bytes);
                        }
                        read = false;
                    }
                }
                else
                {
                    message = "";
                    read = false;
                }
                i++;
                Thread.Sleep(1);//延时1毫秒
            }
            return message;
        }

        public void TCP_Close_Listener()//关闭TCP句柄
        {
            if (Listener_Handle != null)
            {
                Listener_Handle.Stop();
            }
        }
        public void TCP_Close_Client()//关闭TCP句柄
        {
            if (Client_handle != null)
            {
                Client_handle.Close();
            }
        }
        public void TCP_Close_Stream()//关闭TCP句柄
        {
            if (Stream != null)
            {
                Stream.Close();
            }
        }

        
    }
    public class UDP
    {
        private static UdpClient UDPHandle = new UdpClient();

        private static IPEndPoint UDPRemote_Read = new IPEndPoint(IPAddress.Any, 0);//要监听的远程目标，这种写法表示监听任意目标
        private static IPEndPoint UDPRemote_Write;//要发送的远程目标，这种写法表示监听任意目标
        public void UDP_Open(string ip, int port)
        {
            IPEndPoint UDPLocal = new IPEndPoint(IPAddress.Parse(ip), port);//绑定本地的IP（多个网卡中的某一个）和设置本地端口号
            UDPHandle = new UdpClient(UDPLocal);

        }
        public string UDP_Read(out string IP)//从任意远程目标监听数据
        {
            string data = "";
            
            if (UDPHandle.Available > 0)//利用Available属性可以使阻塞式IO当做不阻塞使用
            {
                data = Encoding.Default.GetString(UDPHandle.Receive(ref UDPRemote_Read));//将字节数组转化成字符串
                IP = UDPRemote_Read.Address.ToString();
            }
            else
            {
                data = "";
                IP = "";
            }
            return data;
        }
        public void UDP_Write(string data, string ip, int port)//发送数据到指定远程目标
        {
            UDPRemote_Write = new IPEndPoint(IPAddress.Parse(ip), port);//连接远程目标,ip为255.255.255.255时，数据进行广播。
            Byte[] data_Send = Encoding.Default.GetBytes(data);
            int data_length = data_Send.Length;
            UDPHandle.Send(data_Send, data_length, UDPRemote_Write);
        }
        public void Udp_Close()
        {
            UDPHandle.Close();
        }
    }

    public class Enthernet
    {
        public string GetLocalIp()
        {
            ///获取本地的IP地址
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            return AddressIP;
        }
    }


    public class CustomException : ApplicationException
    {
        public CustomException(string message) : base(message)
        {
        }
    }
}
