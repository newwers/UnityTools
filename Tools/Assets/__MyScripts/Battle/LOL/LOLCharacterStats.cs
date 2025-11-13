namespace Z.Battle
{
    public enum AttributeType:int
    {
        Health = 0,
        Mana,
        PhysicalAttack,
        MagicalAttack,
        PhysicalDefense,
        MagicalDefense,
        AttackSpeed,
        SkillHaste,
        CritChance,
        CritDamageMultiplier,
        MovementSpeed,
        RotationSpeed,
        AttackRange,
        PhysicalDamageReduction,
        MagicalDamageReduction,
        PercentagePhysicalDamageReduction,
        PercentageMagicalDamageReduction,
        AllDamageReduction,
        PercentageAllDamageReduction,

        Count
    }

    [System.Serializable]
    public class LOLCharacterStats
    {
        public int[] Props = new int[(int)AttributeType.Count];

        public int health
        {
            get
            {
                return Props[(int)AttributeType.Health];
            }
            set
            {
                Props[(int)AttributeType.Health] = value;
            }
        }
        public int mana
        {
            get 
            {  
                return Props[(int)AttributeType.Mana];
            }
            set
            {
                Props[(int)AttributeType.Mana] = value;
            }
        }
        /// <summary>
        /// 物理攻击
        /// 这边的物理攻击可以是普通攻击，也可以是物理技能伤害
        /// </summary>
        public int physicalAttack
        {
            get
            {
                return Props[(int)AttributeType.PhysicalAttack];
            }
            set
            {
                Props[(int)AttributeType.PhysicalAttack] = value;
            }
        }
        /// <summary>
        /// 魔法攻击
        /// </summary>
        public int magicalAttack
        {
            get
            {
                return Props[(int)(AttributeType.MagicalAttack)];
            }
            set
            {
                Props[(int)(AttributeType.MagicalAttack)] = value;
            }
        }
        /// <summary>
        /// 物理防御
        /// </summary>
        public int physicalDefense
        {
            get
            {
                return Props[(int)(AttributeType.PhysicalDefense)];
            }
            set
            {
                Props[(int)(AttributeType.PhysicalDefense)] = value;
            }
        }
        /// <summary>
        /// 魔法防御
        /// </summary>
        public int magicalDefense
        {
            get
            {
                return (int)(AttributeType.MagicalDefense);
            }
            set
            {
                Props[(int)AttributeType.MagicalDefense] = value;
            }
        }
        /// <summary>
        /// 攻击速度 次/秒
        /// </summary>
        public float attackSpeed
        {
            get
            {
                return Props[(int)(AttributeType.AttackSpeed)] /100f;
            }
            set
            {
                Props[(int)(AttributeType.AttackSpeed)] = (int)(value * 100f);
            }
        }
        public int skillHaste
        {
            get
            {
                return Props[(int)(AttributeType.SkillHaste)];
            }
            set
            {
                Props[(int)(AttributeType.SkillHaste)] = value;
            }
        }//技能急数
        public int critChance
        {
            get
            {
                return Props[(int)(AttributeType.CritChance)];
            }
            set
            {
                Props[(int)(AttributeType.CritChance)] = value;
            }
        }//暴击
        public int critDamageMultiplier
        {
            get
            {
                return Props[(int)(AttributeType.CritDamageMultiplier)];
            }
            set
            {
                Props[(int)(AttributeType.CritDamageMultiplier)] = value;
            }
        } // 暴击伤害倍数
        public int movementSpeed
        {
            get
            {
                return Props[(int)(AttributeType.MovementSpeed)];
            }
            set
            {
                Props[(int)(AttributeType.MovementSpeed)] = value;
            }
        }
        public int rotationSpeed
        {
            get
            {
                return Props[(int)(AttributeType.RotationSpeed)];
            }
            set
            {
                Props[(int)(AttributeType.RotationSpeed)] = value;
            }
        } // 旋转速度
        
        public float attackRange// 攻击距离
        {
            get
            {
                return Props[(int)(AttributeType.AttackRange)] /100f;
            }
            set
            {
                Props[(int)(AttributeType.AttackRange)] = (int)(value * 100f);
            }
        }


        public int physicalDamageReduction
        {
            get
            {
                return Props[(int)(AttributeType.PhysicalDamageReduction)];
            }
            set
            {
                Props[(int)(AttributeType.PhysicalDamageReduction)] = value;
            }
        } // 物理伤害减少
        public int magicalDamageReduction
        {
            get
            {
                return Props[(int)(AttributeType.PercentageMagicalDamageReduction)];
            }
            set
            {
                Props[(int)(AttributeType.PercentageMagicalDamageReduction)] = value;
            }
        } // 魔法伤害减少
        public int percentagePhysicalDamageReduction
        {
            get
            {
                return Props[(int)(AttributeType.PercentagePhysicalDamageReduction)];
            }
            set
            {
                Props[(int)(AttributeType.PercentagePhysicalDamageReduction)] = value;
            }
        } // 百分比物理伤害减少
        public int percentageMagicalDamageReduction
        {
            get
            {
                return Props[(int)(AttributeType.PercentageMagicalDamageReduction)];
            }
            set
            {
                Props[(int)(AttributeType.PercentageMagicalDamageReduction)] = value; 
            }
        } // 百分比魔法伤害减少
        public int allDamageReduction
        {
            get
            {
                return Props[(int)(AttributeType.AllDamageReduction)];
            }
            set
            {
                Props[(int)(AttributeType.AllDamageReduction)] = value;
            }
        } // 所有伤害减伤
        public int percentageAllDamageReduction
        {
            get
            {
                return Props[(int)(AttributeType.PercentageAllDamageReduction)];
            }
            set
            {
                Props[(int)(AttributeType.PercentageAllDamageReduction)] = value;
            }
        } // 百分比所有伤害减伤

        /// <summary>
        /// 攻击间隔
        /// </summary>
        public float attackInterval
        {
            get
            {
                return 1f / attackSpeed;
            }
        }

        public LOLCharacterStats()
        {
            health = 100;
            mana = 50;
            physicalAttack = 10;
            magicalAttack = 5;
            physicalDefense = 5;
            magicalDefense = 3;
            attackSpeed = 1*100; // 每秒攻击次数
            skillHaste = 1*100; // 技能冷却速度
            critChance = 10; // 暴击几率
            critDamageMultiplier = 150; // 暴击伤害倍数，默认为1.5倍
            movementSpeed = 5*100; // 单位：米/秒
            rotationSpeed = 5; // 旋转速度，
            attackRange = 1; // 攻击距离，单位：米


            // 初始化伤害减少属性
            physicalDamageReduction = 0;
            magicalDamageReduction = 0;
            percentagePhysicalDamageReduction = 0;
            percentageMagicalDamageReduction = 0;
            allDamageReduction = 0;
            percentageAllDamageReduction = 0;
        }
    }
}