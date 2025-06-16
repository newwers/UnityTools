using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

public class DelayAction : MonoBehaviour
{
    struct ActionData
    {
        public string id;
        public float time;
        public Action action;
        public bool IsUnscaledDeltaTime;
    }

    static DelayAction Instance;
    int m_sID = 0;
    int ID
    {
        get
        {
            m_sID++;
            return m_sID;
        }
    }

    List<ActionData> m_vActions = new List<ActionData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        //print("update Time.unscaledDeltaTime:" + Time.unscaledDeltaTime + ",Time.deltaTime:" + Time.deltaTime);
        for (int i = m_vActions.Count - 1; i >= 0; i--)
        {
            if (i >= m_vActions.Count)
            {
                continue;//有可能一次性删除多个数据情况,需要判断
            }
            //UnityEngine.Debug.Log("index:" + i);
            ActionData data = m_vActions[i];
            if (data.IsUnscaledDeltaTime)
            {
                data.time -= Time.unscaledDeltaTime;//受到游戏后台暂停影响
            }
            else
            {
                data.time -= Time.deltaTime;//暂停后,时间计时也暂停
            }


            if (data.time <= 0)
            {
                m_vActions.RemoveAt(i);
                data.action?.Invoke();
            }
            else
            {
                m_vActions[i] = data;
            }
        }
    }

    private void OnDestroy()
    {
        m_vActions.Clear();
    }

    public static string AddAction(Action aciton, float time, bool isUnscaledDeltaTime = false)
    {
        ActionData data = new ActionData();
        data.id = Instance.ID.ToString();
        data.time = time;
        data.action = aciton;
        data.IsUnscaledDeltaTime = isUnscaledDeltaTime;
        Instance.m_vActions.Add(data);
        return data.id;
    }

    public static void RemoveAction(string id)
    {
        var actions = Instance.m_vActions;
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i].id == id)
            {
                actions.RemoveAt(i);
                break;
            }
        }
    }

    public static bool IsExistAction(string id)
    {
        var actions = Instance.m_vActions;
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i].id == id)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 根据调用类名和函数名,进行判断,只有唯一
    /// </summary>
    /// <param name="action"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string AddUniqueAction(Action action, float time, bool isUnscaledDeltaTime = false)
    {
        // 获取当前方法的名称和类名
        // 获取调用者的类名和方法名
        string callerClassName = GetCallingClassName();
        string callerMethodName = MethodBase.GetCurrentMethod().Name;

        string identifier = callerClassName + "." + callerMethodName;
        //print("AddUniqueAction: identifier:" + identifier);

        // 移除已存在的相同标识符的延迟函数
        RemoveAction(identifier);

        // 添加新的延迟函数
        ActionData data = new ActionData();
        data.id = identifier;
        data.time = time;
        data.action = action;
        data.IsUnscaledDeltaTime = isUnscaledDeltaTime;
        Instance.m_vActions.Add(data);
        return data.id;
    }

    // 新增方法，获取调用者的类名
    private static string GetCallingClassName()
    {
        // 获取当前堆栈跟踪
        StackTrace stackTrace = new StackTrace();
        // 获取堆栈帧数组
        StackFrame[] frames = stackTrace.GetFrames();
        if (frames != null && frames.Length > 2) // 跳过GetCallingClassName和AddUniqueAction方法
        {
            // 获取调用者的堆栈帧
            StackFrame callerFrame = frames[2];
            // 获取调用者的类名
            MethodBase method = callerFrame.GetMethod();
            return method.DeclaringType.FullName;
        }
        return null;
    }

    public static void DelayOneFrame(Action action)
    {
        if (action == null)
        {
            return;
        }
        Instance.StartCoroutine(Instance.DelayOneFrameCoroutine(action));
    }

    IEnumerator DelayOneFrameCoroutine(Action action)
    {
        yield return null;
        action?.Invoke();
    }
}
