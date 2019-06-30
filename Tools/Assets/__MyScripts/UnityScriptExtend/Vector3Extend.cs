using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 扩展Vector3类的函数
/// </summary>
public static class Vector3Extend  {

	public static Vector3 Parse(this Vector3 vec,string text)
    {
        string[] vector = text.Split(',');
        float x,y,z = 0f;
        if (float.TryParse(vector[0],out x) && float.TryParse(vector[1], out y) && float.TryParse(vector[2], out z))
        {
            return new Vector3(x,y,z);
        }
        return new Vector3();
    }

}
