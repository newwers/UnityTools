using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Z.UI.HP
{
    public class LOLHP : MaskableGraphic
    {
        public float m_hp = 0;//总血量
        public float m_interval = 1000;//大单位血量一格
        public float m_intervalSmall = 100;//小单位血量一格
        public float m_heightRatio = 1f;
        public float m_heightSmallRatio = 0.7f;
        public float m_thickness = 2;//血条的粗细
        public float m_offsetX;//x轴偏移
        public float m_offsetY;//y轴偏移
        [Range(0,1)]
        public float m_maskValue = 1;//控制绘制百分比

        
        public void RefreshUI(float hp, float interval)
        {
            m_hp = hp;
            m_interval = interval;
            SetVerticesDirty();//调用后会在下一帧重新执行OnPopulateMes
            //SetMaterialDirty();//调用后会在下一帧重新设置material以及Texture
            //SetLayoutDirty();//调用后会重新布局
            //SetAllDirty();//以上全部调用
        }

        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// 当UI元素需要生成顶点时的回调函数。填充顶点缓冲区数据。
        /// 由文本使用，UI.图像，和RawImage，以生成特定于其用例的顶点。
        /// 另一个重载传递过来的参数是Mesh(已经被弃用)
        /// </summary>
        /// <param name="vh"></param>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            //绘制血条
            //1.获取血量
            //2.定义血量每格代表多少血
            //3.获得血条的大小
            //4.计算每个绘制矩形的位置
            if (m_hp == 0)
            {
                return;
            }

            vh.Clear();

            //Test(vh);
            //return;

            int count = Mathf.FloorToInt(m_hp / m_intervalSmall);
            int index = Mathf.FloorToInt(m_interval / m_intervalSmall);
            Rect rect = GetPixelAdjustedRect();//获取到血条UI的长宽
            for (int i = 1; i < count; i++)
            {
                float x = (rect.width / count) * i + rect.xMin + m_offsetX;//绘制的x坐标 = 每格宽度 * 格子索引 + 矩形左下角x坐标 +  偏移
                float y = m_offsetY + rect.yMin;//绘制的y坐标

                if (((x - rect.xMin)/ rect.width) > m_maskValue)//控制绘制区域百分比
                {
                    return;
                }

                //vh.currentIndexCount 表示当前索引的数量,
                int indexCount = vh.currentVertCount;//当前顶点数量
                //0,0中心点,左下角时xmin,ymin  左上角xmin,ymax,右上角xmax,ymax,右下角:xmax,ymin
                //UV,左下角0,0点,右上角1,1点

                if (i != 0 && i % index == 0)//当到达设置的大刻度时
                {
                    vh.AddVert(new Vector3(x, y + rect.height * (1 - m_heightRatio), 0), color, Vector2.zero);
                    vh.AddVert(new Vector3(x, y + rect.height, 0), color, Vector2.zero);
                }
                else//绘制正常的小刻度
                {
                    vh.AddVert(new Vector3(x, y + rect.height * (1 - m_heightSmallRatio), 0), color, Vector2.zero);
                    vh.AddVert(new Vector3(x, y + rect.height, 0), color, Vector2.zero);
                }

                if (i != 0 &&  i % index == 0)
                {
                    vh.AddVert(new Vector3(x + m_thickness, y + rect.height, 0), color, Vector2.zero);
                    vh.AddVert(new Vector3(x + m_thickness, y + rect.height * (1 - m_heightRatio), 0), color, Vector2.zero);
                }
                else
                {
                    vh.AddVert(new Vector3(x + m_thickness, y + rect.height, 0), color, Vector2.zero);
                    vh.AddVert(new Vector3(x + m_thickness, y + rect.height * (1 - m_heightSmallRatio), 0), color, Vector2.zero);
                }

                
                //添加三角形,主要注意绘制的顺序
                vh.AddTriangle(indexCount, indexCount + 1, indexCount + 2);
                vh.AddTriangle(indexCount, indexCount + 2, indexCount + 3);
            }
        }

        [ContextMenu("测试")]
        private void Test()
        {
            RefreshUI(m_hp,m_interval);
        }
    }
}
