using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// View层只进行按钮消息的通知
/// </summary>
public class TestView : BaseUIView {

    public Button Btn;

    // Use this for initialization
    void Start () {
        Btn = GetComponentByName<Button>("Btn");
        Btn.onClick.AddListener(() => {
            //发送按钮点击消息
            print("按钮点击");
        });
    }
	
	
}
