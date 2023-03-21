using System;
using System.Collections.Generic;
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
    }

    [NonReorderable]
    public List<UIConfig> vUIConfigs = new List<UIConfig>();
}
