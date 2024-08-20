using Pal;
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
        private PalController m_PalController;
        

        public ActorAnimatorLogic(ActorAgent actorAgent)
        {
            if (actorAgent.Animator == null)
            {
                Debug.LogError("Animator is null");
            }
            if (actorAgent.PlayableDirector == null)
            {
                Debug.LogError("PlayableDirector is null");
            }
            animator = actorAgent.Animator;
            playableDirector = actorAgent.PlayableDirector;
            m_PalController = actorAgent as PalController;

            //if (playableDirector)
            //{
            //    //playableDirector.stopped += PlayableDirector_stopped;
            //    //playableDirector.played += PlayableDirector_played;
            //    //playableDirector.paused += PlayableDirector_paused;
            //}
            
        }



        #region TimelineAction


        private void PlayableDirector_paused(PlayableDirector obj)
        {
            Debug.Log("PlayableDirector_paused");
        }

        private void PlayableDirector_played(PlayableDirector obj)
        {
            StopPlayableDirector();//每次播放时,先停止当前播放再继续
            Debug.Log("PlayableDirector_played");
        }

        private void PlayableDirector_stopped(PlayableDirector obj)
        {
            Debug.Log("PlayableDirector_stopped");
        }

        public void PlaySleepAction()
        {
            //StopPlayableDirector();
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>(m_PalController.SleepTimelinePath);
            playableDirector.Play(asset, DirectorWrapMode.Hold);
        }

        public void PlaySleepEndAction()
        {
            //StopPlayableDirector();
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>(m_PalController.SleepEndTimelinePath);
            playableDirector.Play(asset, DirectorWrapMode.None);
        }

        public void PlaySkill2Action()
        {
            //StopPlayableDirector();
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>(m_PalController.Skill2TimelinePath);
            playableDirector.Play(asset, DirectorWrapMode.Loop);
        }

        public void PlaySpecialAction(GameObject tool)
        {
            //StopPlayableDirector();
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>(m_PalController.SpecialActionTimelinePath);
            playableDirector.playableAsset = asset;
            TimelineAsset timeline = playableDirector.playableAsset as TimelineAsset;
            if (timeline != null && timeline.outputTrackCount > 3 && timeline.GetOutputTrack(3) != null)//轨道设置绑定物体
            {
                var track1 = timeline.GetOutputTrack(3) as ActivationTrack;
                if (track1 != null)
                {
                    // 设置轨道目标为动态加载的物体
                    playableDirector.SetGenericBinding(track1, tool);
                }

            }
            playableDirector.Play(asset, DirectorWrapMode.Loop);
            //playableDirector.played += PlayableDirector_MineChange;//todo:如何确保矿稿隐藏?
        }

        public void PlayFarSkillAction()
        {
            //StopPlayableDirector();
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>(m_PalController.Skill1TimelinePath);
            playableDirector.Play(asset, DirectorWrapMode.Loop);
        }

        public void PlayEatAction()
        {
            //StopPlayableDirector();
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>(m_PalController.EatTimelinePath);
            playableDirector.Play(asset, DirectorWrapMode.Loop);
        }

        /// <summary>
        /// 挑衅动作
        /// </summary>
        public void PlayEncountAction()
        {
            //StopPlayableDirector();
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>(m_PalController.EncountTimelinePath);
            playableDirector.Play(asset, DirectorWrapMode.Loop);
        }

        /// <summary>
        /// 受击动作
        /// </summary>
        public void PlayDamageAction()
        {
            //StopPlayableDirector();
            PlayableAsset asset = ResourceLoadManager.Instance.Load<PlayableAsset>(m_PalController.DamageTimelinePath);
            playableDirector.Play(asset, DirectorWrapMode.None);
            //Debug.Log("PlayDamangeAction!!");
        }

        public void StopPlayableDirector()
        {
            if (playableDirector)
            {
                playableDirector.time = playableDirector.duration;//设置到timeline最后一帧,然后循环模式设置成None
                playableDirector.extrapolationMode = DirectorWrapMode.None;//目的是为了执行最后一帧的逻辑,把该隐藏的物体隐藏了,直接调用Stop会导致显示出来的物体,还在
                playableDirector.Evaluate();//强制刷新
            }
        }

        #endregion


        #region AnimatorAction

        public void PlayIdleAciton(int state = 0)
        {
            StopPlayableDirector();
            SetInteger(IdleAniName, state);
        }

        public void PlayRunAciton(float speed)
        {
            StopPlayableDirector();

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

        public void OnMoveTargetToPosEnd()
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
