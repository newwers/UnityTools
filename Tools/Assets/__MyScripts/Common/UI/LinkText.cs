/********************************************************************
生成日期:	1:29:2021 10:06
类    名: 	LinkText
作    者:	zdq
描    述:	支持文本链接
*********************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace zdq.UI
{
    public class LinkText : Text, IPointerClickHandler
    {
        /// <summary>
        /// 超链接信息类
        /// </summary>
        public class HyperlinkInfo
        {
            public int startIndex;

            public int endIndex;

            public string name;

            public readonly List<Rect> boxes = new List<Rect>();
        }

        /// <summary>
        /// 解析完最终的文本
        /// </summary>
        private string m_OutputText;

        /// <summary>
        /// 超链接信息列表
        /// </summary>
        private readonly List<HyperlinkInfo> m_HrefInfos = new List<HyperlinkInfo>();

        /// <summary>
        /// 文本构造器
        /// </summary>
        protected static readonly StringBuilder s_TextBuilder = new StringBuilder();

        [Serializable]
        public class HrefClickEvent : UnityEvent<string> { }

        [SerializeField]
        private HrefClickEvent m_OnHrefClick = new HrefClickEvent();

        /// <summary>
        /// 超链接点击事件
        /// </summary>
        public HrefClickEvent onHrefClick
        {
            get { return m_OnHrefClick; }
            set { m_OnHrefClick = value; }
        }

        private string m_LinkColor = "#FFFF00";

        /// <summary>
        /// 超链接正则
        /// <a href=111>xxx</a>
        /// </summary>
        private static readonly Regex s_HrefRegex = new Regex(@"<a href=([^>\n\s]+)>(.*?)(</a>)", RegexOptions.Singleline);

        //------------------------------------------------------
        public override void SetVerticesDirty()
        {
            base.SetVerticesDirty();
#if UNITY_EDITOR
            if (UnityEditor.PrefabUtility.GetPrefabAssetType(this) == UnityEditor.PrefabAssetType.Regular)
            //if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)//为啥预制体就return?
            {
                return;
            }
#endif
            m_OutputText = GetOutputText(text);//计算显示的文本
        }
        //------------------------------------------------------
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            var orignText = m_Text;
            m_Text = m_OutputText;
            base.OnPopulateMesh(toFill);//这边base中会对文本进行某些操作,所以需要先赋值显示文本,再设置m_Text为初始文本
            m_Text = orignText;
            UIVertex vert = new UIVertex();

            //处理多行问题
            var lines = cachedTextGenerator.GetLinesArray().Length;
            //Debug.Log("OnPopulateMesh line:" + lines);
            CalcHrefInfo(orignText,lines);


            // 处理超链接包围框
            foreach (var hrefInfo in m_HrefInfos)
            {
                hrefInfo.boxes.Clear();
                //Debug.Log("startIndex:" + hrefInfo.startIndex + ",currentVertCount:" + toFill.currentVertCount);
                if (hrefInfo.startIndex >= toFill.currentVertCount)
                {
                    continue;
                }

                // 将超链接里面的文本顶点索引坐标加入到包围框

                toFill.PopulateUIVertex(ref vert, hrefInfo.startIndex);
                var pos = vert.position;


                var bounds = new Bounds(pos, Vector3.zero);
                for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i < m; i++)
                {
                    if (i >= toFill.currentVertCount)
                    {
                        break;
                    }

                    toFill.PopulateUIVertex(ref vert, i);
                    pos = vert.position;
                    if (pos.x < bounds.min.x) // 换行重新添加包围框
                    {
                        hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                        bounds = new Bounds(pos, Vector3.zero);
                    }
                    else
                    {
                        bounds.Encapsulate(pos); // 扩展包围框
                    }
                }
                hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
            }
        }
        //------------------------------------------------------
        /// <summary>
        /// 获取超链接解析后的最后输出文本
        /// </summary>
        /// <returns></returns>
        protected virtual string GetOutputText(string outputText)
        {
            s_TextBuilder.Length = 0;
            var indexText = 0;
            foreach (Match match in s_HrefRegex.Matches(outputText))
            {
                s_TextBuilder.Append(outputText.Substring(indexText, match.Index - indexText));

                s_TextBuilder.Append("<color=");  // 超链接颜色
                s_TextBuilder.Append(m_LinkColor);
                s_TextBuilder.Append(">");

                s_TextBuilder.Append(match.Groups[2].Value);
                s_TextBuilder.Append("</color>");
                indexText = match.Index + match.Length;
            }
            s_TextBuilder.Append(outputText.Substring(indexText, outputText.Length - indexText));
            return s_TextBuilder.ToString();
        }
        //------------------------------------------------------
        string CalcHrefInfo(string originText,int lineCount)
        {
            s_TextBuilder.Length = 0;
            var indexText = 0;
            m_HrefInfos.Clear();

            foreach (Match match in s_HrefRegex.Matches(originText))
            {
                s_TextBuilder.Append(originText.Substring(indexText, match.Index - indexText));
                //Debug.Log("CalcHrefInfo line:" + lineCount);

                if (lineCount > 1)
                {
                    s_TextBuilder.Append("<color=");  // 超链接颜色
                    s_TextBuilder.Append(m_LinkColor);
                    s_TextBuilder.Append(">");
                }

                var linkGroup = match.Groups[1];

                var hrefInfo = new HyperlinkInfo
                {
                    startIndex = s_TextBuilder.Length * 4, // 超链接里的文本起始顶点索引
                    endIndex = (s_TextBuilder.Length + match.Groups[2].Length - 1) * 4 + 3,
                    name = linkGroup.Value,
                };

                //Debug.Log("startIndex:" + hrefInfo.startIndex + ",endIndex:" + hrefInfo.endIndex + ",name:" + hrefInfo.name);
                m_HrefInfos.Add(hrefInfo);

                if (lineCount <= 1)//单行的时候,再计算完startIndex后,再进行添加富文本
                {
                    s_TextBuilder.Append("<color=");  // 超链接颜色
                    s_TextBuilder.Append(m_LinkColor);
                    s_TextBuilder.Append(">");
                }
                s_TextBuilder.Append(match.Groups[2].Value);
                s_TextBuilder.Append("</color>");
                indexText = match.Index + match.Length;
            }
            s_TextBuilder.Append(originText.Substring(indexText, originText.Length - indexText));
            return s_TextBuilder.ToString(); ;
        }
        //------------------------------------------------------
        /// <summary>
        /// 点击事件检测是否点击到超链接文本
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            Vector2 lp = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out lp);

            foreach (var hrefInfo in m_HrefInfos)
            {
                var boxes = hrefInfo.boxes;
                Debug.Log("boxes.Count:" + boxes.Count);
                for (var i = 0; i < boxes.Count; ++i)
                {
                    if (boxes[i].Contains(lp))
                    {
                        m_OnHrefClick.Invoke(hrefInfo.name);
                        return;
                    }
                }
            }
        }
        //------------------------------------------------------
        public void SetLinkColor(string hexColor)
        {
            if (string.IsNullOrWhiteSpace(hexColor))
            {
                return;
            }
            m_LinkColor = hexColor;
        }
        //------------------------------------------------------
        public void SetLinkColor(Color color)
        {
            m_LinkColor = ColorUtility.ToHtmlStringRGB(color);
        }
        //------------------------------------------------------
        public List<HyperlinkInfo> GetLinkInfo()
        {
            return m_HrefInfos;
        }
    }
}