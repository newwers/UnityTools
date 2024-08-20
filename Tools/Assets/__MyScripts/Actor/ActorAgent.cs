using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using static Z.Actor.Actor;

namespace Z.Actor
{
    /// <summary>
    /// 该脚本是继承自 MonoBehaviour 的Actor代理类,方便后面角色继承该类
    /// </summary>
    public class ActorAgent : MonoBehaviour,IMoveCharacter
    {
        public float groundCheckDistance = 0.3f; // 高度判断距离


        private Animator animator;
        private PlayableDirector playableDirector;
        private AudioSource audioSource;


        protected Actor m_pActor;
        Collider m_Collider;
        protected ActorAnimatorLogic m_pAnimatorLogic;


        protected Vector3 m_MoveTargetPos
        {
            get
            {
                if (m_pActor == null) return Vector3.zero;
                return m_pActor.targetPosition;
            }
        }

        protected bool m_IsMoveToTargetPosition
        {
            get
            {
                if (m_pActor == null) return false;
                return m_pActor.IsMoveToTargetPosition;
            }
        }

        protected float moveSpeed
        {
            get
            {
                if (m_pActor == null) return 0;
                return m_pActor.moveSpeed;
            }
            set
            {
                if (m_pActor != null) m_pActor.moveSpeed = value;
            }
        }

        public bool IsGrounded
        {
            get
            {
                if (m_pActor == null) return false;
                return m_pActor.IsGrounded;
            }
            set
            {
                if (m_pActor != null) m_pActor.IsGrounded = value;
            }
        }

        public Animator Animator { get => animator; set => animator = value; }
        public PlayableDirector PlayableDirector { get => playableDirector; set => playableDirector = value; }
        public AudioSource AudioSource { get => audioSource; set => audioSource = value; }

        public LayerMask GroundLayer ;

        protected virtual void Awake()
        {

            AudioSource = GetComponent<AudioSource>();
            m_Collider = GetComponent<Collider>();

            m_pActor = new Actor(this.transform, m_Collider);

            m_pActor.groundCheckDistance = groundCheckDistance;
            if (GroundLayer == 0)
            {
                GroundLayer = LayerMask.NameToLayer("Ground");
            }
            m_pActor.GroundLayer = GroundLayer;


            

            if (Animator == null)
            {
                Animator = GetComponent<Animator>();
            }
            if (PlayableDirector == null)
            {
                PlayableDirector = GetComponent<PlayableDirector>();
            }
            m_pAnimatorLogic = new ActorAnimatorLogic(this);

            //加入接口监听
            m_pActor.AddMoveCharacterLogic(m_pAnimatorLogic);
            m_pActor.AddRotateCharacterLogic(m_pAnimatorLogic);

            m_pActor.AddMoveCharacterLogic(this);
        }

        protected virtual void Start()
        {

        }


        protected virtual void Update()
        {
            m_pActor.OnUpdate();
        }



        private void FixedUpdate()
        {
            m_pActor.FixedUpdate();
        }

        private void OnDrawGizmosSelected()
        {
            if (m_pActor != null)
            {
                m_pActor.OnDrawGizmosSelected();
            }
        }

        #region ActorLogic



        public void SetTargetPosition(Vector3 targetPosition)
        {
            m_pActor.SetTargetPosition(targetPosition);
        }

        public void SetTargetPosition(Vector3 targetPosition, bool append)
        {
            if (append)
            {
                SetTargetPosition(transform.position + targetPosition);
            }
            else
            {
                SetTargetPosition(targetPosition);
            }
        }

        public void SetTransformPosition(Vector3 targetPosition)
        {
            m_pActor.SetTransformPosition(targetPosition);
        }

        public void SetCurPosition(Vector3 targetPosition)
        {
            m_pActor.SetCurPosition(targetPosition);
        }

        public void SetCurPosition(Vector3 targetPosition, bool append)
        {
            if (append)
            {
                SetCurPosition(transform.position + targetPosition);
            }
            else
            {
                SetCurPosition(targetPosition);
            }
        }

        public void SetTargetRotation(Quaternion quaternion)
        {
            m_pActor.SetTargetRotation(quaternion);
        }

        public void SetTargetRotation(Vector3 eulerAngle)
        {
            m_pActor.SetTargetRotation(Quaternion.Euler(eulerAngle));
        }

        public void SetCurRotation(Quaternion quaternion)
        {
            m_pActor.SetCurRotation(quaternion);
        }

        public void SetCurRotation(Vector3 eulerAngle)
        {
            m_pActor.SetCurRotation(Quaternion.Euler(eulerAngle));
        }

        public void SetTransformRotation(Vector3 eulerAngle)
        {
            m_pActor.SetTransformRotation(Quaternion.Euler(eulerAngle));
        }

        public void SetTransformRotation(Quaternion quaternion)
        {
            m_pActor.SetTransformRotation(quaternion);
        }

        public void SetIsMoveToTargetPosition(bool isMoveTargetPosition)
        {
            m_pActor.SetIsMoveToTargetPosition(isMoveTargetPosition);
        }

        public void SetIsRotateToTargetRotation(bool isRotate)
        {
            m_pActor.SetIsRotateToTargetRotation(isRotate);
        }

        //public void SetIsUpdateCurrentPos(bool flag)
        //{
        //    m_pActor.SetIsUpdateCurrentPos(flag);
        //}

        #endregion

        #region AnimatorLogic

        public void SetTrigger(string name)
        {
            if (m_pAnimatorLogic != null)
            {
                m_pAnimatorLogic.SetTrigger(name);
            }
        }

        public void SetFloat(string name, float value)
        {
            if (m_pAnimatorLogic != null)
                m_pAnimatorLogic.SetFloat(name, value);
        }

        public void SetInteger(string name, int value)
        {
            if (m_pAnimatorLogic != null)
                m_pAnimatorLogic.SetInteger(name, value);
        }

        public void SetBool(string name, bool value)
        {
            if (m_pAnimatorLogic != null)
                m_pAnimatorLogic.SetBool(name, value);
        }

        public virtual void OnMove(Vector3 deltaPos, float moveSpeed)
        {
            
        }
        /// <summary>
        /// 当移动到目标位置时
        /// 问题:移动到目标位置时,被挤,导致当前位置和目标位置超过阈值后,会再次进入到移动状态,导致多次触发 OnMoveTargetToPosEnd 函数,需要注意
        /// </summary>
        public virtual void OnMoveTargetToPosEnd()
        {
            
        }

        public virtual void OnFallGround()
        {
            
        }

        public virtual void OnGetOffGround()
        {
            
        }

        #endregion
    }
}
