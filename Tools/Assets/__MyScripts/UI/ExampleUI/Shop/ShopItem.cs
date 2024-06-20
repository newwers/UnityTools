using System.Collections.Generic;
using UnityEngine;

namespace MyWorld
{


    [CreateAssetMenu(fileName = "New Item", menuName = "Game/Shop/ShopItem")]
    public class ShopItem : Item
    {
        public float price = 0.0f;
    }

    public class Item : ScriptableObject
    {
        public string itemName = "New Item";
        public string description = "";
        public Sprite icon = null;
        public EItemType type = EItemType.None;
        public List<float> additionValues;
        public long num = 0;

        public Item Clone()
        {
            return (Item)this.MemberwiseClone();
        }
    }

    public enum EItemType
    {
        [InspectorName("无")]
        None=0,
        [InspectorName("食物")]
        Food =1,
        [InspectorName("水")]
        Water =2,
        [InspectorName("数量")]
        Count,
    }

    public enum EAttributeType
    {
        [InspectorName("饥饿")]
        Hungry = 0,
        [InspectorName("饥渴")]
        thirst = 1,
        [InspectorName("数量")]
        Count,
    }
}