/********************************************************************
生成日期:	6:3:2020 14:06
类    名: 	MultiSelectBtnBox
作    者:	JaydenHe
描    述:	允许ScrollRect子物体点击拖动
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TopGame.UI
{
    public class DragScrollRectBox : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        public ScrollRect DragScorll;
        public void Awake()
        {
            if (DragScorll == null)
                DragScorll = transform.GetComponentInParent<ScrollRect>();
        }
        //------------------------------------------------------
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (DragScorll != null)
            {
                DragScorll.OnBeginDrag(eventData);
            }
        }
        //------------------------------------------------------
        public void OnDrag(PointerEventData eventData)
        {
            if (DragScorll != null)
            {
                DragScorll.OnDrag(eventData);
            }
        }
        //------------------------------------------------------
        public void OnEndDrag(PointerEventData eventData)
        {
            if (DragScorll != null)
            {
                DragScorll.OnEndDrag(eventData);
            }
        }
        //------------------------------------------------------
    }
}

