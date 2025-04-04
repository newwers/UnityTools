using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 设置Recttransform按照目标分辨率进行适配的脚本
/// </summary>
public class CanvasScalerAdaptation : MonoBehaviour
{
    public RectTransform rectTransform;
    public Vector2 OriginScreenSize = new Vector2(720, 1280);
    private void Awake()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        float radio =  (OriginScreenSize.x/ OriginScreenSize.y) / (Screen.width / (float)Screen.height);

        rectTransform.sizeDelta = new Vector2(OriginScreenSize.x, OriginScreenSize.y * radio);
    }
}
