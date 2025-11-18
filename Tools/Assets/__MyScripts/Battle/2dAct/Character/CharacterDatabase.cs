using System.Collections.Generic;
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
}