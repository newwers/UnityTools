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
        for (int i = m_vActions.Count-1; i >=0; i--)
        {
            ActionData data = m_vActions[i];
            data.time -= Time.unscaledDeltaTime;
            if (data.time <= 0)
            {
                data.action?.Invoke();
                m_vActions.RemoveAt(i);
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

    public static string AddAction(Action aciton,float time)
    {
        ActionData data = new ActionData();
        data.id = Instance.ID.ToString();
        data.time = time;
        data.action = aciton;
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
    /// ���ݵ��������ͺ�����,�����ж�,ֻ��Ψһ
    /// </summary>
    /// <param name="action"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string AddUniqueAction(Action action, float time)
    {
        // ��ȡ��ǰ���������ƺ�����
        // ��ȡ�����ߵ������ͷ�����
        string callerClassName = GetCallingClassName();
        string callerMethodName = MethodBase.GetCurrentMethod().Name;

        string identifier = callerClassName + "." + callerMethodName;
        print("AddUniqueAction: identifier:" + identifier);

        // �Ƴ��Ѵ��ڵ���ͬ��ʶ�����ӳٺ���
        RemoveAction(identifier);

        // ����µ��ӳٺ���
        ActionData data = new ActionData();
        data.id = identifier;
        data.time = time;
        data.action = action;
        Instance.m_vActions.Add(data);
        return data.id;
    }

    // ������������ȡ�����ߵ�����
    private static string GetCallingClassName()
    {
        // ��ȡ��ǰ��ջ����
        StackTrace stackTrace = new StackTrace();
        // ��ȡ��ջ֡����
        StackFrame[] frames = stackTrace.GetFrames();
        if (frames != null && frames.Length > 2) // ����GetCallingClassName��AddUniqueAction����
        {
            // ��ȡ�����ߵĶ�ջ֡
            StackFrame callerFrame = frames[2];
            // ��ȡ�����ߵ�����
            MethodBase method = callerFrame.GetMethod();
            return method.DeclaringType.FullName;
        }
        return null;
    }
}
