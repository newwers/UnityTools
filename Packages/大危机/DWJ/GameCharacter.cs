using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DWJ
{
    public class GameCharacter : MonoBehaviour
    {
        public CrisisCharacterData Data;
        public string CharacterName => Data.roleName;
        public bool IsAlive = true;
        public int abilityRemainingUses = -1;
        public bool canUseAbility = true;

        // 状态记录
        public bool hasKilledThisRound = false;
        public bool isMuted = false; // 被禁言
        private CrisisCharacterData currentIdentity; // 用于变脸

        // 运行时状态
        public float currentAbilityCooldown = 0f;

        public List<GameCharacter> eatenVictims = new List<GameCharacter>();
        public bool isDoingTask;

        void Start()
        {
            currentIdentity = Data;
            if (Data.specialAbility is KillAbility ka && ka.maxUses > 0)
                abilityRemainingUses = ka.maxUses;
        }

        private void Update()
        {
            if (GameManager.Instance.currentState == GameState.Playing)
            {
                HandleInteraction();
                HandleAbility();
            }
        }

        // 处理死亡逻辑 (重点：实现主播、尖叫、死神/食肉结算)
        public virtual void OnDeath(GameCharacter killer)
        {
            if (!IsAlive) return;
            IsAlive = false;

            // 1. 主播侠技能：死亡提示所有人
            if (Data.roleName == "主播侠")
            {
                UIManager.Instance.ShowNotification("【提示】主播侠 已在某处遇害！");
            }

            // 2. 尖叫侠技能：死亡瞬间开会 (除非被专杀)
            if (Data.roleName == "尖叫侠")
            {
                bool isProfessionalKill = killer != null && killer.Data.roleName == "专业杀手";
                if (!isProfessionalKill)
                {
                    GameManager.Instance.ImmediateMeeting(this);
                }
            }

            // 3. 食肉动物/死神逻辑
            if (Data.roleName == "食肉动物" && eatenVictims.Count > 0)
            {
                ReleaseEatenVictims(); // 死亡后肚子里的人出来
            }

            // 4. 复仇侠：检测周边死亡
            CheckRevengeTrigger();

            gameObject.SetActive(false);
            GameManager.Instance.OnCharacterDie(this);
        }

        // 击杀逻辑
        public void PerformKill(GameCharacter victim)
        {
            hasKilledThisRound = true;
            victim.OnDeath(this);

            // 狂战杀手：击杀重置暴走计时 (这里简化处理)
            if (Data.roleName == "狂战杀手")
            {
                StopAllCoroutines();
                StartCoroutine(BerserkTimer());
            }
        }

        IEnumerator BerserkTimer()
        {
            yield return new WaitForSeconds(120f);
            if (IsAlive) OnDeath(null); // 120秒不杀人自爆
        }

        // 变脸逻辑
        public void MorphInto(GameCharacter target)
        {
            currentIdentity = target.Data;
            Debug.Log($"{CharacterName} 变成了 {target.CharacterName}");
            // 改变外观逻辑...
        }

        // --- 鹈鹕/食肉动物专属 ---

        public void ReleaseEatenVictims()
        {
            foreach (var v in eatenVictims)
            {
                v.gameObject.SetActive(true);
                v.transform.position = transform.position;
            }
            eatenVictims.Clear();
        }

        // --- 环境感知 ---
        void CheckRevengeTrigger()
        {
            // 全局搜索复仇侠，如果他在死亡范围附近，获得1分钟刀
            var revengers = FindObjectsOfType<GameCharacter>().Where(c => c.Data.roleName == "复仇侠" && c.IsAlive);
            foreach (var r in revengers)
            {
                if (Vector3.Distance(r.transform.position, transform.position) < 10f)
                {
                    r.EnableTemporaryKill(60f);
                }
            }
        }

        public void EnableTemporaryKill(float duration)
        {
            StartCoroutine(TempKillRoutine(duration));
        }

        IEnumerator TempKillRoutine(float d)
        {
            canUseAbility = true; // 临时获得刀
            yield return new WaitForSeconds(d);
            if (Data.roleName == "复仇侠") canUseAbility = false;
        }

        public void SetVisibility(bool visible)
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) r.enabled = visible;
            /* 更改Alpha值或材质 */
        }
        /// <summary>
        /// 遇到尸体交互
        /// </summary>
        void HandleInteraction()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Body"))
                    {
                        GameManager.Instance.ImmediateMeeting(this);
                        return;
                    }
                }
            }
        }
        /// <summary>
        /// 使用能力
        /// </summary>
        void HandleAbility()
        {
            if (currentAbilityCooldown > 0) currentAbilityCooldown -= Time.deltaTime;

            // 按 F 释放技能
            if (Input.GetKeyDown(KeyCode.V) && Data.specialAbility != null)
            {
                if (currentAbilityCooldown <= 0)
                {
                    TryUseAbility();
                }
                else
                {
                    Debug.Log("技能冷却中...");
                }
            }
        }

        void TryUseAbility()
        {
            GameCharacter target = null;

            // 如果技能需要目标 (范围检测)
            if (Data.specialAbility.range > 0)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, Data.specialAbility.range);
                float minDist = Mathf.Infinity;
                foreach (var hit in hits)
                {
                    var c = hit.GetComponent<GameCharacter>();
                    if (c != null && c != this && c.IsAlive)
                    {
                        float d = Vector3.Distance(transform.position, c.transform.position);
                        if (d < minDist)
                        {
                            minDist = d;
                            target = c;
                        }
                    }
                }
            }

            // 无论有没有目标，都尝试触发 (有些技能不需要目标，如隐身)
            Data.specialAbility.OnActivate(this, target);
            currentAbilityCooldown = Data.specialAbility.cooldown;
        }
    }
}
