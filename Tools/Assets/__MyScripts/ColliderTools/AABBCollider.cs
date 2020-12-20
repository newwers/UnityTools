using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ColliderTools
{


    /// <summary>
    /// AABB碰撞算法
    /// 有三种记录方式
    /// 1.记录碰撞盒的最小和最大点,计算两个碰撞用a的最小值是否超过b的最大值,或者a的最大值是否比b的最小值还小来判断,如果符合就不碰撞,其他情况则碰撞
    /// 2.记录碰撞盒的最小点和最小点的3个方向向量长度,然后用两个最小点进行相减,只要相减后的3个方向的向量有其中一条大于两个点其中的一个向量,就没碰撞
    /// 3.记录碰撞盒的中心点和中心点到3个面的向量半径
    /// </summary>
    public static class AABBCollider
    {


        public static bool CheckCollider(AABB a, AABB b)
        {
            if (a.maxPoint.x < b.minPoint.x || a.minPoint.x > b.maxPoint.x)
            {
                return false;
            }
            if (a.maxPoint.y < b.minPoint.y || a.minPoint.y > b.maxPoint.y)
            {
                return false;
            }
            if (a.maxPoint.z < b.minPoint.z || a.minPoint.z > b.maxPoint.z)
            {
                return false;
            }
            return true;
        }
    }

    public struct AABB
    {
        public Point minPoint;
        public Point maxPoint;
    }
}