using System.Collections.Generic;
using UnityEngine;

namespace Battle.Test.SkillEffect
{
    public class Enemy : MonoBehaviour, IDamageable, IEnergyHolder
    {
        public int health = 100;
        public int energy = 50;

        //public Ability[] abilities;

        List<IEffect<IDamageable>> activeEffects = new List<IEffect<IDamageable>>();

        public int CurrentEnergy => energy;

        public void TakeDamage(int amount)
        {
            health -= amount;
            if (health <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            LogManager.Log("Enemy died.");

            foreach (var effect in activeEffects)
            {
                effect.OnCompleted -= RemoveEffect;
                effect.Cancel();
            }
            activeEffects.Clear();
            Destroy(gameObject);
        }

        private void RemoveEffect(IEffect<IDamageable> effect)
        {
            effect.OnCompleted -= RemoveEffect;
            activeEffects.Remove(effect);
        }
        /// <summary>
        /// 应用效果
        /// </summary>
        /// <param name="effect"></param>
        public void ApplyEffect(IEffect<IDamageable> effect)
        {
            effect.OnCompleted += RemoveEffect;
            activeEffects.Add(effect);
            effect.Apply(this);
        }

        public void ChangeEnergy(int amount)
        {
            energy += amount;
        }

        public bool CanChangeEnergy(int amount)
        {
            return energy - amount >= 0;
        }
    }
}