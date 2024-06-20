using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace MyWorld
{

    public class ShopItemEditor : EditorWindow
    {
        string itemName = "New Item";
        string itemShowName = "New Item";

        [MenuItem("Game/ShopItem Editor")]
        public static void ShowWindow()
        {
            GetWindow<ShopItemEditor>("Item Editor");
        }

        private void OnGUI()
        {
            GUILayout.Label("Create New Item", EditorStyles.boldLabel);

            itemName = EditorGUILayout.TextField("Item Name", itemName);
            itemShowName = EditorGUILayout.TextField("Show Name", itemShowName);

            if (GUILayout.Button("Create"))
            {
                ShopItem newItem = ScriptableObject.CreateInstance<ShopItem>();
                newItem.itemName = itemName;

                AssetDatabase.CreateAsset(newItem,$"Assets/SimplePoly City - Low Poly Assets/So/{itemShowName}.asset");
                AssetDatabase.SaveAssets();

                Debug.Log("New item created: " + newItem.name);
            }
        }
    }
}
