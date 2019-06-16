using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 消息通知测试类
/// 通过按钮点击事件进行演示
/// 按钮点击的时候只进行点击事件的通知
/// 具体按钮点击后,执行的逻辑操作,由接收者进行处理,发送者不管
/// </summary>
public class NotificationTest : MonoBehaviour {

    public Button Btn;
    public Button Btn2;
    private void Awake()
    {
        //消息的注册,注册都是比发送消息的先,通常在一开始就进行注册,在结束时进行销毁
        Notification.Subscribe("btnTest", BtnClick);
        Notification.Subscribe("btnTest2", BtnClick2);
    }

    void Start () {
        Btn.onClick.AddListener(()=> {
            //发送按钮点击消息
            Notification.Publish("btnTest");
        });
        Btn2.onClick.AddListener(() => {
            //发送按钮点击消息
            Notification.Publish("btnTest2",":zdq");
        });
    }

    private void BtnClick(object arg )
    {
        print("hello world");
    }
    private void BtnClick2(object arg)
    {
        print("hello world" + arg);
    }

}
