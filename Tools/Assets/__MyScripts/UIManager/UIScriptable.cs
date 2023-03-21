using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "UIConfig", menuName = "ScriptableObjects/CreateUIConfig", order = 1)]
public class UIScriptable : ScriptableObject
{
    [Serializable]
    public class UIConfig
    {
        public EUIInstanceID eUIInstanceID;
        public int order;
        public GameObject Prefab;
        public string PrefabPath;

    }

    [NonReorderable]
    public List<UIConfig> vUIConfigs = new List<UIConfig>();
#if UNITY_EDITOR
    private void OnValidate()
    {
        for (int i = 0; i < vUIConfigs.Count; i++)
        {
            var ui = vUIConfigs[i];
            ui.PrefabPath = AssetDatabase.GetAssetPath(ui.Prefab);
        }
    }
#endif
}
