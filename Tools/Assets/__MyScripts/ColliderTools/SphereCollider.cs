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

        //构造函数
        public Point(float x,float y,float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        //运算符
        public static Point operator +(Point a,Point b)
        {
            return new Point(a.x+b.x,a.y+b.y,a.z+b.z);
        }
        public static Point operator -(Point a, Point b)
        {
            return new Point(a.x - b.x, a.y - b.y, a.z - b.z);
        }
        public static Point operator *(Point a, Point b)
        {
            return new Point(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        public static Point operator /(Point a, Point b)
        {
            return new Point(a.x / b.x, a.y / b.y, a.z / b.z);
        }
        public static bool operator ==(Point a, Point b)
        {
            if (a.x == b.x && a.y == b.y && a.z == b.z)
            {
                return true;
            }
            return false;
        }
        public static bool operator !=(Point a, Point b)
        {
            if (a.x != b.x || a.y != b.y || a.z != b.z)
            {
                return true;
            }
            return false;
        }
        //public static Point operator =(Point b,UnityEngine.Vector3 a)
        //{
        //    return new Point(b.x+a.x,b.y+a.y,b.z+a.z);
        //}

        //重写
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        //提供的函数
        public void SetVector3(UnityEngine.Vector3 pos)
        {
            x = pos.x;
            y = pos.y;
            z = pos.z;
        }

        public UnityEngine.Vector3 ConvertVector3()
        {
            return new UnityEngine.Vector3(x, y, z);
        }
    }

    public struct Sphere
    {
        public Point center;
        public float radius;
    }
}


