using System.Collections;
using UnityEngine;

namespace DWJ
{
    // --- 基础枚举 ---
    public enum Faction { Good, Bad, Neutral }
    public enum AIDifficulty { Low, Medium, High }
    public enum GameDifficulty { Low, Medium, High, Random }
    public enum GameState { Playing, Meeting, Voting, GameOver }

    // --- 核心角色配置 (ScriptableObject) ---
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "Crisis/CharacterData")]
    public class CrisisCharacterData : ScriptableObject
    {
        public string roleName;
        public Faction faction;
        [TextArea] public string skillDescription;
        public float baseMoveSpeed = 5f;
        public AbilityData specialAbility;
        public AbilityData specialAbility2;
        public bool immuneToRiddler = false; // 谜语人免疫（普良特权）
    }

    // --- 技能基类 ---
    public abstract class AbilityData : ScriptableObject
    {
        public string abilityName;
        [TextArea] public string description;
        /// <summary>
        /// 冷却时间 0 表示无冷却
        /// </summary>
        public float cooldown = 20f;
        /// <summary>
        /// 持续时间 0表示一直持续
        /// </summary>
        public float duration = 0;
        /// <summary>
        /// 技能作用范围
        /// </summary>
        public float range = 2.5f;
        public virtual bool CanUse(GameCharacter user) => user.IsAlive;
        public abstract void OnActivate(GameCharacter user, GameCharacter target);
    }

    // --- 具体技能实现 ---

    // 1. 击杀技能 (保安, 单刀, 坏人通用)
    [CreateAssetMenu(menuName = "大危机/技能/击杀技能")]
    public class KillAbility : AbilityData
    {
        public bool selfKillOnWrongTarget = false; // 错杀自爆（保安）
        public int maxUses = -1; // 使用次数（单刀）

        public override void OnActivate(GameCharacter user, GameCharacter target)
        {
            if (target == null || !target.IsAlive) return;


            Debug.Log($"<color=red>[击杀] {user.CharacterName} 击杀 {target.CharacterName}！</color>");
            user.PerformKill(target);//先击杀目标

            if (selfKillOnWrongTarget && target.Data.faction == Faction.Good)//判断是否保安误杀好人
            {
                Debug.Log($"{user.CharacterName} 错杀了队友，原地爆炸！");
                user.OnDeath(user);//错杀好人,自己也死亡
            }

            if (maxUses > 0)
            {
                user.abilityRemainingUses--;
                if (user.abilityRemainingUses <= 0) user.canUseAbility = false;
            }
        }
    }

    // 2. 隐身技能 (隐身杀手)
    [CreateAssetMenu(menuName = "大危机/技能/隐身技能")]
    public class InvisibilityAbility : AbilityData
    {
        public override void OnActivate(GameCharacter user, GameCharacter target)
        {
            user.StartCoroutine(InvisibilityRoutine(user));
        }

        IEnumerator InvisibilityRoutine(GameCharacter user)
        {
            user.SetVisibility(false);
            yield return new WaitForSeconds(duration);
            user.SetVisibility(true);
        }
    }

    // 3. 变脸技能 (变脸杀手)
    [CreateAssetMenu(menuName = "大危机/技能/变脸技能")]
    public class MorphAbility : AbilityData
    {
        public override void OnActivate(GameCharacter user, GameCharacter target)
        {
            if (target != null) user.MorphInto(target);
        }
    }

    // 4. 调查/嗅闻技能 (花生侠)
    [CreateAssetMenu(menuName = "大危机/技能/嗅闻技能")]
    public class ScentAbility : AbilityData
    {
        public override void OnActivate(GameCharacter user, GameCharacter target)
        {
            if (target == null) return;
            // 检查目标是否在当前回合有过击杀
            bool isKiller = target.hasKilledThisRound;
            Debug.Log($"嗅闻结果：{target.CharacterName} 身上有杀气吗？ {isKiller}");
            // 这里可以弹出UI告知好人
        }
    }

    // 具体技能实现：吞噬 (中立)
    [CreateAssetMenu(menuName = "大危机/技能/吞噬")]
    public class PelicanEatAbility : AbilityData
    {
        public override void OnActivate(GameCharacter user, GameCharacter target)
        {
            if (target != null && target.IsAlive)
            {
                Debug.Log($"<color=orange>{user.CharacterName} (食肉) 把 {target.CharacterName} 吞掉了！肚子里现在有 {user.eatenVictims.Count + 1} 人</color>");
                // 鹈鹕吃人逻辑：
                // 1. 目标不视为死亡，但从地图上消失（为了简单，我们暂时禁用它）
                // 2. 只有开会时，被吃的人才真正判定死亡

                target.gameObject.SetActive(false); // 暂时隐藏
                user.eatenVictims.Add(target); // 记录在肚子里
            }
        }
    }
}
