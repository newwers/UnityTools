using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace DCG.UI
{
    ///<summary>对文本组建的扩展</summary>
    ///<author name="Danny Yan"></author>
    [RequireComponent(typeof(Text)), DisallowMultipleComponent]
    public class TextExpand : BaseMeshEffect
    {
        [Header("是否使用渐变色")]
        public bool useGradientColor = false;
        [Header("左上颜色,右上颜色,左下颜色,右下颜色")]
        public Color32 gradientColor1 = Color.white;
        public Color32 gradientColor2 = Color.white;
        public Color32 gradientColor3 = Color.white;
        public Color32 gradientColor4 = Color.white;

        [Header("是否开启对齐")]
        public bool useAlign = false;
        [Header("字间距")]
        public float wordSpace = 0;
        [Header("行间距")]
        public float lineSpace = 0;

        [Header("是否显示下划线")]
        public bool useUnderline = false;
        [Header("下划线是否忽略换行符")]
        public bool ignoreBreakSign = true;
        [Header("下划线宽度"), Range(0f, 100f), SerializeField]
        private float _lineHeight = 1.5f;
        private float _lineHeightHalf = 0.75f;
        public float lineHeight
        {
            get { return this._lineHeight; }
            set
            {
                this._lineHeight = value;
                this._lineHeightHalf = value * .5f;
            }
        }
        [Header("行高是否两端扩展,否则向下扩展")]
        public bool lineHeightJustify = true;
        public float lineOffset = 0;
        public Color32 lineColor = Color.white;
        [Header("横线对齐到行的上下中间")]
        public bool lineAlignToMiddle = false;

        [Header("是否使用customLineIndexArray来作为显示线的起止依据,否则是全文字段显示")]
        public bool useCustomLineIndexArray = false;
        public Vector2Int[] customLineIndexArray = new Vector2Int[] { };

        private Text text;
        private UICharInfo[] characters;
        private UILineInfo[] lines;

        private Color[] gradientColors;
        private char[] textChars;

        private List<UIVertex> stream = null;

        // 可视的字符个数
        private int characterCountVisible = 0;

        override protected void Awake()
        {
            this.text = this.GetComponent<Text>();
            if (this.text == null) return;

            text.RegisterDirtyMaterialCallback(OnFontMaterialChanged);
        }
#if UNITY_EDITOR
        override protected void OnEnable()
        {
            this.text = this.GetComponent<Text>();
            text.RegisterDirtyMaterialCallback(OnFontMaterialChanged);
        }
#endif

        private void OnFontMaterialChanged()
        {
            // font纹理发生变化时,在font中注册一个字符
            text.font.RequestCharactersInTexture("*", text.fontSize, text.fontStyle);
        }

        protected override void OnDestroy()
        {
            text.UnregisterDirtyMaterialCallback(OnFontMaterialChanged);
            base.OnDestroy();
        }

        /// 从font纹理中获取指定字符的uv
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

        override public void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || (!this.useUnderline && !this.useAlign && !this.useGradientColor))
            {
                return;
            }

            // 当宽或高足够小则不处理
            if (text.rectTransform.rect.size.x <= 0 || text.rectTransform.rect.size.y <= 0) return;

#if UNITY_EDITOR
            _lineHeightHalf = this.lineHeight * .5f;
#endif

            // cachedTextGenerator是当前实际显示出来的相关信息,cachedTextGeneratorForLayout是所有布局信息(包括看不到的)
            this.characters = text.cachedTextGenerator.GetCharactersArray();
            this.lines = text.cachedTextGenerator.GetLinesArray();
            if (this.useUnderline && this.ignoreBreakSign)
            {
                this.textChars = this.text.text.ToCharArray();
            }

            // 使用characterCountVisible来得到真正显示的字符数量.characterCount会额外包含(在宽度不足)时候裁剪掉的(边缘)字符,会导致显示的下划线多一个空白的宽度
            this.characterCountVisible = text.cachedTextGenerator.characterCountVisible;

            /* stream是6个一组(对应三角形索引信息,左上角开始,顺时针)对应1个字(含空白)
                0-1
                 \|
                  2
                0
                |\
                3-2
                数组索引:
                [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, ...]
                对应三角形索引:
                [0, 1, 2, 2, 3, 0, 4, 5, 6, 6,  7,  4,...]
            */
            this.stream = new List<UIVertex>();

            vh.GetUIVertexStream(stream);
            vh.Clear();

            if (this.useAlign || this.useGradientColor)
            {

                var _offi = 0;
                for (int i = 0; i < stream.Count; i += 6) // 单个字符
                {

                    int charIdx = i / 6;

                    var offx = 0f;
                    var offy = 0f;
                    if (this.useAlign)
                    {

                        // 不能直接使用(wordSpace * charIdx)来计算每个字符在间距下的x偏移,
                        // 要判断该字符是否处于换行的第1个字符序列,如果是第1个应该重置为0,
                        // GetCharCursorPos()中在获取每个字符的起始位置时,也会将该偏移减掉.
                        var lineIndex = this.GetCharInLineIndex(charIdx);
                        if (this.lines[lineIndex].startCharIdx == charIdx)
                        {
                            _offi = 0;
                        }

                        offx = this.wordSpace * _offi;
                        offy = this.lineSpace * lineIndex;

                        _offi++;
                    }

                    for (int j = 0; j < 6; j++)
                    {
                        // 渐变色
                        if (this.useGradientColor)
                        {
                            this.SetGradientColors(i, j);
                        }

                        // 间距
                        if (this.useAlign)
                        {
                            this.DoAlign(i, j, offx, offy);
                        }
                    }
                }
            }

            // 添加下划线
            if (useUnderline)
            {
                if (!this.useCustomLineIndexArray)
                {
                    this.DrawAllLinesLine(vh);
                }
                else
                {
                    this.DrawCustomLine(vh);
                }
            }

            vh.AddUIVertexTriangleStream(stream);
        }

        private void SetGradientColors(int i, int j)
        {
            if (gradientColors == null) gradientColors = new Color[6];

            gradientColors[0] = gradientColor1;
            gradientColors[1] = gradientColor2;
            gradientColors[2] = gradientColor4;
            gradientColors[3] = gradientColor4;
            gradientColors[4] = gradientColor3;
            gradientColors[5] = gradientColor1;

            UIVertex uiv = stream[i + j];
            uiv.color = gradientColors[j];
            stream[i + j] = uiv;
        }

        private void DoAlign(int i, int j, float offx, float offy)
        {
            UIVertex uiv = stream[i + j];
            var pos = uiv.position;
            // pos.x += this.wordSpace * _offi;
            // pos.y += this.lineSpace * lineIndex;
            pos.x += offx;
            pos.y += offy;

            uiv.position = pos;
            stream[i + j] = uiv;
        }

        /// 获取一个字符索引所在的行
        private int GetCharInLineIndex(int charIndex)
        {
            var ei = this.lines.Length - 1;
            for (int i = 0; i < ei; i++)
            {
                var line = lines[i];
                if (charIndex >= line.startCharIdx && charIndex < lines[i + 1].startCharIdx) return i;
            }

            // 是否在最后一行
            if (charIndex >= lines[ei].startCharIdx && charIndex < this.characters.Length) return ei;

            return -1;
        }
        // 显示所有下划线
        private void DrawAllLinesLine(VertexHelper vh)
        {
            var uv0 = this.GetUnderlineCharUV();
            for (int i = 0; i < this.lines.Length; i++)
            {
                var line = this.lines[i];
                var endIndex = 0;
                if (i + 1 < this.lines.Length)
                {
                    // 通过下一行的起始索引减1得到这一行最后一个字符索引位置
                    var nextLineStartCharIdx = this.lines[i + 1].startCharIdx;
                    // 与本行的相同,当文本宽度只够容纳一个字的时候,unity会产生一个空行,要排除改行
                    if (nextLineStartCharIdx == this.lines[i].startCharIdx) continue;

                    endIndex = nextLineStartCharIdx - 1;
                }
                else
                {
                    // 最后一行的最后字符索引位置
                    if (this.characterCountVisible == 0) continue;
                    endIndex = this.characterCountVisible - 1;
                }

                var bottomY = this.GetLineBottomY(i);

                var firstCharOff = line.startCharIdx * this.wordSpace;
                this.AddUnderlineVertTriangle(vh, line.startCharIdx, endIndex, firstCharOff, bottomY, uv0);
            }
        }
        // 显示自定义起止点下划线
        private void DrawCustomLine(VertexHelper vh)
        {
            var uv0 = this.GetUnderlineCharUV();
            var charsMaxIndex = this.characterCountVisible - 1;

            var chars = text.text.ToCharArray();

            for (int i = 0; i < this.customLineIndexArray.Length; i++)
            {
                var v2 = this.customLineIndexArray[i];
                var startIndex = v2[0];
                var endIndex = v2[1];

                if (endIndex < startIndex)
                {
                    startIndex = v2[1];
                    endIndex = v2[0];
                }

                if (startIndex < 0) startIndex = 0;
                if (endIndex > charsMaxIndex) endIndex = charsMaxIndex;

                if (startIndex >= this.characterCountVisible) continue;

                var lineIndex0 = this.GetCharInLineIndex(startIndex);
                var lineIndex1 = this.GetCharInLineIndex(endIndex);
                if (lineIndex0 != lineIndex1)
                {
                    // 跨行
                    for (int j = lineIndex0; j <= lineIndex1; j++)
                    {
                        var bottomY = this.GetLineBottomY(j);
                        var lineStartCharIndex = startIndex;
                        var lineEndCharIndex = endIndex;
                        if (j == lineIndex0)
                        {
                            // 第一行,从指定起始字索引到改行末尾字索引(改行末尾索引是下一行的起始索引-1得到)
                            lineEndCharIndex = lines[j + 1].startCharIdx - 1;
                        }
                        else if (j == lineIndex1)
                        {
                            // 最后一行,从改行起始字索引到指定字索引
                            lineStartCharIndex = lines[j].startCharIdx;
                        }
                        else
                        {
                            // 中间行,从改行起始字所以到该行末尾字索引
                            lineStartCharIndex = lines[j].startCharIdx;
                            lineEndCharIndex = lines[j + 1].startCharIdx - 1;
                        }

                        var firstCharOff = lines[j].startCharIdx * this.wordSpace;
                        this.AddUnderlineVertTriangle(vh, lineStartCharIndex, lineEndCharIndex, firstCharOff, bottomY, uv0);
                    }
                }
                else
                {
                    // 在同一行
                    var bottomY = this.GetLineBottomY(lineIndex0);
                    var firstCharOff = lines[lineIndex0].startCharIdx * this.wordSpace;
                    this.AddUnderlineVertTriangle(vh, startIndex, endIndex, firstCharOff, bottomY, uv0);
                }
            }
        }

        private float GetLineBottomY(int lineIndex)
        {
            UILineInfo line = this.lines[lineIndex];
            var bottomY = line.topY - (lineAlignToMiddle ? line.height * .5f : line.height) - lineOffset;

            // bottomY是原始大小下的信息,但文本在不同分辨率下会被进一步缩放处理,所以要将比例带入计算
            bottomY /= this.text.pixelsPerUnit;

            if (this.useAlign)
            {
                bottomY += lineIndex * this.lineSpace;
            }
            return bottomY;
        }

        private Vector2 GetCharCursorPos(int charIdx, float firstCharOff)
        {
            var charInfo = this.characters[charIdx];
            var cursorPos = charInfo.cursorPos;
            // cursorPos是原始大小下的信息,但文本在不同分辨率下会被进一步缩放处理,所以要将比例带入计算
            cursorPos /= this.text.pixelsPerUnit;

            var rtf = (this.transform as RectTransform);
            if (this.useAlign)
            {
                // 行的第1个字符在DoAlign()中是修正过偏移的,所有后面的所有字符都要相应使用此偏移
                cursorPos.x += this.wordSpace * charIdx - firstCharOff;
            }
            return cursorPos;
        }
        private UIVertex[] underlineUIVertexs = new UIVertex[6];
        private void AddUnderlineVertTriangle(VertexHelper vh, int startIndex, int endIndex, float firstCharOff, float bottomY, Vector2 uv0)
        {
            if (this.ignoreBreakSign && this.textChars[endIndex] == '\n')
            {
                // 跳过换行符
                endIndex--;
            }
            if (endIndex < startIndex) return;

            // 左上
            var pos0 = new Vector3(this.GetCharCursorPos(startIndex, firstCharOff).x, bottomY + (this.lineHeightJustify ? this._lineHeightHalf : 0), 0);

            // 左下, 向下扩展
            var pos1 = new Vector3(pos0.x, pos0.y - this.lineHeight, 0);

            // 右下. charWidth是原始大小下的信息,但文本在不同分辨率下会被进一步缩放处理,所以要将比例带入计算
            var pos2 = new Vector3(this.GetCharCursorPos(endIndex, firstCharOff).x + characters[endIndex].charWidth / this.text.pixelsPerUnit, pos1.y, 0);

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
            // stream.Add(underlineUIVertexs[0]);
            // stream.Add(underlineUIVertexs[3]);
            // stream.Add(underlineUIVertexs[2]);
            // stream.Add(underlineUIVertexs[0]);
            // stream.Add(underlineUIVertexs[2]);
            // stream.Add(underlineUIVertexs[1]);
        }
    }
}