using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


/// <summary>
/// socket客户端
/// TCP
/// 流
/// UTF-8编码格式
/// 
/// </summary>
public class SocketClient  {

    private Socket m_TcpClient;

    /// <summary>
    /// 接收的头指令
    /// </summary>
    private byte[] m_ReceiveData = new byte[6];

    Thread m_thread;

    /// <summary>
    /// 开始客户端连接
    /// </summary>
	public void StartSocketClient()
    {

        try
        {
            //1.创建socket,并指定socket类型
            m_TcpClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //2.发起连接请求
            IPAddress iPAddress = IPAddress.Parse(GetLocalIp());
            //3.指定连接IP和端口
            IPEndPoint endPoint = new IPEndPoint(iPAddress, 45645);
            //4.开始连接
            m_TcpClient.Connect(endPoint);
            Loom.RunAsync(() =>
            {
                m_thread = new Thread(new ThreadStart(ReceiveMessage));
                m_thread.IsBackground = true;
                m_thread.Start();
            });
            
            Debug.Log("客户端开始连接服务器,连接IP:" + endPoint.Address + ",端口:" + endPoint.Port);
        }
        catch (System.Exception e)
        {
            Debug.LogError("客户端连接服务器失败:" + e);
        }

        
    }


    /// <summary>
    /// 向服务端发送数据
    /// </summary>
    /// <param name="messageCommand">要发送的数据</param>
    public void SendMessage(MessageCommand messageCommand)
    {
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
        m_TcpClient.Send(sendMessage);
        Debug.Log("发送模块:" + messageCommand.Module + ",指令:" + messageCommand.Order + ",消息:" + Encoding.UTF8.GetString(messageCommand.Message));
    }


    #region 数据接收与处理



    private void ReceiveMessage()
    {
        while (true)
        {
            if (m_TcpClient == null || m_TcpClient.Connected == false)
            {
                return;
            }

            int length = m_TcpClient.Receive(m_ReceiveData);
            if (length > 0)
            {
                //获取长度
                int size = BitConverter.ToInt32(m_ReceiveData, 2);
                //Debug.Log("接收到的数据长度为:" + size);
                MessageCommand messageCommand = new MessageCommand(m_ReceiveData[0], m_ReceiveData[1], size);
                byte[] messageBytes = new byte[size];
                length = m_TcpClient.Receive(messageBytes);
                if (length > 0)
                {
                    //通过UTF8进行操作
                    messageCommand.Message = messageBytes;
                    //开始对接收到的数据进行处理
                    MessageModelHandle(messageCommand);
                }
            }
            else
            {
                Debug.Log("和服务器断开连接");
            }
            
        }
    }

    /// <summary>
    /// 处理接收到的消息
    /// </summary>
    /// <param name="messageCommand"></param>
    private void MessageModelHandle(MessageCommand messageCommand)
    {
        switch (messageCommand.Module)
        {
            case 0:
                Debug.Log("接收到模块命令为0");
                MessageOrderHandle_0(messageCommand);
                break;
            default:
                //Debug.Log("接收到模块命令为:" + messageCommand.Module);
                Notification.Publish("ClientMessage", messageCommand);//将指令通知出去
                break;
        }
    }

    /// <summary>
    /// 对模块为0的指令命令进行操作处理
    /// </summary>
    /// <param name="messageCommand"></param>
    private void MessageOrderHandle_0(MessageCommand messageCommand)
    {
        //这边只处理模块为0的指令
        switch (messageCommand.Order)
        {
            case 0:
                Debug.Log("接收到指令命令为0");
                //将接收到的内容打印出来
                LogManager.LogColor(Encoding.UTF8.GetString(messageCommand.Message), LogColorEnum.Green);
                break;
            default:
                break;
        }
    }

    #endregion

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
        if (m_TcpClient == null)
        {
            return;
        }
        m_thread.Abort();
        m_TcpClient.Shutdown(SocketShutdown.Both);
        m_TcpClient.Dispose();
        m_TcpClient = null;
    }
}
