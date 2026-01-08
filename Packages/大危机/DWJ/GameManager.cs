using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DWJ
{
    public class GameManager : BaseMonoSingleClass<GameManager>
    {
        public List<GameCharacter> players = new List<GameCharacter>();
        public int fuelCount = 0;
        public int jumpCount = 0;
        public GameState currentState;

        private void Start()
        {
            currentState = GameState.Playing;
        }

        void Update()
        {
            CheckVictory();
        }

        void CheckVictory()
        {
            // 好人胜利：燃料+跃迁 或 踢出所有狼
            if (fuelCount >= 10 && jumpCount >= 3) { EndGame(Faction.Good); return; }

            int goodCount = players.Count(p => p.IsAlive && p.Data.faction == Faction.Good);
            int badCount = players.Count(p => p.IsAlive && p.Data.faction == Faction.Bad);
            int neutralCount = players.Count(p => p.IsAlive && p.Data.faction == Faction.Neutral);

            // 坏人胜利
            if (badCount >= goodCount && badCount > 0) { EndGame(Faction.Bad); return; }

            // 阿呆胜利 (在投票逻辑中触发)
        }

        public void ImmediateMeeting(GameCharacter reporter)
        {
            Debug.Log("立即开启会议！");
            // 会议逻辑...
            currentState = GameState.Meeting;
        }

        public void OnCharacterDie(GameCharacter c) { /* 记录 */ }

        void EndGame(Faction winner)
        {
            currentState = GameState.GameOver;
            Debug.Log($"{winner} 胜利！");
        }

        void PrintGameStateSummary()
        {
            string summary = "<b>==== 战场情报摘要 ====</b>\n";
            foreach (var p in players.Where(p => p.IsAlive))
            {
                if (p.GetComponent<GameAIController>() is GameAIController ai)
                {
                    var worstEnemy = ai.trustLevels.OrderBy(kvp => kvp.Value).First();
                    summary += $"[{p.CharacterName}] 最怀疑: {worstEnemy.Key.CharacterName}({worstEnemy.Value})\n";
                }
            }
            Debug.Log(summary);
        }
        /// <summary>
        /// 整合投票和发言的完整流程日志
        /// </summary>
        public void ConductMeeting()
        {
            Debug.Log("<color=white><b>======= 会议开始 =======</b></color>");
            List<MeetingStatement> allStatements = new List<MeetingStatement>();

            // 1. 收集所有人的发言
            foreach (var p in players.Where(p => p.IsAlive))
            {
                var ai = p.GetComponent<GameAIController>();
                var statement = new MeetingStatement
                {
                    Speaker = p,
                    LastLocation = "Room_" + Random.Range(1, 5), // 模拟位置
                    Suspect = ai.ThinkVote(),
                    ClaimsGood = true
                };
                allStatements.Add(statement);
                Debug.Log($"[发言] {p.CharacterName}: 我刚才在 {statement.LastLocation}，我觉得 {(statement.Suspect ? statement.Suspect.CharacterName : "没发现嫌疑人")} 有问题。");
            }

            // 2. 所有人互相分析发言
            foreach (var listener in players.Where(p => p.IsAlive))
            {
                var ai = listener.GetComponent<GameAIController>();
                foreach (var s in allStatements) ai.ListenToStatement(s);
            }

            // 3. 最终投票
            Dictionary<GameCharacter, int> votes = new Dictionary<GameCharacter, int>();
            foreach (var p in players.Where(p => p.IsAlive))
            {
                var target = p.GetComponent<GameAIController>().ThinkVote();
                if (target != null)
                {
                    if (!votes.ContainsKey(target)) votes[target] = 0;
                    votes[target]++;
                }
            }

            // 4. 结算
            var mostVoted = votes.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
            if (mostVoted.Value > players.Count(p => p.IsAlive) / 2)
            {
                Debug.Log($"<color=red>[投票结果] {mostVoted.Key.CharacterName} 被最高票({mostVoted.Value}票)驱逐！</color>");
                mostVoted.Key.OnDeath(null);
            }
            else
            {
                Debug.Log("<color=gray>[投票结果] 票数不足，流票。</color>");
            }
        }
    }

    // --- 谜语杀手会议逻辑 (特供界面) ---
    public class RiddlerUI : MonoBehaviour
    {
        public void OnTryGuess(GameCharacter target, string predictedRole)
        {
            if (target.Data.roleName == predictedRole)
            {
                Debug.Log("狙击成功！");
                target.OnDeath(null);
            }
            else
            {
                Debug.Log("狙击失败，反噬！");
                // 谜语人本体死亡逻辑
            }
        }
    }
}
