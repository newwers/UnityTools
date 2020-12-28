/********************************************************************
生成日期:	6:17:2020 15:16
类    名: 	InputEvent
作    者:	zdq
描    述:	输入监听事件
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TopGame.UI
{
    public class InputEvent : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Range(0.01f, 2f)]
        public float DragRotateTime = 0.5f;

        public System.Action OnSwapLeft;
        public System.Action OnSwapRight;
        public System.Action OnRotationBegin;
        public System.Action<float> OnRotation;
        public System.Action OnRotationEnd;
        public System.Action OnClick;

        float m_PointDownTime;
        bool m_IsRotation = false;


        void OnNext(PointerEventData eventData)
        {
            Vector3 delta = eventData.pressPosition - eventData.position;
            //Debug.Log("按下时位置:" + eventData.pressPosition + ",现在位置:" + eventData.position + ",delta:" + delta);
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                //Debug.Log("左右滑动");
                if (delta.x > 0)//左下角(0,0)点,右上角(屏幕分辨率)
                {
                    //Debug.Log("右滑动");
                    OnSwapRight?.Invoke();
                }
                else
                {
                    //Debug.Log("左滑动");
                    OnSwapLeft?.Invoke();
                }
            }
            else
            {
                //Debug.Log("上下滑动");
                if (delta.y > 0)//从左下角开始
                {
                    //Debug.Log("下滑动");
                }
                else
                {
                    //Debug.Log("上滑动");
                }
            }
        }

        void OnRotateBegin()
        {
            //Debug.Log("旋转开始");
            m_IsRotation = true;
            OnRotationBegin?.Invoke();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //Debug.Log("拖拽开始,time:" + Time.unscaledTime);
            //这边根据按下时停留的时间判断是滑动操作还是旋转操作
            if (Time.unscaledTime - m_PointDownTime >= DragRotateTime)
            {
                OnRotateBegin();
            }
            else
            {
                OnNext(eventData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            //Debug.Log("拖拽中");
            if (m_IsRotation)
            {
                OnRotation?.Invoke(eventData.delta.x);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //Debug.Log("拖拽结束");
            if (m_IsRotation)
            {
                OnRotationEnd?.Invoke();
            }
            m_IsRotation = false;
            
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging)
            {
                //拖拽的操作就不执行点击事件
                //Debug.Log("拖拽操作覆盖点击");
            }
            else
            {
                //Debug.Log("点击");
                OnClick?.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_PointDownTime = Time.unscaledTime;
            //Debug.Log("按下,time:" + Time.unscaledTime);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //Debug.Log("松开");
        }
    }
}