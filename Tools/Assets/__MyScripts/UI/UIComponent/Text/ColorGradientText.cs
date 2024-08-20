/********************************************************************
生成日期:	25:7:2019   14:35
类    名: 	User
作    者:	zdq
描    述:   文本颜色渐变
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorGradientText : BaseMeshEffect
{
    public Color color1;
    public Color color2;
    public Color color3;
    public Color color4;

    public bool IsUseWholeGradient = false;

    private List<UIVertex> stream = null;
    //------------------------------------------------------
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }

        stream = new List<UIVertex>(vh.currentVertCount);
        vh.GetUIVertexStream(stream);

        if (IsUseWholeGradient)
        {
            ModifyVertices1(stream);
        }
        else
        {
            ModifyVertices(stream);
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(stream);
    }
    //------------------------------------------------------
    private void ModifyVertices(List<UIVertex> verts)
    {
        //获得到第一个和最后一个的格子,然后设置颜色
        for (int i = 0; i < verts.Count; i += 6)
        {
            setColor(verts, i + 0, color1);//左上
            setColor(verts, i + 1, color2);//右上
            setColor(verts, i + 2, color3);//右下
            setColor(verts, i + 3, color3);//右下

            setColor(verts, i + 4, color4);//左下
            setColor(verts, i + 5, color1);//左上
        }
    }
    //------------------------------------------------------
    private static void setColor(List<UIVertex> verts, int index, Color32 c)
    {
        UIVertex vertex = verts[index];
        vertex.color = c;
        verts[index] = vertex;
    }
    //------------------------------------------------------
    /// <summary>
    /// 只设置一段文字中的四个顶点颜色,而不是每个文字的顶点颜色
    /// </summary>
    /// <param name="verts"></param>
    private void ModifyVertices1(List<UIVertex> verts)
    {
        if (verts.Count == 0)
        {
            return;
        }
        //获得到第一个和最后一个的格子,然后设置颜色
        int textCount = verts.Count / 6;//文字个数
        int curTextCount = 1;//当前第几个文字

        int startLeftTopIndex = 0;
        int startLeftBottomIndex = 4;
        int lastRightTopIndex = 6 * (textCount - 1) + 1;
        int lastRightBottomIndex = lastRightTopIndex + 1;
        int lastRightBottomIndex2 = lastRightBottomIndex + 1;

        int[] specialIndex = new int[] { startLeftTopIndex, startLeftBottomIndex, lastRightTopIndex, lastRightBottomIndex, lastRightBottomIndex2 };

        setColor(verts, startLeftTopIndex, color1);//左上

        setColor(verts, lastRightTopIndex, color2);//右上

        setColor(verts, lastRightBottomIndex, color3);//右下
        setColor(verts, lastRightBottomIndex2, color3);//右下

        setColor(verts, startLeftBottomIndex, color4);//左下

        //每个文字的顶点颜色需要进行插值计算出正确的颜色
        for (int i = 0; i < verts.Count; i += 6)
        {
            
            if (System.Array.IndexOf(specialIndex, (i + 0)) == -1)//不包含特殊顶点index时,才进行颜色设置
            {
                //左上角的颜色 = 最左上角的颜色 和 最右上角的颜色的插值,插值的多少根据当前字数和总字数计算比例.左边还需要-1个字数
                setColor(verts, i + 0, Color.Lerp(color1, color2, (float)(curTextCount - 1) / textCount));//左上
            }
            if (System.Array.IndexOf(specialIndex, (i + 1)) == -1)//不包含特殊顶点index时,才进行颜色设置
            {
                //右上角的颜色 = 最左上角的颜色 和 最右上角的颜色的插值,插值的多少根据当前字数和总字数计算比例
                setColor(verts, i + 1, Color.Lerp(color1, color2, (float)curTextCount / textCount));//右上
            }
            if (System.Array.IndexOf(specialIndex, (i + 2)) == -1)//不包含特殊顶点index时,才进行颜色设置
            {
                setColor(verts, i + 2, Color.Lerp(color3, color4, (float)curTextCount / textCount));//右下
            }
            if (System.Array.IndexOf(specialIndex, (i + 3)) == -1)//不包含特殊顶点index时,才进行颜色设置
            {
                setColor(verts, i + 3, Color.Lerp(color3, color4, (float)curTextCount / textCount));//右下
            }
            if (System.Array.IndexOf(specialIndex, (i + 4)) == -1)//不包含特殊顶点index时,才进行颜色设置
            {
                setColor(verts, i + 4, Color.Lerp(color3, color4, (float)(curTextCount-1) / textCount));//左下
            }
            if (System.Array.IndexOf(specialIndex, (i + 5)) == -1)//不包含特殊顶点index时,才进行颜色设置
            {
                setColor(verts, i + 5, Color.Lerp(color1, color2, (float)(curTextCount - 1) / textCount));//左上
            }

            curTextCount++;
        }
    }
}
