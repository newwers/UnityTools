using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;


/// <summary>
/// 服务器接收到的客户端
/// </summary>
public class ServerReceiveClient  {

    /// <summary>
    /// 连接到服务器的客户端对象
    /// </summary>
    private Socket m_Client;

    /// <summary>
    /// 定义一个接收6字节的头数据
    /// 模块 + 指令 + 长度 = 1 + 1 + 4 = 6; 
    /// </summary>
    private byte[] m_ReceiveData = new byte[6];

    Thread m_thread;


    public ServerReceiveClient(Socket socket)
    {
        m_Client = socket;

        m_thread = new Thread(new ThreadStart(ReceiveClientMessage));
        m_thread.IsBackground = true;
        m_thread.Start();
    }

    /// <summary>
    /// 接收来自客户端的消息
    /// </summary>
    private void ReceiveClientMessage()
    {
        while (true)
        {
            if (m_Client == null || m_Client.Connected == false)
            {
                return;
            }
            
            int length = m_Client.Receive(m_ReceiveData);
            if (length > 0)
            {
                //获取长度
                //foreach (var item in m_ReceiveData)
                //{
                //    print(item);
                //}
                int size = BitConverter.ToInt32(m_ReceiveData, 2);
                Debug.Log("接收到的数据长度为:" + size);
                MessageCommand messageCommand = new MessageCommand(m_ReceiveData[0], m_ReceiveData[1], size);
                byte[] messageBytes = new byte[size];
                length = m_Client.Receive(messageBytes);
                if (length > 0)
                {
                    //通过UTF8进行操作
                    messageCommand.Message = messageBytes;
                    Debug.Log("接收到的数据为:" + Encoding.UTF8.GetString(messageCommand.Message));
                    //开始对接收到的数据进行处理
                    MessageModelHandle(messageCommand);
                }
            }
            else
            {
                if (m_thread != null)
                {
                    m_thread.Abort();
                }
                
                Debug.Log("和客户端断开连接:" + (m_Client.RemoteEndPoint as IPEndPoint).ToString());
            }

        }
        
    }

    /// <summary>
    /// 处理接收到的模块命令
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
                Debug.Log("接收到模块命令为:" + messageCommand.Module);
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

    public void Dispose()
    {
        if (m_thread != null)
        {
            m_thread.Abort();
            m_thread = null;
        }

        if (m_Client != null)
        {
            m_Client.Shutdown(SocketShutdown.Both);
            m_Client.Disconnect(false);
            m_Client.Close();
            m_Client = null;
        }
    }

}
