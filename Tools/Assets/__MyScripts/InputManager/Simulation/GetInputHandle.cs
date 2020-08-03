using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 获取玩家输入操作,目前只支持PC,(Android 暂不支持)
/// IPointerClickHandler 的形式,只适合unityUI上面进行的点击操作才有,这样会导致监听的问题,每个UI都挂载脚本吗?不然就用Input进行检测
/// </summary>
public class GetInputHandle : MonoBehaviour
{

    public int Frequency = 60;

    Queue<InputData> m_InputQueue = new Queue<InputData>();

    /// <summary>
    /// 点击开始点
    /// </summary>
    private Vector2 m_BeginClickPoint;
    private POINT m_BeginClick_Point;

    private float m_Timer;

    /// <summary>
    /// 是否处在模拟状态,用来禁用按键监听
    /// </summary>
    private bool m_IsSimulater;

    WaitForSecondsRealtime m_Wait;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && m_IsSimulater == false)
        {
            m_BeginClickPoint = Input.mousePosition;
            POINT point = default;
            MouseSimulater.GetCursorPos(ref point);
            m_BeginClick_Point = point;
            //print("m_BeginClickPoint=" + m_BeginClickPoint);
            m_Timer = Time.time;
        }
        if (Input.GetMouseButtonUp(0) && m_IsSimulater == false)
        {
            POINT point = default;
            MouseSimulater.GetCursorPos(ref point);
            
            m_InputQueue.Enqueue(new InputData(InputType.Click, m_BeginClickPoint, Input.mousePosition, m_BeginClick_Point, point, Time.time - m_Timer));
            print("Time.time - m_Timer=" + (Time.time - m_Timer));
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (m_InputQueue != null && m_InputQueue.Count >0)
            {
                m_IsSimulater = true;
                //StartCoroutine(Simulater(m_InputQueue.Dequeue()));
                StartCoroutine(SimulaterByWindow(m_InputQueue.Dequeue()));
            }
        }
    }

    /// <summary>
    /// 通过unity和window坐标系进行转换后模拟鼠标
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    IEnumerator Simulater(InputData data)
    {
        Vector2 pointStart = MouseSimulater.UnityScreenToWindowPos(data.pointStart_Unity);
        Vector2 pointEnd = MouseSimulater.UnityScreenToWindowPos(data.pointEnd_Unity);
        MouseSimulater.MoveTo(pointStart.x, pointStart.y);
        MouseSimulater.LeftClickDown();
        yield return null;
        m_Wait = new WaitForSecondsRealtime(data.pressTime / Frequency);
        for (int i = 1; i <= Frequency; i++)
        {
            Vector2 point = Vector2.Lerp(pointStart, pointEnd, i / (float)Frequency);
            MouseSimulater.MoveTo(point.x, point.y);
            yield return m_Wait;
        }
        MouseSimulater.LeftClickUp();
        yield return null;
        
        //这边可以开递归去把队列里面全部出队
        if (m_InputQueue.Count >0)
        {
            StartCoroutine(Simulater(m_InputQueue.Dequeue()));
        }
        else
        {
            m_IsSimulater = false;
        }
    }

    /// <summary>
    /// 通过直接用window坐标系进行模拟
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    IEnumerator SimulaterByWindow(InputData data)
    {
        Vector2 pointStart = new Vector2(data.pointStart_Window.X, data.pointStart_Window.Y);
        Vector2 pointEnd = new Vector2(data.pointEnd_Window.X, data.pointEnd_Window.Y);
        MouseSimulater.MoveTo(pointStart.x, pointStart.y);
        MouseSimulater.LeftClickDown();
        yield return null;
        m_Wait = new WaitForSecondsRealtime(data.pressTime / Frequency);
        for (int i = 1; i <= Frequency; i++)
        {
            Vector2 point = Vector2.Lerp(pointStart, pointEnd, i / (float)Frequency);
            MouseSimulater.MoveTo(point.x, point.y);
            yield return m_Wait;
        }
        MouseSimulater.LeftClickUp();
        yield return null;
        //这边可以开递归去把队列里面全部出队
        if (m_InputQueue.Count > 0)
        {
            StartCoroutine(SimulaterByWindow(m_InputQueue.Dequeue()));
        }
        else
        {
            m_IsSimulater = false;
        }
    }


}

public struct InputData
{
    public InputType inputType;
    /// <summary>
    /// 鼠标按下位置
    /// </summary>
    public Vector2 pointStart_Unity;
    /// <summary>
    /// 鼠标松开位置
    /// </summary>
    public Vector2 pointEnd_Unity;

    /// <summary>
    /// 鼠标按下位置(基于1920*1080分辨率)
    /// </summary>
    public POINT pointStart_Window;
    /// <summary>
    /// 鼠标松开位置(基于1920*1080分辨率)
    /// </summary>
    public POINT pointEnd_Window;

    /// <summary>
    /// 按下时间
    /// </summary>
    public float pressTime;


    public InputData(InputType inputType, Vector2 pointStart_Unity, Vector2 pointEnd_Unity, POINT pointStart_Window, POINT pointEnd_Window, float pressTime)
    {
        this.inputType = inputType;
        this.pointStart_Unity = pointStart_Unity;
        this.pointEnd_Unity = pointEnd_Unity;
        this.pointStart_Window = pointStart_Window;
        this.pointEnd_Window = pointEnd_Window;
        this.pressTime = pressTime;
    }

    //todo:考虑到多点触控
    //可以参考 PointerEventData

}

public enum InputType
{
    None,
    Click,
}
