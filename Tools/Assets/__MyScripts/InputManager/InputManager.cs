using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 输入模块通过消息通知模块进行消息通知
/// 首先是单个按键检测 -> 抽出来封装成函数 ->函数优化可以多参数 -> 考虑到需要不同按键的时候每次都要修改,所以添加数组进行存储要遍历的数据
/// 默认有wasd和方向键
/// 
/// 信号字段定义 readonly可以通过静态的单例进行访问到,如果是const就不行
/// </summary>
public class InputManager : BaseMonoSingleClass<InputManager>
{
    /// <summary>
    /// 通知键盘按下信号类型
    /// </summary>
    public readonly string InputKeyDown_signal = "Input_KeyDown";
    /// <summary>
    /// 键盘松开
    /// </summary>
    public readonly string InputKeyUp_signal = "Input_KeyUp";
    /// <summary>
    /// 键盘按住时
    /// </summary>
    public readonly string InputKey_signal = "Input_Key";
    /// <summary>
    /// 任意键按下时通知
    /// </summary>
    public readonly string InputAnyKey_signal = "Input_anyKey";

    List<KeyCode> m_keyCode_Down_list = new List<KeyCode>();
    List<KeyCode> m_keyCode_Up_list = new List<KeyCode>();
    List<KeyCode> m_keyCode_list = new List<KeyCode>();

    private void Awake()
    {
        AddKeyCodeDown(KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow);
        AddKeyCodeUp(KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow);
        AddKeyCode(KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow);
    }



    //在update里面写,会受到每个设备帧数的影响,帧数越低,检测频率越少,所以要加上Time.deltaTime进行修正
    // FixedUpdate 帧率固定,并且适合处理物理相关的操作
    //unity推荐把获取输入写在Update里面,因为没帧都会重置按键状态,如果放在FixedUpdate就会导致按下没有反应的情况
    //void FixedUpdate()
    //{
    //    print("FixedUpdate");
    //}
    //
    void Update()
    {
        for (int i = 0; i < m_keyCode_Down_list.Count; i++)
        {
            CheckKeyDown(m_keyCode_Down_list[i]);
        }

        for (int i = 0; i < m_keyCode_Up_list.Count; i++)
        {
            CheckKeyUp(m_keyCode_Down_list[i]);
        }

        for (int i = 0; i < m_keyCode_list.Count; i++)
        {
            CheckKey(m_keyCode_list[i]);
        }



        if (Input.anyKey)
        {
            Notification.Publish("Input_anyKey", null);
        }

    }

    /// <summary>
    /// 检测键盘按下
    /// </summary>
    /// <param name="keyCodes"></param>
    void CheckKeyDown(params KeyCode[] keyCodes)
    {
        foreach (var keyCode in keyCodes)
        {
            if (Input.GetKeyDown(keyCode))
            {
                Notification.Publish(InputKeyDown_signal, keyCode);
            }
        }
        
    }

    /// <summary>
    /// 检测键盘松开
    /// </summary>
    /// <param name="keyCodes"></param>
    void CheckKeyUp(params KeyCode[] keyCodes)
    {
        foreach (var keyCode in keyCodes)
        {
            if (Input.GetKeyUp(keyCode))
            {
                Notification.Publish(InputKeyUp_signal, keyCode);
            }
        }
    }

    /// <summary>
    /// 检测键盘按住
    /// </summary>
    /// <param name="keyCodes"></param>
    void CheckKey(params KeyCode[] keyCodes)
    {
        foreach (var keyCode in keyCodes)
        {
            if (Input.GetKey(keyCode))
            {
                Notification.Publish(InputKey_signal, keyCode);
            }
        }
    }

    /// <summary>
    /// 添加按下按键监听
    /// </summary>
    public void AddKeyCodeDown(params KeyCode[] keyCodes)
    {
        foreach (var keyCode in keyCodes)
        {
            if (m_keyCode_Down_list.Contains(keyCode))
            {
                //重复按键监听
            }
            else
            {
                m_keyCode_Down_list.Add(keyCode);
            }
            
        }
        
    }

    /// <summary>
    /// 移除按键监听
    /// </summary>
    /// <param name="keyCode"></param>
    public void RemoteKeyCodeDown(params KeyCode[] keyCodes)
    {
        foreach (var keyCode in keyCodes)
        {
            if (m_keyCode_Down_list.Contains(keyCode))
            {
                m_keyCode_Down_list.Remove(keyCode);
            }
        }
    }

    /// <summary>
    /// 添加松开按键监听
    /// </summary>
    public void AddKeyCodeUp(params KeyCode[] keyCodes)
    {
        foreach (var keyCode in keyCodes)
        {
            if (m_keyCode_Up_list.Contains(keyCode))
            {
                //重复添加
            }
            else
            {
                m_keyCode_Up_list.Add(keyCode);
            }
        }
    }

    /// <summary>
    /// 移除松开按键监听
    /// </summary>
    /// <param name="keyCode"></param>
    public void RemoteKeyCodeUp(params KeyCode[] keyCodes)
    {
        foreach (var keyCode in keyCodes)
        {
            if (m_keyCode_Up_list.Contains(keyCode))
            {
                m_keyCode_Up_list.Remove(keyCode);
            }
        }
    }

    /// <summary>
    /// 添加按住按键监听
    /// </summary>
    public void AddKeyCode(params KeyCode[] keyCodes)
    {
        foreach (var keyCode in keyCodes)
        {
            if (m_keyCode_list.Contains(keyCode))
            {
                //重复添加
            }
            else
            {
                m_keyCode_list.Add(keyCode);
            }
        }
    }

    /// <summary>
    /// 移除按住按键监听
    /// </summary>
    /// <param name="keyCode"></param>
    public void RemoteKeyCode(params KeyCode[] keyCodes)
    {
        foreach (var keyCode in keyCodes)
        {
            if (m_keyCode_list.Contains(keyCode))
            {
                m_keyCode_list.Remove(keyCode);
            }
        }
    }
}
