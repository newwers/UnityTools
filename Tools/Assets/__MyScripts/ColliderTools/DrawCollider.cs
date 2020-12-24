using ColliderTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawCollider : MonoBehaviour
{
    public Vector3 size;
    Vector3 m_size;
    AABB colloder;

    Renderer render;
    private void OnEnable()
    {
        Debug.Log("OnEnable");
        {
            render = GetComponent<Renderer>();
            if (render)
            {
                size = render.bounds.size;
            }
            colloder.maxPoint.SetVector3(transform.position + size / 2f);
            colloder.minPoint.SetVector3(transform.position - size / 2f);
            m_size = size;
        }
        
    }


    private void OnDrawGizmos()
    {
        if (m_size != size)
        {
            colloder.maxPoint.SetVector3(transform.position + size / 2f);
            colloder.minPoint.SetVector3(transform.position - size / 2f);
            m_size = size;
        }
        Gizmos.color = Color.green;
        Point p = colloder.maxPoint - colloder.minPoint;
        Gizmos.DrawWireCube(transform.position, p.ConvertVector3());
    }
}
