using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UnderLineText : Text
{

    public float UnderLineSpace = 1f;
    public float UnderLineSize = 0.2f;
    public Color UnderLineColor = Color.yellow;

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        UIVertex vert = new UIVertex();
        var bounds = new Bounds(vert.position, Vector3.zero);
        for (int i = 0; i < toFill.currentVertCount; i++)
        {
            toFill.PopulateUIVertex(ref vert, i);
            Vector3 pos = vert.position;
            if (pos.x < bounds.min.x) // 换行重新添加包围框
            {
                bounds = new Bounds(pos, Vector3.zero);
            }
            else
            {
                bounds.Encapsulate(pos); // 扩展包围框
            }
        }
        //下划线
        toFill.PopulateUIVertex(ref vert, 0);
        float width = bounds.size.x;

        Vector3 lb = vert.position + new Vector3(0,UnderLineSpace,0);
        Vector3 rb = lb + new Vector3(width,0,0);
        Vector3 rt = rb + new Vector3(0,UnderLineSize,0);
        Vector3 lt = lb + new Vector3(0, UnderLineSize, 0);

        int count = toFill.currentVertCount;

        toFill.AddVert(lb,UnderLineColor,Vector2.zero);
        toFill.AddVert(rb, UnderLineColor, Vector2.zero);
        toFill.AddVert(rt, UnderLineColor, Vector2.zero);
        toFill.AddVert(lt, UnderLineColor, Vector2.zero);

        toFill.AddTriangle(count, count + 3, count + 2);
        toFill.AddTriangle(count + 0, count + 2, count + 1);
    }

}

#if UNITY_EDITOR

[CustomEditor(typeof(UnderLineText))]
public class UnderLineTextEditor :Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}

#endif
