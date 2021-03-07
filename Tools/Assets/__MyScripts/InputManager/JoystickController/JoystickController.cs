/*
 摇杆控制器
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace zdq.InputModule
{
    public class JoystickController : MonoBehaviour
    {
        public JoystickEventListener listener;

        public GameObject Icon;

        public ScrollRect OtherScroll;
        public JoystickController OtherJoystick;

        PointerEventData m_EventData;
        public PointerEventData eventData
        {
            get
            {
                return m_EventData;
            }
        }


        public UnityEvent<PointerEventData> OnSliderUpEvent;
        public UnityEvent<PointerEventData> OnSliderDownEvent;
        public UnityEvent<PointerEventData> OnSliderLeftEvent;
        public UnityEvent<PointerEventData> OnSliderRightEvent;

        private void Awake()
        {
            listener.OnBeginDragEvent = OnBeginDrag;
            listener.OnDragEvent = OnDrag;
            listener.OnEndDragEvent = OnEndDrag;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            m_EventData = eventData;

            if (OtherScroll)
            {
                OtherScroll.OnBeginDrag(eventData);
            }
            if (OtherJoystick)
            {
                OtherJoystick.OnBeginDrag(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            m_EventData = eventData;

            if (OtherScroll)
            {
                OtherScroll.OnDrag(eventData);
            }
            if (OtherJoystick)
            {
                OtherJoystick.OnDrag(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            m_EventData = eventData;
            m_EventData = null;

            //计算滑动方向 = 拖拽结束位置 - 开始按下时位置
            Vector3 dir = eventData.position - eventData.pressPosition;
            //通过点乘得到上下滑动
            float dot = Vector3.Dot(Vector3.up, dir.normalized);
            //通过叉乘得到左右滑动
            Vector3 cross = Vector3.Cross(Vector3.up, dir.normalized);
            //Debug.LogError("dot:" + dot + ",angle:" + Vector3.Angle(Vector3.up, dir.normalized) + ",cross:" + cross);

            float angle = Vector3.Angle(Vector3.up, dir.normalized);



            if (dot >= 0 && angle <= 45)
            {
                //需要计算滑动角度决定是上下滑,还是左右滑
                OnSliderUpEvent?.Invoke(eventData);
                print("上滑");
            }
            else if (dot <= 0 && angle >= 135)
            {
                OnSliderDownEvent?.Invoke(eventData);
                print("下滑");
            }

            if (cross.z >= 0 && angle > 45 && angle < 135)
            {
                OnSliderLeftEvent?.Invoke(eventData);
                print("左滑");
            }
            else if(cross.z <= 0 && angle > 45 && angle < 135)
            {
                OnSliderRightEvent?.Invoke(eventData);
                print("右滑");
            }

            if (OtherScroll)
            {
                OtherScroll.OnEndDrag(eventData);
            }
            if (OtherJoystick)
            {
                OtherJoystick.OnEndDrag(eventData);
            }
        }
    }
}
