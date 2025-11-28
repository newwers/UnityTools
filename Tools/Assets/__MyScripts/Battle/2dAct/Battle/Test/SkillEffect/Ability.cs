using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Test.SkillEffect
{

    /// <summary>
    /// 效果接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEffect<T>
    {
        void Apply(T target);
        void Cancel();
        event Action<IEffect<T>> OnCompleted;
    }

    #region 能量

    /// <summary>
    /// 能量改变接口
    /// 实现该接口的类可以改变能量值
    /// </summary>
    public interface IEnergyHolder
    {
        int CurrentEnergy { get; }
        void ChangeEnergy(int amount);
        bool CanChangeEnergy(int amount); // 检查是否可以改变能量
    }

    [Serializable]
    /// <summary>
    /// 能量扣除效果
    /// </summary>
    public class EnergyCostEffect : IEffect<IEnergyHolder>
    {
        public event Action<IEffect<IEnergyHolder>> OnCompleted;

        public int EnergyCost = 10;


        public void Apply(IEnergyHolder target)
        {
            if (target.CanChangeEnergy(-EnergyCost))
            {

            }
            OnCompleted?.Invoke(this);
        }

        public void Cancel()
        {
            // 直接能量扣除效果无法撤销
        }
    }

    #endregion

    #region 伤害


    /// <summary>
    /// 伤害承受接口
    /// 实现该接口的类可以受到伤害
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }


    [Serializable]
    /// <summary>
    /// 伤害效果
    /// </summary>
    public class DamageEffect : IEffect<IDamageable>
    {
        public event Action<IEffect<IDamageable>> OnCompleted;

        public int DamageAmount = 1;

        public void Apply(IDamageable target)
        {
            target.TakeDamage(DamageAmount);
            OnCompleted?.Invoke(this);
        }

        public void Cancel()
        {
            // 直接伤害效果没有持续时间,取消时不需要做任何操作
        }

    }
    [Serializable]
    /// <summary>
    /// 持续伤害效果
    /// </summary>
    public class DamageOverTimeEffect : IEffect<IDamageable>
    {
        public event Action<IEffect<IDamageable>> OnCompleted;

        public float Duration;
        public float TickInterval;
        public int DamagePerTick;

        private IntervalTimer timer;
        IDamageable currentTarget;

        public void Apply(IDamageable target)
        {
            currentTarget = target;
            timer = new IntervalTimer(Duration, TickInterval);
            timer.OnTick += OnTick;
            timer.OnTimerStop += OnTimerStop;
            timer.Start();
        }

        void OnTimerStop()
        {
            Cleanup();
        }

        void OnTick()
        {
            currentTarget.TakeDamage(DamagePerTick);
        }
        /// <summary>
        /// 取消效果
        /// </summary>
        public void Cancel()
        {
            timer?.Stop();
            Cleanup();
        }
        /// <summary>
        /// 清理
        /// </summary>
        void Cleanup()
        {
            timer = null;
            currentTarget = null;
            OnCompleted?.Invoke(this);
        }

    }
    #endregion
    /// <summary>
    /// 技能,能力
    /// </summary>
    [Serializable]
    public class Ability
    {
        [SerializeReference]//[SerializeReference] 的核心作用：保留多态的序列化信息
        public List<IEffect<IDamageable>> Effects;
        public GameObject VFXPrefab;
        public AudioClip SFXClip;

        public void Execute(IDamageable target)
        {
            foreach (var effect in Effects)
            {
                effect.Apply(target);
            }

            // Play VFX and SFX 由调用者处理特效和音频播放
            //if (VFXPrefab != null)
            //{
            //    Instantiate(VFXPrefab, target.transform.position, Quaternion.identity);
            //}
            //if (SFXClip != null)
            //{
            //    AudioSource.PlayClipAtPoint(SFXClip, target.transform.position);
            //}
        }
    }

    public class IntervalTimer
    {
        private float duration;
        private float tickInterval;
        private float elapsedTime;
        private bool isRunning;

        public IntervalTimer(float duration, float tickInterval)
        {
            this.duration = duration;
            this.tickInterval = tickInterval;
            isRunning = false;
        }

        public void Start()
        {
            isRunning = true;
            elapsedTime = 0f;
        }

        public void Stop()
        {
            isRunning = false;
        }

        public void Update(float deltaTime)
        {
            if (isRunning)
            {
                elapsedTime += deltaTime;

                if (elapsedTime >= tickInterval)
                {
                    elapsedTime -= tickInterval;
                    OnTick?.Invoke();
                }

                if (elapsedTime >= duration)
                {
                    OnTimerStop?.Invoke();
                    Stop();
                }
            }
        }

        public event Action OnTick;
        public event Action OnTimerStop;
    }
}