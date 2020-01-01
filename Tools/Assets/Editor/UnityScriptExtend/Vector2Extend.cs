using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extend  {
    /*思维上有误,应该从String类型进行扩展,而不是从Vector2进行扩展
    /// <summary>
    /// 将 1,2 格式的字符串解析成Vector2类型
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Vector2 Parse(this Vector2 vec, string text)
    {
        string[] vector = text.Split(',');
        float x, y = 0f;
        if (float.TryParse(vector[0], out x) && float.TryParse(vector[1], out y))
        {
            return new Vector2(x, y);
        }
        return new Vector2();
    }
    */
}
