using System;
using UnityEngine;

namespace SteamSDK
{
    /// <summary>
    /// 成就值类型枚举，用于指示成就应该使用整数值还是浮点值
    /// </summary>
    public enum ValueType
    {
        Int,
        Float
    }

    [Serializable]
    public class SteamAchievementData
    {
        /// <summary>
        /// 成就ID，必须唯一，通常与Steamworks中定义的成就ID一致
        /// </summary>
        [SerializeField]
        private string achievementId;

        [SerializeField]
        private string achievementName;

        [SerializeField]
        private string description;

        [SerializeField]
        private bool isAchieved;

        /// <summary>
        /// 成就关联的变量名称，用于在游戏中跟踪成就状态，必须与Steamworks中定义的统计变量名称一致
        /// </summary>
        [SerializeField]
        private string associatedVariableName;

        /// <summary>
        /// 成就值类型，指示应该使用整数值还是浮点值
        /// </summary>
        [SerializeField]
        private ValueType valueType;

        /// <summary>
        /// 成就条件的整数值（可选），用于某些需要计数的成就，例如“玩了100场游戏”
        /// </summary>
        [SerializeField]
        private int intValue;
        /// <summary>
        /// 成就条件的浮点值（可选），用于某些需要计数的成就，例如“累计游戏时间达到10小时”
        /// </summary>
        [SerializeField]
        private float floatValue;

        public string AchievementId
        {
            get { return achievementId; }
            set { achievementId = value; }
        }

        public string AchievementName
        {
            get { return achievementName; }
            set { achievementName = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public bool IsAchieved
        {
            get { return isAchieved; }
            set { isAchieved = value; }
        }

        public string AssociatedVariableName
        {
            get { return associatedVariableName; }
            set { associatedVariableName = value; }
        }

        public int IntValue
        {
            get { return intValue; }
            set { intValue = value; }
        }

        public float FloatValue
        {
            get { return floatValue; }
            set { floatValue = value; }
        }

        public ValueType ValueType
        {
            get { return valueType; }
            set { valueType = value; }
        }

        public SteamAchievementData()
        {
            achievementId = "";
            achievementName = "";
            description = "";
            isAchieved = false;
            associatedVariableName = "";
            valueType = ValueType.Int;
        }

        public SteamAchievementData(string id, string name, string desc, string variableName)
        {
            achievementId = id;
            achievementName = name;
            description = desc;
            isAchieved = false;
            associatedVariableName = variableName;
            valueType = ValueType.Int;
        }

        public SteamAchievementData(string id, string name, string desc, string variableName, ValueType type)
        {
            achievementId = id;
            achievementName = name;
            description = desc;
            isAchieved = false;
            associatedVariableName = variableName;
            valueType = type;
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(achievementId))
            {
                throw new ArgumentException("Achievement ID cannot be empty");
            }

            if (string.IsNullOrEmpty(achievementName))
            {
                throw new ArgumentException("Achievement name cannot be empty");
            }

            if (string.IsNullOrEmpty(associatedVariableName))
            {
                throw new ArgumentException("Associated variable name cannot be empty");
            }
        }

        public override string ToString()
        {
            return $"{achievementId}: {achievementName} - {description} ({(isAchieved ? "Achieved" : "Not Achieved")})";
        }
    }
}