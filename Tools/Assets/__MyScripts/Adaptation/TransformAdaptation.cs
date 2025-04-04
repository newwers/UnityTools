using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 让物体按照目标分辨率进行缩放适配,需要配置相机的进行使用
/// </summary>
public class TransformAdaptation : MonoBehaviour
{
    public Vector2 OriginScreenSize = new Vector2(720, 1280);
    Vector3 m_InitSize;
    // Start is called before the first frame update
    void Start()
    {
        m_InitSize = transform.localScale;

        float radio = (OriginScreenSize.x / OriginScreenSize.y) / (Screen.width / (float)Screen.height);

        transform.localScale = new Vector3(m_InitSize.x, m_InitSize.y, m_InitSize.z * radio);
    }

}
