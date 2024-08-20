using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Z.Actor
{
    /// <summary>
    /// 角色基类
    /// 提供基础移动,旋转功能,自带过渡效果
    /// 提供移动旋转时,接口调用功能,需要监听,要调用 AddMoveCharacterLogic 和 AddRotateCharacterLogic
    /// 具有MonoBehaviour脚本,可以持有该Actor对象,并调用OnUpdate函数,在需要设置移动和旋转时,调用 SetTargetPosition 和 SetTargetRotation即可
    /// MonoBehaviour需要使用该类型,可以继承ActorAgent类
    /// </summary>
    public class Actor
    {
        public interface IMoveCharacter
        {
            public void OnMove(Vector3 deltaPos, float moveSpeed);

            public void OnMoveTargetToPosEnd();

            /// <summary>
            /// 当落到地面时
            /// </summary>
            public void OnFallGround();
            /// <summary>
            /// 当离开地面时
            /// </summary>
            public void OnGetOffGround();

        }

        public interface IRotateCharacter
        {
            public void OnRotate();

        }

        private List<IMoveCharacter> m_vMoveCharacterLogic;
        private List<IRotateCharacter> m_vRotateCharacterLogic;

        //public Vector3 currentPosition;
        public Vector3 targetPosition;
        public float moveSpeed = 1f;
        public float groundCheckDistance = 0.2f; // 高度判断距离
        public float Height = 1f; // 角色高度
        Vector3 m_HalfHeight = Vector3.zero;
        /// <summary>
        /// 当前位置过渡到目标位置开关
        /// </summary>
        public bool IsMoveToTargetPosition = false;
        public bool IsMoveFlag = false;

        public bool IsUseGravity = true;
        public bool IsDragging = false;

        /// <summary>
        /// 移动距离阈值
        /// </summary>
        public float moveDistanceThreshold = 0.05f;

        //public Quaternion currentRotation;
        public Quaternion targetRotation;
        public float rotateSpeed = 5f;
        /// <summary>
        /// 当前位置过渡到目标位置开关
        /// </summary>
        public bool IsRotateToTargetRotation = false;

        /// <summary>
        /// 旋转角度阈值
        /// </summary>
        public float angleThreshold = 1f;

        private Transform transform;

        private Transform m_GroundCheckTransform;
        private LayerMask m_GroundLayer;

        private bool m_IsGrounded;

        public Transform GroundCheckTransform { get => m_GroundCheckTransform; set => m_GroundCheckTransform = value; }
        public LayerMask GroundLayer { get => m_GroundLayer; set => m_GroundLayer = value; }
        public bool IsGrounded { 
            get => m_IsGrounded; 
            set { 
                if (m_IsGrounded == value) return;
                m_IsGrounded = value;
                Debug.Log("set m_IsGrounded:" + m_IsGrounded);
                if (m_IsGrounded)
                {
                    for (int i = 0; i < m_vMoveCharacterLogic.Count; i++)
                    {
                        m_vMoveCharacterLogic[i].OnFallGround();
                    }
                }
                else
                {
                    for (int i = 0; i < m_vMoveCharacterLogic.Count; i++)
                    {
                        m_vMoveCharacterLogic[i].OnGetOffGround();
                    }
                }
            }}

        public bool isKinematic { get; internal set; }

        public Actor(Transform tra, Collider collider)
        {
            transform = tra;
            m_vMoveCharacterLogic = new List<IMoveCharacter>();
            m_vRotateCharacterLogic = new List<IRotateCharacter>();
            GroundLayer = LayerMask.NameToLayer("Ground");
            m_GroundCheckTransform = transform;


            if (collider)
            {
                Height = collider.bounds.size.y;
            }
            m_HalfHeight = new Vector3(0, Height / 2f, 0);
        }


        ~Actor()
        {
            //析构函数通常用于释放非托管资源，如文件句柄、数据库连接、网络连接等
            //C#中的析构函数并不是必需的，因为C#有垃圾回收器来管理内存，并且会自动释放托管资源
        }


        public void OnUpdate()
        {
            MoveCharacter();
            RotateCharacter();

        }

        public void FixedUpdate()
        {
            if (!GroundCheckTransform)
            {
                return;
            }

            if (IsDragging || IsUseGravity)//使用重力,拖拽中,并且有检测地面才检测
            {
                
                //地面检测,用角色中心坐标进行发射射线检测是否和 GroundLayer 有碰撞
                RaycastHit[] hits = Physics.RaycastAll(m_GroundCheckTransform.position + m_HalfHeight * transform.localScale.x, Vector3.down, groundCheckDistance* transform.localScale.x, GroundLayer);

                bool result = false;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != null && !hit.collider.isTrigger)
                    {
                        result = true;
                        break;
                    }
                }

                IsGrounded = result;

            }
        }


        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            if (GroundCheckTransform)
            {
                Gizmos.DrawLine(m_GroundCheckTransform.position + m_HalfHeight, m_GroundCheckTransform.position + m_HalfHeight + Vector3.down * groundCheckDistance);
            }
        }


        public void AddMoveCharacterLogic(IMoveCharacter moveCharacter)
        {
            m_vMoveCharacterLogic.Add(moveCharacter);
        }

        public void AddRotateCharacterLogic(IRotateCharacter  rotateCharacter)
        {
            m_vRotateCharacterLogic.Add(rotateCharacter);
        }


        void MoveCharacter()
        {
            if (IsMoveToTargetPosition == false)//不进行过渡位置
            {
                return;
            }
            if (IsUseGravity)//使用重力情况下,目标位置Y更新为当前位置
            {
                targetPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
            }

            if (Vector3.Distance(transform.position, targetPosition) <= moveDistanceThreshold)
            {
                if (IsMoveFlag)
                {
                    for (int i = 0; i < m_vMoveCharacterLogic.Count; i++)
                    {
                        m_vMoveCharacterLogic[i].OnMoveTargetToPosEnd();
                    }
                    IsMoveFlag = false;
                }
                

                return;
            }

            Vector3 currentPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);//Vector3.MoveTowards 每一帧移动 moveSpeed * Time.deltaTime 的长度
            Vector3 deltaPos = currentPosition - transform.position;

            if (Vector3.Distance(currentPosition, targetPosition) < moveDistanceThreshold)
            {
                currentPosition = targetPosition;
            }

            transform.position = currentPosition;

            for (int i = 0; i < m_vMoveCharacterLogic.Count; i++)
            {
                m_vMoveCharacterLogic[i].OnMove(deltaPos, moveSpeed);
                IsMoveFlag = true;
            }

        }

        void RotateCharacter()
        {
            if (IsRotateToTargetRotation == false)
            {
                return;
            }


            if (Quaternion.Angle(transform.rotation, targetRotation) <= angleThreshold)
            {
                return;
            }
            Quaternion currentRotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            if (Quaternion.Angle(currentRotation, targetRotation) < angleThreshold)
            {
                currentRotation = targetRotation;
            }

            transform.rotation = currentRotation;

            for (int i = 0; i < m_vRotateCharacterLogic.Count; i++)
            {
                m_vRotateCharacterLogic[i].OnRotate();
            }
        }

        public void SetTargetPosition(Vector3 target)
        {
            targetPosition = target;
            SetIsMoveToTargetPosition(true);
        }

        public void SetCurPosition(Vector3 target)
        {
            //currentPosition = target;
            //targetPosition = transform.position;
            SetTransformPosition(target);
        }

        public void SetTransformPosition(Vector3 target)
        {
            //currentPosition = target;
            transform.position = target;
            targetPosition = target;
        }

        public void SetTargetRotation(Quaternion target)
        {
            targetRotation = target;
            SetIsRotateToTargetRotation(true);
            //Debug.Log("targetRotation:" + targetRotation);
        }

        public void SetCurRotation(Quaternion target)
        {
            //currentRotation = target;
            //targetRotation = transform.rotation;
            SetTransformRotation(target);
        }

        public void SetTransformRotation(Quaternion target)
        {
            //currentRotation = target;
            targetRotation = target;
            transform.rotation = target;
        }

        public void SetIsMoveToTargetPosition(bool isMoveTargetPosition)
        {
            IsMoveToTargetPosition=isMoveTargetPosition;
        }

        public void SetIsRotateToTargetRotation(bool flag)
        {
            IsRotateToTargetRotation = flag;
        }

        //public void SetIsUpdateCurrentPos(bool flag)
        //{
        //    Debug.Log("SetIsUpdateCurrentPos:" + flag);
        //    IsUpdateCurrentPos = flag;
        //}

        //public IEnumerator UpdateCurrentPos()
        //{
        //    while (IsUpdateCurrentPos)
        //    {
        //        yield return m_WaitForFixedUpdate;
        //        if (currentPosition == transform.position)
        //        {
        //            IsUpdateCurrentPos = false;
        //            Debug.Log("SetIsUpdateCurrentPos:" + false);
        //        }
        //        else
        //        {
        //            Debug.Log($"currentPosition:{currentPosition},transform.position:{transform.position}");
        //            currentPosition = transform.position;
        //        }
        //    }
        //}
    }
}
