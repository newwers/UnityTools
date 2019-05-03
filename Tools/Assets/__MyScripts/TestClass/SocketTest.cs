using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketTest : MonoBehaviour {

    public byte[] vs;
    public int size = 3655;


    SocketServer m_SocketServer;

    SocketClient m_SocketClient;

    private void Start()
    {
        m_SocketServer = new SocketServer();
        m_SocketServer.StartSocketServer();

        
    }

    [ContextMenu("开启一个客户端测试")]
    private void StartSocketClient()
    {
        m_SocketClient = new SocketClient();
        m_SocketClient.StartSocketClient();
    }

    [ContextMenu("客户端发送指令测试")]
    private void SocketClientSendMessage()
    {
        if (m_SocketClient == null)
        {
            return;
        }
        MessageCommand messageCommand = new MessageCommand(0, 0, "客户端给服务器发消息");
        m_SocketClient.SendMessage(messageCommand);
    }

    [ContextMenu("测试从服务器发送消息")]
    private void TestSendMessageFromServer()
    {
        if (m_SocketServer == null)
        {
            print("服务器为空");
            return;
        }
        MessageCommand messageCommand = new MessageCommand(0, 0, "服务器向客户端问好");
        m_SocketServer.SendMessage(messageCommand,m_SocketServer.Clients[0]);
    }

    private void OnDestroy()
    {
        m_SocketServer.Dispose();
        if (m_SocketClient != null)
        {
            m_SocketClient.Dispose();
        }
    }



    [ContextMenu("测试字节转整型")]
	void Start2 () {
        byte a = 0x12;
        byte b = 0x08;
        byte[] bs = new byte[] { 1,0,0,0};
        var result = BitConverter.ToInt32(vs, 0);
        print(result);

    }
    
    [ContextMenu("测试整型转字节")]
    void Start1()
    {
        int size = 3655;
        var result = BitConverter.GetBytes(size);
        foreach (var item in result)
        {
            print(item);
        }
        

    }

}
