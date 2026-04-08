﻿using UnityEngine;

namespace StateMachineSystem
{
    [CreateAssetMenu(fileName = "ButterflyStayState", menuName = "StateMachine/ButterflyStayState", order = 6)]
    public class ButterflyStayStateSO : StayStateSO
    {
        [Header("Animation Update Settings")]
        [Range(1f, 10f)]
        public float minAnimationUpdateTime = 3f; // 最小更新时间（秒）

        [Range(1f, 120f)]
        public float maxAnimationUpdateTime = 5f; // 最大更新时间（秒）

        [Header("Animation Speed Settings")]
        [Range(0.1f, 3f)]
        public float minAnimationSpeed = 0.5f; // 最小动画速度

        [Range(0.1f, 10f)]
        public float maxAnimationSpeed = 1.5f; // 最大动画速度
    }
}