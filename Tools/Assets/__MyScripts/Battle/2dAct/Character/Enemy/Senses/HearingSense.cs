using System.Collections.Generic;
using UnityEngine;

namespace Senses
{
    public class HearingSense : BaseSense
    {
        [Header("听力系统配置")]
        [Tooltip("声音传播距离")]
        public float hearingDistance = 15f;
        [Tooltip("音量衰减系数")]
        public float volumeAttenuation = 0.1f;
        [Tooltip("障碍物隔音效果")]
        public float obstacleSoundReduction = 0.5f;
        [Tooltip("声音检测频率")]
        public float soundCheckFrequency = 0.2f;

        private List<SoundSource> detectedSounds = new List<SoundSource>();
        private float soundCheckTimer = 0f;

        public override void UpdateDetection()
        {
            if (!isEnabled)
                return;

            soundCheckTimer += Time.deltaTime;
            if (soundCheckTimer >= soundCheckFrequency)
            {
                detectedSounds.Clear();
                DetectSounds();
                soundCheckTimer = 0f;
            }
        }

        private void DetectSounds()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, hearingDistance);

            foreach (Collider2D collider in colliders)
            {
                SoundSource soundSource = collider.GetComponent<SoundSource>();
                if (soundSource != null && soundSource.isActive)
                {
                    float soundIntensity = CalculateSoundIntensity(soundSource);
                    if (soundIntensity > 0)
                    {
                        detectedSounds.Add(soundSource);
                        TriggerSenseEvent(new SenseEvent(SenseType.Hearing, soundSource.gameObject, soundSource.transform.position, soundIntensity, soundSource.soundTag));
                    }
                }
            }
        }

        private float CalculateSoundIntensity(SoundSource soundSource)
        {
            float distance = Vector3.Distance(transform.position, soundSource.transform.position);
            if (distance > hearingDistance || distance > soundSource.maxDistance)
                return 0;

            float baseIntensity = soundSource.volume * (1.0f - (distance / Mathf.Max(hearingDistance, soundSource.maxDistance)));
            float attenuatedIntensity = baseIntensity * Mathf.Pow(1.0f - volumeAttenuation, distance);

            if (IsSoundObstructed(soundSource))
            {
                attenuatedIntensity *= (1.0f - obstacleSoundReduction);
            }

            return Mathf.Clamp01(attenuatedIntensity);
        }

        private bool IsSoundObstructed(SoundSource soundSource)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, soundSource.transform.position);
            return hit.collider != null && hit.collider.gameObject != soundSource.gameObject;
        }
#if UNITY_EDITOR
        public override void DrawGizmos()
        {
            if (!isEnabled)
                return;

            Gizmos.color = new Color(0, 0.7f, 1, 0.3f);
            Gizmos.DrawWireSphere(transform.position, hearingDistance);

            Gizmos.color = Color.cyan;
            foreach (SoundSource sound in detectedSounds)
            {
                if (sound != null)
                {
                    Gizmos.DrawLine(transform.position, sound.transform.position);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            DrawGizmos();
        }
#endif

        public List<SoundSource> GetDetectedSounds()
        {
            return new List<SoundSource>(detectedSounds);
        }

        public SoundSource GetLoudestSound()
        {
            SoundSource loudestSound = null;
            float highestIntensity = 0;

            foreach (SoundSource sound in detectedSounds)
            {
                if (sound == null)
                    continue;

                float intensity = CalculateSoundIntensity(sound);
                if (intensity > highestIntensity)
                {
                    highestIntensity = intensity;
                    loudestSound = sound;
                }
            }

            return loudestSound;
        }

        public bool IsSoundDetected(string soundTag)
        {
            foreach (SoundSource sound in detectedSounds)
            {
                if (sound != null && sound.soundTag == soundTag)
                {
                    return true;
                }
            }
            return false;
        }
    }

    [System.Serializable]
    public class SoundSource : MonoBehaviour
    {
        [Tooltip("声音标签")]
        public string soundTag = "General";
        [Tooltip("音量")]
        public float volume = 1.0f;
        [Tooltip("最大传播距离")]
        public float maxDistance = 20f;
        [Tooltip("是否激活")]
        public bool isActive = true;
        [Tooltip("声音持续时间 (秒)")]
        public float duration = 0f;

        private float activeTime = 0f;

        private void Update()
        {
            if (duration > 0 && isActive)
            {
                activeTime += Time.deltaTime;
                if (activeTime >= duration)
                {
                    isActive = false;
                    activeTime = 0f;
                }
            }
        }

        public void PlaySound(float duration = 0f)
        {
            isActive = true;
            this.duration = duration;
            activeTime = 0f;
        }

        public void StopSound()
        {
            isActive = false;
            activeTime = 0f;
        }

        private void OnDrawGizmosSelected()
        {
            if (isActive)
            {
                Gizmos.color = new Color(0, 0.7f, 1, 0.2f);
                Gizmos.DrawWireSphere(transform.position, maxDistance);
            }
        }
    }
}
