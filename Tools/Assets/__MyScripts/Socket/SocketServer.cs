using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using System;
using System.Text;

/// <summary>
/// Socket 服务器
/// TCP连接方式
/// 流的传输方式
/// </summary>
public class SocketServer  {

    /// <summary>
    /// 最大客户端连接数
    /// </summary>
    private int m_MaxClientConnectCount = 10;

    /// <summary>
    /// 服务器socket对象
    /// </summary>
    private Socket m_TcpSocket;

    /// <summary>
    /// 存放所有的连接过来的客户端
    /// </summary>
    public List<Socket> Clients = new List<Socket>();


    ServerReceiveClient m_serverReceiveClient;

    /// <summary>
    /// 开启socket服务器
    /// </summary>
	public void StartSocketServer()
    {
        try
        {
            //指定服务器类型
            m_TcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //指定服务器IP和端口
            IPAddress ipAddress = IPAddress.Parse(GetLocalIp());
            IPEndPoint point = new IPEndPoint(ipAddress, 45645);
            m_TcpSocket.Bind(point);
            Debug.Log("服务器绑定IP为:" + point.Address + ",端口:" + point.Port);
            m_TcpSocket.Listen(m_MaxClientConnectCount);
            Debug.Log("服务器开始监听,最大连接数:" + m_MaxClientConnectCount);

            //开始线程,等待客户端连接
            Loom.RunAsync(() => {
                Thread thread = new Thread(new ThreadStart(ListenClientConnect));
                //IsBackfround 的意思是让子线程随着主线程的退出而退出
                thread.IsBackground = true;
                thread.Start();
            });
            
            Debug.Log("<color=#00ff00>服务器开启成功</color>");
        }
        catch (System.Exception e)
        {
            Debug.Log("<color=#ff0000>服务器开启失败,失败原因:</color>" + e.Message);
        }
        
    }

    /// <summary>
    /// 监听客户端连接
    /// </summary>
    private void ListenClientConnect()
    {
        while (true)
        {
            Socket client = m_TcpSocket.Accept();
            m_serverReceiveClient = new ServerReceiveClient(client);
            Clients.Add(client);
            Debug.Log("<color=#00ff00>有客户端连接!</color>");
            IPEndPoint point = client.RemoteEndPoint as IPEndPoint;
            Debug.Log("客户端IP:" + point.Address + ",端口:" + point.Port);
        }
    }

    /// <summary>
    /// 向客户端发送数据
    /// </summary>
    /// <param name="messageCommand">要发送的数据</param>
    /// <param name="socket">发送给哪个客户端</param>
    public void SendMessage(MessageCommand messageCommand,Socket socket)
    {
        //Debug.Log("要发送的数据长度:" + messageCommand.Size);
        byte[] sendMessage = new byte[1 + 1 + 4 + messageCommand.Size];

        sendMessage[0] = messageCommand.Module;
        sendMessage[1] = messageCommand.Order;
        //将int类型转成4个byte类型
        byte[] size = BitConverter.GetBytes(messageCommand.Size);
        //将表示大小的字节复制到头文件中
        Buffer.BlockCopy(size, 0, sendMessage, 2, size.Length);
        //获取要发送的字符串,转化为UTF8格式字节数据
        byte[] message = messageCommand.Message;
        //将内容和头命令合并一起
        Buffer.BlockCopy(message, 0, sendMessage, 6, message.Length);
        socket.Send(sendMessage);
        //Debug.Log("发送模块:" + messageCommand.Module + ",指令:" + messageCommand.Order + ",消息:" + Encoding.UTF8.GetString(messageCommand.Message));
    }

    /// <summary>
    /// 获取本地的IP地址
    /// </summary>
    /// <returns></returns>
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
        Debug.Log("获取本地IP为:" + AddressIP);
        return AddressIP;
    }

    public void Dispose()
    {
        if (m_TcpSocket == null || m_serverReceiveClient == null)
        {
            return;
        }
        //m_TcpSocket.Shutdown(SocketShutdown.Both);

        m_serverReceiveClient.Dispose();
        m_TcpSocket.Close();

        m_serverReceiveClient = null;
        m_TcpSocket = null;
        Debug.Log("关闭服务器socket");

    }
}
