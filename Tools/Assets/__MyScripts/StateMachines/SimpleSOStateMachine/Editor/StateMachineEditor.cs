using UnityEditor;
using UnityEngine;

namespace StateMachineSystem.Editor
{
    [CustomEditor(typeof(StateMachine))]
    public class StateMachineEditor : UnityEditor.Editor
    {
        private StateMachine stateMachine;

        private void OnEnable()
        {
            stateMachine = (StateMachine)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            if (stateMachine == null)
                return;

            // 检查当前状态是否为Walk状态
            if (stateMachine.currentStateType == StateType.Walk)
            {
                // 获取WalkState实例
                WalkState walkState = null;
                foreach (var state in stateMachine.GetType().GetField("states", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(stateMachine) as System.Collections.Generic.Dictionary<StateType, StateBase>)
                {
                    if (state.Key == StateType.Walk)
                    {
                        walkState = state.Value as WalkState;
                        break;
                    }
                }

                if (walkState != null)
                {
                    // 获取目标位置
                    Vector3 targetPosition = (Vector3)walkState.GetType().GetField("targetPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(walkState);

                    // 绘制目标位置点
                    Handles.color = Color.green;
                    Handles.SphereHandleCap(0, targetPosition, Quaternion.identity, 0.2f, EventType.Repaint);

                    // 绘制从当前位置到目标位置的线
                    Vector3 currentPosition = stateMachine.transform.position;
                    Handles.DrawLine(currentPosition, targetPosition);

                    // 计算并绘制距离
                    float distance = Vector3.Distance(currentPosition, targetPosition);
                    Vector3 labelPosition = (currentPosition + targetPosition) / 2;
                    Handles.Label(labelPosition, $"距离: {distance:F2}");
                }
            }
        }
    }
}