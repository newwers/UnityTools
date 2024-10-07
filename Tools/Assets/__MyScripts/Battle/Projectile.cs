using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z.DefenseTower
{



    public class Projectile : MonoBehaviour//这边先通过Mono挂载在投掷物上,后面可以考虑改成通过new的方式创建,然后里面存放创建后的投掷物实例对象进行控制
    {
        public enum State
        {
            None,
            Init,
            Move,
            Hit,
        }


        /// <summary>
        /// 攻击者
        /// </summary>
        public Transform Attacker { get; private set; }
        /// <summary>
        /// 目标
        /// </summary>
        public Transform Target { get; private set; }

        public bool IsUse
        {
            get
            {
                return m_State != State.None;
            }
        }

        public float speed = 5f;//单位应该是5m每秒

        private float m_StartTime;
        private float m_Distance;
        private State m_State = State.None;
        Vector3 m_HidePos = new Vector3 (9999f, 9999f, 9999f);


        public void Init(Transform attacker, Transform target)
        {
            Attacker = attacker;
            Target = target;

            transform.position = attacker.transform.position;
            gameObject.SetActive(true);

            m_Distance = Vector3.Distance(Target.position, Attacker.position);
            m_StartTime = Time.time;
            m_State = State.Init;
        }


        public void OnUpdate()
        {
            if (!Attacker || !Target || m_State == State.Hit || m_State == State.None)
            {
                return;
            }

            m_Distance = Vector3.Distance(Target.position, transform.position);//算出距离

            Vector3 direction = Target.position - transform.position; // 计算子弹朝向目标的方向
            transform.Translate(direction.normalized * speed * Time.deltaTime); // 根据速度和方向移动子弹


            if (m_Distance <= 0.1f)
            {
                // 子弹到达目标，进行击中处理
                HitTarget();
            }
            else
            {
                m_State = State.Move;
            }
        }
        //------------------------------------------------------
        private void HitTarget()
        {
            m_State = State.Hit;
            ProjectileManager.Instance.RecycleProjectile(this);
        }
        
        public void OnRecycle()
        {
            m_State = State.None;
            Attacker = null;
            Target = null;
            transform.position = m_HidePos;
        }
    }
}