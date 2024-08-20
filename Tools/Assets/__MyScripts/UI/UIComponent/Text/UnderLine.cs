/********************************************************************
生成日期:	25:7:2019   14:35
类    名: 	User
作    者:	zdq
描    述:   文本下划线
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace zdq.UI
{
    [RequireComponent(typeof(Text)), DisallowMultipleComponent]
    public class UnderLine : BaseMeshEffect
    {
        List<Rect> m_Rects = new List<Rect>();

        private List<UIVertex> stream = null;

        public Color32 lineColor = Color.white;

        public Text text;

        private UIVertex[] underlineUIVertexs = new UIVertex[6];

        /// <summary>
        /// 距离字体间距
        /// </summary>
        public float underLineSpace = 1f;
        /// <summary>
        /// 下划线粗细
        /// </summary>
        public float underLineSize = 1f;

        public LinkText linkText;


        //------------------------------------------------------
        protected override void Awake()
        {
            base.Awake();
            if (text == null)
            {
                text = GetComponent<Text>();
            }
            text.RegisterDirtyMaterialCallback(OnFontMaterialChanged);
        }
        //------------------------------------------------------
#if UNITY_EDITOR
        protected override void OnEnable()
        {
            this.text = this.GetComponent<Text>();
            text.RegisterDirtyMaterialCallback(OnFontMaterialChanged);
        }
#endif
        //------------------------------------------------------
        private void OnFontMaterialChanged()
        {
            // font纹理发生变化时,在font中注册一个字符
            text.font.RequestCharactersInTexture("*", text.fontSize, text.fontStyle);
        }
        //------------------------------------------------------
        public override void ModifyMesh(VertexHelper vh)
        {
            this.stream = new List<UIVertex>();

            vh.GetUIVertexStream(stream);
            vh.Clear();

            Vector2 uv0 = GetUnderlineCharUV();

            UpdateRect();

            foreach (var rect in m_Rects)
            {
                float height = rect.height;
                // 左上
                var pos0 = new Vector3(rect.min.x, rect.min.y,0) - new Vector3(0, underLineSpace, 0);

                // 左下, 向下扩展
                var pos1 = new Vector3(pos0.x, pos0.y - this.underLineSize, 0);

                // 右下.  = 字体矩形 - 矩形高度 - 下划线间隔 - 下划线粗细
                var pos2 = new Vector3(rect.max.x, rect.max.y, 0) - new Vector3(0, height + underLineSpace + underLineSize,0);

                // 右上
                var pos3 = new Vector3(pos2.x, pos0.y, 0);

                // 按照stream存储的规范,构建6个顶点: 左上和右下是2个三角形的重叠, 
                UIVertex vert = UIVertex.simpleVert;
                vert.color = this.lineColor;
                vert.uv0 = uv0;

                vert.position = pos0;
                underlineUIVertexs[0] = vert;
                underlineUIVertexs[3] = vert;

                vert.position = pos1;
                underlineUIVertexs[5] = vert;

                vert.position = pos2;
                underlineUIVertexs[2] = vert;
                underlineUIVertexs[4] = vert;

                vert.position = pos3;
                underlineUIVertexs[1] = vert;

                stream.AddRange(underlineUIVertexs);
            }

            vh.AddUIVertexTriangleStream(stream);
        }
        //------------------------------------------------------
        public void SetLinkText(LinkText link)
        {
            linkText = link;
        }
        //------------------------------------------------------
        public void UpdateRect()
        {
            if (linkText == null)
            {
                return;
            }
            m_Rects.Clear();
            foreach (var item in linkText.GetLinkInfo())
            {
                m_Rects.AddRange(item.boxes);
            }
        }
        //------------------------------------------------------
        /// <summary>
        /// 从font纹理中获取指定字符的uv
        /// </summary>
        /// <returns></returns>
        private Vector2 GetUnderlineCharUV()
        {
            CharacterInfo info;
            if (text.font.GetCharacterInfo('*', out info, text.fontSize, text.fontStyle))
            {
                return (info.uvBottomLeft + info.uvBottomRight + info.uvTopLeft + info.uvTopRight) * 0.25f;
            }
            Debug.LogWarning("GetCharacterInfo failed");
            return Vector2.zero;
        }
    }
}