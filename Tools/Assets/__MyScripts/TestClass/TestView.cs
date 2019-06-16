using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View层只进行按钮消息的通知
/// </summary>
public class TestView : MonoBehaviour {

    public Button Btn;

    // Use this for initialization
    void Start () {
        Btn.onClick.AddListener(() => {
            //发送按钮点击消息
            Notification.Publish("btnTest","隐藏按钮");
        });
    }
	
	
}
