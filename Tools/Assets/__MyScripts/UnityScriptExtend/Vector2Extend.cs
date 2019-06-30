using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extend  {

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
}
