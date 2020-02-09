/*
	newwer
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Asset.Core.Tools.ExpandEditor
{
    [CustomEditor(typeof(MyTween),true)]//true的意思是,当所有没有回调的编辑器都不匹配的时候才匹配此编辑器
    [CanEditMultipleObjects]
    /// <summary>
    /// 扩展编辑器
    /// </summary>
	public class MyTweenInspect : Editor {

        SerializedProperty _isScale;
        SerializedProperty _scaleCurve;
        SerializedProperty _moveTime;
        SerializedProperty _from;
        SerializedProperty _to;
        SerializedProperty _positionCurve;

        MyTween _myTween;

        private void OnEnable()
        {
            if (serializedObject!=null)
            {
                _isScale=serializedObject.FindProperty("isScale");
                _scaleCurve=serializedObject.FindProperty("Scale_curve");
                _moveTime=serializedObject.FindProperty("Move_time");
                _from = serializedObject.FindProperty("From");
                _to = serializedObject.FindProperty("To");
                _positionCurve = serializedObject.FindProperty("Position_curve");
            }
            _myTween=target as MyTween;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPositionProperty();

            DrawScaleProperty();

            serializedObject.ApplyModifiedProperties();//将做出的修改应用到原先的脚本中,不加这句则会造成修改不了属性的情况
        }

        private void DrawPositionProperty()
        {
            _myTween.Move_time = EditorGUILayout.FloatField("移动时间", _myTween.Move_time);
            _myTween.From = EditorGUILayout.Vector3Field("位移开始点:", _myTween.From);
            _myTween.To = EditorGUILayout.Vector3Field("位移结束点:", _myTween.To);
            _myTween.Position_curve = EditorGUILayout.CurveField("位移曲线", _myTween.Position_curve);
            //EditorGUILayout.PropertyField(_positionCurve);
        }

        private void DrawScaleProperty()
        {
            _myTween.isScale = EditorGUILayout.Toggle("是否缩放", _myTween.isScale);
            if (_myTween.isScale)
            {
                
                _myTween.Scale_curve = EditorGUILayout.CurveField("缩放曲线", _myTween.Scale_curve);
            }
        }

    }
}
