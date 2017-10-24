using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    class Program
    {

        static byte[] buffer = new byte[1024];
        private static byte[] result = new byte[1024];
        private static Socket socket;
        private static List<Socket> clientSockets = new List<Socket>();
        static void Main(string[] args)
        {
            //创建一个新的Socket,这里我们使用最常用的基于TCP的Stream Socket（流式套接字）
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //将socket 绑定到主机的某一个端口上
            socket.Bind(new IPEndPoint(IPAddress.Any, 4530));
            //启动监听并设置队列的最大链接数
            socket.Listen(4);
            ////开始接受客户端链接请求
            //socket.BeginAccept(new AsyncCallback((ar) =>
            //{
            //    var client = socket.EndAccept(ar);
            //    client.Send(Encoding.Unicode.GetBytes("hi there , I accept you request at " + DateTime.Now.ToString()));
            //    ////通过Clientsoket发送数据
            //    //Thread myThread = new Thread(ListClientConnent);
            //    //myThread.Start();
            //}), null);
            //-----------------------------------------接收客户端事件-----------------------------------------------
            Socket netsocket = socket.Accept();//当有可用的客户端连接尝试时执行，并返回一个新的socket,用于与客户端之间的通信
            IPEndPoint clientip = (IPEndPoint)netsocket.RemoteEndPoint;//获取请求的ip链接
            Console.WriteLine("connect with client:" + clientip.Address + " at port:" + clientip.Port);
            string welcome = "welcome here!";
            buffer = Encoding.Unicode.GetBytes(welcome);
            netsocket.Send(buffer, buffer.Length, SocketFlags.None);//发送信息
            while (true)
            {
                //用死循环来不断的从客户端获取信息
                //buffer = new byte[1024];
                int recv = netsocket.Receive(buffer);
                //if (recv == 0)//当信息长度为0，说明客户端连接断开
                //    break;
                Console.WriteLine("recv=" + recv);
                Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, recv));
                netsocket.Send(buffer, recv, SocketFlags.None);
            }
            Console.WriteLine("Disconnected from" + clientip.Address);
            Console.WriteLine("Server is ready!");
            Console.Read();
        }
        /// <summary>
        /// 监听客户端的消息
        /// </summary>
        /// <param name="serverSocket"></param>
        private static void ListClientConnent()
        {
            Console.WriteLine("启动监听{0}成功", socket.LocalEndPoint.ToString());
            while (true)
            {
                Socket clientSocket = socket.Accept();
                clientSockets.Add(clientSocket);
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);
            }
        }
        /// <summary>
        /// 接受客户端消息输出
        /// </summary>
        /// <param name="clientSocket"></param>
        private static void ReceiveMessage(object clientSocket)
        {
            Socket myclient = clientSocket as Socket;
            while (true)
            {
                try
                {
                    int receiveNum = myclient.Receive(result);
                    string stringContent = Encoding.ASCII.GetString(result, 0, receiveNum);
                    Console.WriteLine("接收客户端{0}消息{1}", myclient.RemoteEndPoint.ToString(), Encoding.ASCII.GetString(result, 0, receiveNum));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    myclient.Shutdown(SocketShutdown.Both);
                    myclient.Close();
                    break;
                }
            }

        }
        /// <summary>
        /// 发送message
        /// </summary>
        /// <param name="message"></param>
        public static void SendMessage(string message)
        {
            foreach (Socket clientSocket in clientSockets)
                clientSocket.Send(Encoding.ASCII.GetBytes(message));
        }

    }
}
