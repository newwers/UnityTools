using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(Transform), true)]
public class TransformInspector : Editor
{
    private static bool _hasObj;

    static public TransformInspector instance;

    SerializedProperty mPos;
    SerializedProperty mRot;
    SerializedProperty mScale;


    private void OnEnable()
    {
        try
        {
            instance = this;

            if (serializedObject != null)
            {
                _hasObj = true;
                mPos = serializedObject.FindProperty("m_LocalPosition");
                mRot = serializedObject.FindProperty("m_LocalRotation");
                mScale = serializedObject.FindProperty("m_LocalScale");
                Vector3 scaleAll_Vec3 = mScale.vector3Value;
                if (scaleAll_Vec3.x == scaleAll_Vec3.y && scaleAll_Vec3.x == scaleAll_Vec3.z)
                {
                    m_Scale = scaleAll_Vec3.x;
                }
            }
        }
        catch { }
    }

    private void OnDestroy()
    {
        instance = null;
        _hasObj = false;
    }

    public override void OnInspectorGUI()
    {
        if (!_hasObj)
        {
            base.OnInspectorGUI();
            return;
        }

        EditorGUIUtility.labelWidth = 15f;

        serializedObject.Update();

        DrawPosition();
        DrawRotation();
        DrawScale();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPosition()
    {
        GUILayout.BeginHorizontal();

        var reset = GUILayout.Button("R", GUILayout.Width(20f));
        var copy = GUILayout.Button("C", GUILayout.Width(20f));
        var paste = GUILayout.Button("P", GUILayout.Width(20f));
        EditorGUILayout.LabelField("Position");
        EditorGUILayout.PropertyField(mPos.FindPropertyRelative("x"));
        EditorGUILayout.PropertyField(mPos.FindPropertyRelative("y"));
        EditorGUILayout.PropertyField(mPos.FindPropertyRelative("z"));

        if (reset) mPos.vector3Value = Vector3.zero;

        if (copy)
        {
            GUIUtility.systemCopyBuffer = mPos.vector3Value.x + "," + mPos.vector3Value.y + "," + mPos.vector3Value.z;
            Debug.LogError("剪切板:" + GUIUtility.systemCopyBuffer);
        }

        if (paste)
        {
            Debug.LogError("剪切板:" + GUIUtility.systemCopyBuffer);
            string[] pos = GUIUtility.systemCopyBuffer.Split(',');
            mPos.vector3Value = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
            //mPos.vector3Value = GUIUtility.systemCopyBuffer.ParseVector3();
        }

        GUILayout.EndHorizontal();
    }

    private void DrawRotation()
    {
        GUILayout.BeginHorizontal();
        {
            var reset = GUILayout.Button("R", GUILayout.Width(20f));
            var copy = GUILayout.Button("C", GUILayout.Width(20f));
            var paste = GUILayout.Button("P", GUILayout.Width(20f));
            EditorGUILayout.LabelField("Rotation");
            var visible = (serializedObject.targetObject as Transform).localEulerAngles;

            visible.x = WrapAngle(visible.x);
            visible.y = WrapAngle(visible.y);
            visible.z = WrapAngle(visible.z);

            var changed = CheckDifference(mRot);
            var altered = Axes.None;

            var opt = GUILayout.MinWidth(30f);

            if (FloatField("X", ref visible.x, (changed & Axes.X) != 0, opt)) altered |= Axes.X;
            if (FloatField("Y", ref visible.y, (changed & Axes.Y) != 0, opt)) altered |= Axes.Y;
            if (FloatField("Z", ref visible.z, (changed & Axes.Z) != 0, opt)) altered |= Axes.Z;

            if (reset)
            {
                mRot.quaternionValue = Quaternion.identity;
            }
            else if (altered != Axes.None)
            {
                RegisterUndo("Change Rotation", serializedObject.targetObjects);

                foreach (var obj in serializedObject.targetObjects)
                {
                    var t = obj as Transform;
                    var v = t.localEulerAngles;

                    if ((altered & Axes.X) != 0) v.x = visible.x;
                    if ((altered & Axes.Y) != 0) v.y = visible.y;
                    if ((altered & Axes.Z) != 0) v.z = visible.z;

                    t.localEulerAngles = v;
                }
            }

            if (copy)
            {
                GUIUtility.systemCopyBuffer = mRot.quaternionValue.x + "," + mRot.quaternionValue.y + "," + mRot.quaternionValue.z + "," + mRot.quaternionValue.w;
                Debug.LogError("剪切板:" + GUIUtility.systemCopyBuffer);
            }

            if (paste)
            {
                Debug.LogError("剪切板:" + GUIUtility.systemCopyBuffer);
                string[] pos = GUIUtility.systemCopyBuffer.Split(',');
                mRot.quaternionValue = new Quaternion(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]), float.Parse(pos[3]));
                //Vector4 vector4 = GUIUtility.systemCopyBuffer.ParseVector4();
                //mRot.quaternionValue = new Quaternion(vector4.x, vector4.y, vector4.z, vector4.w);
            }
        }
        GUILayout.EndHorizontal();
    }

    float m_Scale = 1f;
    private void DrawScale()
    {
        GUILayout.BeginHorizontal();

        var reset = GUILayout.Button("R", GUILayout.Width(20f));
        var copy = GUILayout.Button("C", GUILayout.Width(20f));
        var paste = GUILayout.Button("P", GUILayout.Width(20f));
        var set = GUILayout.Button("S", GUILayout.Width(20f));
        m_Scale = EditorGUILayout.FloatField("All", m_Scale);
        EditorGUILayout.LabelField("Scale");

        if (set)
        {
            mScale.vector3Value = new Vector3(m_Scale, m_Scale, m_Scale);
            Undo.RecordObject(serializedObject.targetObject, "m_Scale");
        }


        EditorGUILayout.PropertyField(mScale.FindPropertyRelative("x"));
        EditorGUILayout.PropertyField(mScale.FindPropertyRelative("y"));
        EditorGUILayout.PropertyField(mScale.FindPropertyRelative("z"));

        if (reset) mScale.vector3Value = Vector3.one;

        if (copy)
        {
            GUIUtility.systemCopyBuffer = mScale.vector3Value.x + "," + mScale.vector3Value.y + "," + mScale.vector3Value.z;
            Debug.LogError("剪切板:" + GUIUtility.systemCopyBuffer);
        }

        if (paste)
        {
            Debug.LogError("剪切板:" + GUIUtility.systemCopyBuffer);
            string[] pos = GUIUtility.systemCopyBuffer.Split(',');

            mScale.vector3Value = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
            //mScale.vector3Value = GUIUtility.systemCopyBuffer.ParseVector3();//通过扩展方法进行字符串解析,但是同时也把两个脚本关联在一起了,如果缺少扩展脚本,上面两行注释就是源代码
        }

        GUILayout.EndHorizontal();
    }

    #region Rotation quaternion property drawing
    enum Axes : int
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
        All = 7,
    }

    Axes CheckDifference(Transform t, Vector3 original)
    {
        var next = t.localEulerAngles;

        var axes = Axes.None;

        if (Differs(next.x, original.x)) axes |= Axes.X;
        if (Differs(next.y, original.y)) axes |= Axes.Y;
        if (Differs(next.z, original.z)) axes |= Axes.Z;

        return axes;
    }

    Axes CheckDifference(SerializedProperty property)
    {
        var axes = Axes.None;

        if (property.hasMultipleDifferentValues)
        {
            var original = property.quaternionValue.eulerAngles;

            foreach (var obj in serializedObject.targetObjects)
            {
                axes |= CheckDifference(obj as Transform, original);
                if (axes == Axes.All) break;
            }
        }
        return axes;
    }

    /// <summary>
    /// Draw an editable float field.
    /// </summary>
    /// <param name="hidden">Whether to replace the value with a dash</param>

    private bool FloatField(string name, ref float value, bool hidden, GUILayoutOption opt)
    {
        var newValue = value;
        GUI.changed = false;

        if (!hidden) newValue = EditorGUILayout.FloatField(name, newValue, opt);
        else float.TryParse(EditorGUILayout.TextField(name, "--", opt), out newValue);

        if (GUI.changed && Differs(newValue, value))
        {
            value = newValue;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Because Mathf.Approximately is too sensitive.
    /// </summary>

    private bool Differs(float a, float b) { return Mathf.Abs(a - b) > 0.0001f; }

    /// <summary>
    /// Create an undo point for the specified objects.
    /// </summary>

    private void RegisterUndo(string name, params Object[] objects)
    {
        if (objects == null || objects.Length <= 0) return;

        Undo.RecordObjects(objects, name);

        foreach (var obj in objects)
            if (obj != null)
                EditorUtility.SetDirty(obj);
    }

    /// <summary>
    /// Ensure that the angle is within -180 to 180 range.
    /// </summary>

    private float WrapAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    #endregion
}