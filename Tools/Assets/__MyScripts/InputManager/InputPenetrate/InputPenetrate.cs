using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InputModule
{
    /// <summary>
    /// 鼠标穿透
    /// </summary>
    public class InputPenetrate : MonoBehaviour, IPointerClickHandler
    {
        public GraphicRaycaster gr;
        List<RaycastResult> m_Results = new List<RaycastResult>();
        public void OnPointerClick(PointerEventData eventData)
        {
            gr.Raycast(eventData, m_Results);
            foreach (var item in m_Results)
            {
                if (name == item.gameObject.name)
                {
                    continue;
                }
                //print(item.gameObject.name);
                UIEventListener.Get(item.gameObject).onPointerClick?.Invoke(item.gameObject);
            }
        }
        
    }
}
