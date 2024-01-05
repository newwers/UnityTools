/*
 美术定义的一些颜色
 */
using UnityEngine;
using TopGame.Base;

public static class ColorUtil 
{
    public static Color RedColor
    {
        get
        {
            return GetColor("#FF8C8C");
        }
    }
    //------------------------------------------------------
    public static Color WhiteColor
    {
        get
        {
            return GetColor("#EEF9FE");
        }
    }
    //------------------------------------------------------
    public static Color GreenColor
    {
        get
        {
            return GetColor("#51FB5D");
        }
    }
    //------------------------------------------------------
    public static Color GrayColor
    {
        get
        {
            return GetColor("#7B7B7B");
        }
    }
    //------------------------------------------------------
    public static Color BlueColor
    {
        get
        {
            return GetColor("#52C8F6");
        }
    }
    //------------------------------------------------------
    public static Color PurpleColor
    {
        get
        {
            return GetColor("#C588F9");
        }
    }
    //------------------------------------------------------
    public static Color YellowColor
    {
        get
        {
            return GetColor("#FEDD5C");
        }
    }
    //------------------------------------------------------
    public static Color GetColor(string strColor)
    {
        Color color = Color.white;
        if (ColorUtility.TryParseHtmlString(strColor, out color))
        {
            return color;
        }
        return color;
    }
}
