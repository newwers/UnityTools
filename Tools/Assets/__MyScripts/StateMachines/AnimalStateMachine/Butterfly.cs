﻿/*
蝴蝶实现：参考瓢虫功能，没有Idle状态，平时处于walk状态在地图上随意飞行
飞行时碰到碰撞器，有50%概率停留下来，切换到Stay状态
Stay状态下间隔一段时间或者点击蝴蝶会让蝴蝶再次进入Walk状态开始飞行
飞行状态下点击会让蝴蝶调换朝向飞行
*/
using StateMachineSystem;
using UnityEngine;

public class Butterfly : AnimalBase
{

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if (StateMachine != null)
        {
            if (IsDragging)
            {
                if (StateMachine.currentStateType == StateType.Stay)
                {
                    // 获取ButterflyWalkState实例
                    var state = StateMachine.GetState(StateType.Stay);
                    if (state != null)
                    {
                        var butterflyStayState = state as ButterflyStayState;
                        if (butterflyStayState != null)
                        {
                            butterflyStayState.OnDraggingTriggerExit2D(collision);

                        }
                    }
                }
            }
            else
            {
                if (StateMachine.currentStateType == StateType.Walk)
                {
                    // 获取ButterflyWalkState实例
                    var state = StateMachine.GetState(StateType.Walk);
                    if (state != null)
                    {
                        ButterflyWalkState butterflyWalkState = state as ButterflyWalkState;
                        if (butterflyWalkState != null)
                        {
                            butterflyWalkState.OnTriggerExit2D(collision);
                        }
                    }
                }
            }
        }

    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        //var mushRoom = collision.GetComponent<MushRoom>();
        //if (mushRoom != null)
        //{
        //    //mushRoom.OnAnimalTriggerClickMound();//触发点击土包
        //    mushRoom.OnAnimalTriggerClickMushroom();//触发点击蘑菇
        //}

        if (StateMachine == null)
        {
            return;
        }
        if (IsDragging)//拖拽中
        {
            if (StateMachine.currentStateType == StateType.Stay)
            {
                // 获取ButterflyWalkState实例
                var state = StateMachine.GetState(StateType.Stay);
                if (state != null)
                {
                    var butterflyStayState = state as ButterflyStayState;
                    if (butterflyStayState != null)
                    {
                        butterflyStayState.OnDraggingTriggerEnter2D(collision);

                    }
                }
            }
        }
        else
        {
            if (StateMachine.currentStateType == StateType.Walk)
            {
                // 获取ButterflyWalkState实例
                var state = StateMachine.GetState(StateType.Walk);
                if (state != null)
                {
                    ButterflyWalkState butterflyWalkState = state as ButterflyWalkState;
                    if (butterflyWalkState != null)
                    {
                        butterflyWalkState.OnTriggerEnter2D(collision);

                    }
                }
            }
        }



    }

    public override void OnMouse_Down()
    {
        if (StateMachine != null)
        {
            switch (StateMachine.currentStateType)
            {
                case StateType.Walk:
                    // 飞行状态下点击调换朝向
                    // 获取ButterflyWalkState实例
                    var state = StateMachine.GetState(StateType.Walk);
                    if (state != null)
                    {
                        ButterflyWalkState butterflyWalkState = state as ButterflyWalkState;
                        if (butterflyWalkState != null)
                        {
                            butterflyWalkState.ToggleFlightDirection();
                        }
                        break;
                    }
                    break;
                case StateType.Stay:
                    // 停留状态下点击切换到飞行状态
                    StateMachine.SetState(StateType.Walk);
                    break;
                default:
                    break;
            }
        }
    }

    public override bool CanDrag()
    {
        return StateMachine != null && StateMachine.currentStateType == StateType.Stay;
    }

    public override void OnDragEnd()
    {
        if (StateMachine != null)
        {
            // 如果不在物体内部，切换到Walk状态
            if (!IsInObject)
            {
                StateMachine.SetState(StateType.Walk);
            }
            // 如果在物体内部，保持原有状态
        }
    }


}