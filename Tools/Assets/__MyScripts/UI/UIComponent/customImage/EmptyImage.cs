/*
    空图片,不需要绘制,没有顶点和三角面,不产生drawcall 适用于做按钮点击区域大小的控制
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace zdq.UI
{
    public class EmptyImage : MaskableGraphic
    {
        protected EmptyImage()
        {
            useLegacyMeshGeneration = false;//不使用旧的mesh生成函数
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();// clear the vertex helper so invalid graphics dont draw.
        }
    }
}
