using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Z.Actor.Actor;

namespace Z.Actor
{
    /// <summary>
    /// 角色动画相关逻辑
    /// </summary>
    public class ActorAnimatorLogic : IMoveCharacter, IRotateCharacter
    {

        public string IdleAniName = "IdleState";
        public string RunAniName = "Run";
        public string RunSpeedName = "RunSpeed";
        public string JumpAniName = "Jump";
        public string JumpEndAniName = "JumpEnd";
        //public int Idle = Animator.StringToHash("IdleState");


        private Animator animator;

        public ActorAnimatorLogic(Animator ani)
        {
            animator = ani;
        }

        #region AnimatorAction

        public void PlayIdleAciton(int state = 0)
        {
            SetInteger(IdleAniName, state);
        }

        public void PlayRunAciton(float speed)
        {
            if (speed > 0)
            {
                //如果当前不在移动状态中,出发trigger
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName("MoveBlendTree"))
                {
                    SetTrigger(RunAniName);
                }
            }
            else
            {
                //SetTrigger(RunAniName);
            }
            
            SetFloat(RunSpeedName, speed);
        }

        public void PlayJumpAciton()
        {
            SetTrigger(JumpAniName);
        }

        public void PlayJumpEndAciton()
        {
            SetTrigger(JumpEndAniName);
        }

        #endregion


        public void SetTrigger(string name)
        {
            if (animator != null)
            {
                animator.SetTrigger(name);
            }
        }

        public void SetFloat(string name, float value)
        {
            if (animator != null)
                animator.SetFloat(name, value);
        }

        public void SetInteger(string name, int value)
        {
            if (animator != null)
                animator.SetInteger(name, value);
        }

        public void SetBool(string name, bool value)
        {
            if (animator != null)
                animator.SetBool(name, value);
        }

        public virtual void OnMove(Vector3 deltaPos)
        {
            //停止其他动作

            //根据偏移播放对应方向的移动动作
            PlayRunAciton(1);//todo:需要获取到移动速度
        }

        public void OnStopMove()
        {
            PlayRunAciton(0);
        }

        public virtual void OnRotate()
        {
            
        }

        public void OnFallGround()
        {
            PlayJumpEndAciton();
            Debug.Log("OnFallGround!!");
        }

        public void OnGetOffGround()
        {
            PlayJumpAciton();
            Debug.Log("OnGetOffGround!!");
        }
    }
}
