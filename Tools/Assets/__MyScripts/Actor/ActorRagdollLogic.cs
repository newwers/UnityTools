using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z.Event;


namespace Z.Actor
{
    public class ActorRagdollLogic :MonoBehaviour
    {

        public Animator animator;
        public Rigidbody[] rigidbodies;
        public Rigidbody rig;
        public Collider col;
        private bool m_bRagdoll;

        public bool IsRagdoll
        {
            get
            {
                return m_bRagdoll;
            }
        }



        private void OnEnable()
        {
            EventsSystem.StartListening<bool>("SetRagdoll", SetRagdoll);
        }

        private void OnDisable()
        {
            EventsSystem.StopListening<bool>("SetRagdoll", SetRagdoll);
        }

        void Start()
        {
            m_bRagdoll = false;
            DisableRagdoll();
        }

        [ContextMenu("获得关节组件")]
        void GetRigidbody()
        {
            rigidbodies = GetComponentsInChildren<Rigidbody>();
        }

        void SetRagdoll(bool enable)
        {
            if (enable)
            {
                EnableRagdoll();
            }
            else
            {
                DisableRagdoll();
            }
        }

        [ContextMenu("开启ragdoll")]
        // 开启ragdoll
        public void EnableRagdoll()
        {
            m_bRagdoll = true;
            animator.enabled = false;
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }

            //rig.useGravity = false;
            //rig.isKinematic = false;
            //rig.detectCollisions = false;
            //col.enabled = false;
        }

        [ContextMenu("关闭ragdoll")]
        // 关闭ragdoll
        public void DisableRagdoll()
        {
            m_bRagdoll = false;
            animator.enabled = true;
            foreach (Rigidbody rb in rigidbodies)
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            //rig.useGravity = true;
            //rig.isKinematic = false;
            //rig.detectCollisions = true;
            //col.enabled = true;
        }
    }
}
