using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
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
        private PlayableDirector playableDirector;
        private ActorAgent actorAgent;

        public ActorAnimatorLogic(Animator ani, PlayableDirector pd)
        {
            if (ani == null)
            {
                Debug.LogError("Animator is null");
            }
            if (pd == null)
            {
                Debug.LogError("PlayableDirector is null");
            }
            animator = ani;
            playableDirector = pd;
        }

        #region TimelineAction


        public void PlaySleepAction()
        {
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>("Assets/pal/PinkCat/TimeLine/PinkCat_Sleep_Timeline.playable");
            playableDirector.Play(asset, DirectorWrapMode.None);
        }

        public void PlayNekoPunchAction()
        {
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>("Assets/pal/PinkCat/TimeLine/PinkCat_NekoPunch_Timeline.playable");
            playableDirector.Play(asset, DirectorWrapMode.None);
        }

        public void PlayMineAction()
        {
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>("Assets/pal/PinkCat/TimeLine/PinkCat_Mine_Timeline.playable");
            playableDirector.Play(asset, DirectorWrapMode.None);
        }

        public void PlayFarSkillAction()
        {
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>("Assets/pal/PinkCat/TimeLine/PinkCat_FarSkill_Timeline.playable");
            playableDirector.Play(asset, DirectorWrapMode.None);
        }

        public void PlayEatAction()
        {
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>("Assets/pal/PinkCat/TimeLine/PinkCat_Eat_Timeline.playable");
            playableDirector.Play(asset, DirectorWrapMode.None);
        }

        /// <summary>
        /// 挑衅动作
        /// </summary>
        public void PlayEncountAction()
        {
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>("Assets/pal/PinkCat/TimeLine/PinkCat_Encount_Timeline.playable");
            playableDirector.Play(asset, DirectorWrapMode.None);
        }

        /// <summary>
        /// 受击动作
        /// </summary>
        public void PlayDamangeAction()
        {
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>("Assets/pal/PinkCat/TimeLine/PinkCat_Damage_Timeline.playable");
            playableDirector.Play(asset, DirectorWrapMode.None);
            //Debug.Log("PlayDamangeAction!!");
        }

        #endregion


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

        public virtual void OnMove(Vector3 deltaPos, float moveSpeed)
        {
            //停止其他动作

            //根据偏移播放对应方向的移动动作
            PlayRunAciton(moveSpeed);
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
            //Debug.Log("OnFallGround!!");
        }

        public void OnGetOffGround()
        {
            PlayJumpAciton();
            //Debug.Log("OnGetOffGround!!");
        }
    }
}
