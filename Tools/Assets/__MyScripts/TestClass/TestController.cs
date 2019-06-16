using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controller层进行按钮点击的逻辑处理
/// </summary>
public class TestController : MonoBehaviour {

    /// <summary>
    /// 持有的View层的对象引用
    /// </summary>
    public TestView View;
	void Awake () {
        //首先是消息的注册,注册的类型需要一致
        Notification.Subscribe("btnTest", OnBtnClick);
	}
	
	void OnBtnClick(object arg)
    {
        //如果我这边想要隐藏按钮,那么需要去View层调用
        View.Btn.gameObject.SetActive(false);
        //同时将参数打印出来
        if (arg != null)
        {
            print(arg);

        }
    }
}
