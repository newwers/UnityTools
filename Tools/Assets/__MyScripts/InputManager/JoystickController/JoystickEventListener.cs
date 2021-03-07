/*
 摇杆事件监听
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace zdq.InputModule
{
    public class JoystickEventListener : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        public GameObject ScrollGameObject;

        public Action<PointerEventData> OnBeginDragEvent;
        public Action<PointerEventData> OnDragEvent;
        public Action<PointerEventData> OnEndDragEvent;

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragEvent?.Invoke(eventData);
            ExecuteEvents.Execute<IBeginDragHandler>(ScrollGameObject, eventData, ExecuteEvents.beginDragHandler);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragEvent?.Invoke(eventData);
            ExecuteEvents.Execute<IDragHandler>(ScrollGameObject, eventData, ExecuteEvents.dragHandler);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragEvent?.Invoke(eventData);
            ExecuteEvents.Execute<IEndDragHandler>(ScrollGameObject, eventData, ExecuteEvents.endDragHandler);
        }
    }
}
