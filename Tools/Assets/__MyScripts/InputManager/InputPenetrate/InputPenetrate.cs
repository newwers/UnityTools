using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InputModule
{
    /// <summary>
    /// 鼠标穿透
    /// 通过 GraphicRaycaster.Raycast 实现的优点:效率高,减少多余的消息传递     缺点:无法解决多个消息传递的需求,
    /// 通过 EventSystem.current.RaycastAll 优点:可以多个 GraphicRaycaster 同时进行检测,   缺点:性能相对较低
    /// </summary>
    public class InputPenetrate : MonoBehaviour, IPointerClickHandler
    {
        //public GraphicRaycaster gr;
        List<RaycastResult> m_Results = new List<RaycastResult>();
        public void OnPointerClick(PointerEventData eventData)
        {
            //gr.Raycast(eventData, m_Results);
            EventSystem.current.RaycastAll(eventData, m_Results);
            foreach (var item in m_Results)
            {
                if (name == item.gameObject.name)
                {
                    continue;
                }
                //print(item.gameObject.name);
                //UIEventListener.Get(item.gameObject).onPointerClick?.Invoke(item.gameObject);
                ExecuteEvents.Execute<IPointerClickHandler>(item.gameObject, eventData, ExecuteEvents.pointerClickHandler);
            }
        }
        
    }
}
