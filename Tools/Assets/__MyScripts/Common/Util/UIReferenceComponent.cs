/********************************************************************
生成日期:	2020-06-28
类    名: 	UIReferenceComponent
作    者:	zdq
描    述:	UI控件引用获取组件,主要用在Scroll View的Cell上,避免每次获取组件都通过Find的方式进行
*********************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TopGame.UI
{
    public class UIReferenceComponent : MonoBehaviour
    {
        [System.Serializable]
        public class UIReferenceData
        {
            public string name;
            public GameObject go;
        }

        public List<UIReferenceData> uiReferenceList;

        public T GetUI<T>(string name)
        {
            for (int i = 0; i < uiReferenceList.Count; i++)
            {
                if (uiReferenceList[i].name.Equals(name))
                {
                    return uiReferenceList[i].go.GetComponent<T>();
                }
            }

            return default(T);
        }
#if UNITY_EDITOR
        [ContextMenu("设置所有元素名字和物体名字一致")]
        void SetAllElementName()
        {
            for (int i = 0; i < uiReferenceList.Count; i++)
            {
                if (uiReferenceList[i].go != null)
                {
                    uiReferenceList[i].name = uiReferenceList[i].go.name;
                }
            }

            UnityEditor.EditorUtility.SetDirty(gameObject);
            //UnityEditor.AssetDatabase.SaveAssets();
        }
#endif

    }
}
