using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace MyWorld
{
    /// <summary>
    /// 该特性的目的是在float字段上绘制一个按钮
    /// </summary>
    public class ButtonAttribute : PropertyAttribute
    {
        public string showName;

        public ButtonAttribute(string showName)
        {
            this.showName = showName;
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
    [CustomPropertyDrawer(typeof(ButtonAttribute))]
    public class ButtonDrawer : PropertyDrawer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">屏幕上用于属性 GUI 的矩形。</param>
        /// <param name="property">要为其创建自定义 GUI 的序列化属性。</param>
        /// <param name="label">此属性的标签。</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //EditorGUI.DrawRect(position, Color.yellow);

            //绘制前缀label字段的名称
            Rect totalValueAtra = EditorGUI.PrefixLabel(position, label);

            //绘制rect区域
            //Rect drawRect = new Rect(totalValueAtra.x + 10, totalValueAtra.y + 10, totalValueAtra.width - 10, totalValueAtra.height - 10);
            //EditorGUI.DrawRect(drawRect, Color.red);

            
            //取到特性
            ButtonAttribute btnAttribute = attribute as ButtonAttribute;


            //绘制按钮
            Rect btnRect = new Rect(totalValueAtra.x, totalValueAtra.y, 100, totalValueAtra.height);//这边的50有没有办法根据文本长度,设置对于的长度,而不是固定的数值

            //btnRect = GUILayoutUtility.GetRect(new GUIContent(btnAttribute.showName, string.Empty), GUI.skin.button);//无法使用这个会报错
            //ArgumentException: Getting control 1's position in a group with only 1 controls when doing repaint  因为这个无法找到具体的rect位置

            if (GUI.Button(btnRect, btnAttribute.showName))
            {
                //获取指定的字段
                //怎么知道挂载了哪个类型? 只能根据 property.propertyType 进行判断,然后单独处理

                Debug.Log("property.propertyType:" + property.propertyType);

                switch (property.propertyType)//通过这边进行指定字段类型的判断,并且根据类型不同进行对应值的获取
                {
                    case SerializedPropertyType.Integer:

                        Debug.Log(property.intValue);
                        break;
                    case SerializedPropertyType.Float:
                        Debug.Log(property.floatValue);
                        break;
                    default:
                        break;
                }
            }
            //EditorGUI.LinkButton
            //GUILayout.Button("'")

            //绘制出值
            Rect labelRect = new Rect(btnRect.xMax, totalValueAtra.y, totalValueAtra.width - 50, position.height);
            //property.serializedObject.Update();
            EditorGUI.PropertyField(labelRect, property);
            //property.serializedObject.ApplyModifiedProperties();
            EditorGUI.DrawRect(labelRect, new Color(0,0,1,0.2f));
            //switch (property.propertyType)//通过这边进行指定字段类型的判断,并且根据类型不同进行对应值的获取
            //{
            //    case SerializedPropertyType.Integer:
            //        EditorGUI.LabelField(labelRect, property.intValue.ToString());
            //        break;
            //    case SerializedPropertyType.Float:
            //        EditorGUI.LabelField(labelRect, property.floatValue.ToString("f2"));
            //        break;
            //    default:
            //        break;
            //}




        }
    }

#endif
}
