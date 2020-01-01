using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 扩展Vector3类的函数
/// </summary>
public static class Vector3Extend  {
    /*思维上有误,应该从String类型进行扩展,而不是从Vector3进行扩展
    /// <summary>
    /// 将 1,2,3 格式的字符串解析成Vector3类型
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="text"></param>
    /// <returns></returns>
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
    */
}
