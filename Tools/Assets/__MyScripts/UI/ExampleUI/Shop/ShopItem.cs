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
        [InspectorName("��")]
        None=0,
        [InspectorName("ʳ��")]
        Food =1,
        [InspectorName("ˮ")]
        Water =2,
        [InspectorName("����")]
        Count,
    }

    public enum EAttributeType
    {
        [InspectorName("����")]
        Hungry = 0,
        [InspectorName("����")]
        thirst = 1,
        [InspectorName("����")]
        Count,
    }
}