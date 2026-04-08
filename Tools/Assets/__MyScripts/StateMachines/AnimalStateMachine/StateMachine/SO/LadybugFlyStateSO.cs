using UnityEngine;

namespace StateMachineSystem
{
    [CreateAssetMenu(fileName = "FlyState", menuName = "StateMachine/FlyState", order = 7)]
    public class LadybugFlyStateSO : InteractIdleStateSO
    {
        [Header("Fly Settings")]
        [Range(1f, 10f)]
        public float minFlyTime = 3f;

        [Range(1f, 20f)]
        public float maxFlyTime = 8f;

        [Header("Flight Path Settings")]
        public float maxFlightHeight = 2.0f;
        public float flightFrequency = 2.0f;

        [Header("Movement Settings")]
        public float moveSpeed = 1.0f;
        public float moveRangeX = 10.0f;
        public float targetReachDistance = 0.5f;

        [Header("X Movement Interval")]
        public float minXMovementInterval = 1.0f;
        public float maxXMovementInterval = 3.0f;

        [Header("停留检查间隔")]
        [Range(0.01f, 5f)]
        public float stayCheckInterval = 0.3f; // 每隔多少秒检查一次是否停留

        [Header("Fly状态切换到Stay状态概率")]
        [Range(0, 100)]
        public float FlyChangeToStayProbability = 30f; // 从Fly状态切换到Stay状态的概率

        private void OnEnable()
        {
            stateType = StateType.Interact_Idle;
        }
    }
}