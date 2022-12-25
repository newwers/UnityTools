using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace MyWorld
{
    /// <summary>
    /// 最大最小特性
    /// 1.继承 PropertyAttribute 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]//Inherited 是否能够被继承 AllowMultiple是否运行同一个字段使用多个特性
    public class MinMaxAttribute : PropertyAttribute
    {
        [MinMax(1,2)]
        public float min;
        public float max;

        public MinMaxAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }

#if UNITY_EDITOR

    /// <summary>
    /// 特性绘制
    /// 2.继承 PropertyDrawer
    /// 3.在改类上加上  CustomPropertyDrawer 特性进行标记绘制的是哪个特性
    /// 4.在OnGUI函数进行绘制
    /// 5.加上Unity_Editor 宏进行区分,防止打包问题
    /// </summary>
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public class MinMaxDrawer : PropertyDrawer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">屏幕上用于属性 GUI 的矩形。</param>
        /// <param name="property">要为其创建自定义 GUI 的序列化属性。</param>
        /// <param name="label">此属性的标签。</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            //可以使用这个方法进行判断类型是否正确
            UnityEngine.Assertions.Assert.IsTrue(property.propertyType == SerializedPropertyType.Vector2, "类型错误");

            if (property.propertyType == SerializedPropertyType.Vector2)//只取vector2类型的字段
            {

                
                //这个rect应该是Inspector中,使用了特性的字段的rect大小
                //rect 的特点就是给点指定的x,y和宽,高,就会已左下角为起始点,构造一个rect出来
                Rect totalValueAtra = EditorGUI.PrefixLabel(position, label);
               
                //构造左边的值的rect
                Rect leftArea = new Rect(totalValueAtra.x, totalValueAtra.y, 50, totalValueAtra.height);
                
                //构造滑动条的rect
                //*2的意思 其实这边公式是 中间滑动条的矩形大小= 总宽度 - 左边数值显示rect - 右边的数值显示rect,由于两边rect给定宽度一样,所以直接*2
                Rect valueArea = new Rect(leftArea.xMax, totalValueAtra.y, totalValueAtra.width - leftArea.width * 2 , totalValueAtra.height);
                EditorGUI.DrawRect(position , Color.green);
                //构造右边值的rect
                Rect rightArea = new Rect(totalValueAtra.xMax - leftArea.width, totalValueAtra.y, leftArea.width, totalValueAtra.height);

                //到此,上面的代码只是定义了矩形 rect,并没有实际绘制,所以这时候界面上还是空的

                //获取min和max的值,这边是Inspector面板上的minMax数值
                float min = property.vector2Value.x;
                float max = property.vector2Value.y;

                //取到特性
                MinMaxAttribute minMax = attribute as MinMaxAttribute;

                EditorGUI.MinMaxSlider(valueArea, ref min, ref max, minMax.min, minMax.max);//这边才会用到特性指定时设置的范围(这时候界面上才有滑动条,但是还无法拖动)

                property.vector2Value = new Vector2(min, max);//刷新更改后的数值(这时候可以拖动)

                EditorGUI.LabelField(leftArea, min.ToString("f2"));//加左侧数值显示 f2 表示小数后两位
                EditorGUI.LabelField(rightArea, max.ToString("f2"));//右侧数值显示
            }
        }
    }

#endif
}
