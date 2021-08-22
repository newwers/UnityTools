#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ListenerResolutionChange : MonoBehaviour
{
    public CanvasScaler m_CanvasScaler;
    public Camera[] m_Cameras;

    //------------------------------------------------------
    private void OnEnable()
    {
        if (m_CanvasScaler == null)
        {
            m_CanvasScaler = GetComponent<CanvasScaler>();
        }
        if (m_Cameras == null)
        {
            m_Cameras = FindObjectsOfType(typeof(Camera)) as Camera[];
        }
    }
    //------------------------------------------------------
    private void OnGUI()
    {
        if (Screen.width > Screen.height)
        {
            m_CanvasScaler.referenceResolution = new Vector2(1334, 750);
        }
        else
        {
            m_CanvasScaler.referenceResolution = new Vector2(720, 1280);
        }
    }
}
#endif