using System.Collections;
using System.Collections.Generic;

namespace ColliderTools
{

    public static class SphereCollider
    {
        public static bool CheckCollider(Sphere a,Sphere b)
        {
            //两点距离公式 = ((ax - bx)平方 + (ay - by)平方 + (az - bz)平方)根号
            float distance =(a.center.x - b.center.x) * (a.center.x - b.center.x) + (a.center.y - b.center.y) * (a.center.y - b.center.y) + (a.center.z - b.center.z) * (a.center.z - b.center.z);
            float radiusSum = a.radius + b.radius;
            return distance <= radiusSum * radiusSum;//由于上面距离没有开根号,所以下面总半径长度就平方
        }
    }



    public struct Point
    {
       public float x;
       public float y;
       public float z;
    }

    public struct Sphere
    {
        public Point center;
        public float radius;
    }
}


