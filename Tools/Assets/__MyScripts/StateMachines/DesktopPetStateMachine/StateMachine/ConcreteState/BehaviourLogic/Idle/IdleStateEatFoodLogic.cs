//#if UNITY_EDITOR    
//using UnityEditor;
//#endif
using UnityEngine;

[CreateAssetMenu(fileName = "IdleStateEatFoodLogic", menuName = "状态机/休闲/吃东西逻辑")]
public class IdleStateEatFoodLogic : IdleStateChangeStateByAnimation
{


    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Initialize(Character character)
    {
        base.Initialize(character);

    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }


    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void ResetValues()
    {
        base.ResetValues();
    }
}



//[CustomEditor(typeof(IdleStateEatFoodLogic))]
//public class IdleStateEatFoodLogicEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        // 自动绘制标准字段
//        DrawDefaultInspector();
//    }
//}