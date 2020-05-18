using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 只处理客户端的消息
/// </summary>
public class ClientNetManager : MonoBehaviour
{
    public static ClientNetManager Instance;

    SocketClient socketClient;

    private void Awake()
    {
        Instance = this;

        Notification.Subscribe("ClientMessage", ClientMessage);
        socketClient = new SocketClient();
        socketClient.StartSocketClient();
    }

    public void SendDragPos(Vector2 pos)
    {
        MessageCommand message = new MessageCommand(1, 1, 8);
        message.WriteFloat(pos.x);
        message.WriteFloat(pos.y);
        socketClient.SendMessage(message);
    }


    /// <summary>
    /// 客户端接收到的消息
    /// 模块2,属于客户端
    /// 
    /// </summary>
    /// <param name="obj"></param>
    private void ClientMessage(object obj)
    {
        MessageCommand message = obj as MessageCommand;
        switch (message.Module)
        {
            case 2:
                MessageModule_2_Handle(message);
                break;

        }
    }

    /// <summary>
    /// 模块2消息处理
    /// 指令1处理接收
    /// </summary>
    /// <param name="message"></param>
    private void MessageModule_2_Handle(MessageCommand message)
    {
        switch (message.Order)
        {
            case 1:
                float offsetX = message.GetFloat();
                float offsetY = message.GetFloat();
                //LogManager.Log("offsetX=" + offsetX + "offsetY=" + offsetY);


                Notification.Publish("UpdatePos", new MovePicStruct(offsetX, offsetY));
                break;

            default:
                break;
        }
    }
}
