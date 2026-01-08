using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace DWJ
{
    /// <summary>
    /// AI 认知记忆条目
    /// </summary>
    public class SightingRecord
    {
        public GameCharacter seenCharacter;
        public Vector3 position;
        public float timestamp;
        public string locationName;
    }
    /// <summary>
    /// AI 在会议上的“供词”数据结构
    /// </summary>
    public struct MeetingStatement
    {
        public GameCharacter Speaker;
        public string LastLocation;
        public GameCharacter Suspect; // 他怀疑谁
        public bool ClaimsGood;      // 是否自证好人
    }

    /// <summary>
    /// AI 决策核心：集成视觉、记忆、信任与移动系统
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class GameAIController : MonoBehaviour
    {
        public GameCharacter Character { get; private set; }
        public AIDifficulty Difficulty;
        public NavMeshAgent Agent { get; private set; }

        [Header("AI Sensors")]
        public float viewDistance = 15f;
        public float viewAngle = 120f;
        public LayerMask obstacleMask;

        [Header("Social Engine")]
        // 对其他玩家的信任值: -100(死敌/确认为狼) 到 100(绝对信任)
        public Dictionary<GameCharacter, float> trustLevels = new Dictionary<GameCharacter, float>();
        // 记忆库：记录看到的玩家
        public List<SightingRecord> memoryStack = new List<SightingRecord>();
        // 记忆库：记录看到的尸体
        public List<Vector3> corpseDiscoveries = new List<Vector3>();

        private GameCharacter currentTarget;
        private Vector3 targetDestination;
        private bool isAtDestination => !Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance;

        void Awake()
        {
            Character = GetComponent<GameCharacter>();
            Agent = GetComponent<NavMeshAgent>();
            Agent.speed = Character.Data.baseMoveSpeed;
        }

        void Start()
        {
            InitializeTrust();
            StartCoroutine(MainLogicLoop());
            StartCoroutine(PerceptionLoop());
        }

        private void InitializeTrust()
        {
            foreach (var player in GameManager.Instance.players)
            {
                if (player == Character) continue;
                // 初始信任值根据难度有所不同，高难度AI初始更谨慎(0)，低难度更盲信(50)
                float initialTrust = Difficulty == AIDifficulty.High ? 0f : 30f;
                trustLevels[player] = initialTrust;
            }
        }

        #region 视觉与感知系统 (Perception)

        IEnumerator PerceptionLoop()
        {
            while (Character.IsAlive)
            {
                ScanEnvironment();
                yield return new WaitForSeconds(0.2f); // 感知频率
            }
        }

        private bool IsInView(GameCharacter other)
        {
            Vector3 dirToTarget = (other.transform.position - transform.position).normalized;
            float distToTarget = Vector3.Distance(transform.position, other.transform.position);

            if (distToTarget < viewDistance)
            {
                float angle = Vector3.Angle(transform.forward, dirToTarget);
                if (angle < viewAngle / 2)
                {
                    if (!Physics.Raycast(transform.position + Vector3.up, dirToTarget, distToTarget, obstacleMask))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ScanEnvironment()
        {
            foreach (var other in GameManager.Instance.players)
            {
                if (other == Character || !other.IsAlive) continue;


                if (IsInView(other)) // 封装后的视野检查
                {
                    RecordSighting(other);

                    // 观察任务行为：如果对方在任务点停留且未杀人
                    if (other.isDoingTask && other.Data.faction != Faction.Bad)
                    {
                        // 缓慢增加信任：每次扫描+1，最高恢复到初值
                        float recoveryAmount = Difficulty == AIDifficulty.High ? 0.5f : 1.0f;
                        float maxTrust = Difficulty == AIDifficulty.High ? 30f : 50f;

                        if (trustLevels[other] < maxTrust)
                        {
                            trustLevels[other] += recoveryAmount;
                            Debug.Log($"<color=green>[观察] {Character.CharacterName} 看到 {other.CharacterName} 在做任务，信任度微增至 {trustLevels[other]}</color>");
                        }
                    }
                }
            }

            // 扫描尸体 (假设尸体在特定Layer或通过其他方式获取)
            // 这里简化逻辑：如果附近有死亡角色且在视野内，记录尸体位置并触发推理
            var deadPlayers = GameManager.Instance.players.Where(p => !p.IsAlive).ToList();
            foreach (var dead in deadPlayers)
            {
                if (Vector3.Distance(transform.position, dead.transform.position) < 5f)
                {
                    if (!corpseDiscoveries.Contains(dead.transform.position))
                    {
                        corpseDiscoveries.Add(dead.transform.position);
                        InferFromCorpse(dead);
                    }
                }
            }


        }

        private void RecordSighting(GameCharacter other)
        {
            var record = new SightingRecord
            {
                seenCharacter = other,
                position = other.transform.position,
                timestamp = Time.time,
                locationName = "Room_" + Random.Range(1, 5) // 实际项目中可由区域触发器提供
            };
            memoryStack.Add(record);
            if (memoryStack.Count > 50) memoryStack.RemoveAt(0); // 限制记忆大小

            // 逻辑修正：如果看到某人正在杀人，信任直接降到底
            if (other.hasKilledThisRound && Difficulty != AIDifficulty.Low)
            {
                trustLevels[other] = -100f;
                Debug.Log($"<color=red>[目击证词] {Character.CharacterName} 目击了 {other.CharacterName} 的行凶现场！信任度降至死敌。</color>");
            }
        }

        #endregion

        #region 逻辑推断 (Reasoning)

        private void InferFromCorpse(GameCharacter victim)
        {
            Debug.Log($"<color=yellow>[发现尸体] {Character.CharacterName} 发现了 {victim.CharacterName} 的尸体！开始排查记忆...</color>");

            if (Difficulty == AIDifficulty.Low) return;

            // 寻找尸体被发现时间点附近，出现在该区域的人
            float discoveryTime = Time.time;
            foreach (var record in memoryStack)
            {
                // 如果在死亡时间附近看到某人在尸体附近
                if (Mathf.Abs(record.timestamp - discoveryTime) < 10f &&
                    Vector3.Distance(record.position, victim.transform.position) < 15f)
                {
                    // 增加怀疑 (降低信任)
                    float suspicionImpact = Difficulty == AIDifficulty.High ? -40f : -20f;
                    trustLevels[record.seenCharacter] += suspicionImpact;

                    Debug.Log($"<color=orange>[推断] {Character.CharacterName} 怀疑 {record.seenCharacter.CharacterName}，因为它在案发时间出现在现场。当前信任度: {trustLevels[record.seenCharacter]}</color>");
                }
            }

            // 如果是中高难度AI，发现尸体会报警
            if (Difficulty != AIDifficulty.Low)
            {
                GameManager.Instance.ImmediateMeeting(Character);
            }
        }

        public GameCharacter ThinkVote()
        {
            // 找出信任值最低的人
            var suspect = trustLevels.OrderBy(kvp => kvp.Value).FirstOrDefault();

            if (suspect.Value <= -40f) // 稍微调低阈值方便测试
            {
                Debug.Log($"<color=cyan>[投票决策] {Character.CharacterName} 决定投票给 {suspect.Key.CharacterName} (信任度: {suspect.Value})</color>");
                return suspect.Key;
            }

            Debug.Log($"<color=white>[投票决策] {Character.CharacterName} 决定弃票 (最高嫌疑人信任度: {suspect.Value})</color>");
            return null; // 信任值都不低则弃权
        }

        #endregion

        #region 移动与决策 (Navigation & Behavior)

        IEnumerator MainLogicLoop()
        {
            while (Character.IsAlive)
            {
                DecideDestination();
                yield return new WaitForSeconds(1.0f);
            }
        }

        private void DecideDestination()
        {
            if (Character.Data.faction == Faction.Bad)
            {
                ExecuteBadMovement();
            }
            else
            {
                ExecuteGoodMovement();
            }
        }

        private void ExecuteBadMovement()
        {
            // 坏人决策：
            // 1. 如果有非常怀疑我的人(信任值极低)，避开他
            // 2. 如果看到落单的人且信任值尚可，靠近他准备击杀
            // 3. 否则伪装做任务

            // 寻找队友：坏人天然知道谁是队友，但需要判断谁“值得信任”（比如没被好人怀疑的）
            var teammates = GameManager.Instance.players
                .Where(p => p.Data.faction == Faction.Bad && p != Character && p.IsAlive);

            foreach (var mate in teammates)
            {
                // 识别狼外婆 (RoleName判断)
                if (mate.Data.roleName == "狼外婆")
                {
                    // 坏人会倾向于保护或靠近狼外婆
                    if (Vector3.Distance(transform.position, mate.transform.position) > 10f)
                    {
                        Debug.Log($"<color=purple>[狼群] {Character.CharacterName} 正在向狼外婆 {mate.CharacterName} 靠拢。</color>");
                        Agent.SetDestination(mate.transform.position);
                        return;
                    }
                }
            }

            var target = FindIsolatedGoodPerson();
            if (target != null && !Character.hasKilledThisRound)
            {
                Agent.SetDestination(target.transform.position);
                if (Vector3.Distance(transform.position, target.transform.position) < 2f)
                {
                    Character.Data.specialAbility.OnActivate(Character, target);
                }
            }
            else
            {
                GoToRandomTaskPoint();
            }
        }

        private void ExecuteGoodMovement()
        {
            // 好人决策：
            // 1. 远离怀疑对象 (信任度 < -20)
            // 2. 靠近信任对象 (信任度 > 50)
            // 3. 去做任务

            var threat = trustLevels.FirstOrDefault(kvp => kvp.Value < -20f).Key;
            if (threat != null && Vector3.Distance(transform.position, threat.transform.position) < 8f)
            {
                // 逃跑逻辑：向反方向移动
                Vector3 runDir = (transform.position - threat.transform.position).normalized;
                Agent.SetDestination(transform.position + runDir * 5f);
            }
            else
            {
                GoToRandomTaskPoint();
            }
        }

        private void GoToRandomTaskPoint()
        {
            if (isAtDestination)
            {
                // 模拟随机任务点移动
                targetDestination = new Vector3(Random.Range(-20, 20), 0, Random.Range(-20, 20));
                Agent.SetDestination(targetDestination);
            }
        }

        private GameCharacter FindIsolatedGoodPerson()
        {
            // 高难度：不仅看是否落单，还看附近有没有监控或其他人视野覆盖
            return GameManager.Instance.players.FirstOrDefault(p =>
                p != Character && p.IsAlive && p.Data.faction == Faction.Good);
        }

        /// <summary>
        /// 处理他人发言的逻辑
        /// </summary>
        /// <param name="statement"></param>
        public void ListenToStatement(MeetingStatement statement)
        {
            if (statement.Speaker == Character) return;

            // 逻辑 A：如果他说我坏，我立刻降低对他的信任
            if (statement.Suspect == Character)
            {
                trustLevels[statement.Speaker] -= 30f;
                Debug.Log($"<color=red>[会议] {statement.Speaker.CharacterName} 居然反咬我！{Character.CharacterName} 降低了对他的信任。</color>");
            }

            // 逻辑 B：如果他说了一个我知道是谎言的信息（例如他说他在A，但我刚才在B见过他）
            var myRecord = memoryStack.LastOrDefault(r => r.seenCharacter == statement.Speaker);
            if (myRecord != null && myRecord.locationName != statement.LastLocation && Mathf.Abs(Time.time - myRecord.timestamp) < 20f)
            {
                trustLevels[statement.Speaker] -= 50f;
                Debug.Log($"<color=red>[会议] {Character.CharacterName} 发现 {statement.Speaker.CharacterName} 在报假位置！</color>");
            }
        }

        #endregion
    }

}