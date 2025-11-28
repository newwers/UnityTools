using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Characters/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    [System.Serializable]
    public class CharacterEntry
    {
        public string characterName;
        public GameObject characterPrefab;
        public ActionManager actionManager;
        public Vector3 posOffset;
        //public Vector2 colliderSize;
        //public Vector2 colliderOffset;
    }

    [Header("Character References")]
    public List<CharacterEntry> characters = new List<CharacterEntry>();


    /// <summary>
    /// 根据角色名称获取对应的角色预制体
    /// </summary>
    /// <param name="characterName">角色名称</param>
    /// <returns>对应的角色预制体，未找到则返回null</returns>
    public CharacterEntry GetCharacterPrefab(string characterName)
    {
        return characters.Find(c => c.characterName == characterName);
    }

    /// <summary>
    /// 根据索引获取角色预制体
    /// </summary>
    /// <param name="index">角色在列表中的索引</param>
    /// <returns>对应的角色预制体，索引无效则返回null</returns>
    public CharacterEntry GetCharacterPrefab(int index)
    {
        if (index >= 0 && index < characters.Count)
        {
            return characters[index];
        }
        return null;
    }

    /// <summary>
    /// 获取角色数量
    /// </summary>
    /// <returns>角色总数</returns>
    public int GetCharacterCount()
    {
        return characters.Count;
    }

    /// <summary>
    /// 根据ActionData找到对应的角色条目
    /// </summary>
    /// <param name="actionData">目标ActionData</param>
    /// <returns>包含该ActionData的角色条目，未找到返回null</returns>
    public CharacterEntry GetCharacterEntry(ActionData actionData)
    {
        if (actionData == null)
        {
            return null;
        }

        foreach (var entry in characters)
        {
            if (entry?.actionManager == null)
            {
                continue;
            }

            if (ActionManagerContains(entry.actionManager, actionData))
            {
                return entry;
            }
        }

        return null;
    }

    private bool ActionManagerContains(ActionManager actionManager, ActionData targetAction)
    {
        if (actionManager == null || targetAction == null)
        {
            return false;
        }

        var fields = actionManager.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        foreach (var field in fields)
        {
            var value = field.GetValue(actionManager);

            if (value == null)
            {
                continue;
            }

            if (value == targetAction)
            {
                return true;
            }

            if (field.FieldType.IsArray && typeof(ActionData).IsAssignableFrom(field.FieldType.GetElementType()))
            {
                if (ArrayContainsAction((Array)value, targetAction))
                {
                    return true;
                }
            }
            else if (typeof(ActionData).IsAssignableFrom(field.FieldType))
            {
                if (value == targetAction)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool ArrayContainsAction(Array array, ActionData targetAction)
    {
        if (array == null || targetAction == null)
        {
            return false;
        }

        foreach (var element in array)
        {
            if (element == targetAction)
            {
                return true;
            }
        }

        return false;
    }

    private void OnValidate()
    {
        //让characters中的characterName等于characterPrefab的name
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].characterName != characters[i].characterPrefab.name)
            {
                characters[i].characterName = characters[i].characterPrefab.name;
            }
        }
    }
}