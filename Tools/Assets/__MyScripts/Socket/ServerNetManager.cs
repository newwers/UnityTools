using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerNetManager : MonoBehaviour
{
    public static ServerNetManager Instance;

    SocketServer socketServer;

    private void Awake()
    {
        Instance = this;
        Notification.Subscribe("ServerMessage", ServerMessage);
        socketServer = new SocketServer();
        socketServer.StartSocketServer();//开启socket服务器
    }

    /// <summary>
    /// 服务器向客户端发送图片位置
    /// </summary>
    public void SendDragPos(Vector2 pos)
    {
        MessageCommand message = new MessageCommand(2,1,8);
        message.WriteFloat(pos.x);
        message.WriteFloat(pos.y);
        if (socketServer.Clients.Count > 0)
        {
            socketServer.SendMessage(message, socketServer.Clients[0]);
        }
    }


    /// <summary>
    /// 服务端接收到的消息
    /// 模块1服务器消息
    /// </summary>
    /// <param name="obj"></param>
    private void ServerMessage(object obj)
    {
        MessageCommand message = obj as MessageCommand;
        switch (message.Module)
        {
            case 1:
                MessageModule_1_Handle(message);
                break;

        }
    }

    /// <summary>
    /// 指令1处理接收
    /// </summary>
    /// <param name="message"></param>
    private void MessageModule_1_Handle(MessageCommand message)
    {
        switch (message.Order)
        {
            case 1:
                float offsetX = message.GetFloat();
                float offsetY = message.GetFloat();

                Notification.Publish("UpdatePos", new MovePicStruct(offsetX, offsetY));
                break;

            default:
                break;
        }
    }

    
}
