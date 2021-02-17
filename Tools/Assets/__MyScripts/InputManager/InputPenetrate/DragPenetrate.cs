using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragPenetrate : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GraphicRaycaster raycaster;

    List<RaycastResult> results = new List<RaycastResult>();

    public void OnBeginDrag(PointerEventData eventData)
    {
        results.Clear();
        if (raycaster)
        {
            raycaster.Raycast(eventData, results);
        }
        else
        {
            EventSystem.current.RaycastAll(eventData, results);
        }
        
        //print("results.count:" + results.Count);
        foreach (var item in results)
        {
            if (item.gameObject == gameObject)
            {
                continue;//防止死循环
            }
            ExecuteEvents.Execute<IBeginDragHandler>(item.gameObject, eventData, ExecuteEvents.beginDragHandler);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //为了触发 IBeginDragHandler 而实现的 IDragHandler

        results.Clear();
        if (raycaster)
        {
            raycaster.Raycast(eventData, results);
        }
        else
        {
            EventSystem.current.RaycastAll(eventData, results);
        }
        foreach (var item in results)
        {
            if (item.gameObject == gameObject)
            {
                continue;//防止死循环
            }
            ExecuteEvents.Execute<IDragHandler>(item.gameObject, eventData, ExecuteEvents.dragHandler);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        results.Clear();
        if (raycaster)
        {
            raycaster.Raycast(eventData, results);
        }
        else
        {
            EventSystem.current.RaycastAll(eventData, results);
        }
        //print("end results.count:" + results.Count);
        foreach (var item in results)
        {
            if (item.gameObject == gameObject)
            {
                continue;//防止死循环
            }
            ExecuteEvents.Execute<IEndDragHandler>(item.gameObject, eventData, ExecuteEvents.endDragHandler);
        }
    }
}
