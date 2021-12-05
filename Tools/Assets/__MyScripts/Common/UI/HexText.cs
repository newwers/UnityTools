/********************************************************************
生成日期:	1:29:2021 10:06
类    名: 	LinkText
作    者:	zdq
描    述:	支持文本链接
*********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace zdq.UI
{
    [DisallowMultipleComponent]
    public class HexText : Text
    {
        public enum ShowType
        {
            None,
            Small,
            Big
        }

        [SerializeField]
        public ShowType numberShowType = ShowType.Big;

        StringBuilder stringBuilder = new StringBuilder();

        protected override void Awake()
        {
            base.Awake();
            raycastTarget = false;
            resizeTextForBestFit = true;
            horizontalOverflow = HorizontalWrapMode.Wrap;
            verticalOverflow = VerticalWrapMode.Truncate;
        }
        //------------------------------------------------------
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            switch (numberShowType)
            {
                case ShowType.None:
                    break;
                case ShowType.Small:
                    NumberSmall();
                    break;
                case ShowType.Big:
                    NumberBig();
                    break;
                default:
                    break;
            }

            base.OnPopulateMesh(toFill);
        }
        //------------------------------------------------------
        /// <summary>
        /// 小进制：
        /// 满万进K；起始10K，低于10K 取消K进制
        /// 满千万(1000K)进M；起始10M，低于10M、高于10K， 取消M、恢复K进制
        /// （不保留小数点，向下取整 ）
        /// </summary>
        void NumberSmall()
        {
            if (long.TryParse(text, out long num))
            {
                m_Text = GetNumString(num, 10000, 1000000);
            }
        }
        //------------------------------------------------------
        void NumberBig()
        {
            if (long.TryParse(text, out long num))
            {
                m_Text = GetNumString(num, 10000000, 100000000);
            }
        }
        //------------------------------------------------------
        public void SetCostNum(long leftNum, long rightNum)
        {
            m_Text = SetNum(leftNum, rightNum);
            SetVerticesDirty();
        }
        //------------------------------------------------------
        private string SetNum(long leftNum, long rightNum)
        {
            stringBuilder.Clear();

            string left = GetNumString(leftNum, 10000, 1000000);
            string right = GetNumString(rightNum, 10000, 1000000);

            if (leftNum < rightNum)
            {
                stringBuilder.Append("<color=#FF0000>");
            }

            stringBuilder.Append(left);

            if (leftNum < rightNum)
            {
                stringBuilder.Append("</color>");
            }

            stringBuilder.Append("/");

            stringBuilder.Append(right);

            return stringBuilder.ToString();
        }
        //------------------------------------------------------
        private string GetNumString(long num, int k, int m)
        {
            stringBuilder.Clear();

            if (num >= m)
            {
                stringBuilder.Append(Mathf.FloorToInt(num / m)).Append("M");
            }
            else if (num >= k)
            {
                stringBuilder.Append(Mathf.FloorToInt(num / k)).Append("M");
            }

            return stringBuilder.ToString();
        }
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(HexText))]
    public class HexTextEditor : UnityEditor.UI.TextEditor
    {
        SerializedProperty m_ShowType;
        protected override void OnEnable()
        {
            base.OnEnable();
            m_ShowType = serializedObject.FindProperty("numberShowType");
        }
        //------------------------------------------------------
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(m_ShowType);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}