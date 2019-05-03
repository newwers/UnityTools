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

        var reset = GUILayout.Button("P", GUILayout.Width(20f));
        EditorGUILayout.PropertyField(mPos.FindPropertyRelative("x"));
        EditorGUILayout.PropertyField(mPos.FindPropertyRelative("y"));
        EditorGUILayout.PropertyField(mPos.FindPropertyRelative("z"));

        if (reset) mPos.vector3Value = Vector3.zero;

        GUILayout.EndHorizontal();
    }

    private void DrawRotation()
    {
        GUILayout.BeginHorizontal();
        {
            var reset = GUILayout.Button("R", GUILayout.Width(20f));

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
        }
        GUILayout.EndHorizontal();
    }

    private void DrawScale()
    {
        GUILayout.BeginHorizontal();

        var reset = GUILayout.Button("S", GUILayout.Width(20));

        EditorGUILayout.PropertyField(mScale.FindPropertyRelative("x"));
        EditorGUILayout.PropertyField(mScale.FindPropertyRelative("y"));
        EditorGUILayout.PropertyField(mScale.FindPropertyRelative("z"));

        if (reset) mScale.vector3Value = Vector3.one;

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