using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手势识别
/// 检测输入方式有两种,一种是通过引用 UnityEngine.EventSystems; 然后实现 IDragHandler,的方式进行获取输入
/// 缺点是这种输入需要在UI上进行,如果UI不是一个整体还有问题,在需要检测的UI上挂载脚本
/// 另一种通过Update函数获取用户鼠标输入
/// Input.GetTouch 可以获取输入 可以看看Touch类的实现
/// unity 左下角0,0点 到右上角
/// </summary>
public class GestureRecognition : MonoBehaviour
{
    private Vector2 m_BeginPoint;
    private float m_Timer;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_BeginPoint = Input.mousePosition;
            m_Timer = Time.time;
        }
        if (Input.GetMouseButtonUp(0))
        {
            GetDirection(Input.mousePosition);
        }
    }

    void GetDirection(Vector2 mousePos)
    {
        float deltaTime = Time.time - m_Timer;//按下时间

        float deltaX = m_BeginPoint.x - mousePos.x;
        float deltaY = m_BeginPoint.y - mousePos.y;

        if (Mathf.Abs(deltaX) > Mathf.Abs(deltaY))//左右滑动
        {
            if (deltaX >0)
            {
                print("从右往左滑");
            }
            else
            {
                print("从左往右滑");
            }
        }
        else//上下滑动,如果距离一样也算上下滑懂
        {
            if (deltaY > 0)
            {
                print("从上往下滑");
            }
            else
            {
                print("从下往上滑");
            }
        }
    }

}
