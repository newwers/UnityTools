﻿using UnityEngine;

namespace StateMachineSystem
{
    [CreateAssetMenu(fileName = "ButterflyWalkState", menuName = "StateMachine/ButterflyWalkState", order = 5)]
    public class ButterflyWalkStateSO : WalkStateSO
    {
        [Header("walk状态切换到Stay状态概率")]
        [Range(0, 100)]
        public float WalkChangeToStayProbability = 0.1f; // 从Walk状态切换到Stay状态的概率

        [Range(0.5f, 3f)]
        public float flightFrequency = 1f;

        [Range(0.5f, 3f)]
        public float maxFlightHeight = 1.5f;
        [Header("飞行距离地面最低高度")]
        public float goundHeight = 0.1f;

        [Header("停留检查间隔")]
        [Range(0.01f, 5f)]
        public float stayCheckInterval = 2f; // 每隔多少秒检查一次是否停留


        private void OnEnable()
        {
            stateType = StateType.Walk;
        }
    }
}