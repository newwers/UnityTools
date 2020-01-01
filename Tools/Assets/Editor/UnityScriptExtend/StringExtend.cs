using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StringExtend  {

    /// <summary>
    /// 将 1,2 格式的字符串解析成Vector2类型
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Vector2 ParseVector2(this string text)
    {
        string[] vector = text.Split(',');
        float x, y = 0f;
        if (float.TryParse(vector[0], out x) && float.TryParse(vector[1], out y))
        {
            return new Vector2(x, y);
        }
        Debug.LogError("剪切板上格式有问题:" + text);
        return new Vector2();
    }

    /// <summary>
    /// 将 1,2,3 格式的字符串解析成Vector3类型
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
	public static Vector3 ParseVector3(this string text)
    {
        string[] vector = text.Split(',');
        float x, y, z = 0f;
        if (float.TryParse(vector[0], out x) && float.TryParse(vector[1], out y) && float.TryParse(vector[2], out z))
        {
            return new Vector3(x, y, z);
        }
        Debug.LogError("剪切板上格式有问题:" + text);
        return new Vector3();
    }

    public static Vector4 ParseVector4(this string text)
    {
        string[] vector = text.Split(',');
        float x, y, z, w = 0f;
        if (float.TryParse(vector[0], out x) && float.TryParse(vector[1], out y) && float.TryParse(vector[2], out z) && float.TryParse(vector[3], out w))
        {
            return new Vector4(x, y, z,w);
        }
        Debug.LogError("剪切板上格式有问题:" + text);
        return new Vector4();
    }
}
