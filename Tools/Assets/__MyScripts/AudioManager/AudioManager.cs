/*
    管理音频的加载和播放
*/
using System.Collections.Generic;
using UnityEngine;

namespace Z.Core.Audio
{
    /// <summary>
    /// 管理所有音频
    /// 在一开始加载所有的音频
    /// 区分音乐和音效播放类型，实现分开单独控制
    /// </summary>
    public class AudioManager : BaseMonoSingleClass<AudioManager>
    {
        [Header("BGM音乐音量大小")]
        [Range(0f, 1f)]
        public float m_BGMVolume = 0.5f;

        [Header("音效音量")]
        [Range(0f, 1f)]
        public float AudioEffectVolume = 1.0f;

        [Header("总音量")]
        [Range(0f, 1f)]
        public float TotalVolume = 1.0f;

        private List<AudioSource> m_MusicSources = new List<AudioSource>();
        private List<AudioSource> m_EffectSources = new List<AudioSource>();
        // 存储音频片段的最后播放时间，用于限制播放频率
        private Dictionary<AudioClip, float> _audioLastPlayTime = new Dictionary<AudioClip, float>();

        [Header("音效")]
        public AudioClip SpawnGiftAduio;
        public AudioClip ClickGiftAduio;
        public AudioClip PickItemAduio;
        public AudioClip DropItemAduio;
        public AudioClip ClickBalloonAduio;
        public AudioClip PickCoinAduio;
        public AudioClip DragRopeAudio;

        [Header("图鉴UI")]
        public AudioClip OpenCollectionAudio;
        public AudioClip ClickBalloonPageAduio;
        public AudioClip ClickButtonAduio;
        public AudioClip ClickBuyBtnAduio;

        protected override void Awake()
        {
            base.Awake();
            _audioLastPlayTime = new Dictionary<AudioClip, float>();
        }
        private void Start()
        {
            PlayBackgroundMusic();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            m_MusicSources.Clear();
            m_MusicSources = null;
            m_EffectSources.Clear();
            m_EffectSources = null;
            _audioLastPlayTime.Clear();
            _audioLastPlayTime = null;
            mInstance = null;
        }

        /// <summary>
        /// 设置总音量
        /// </summary>
        public void SetTotalVolumeValue(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (TotalVolume == volume)
            {
                return;
            }

            TotalVolume = volume;
            UpdateAllVolumes();
        }

        /// <summary>
        /// 设置BGM音量
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (m_BGMVolume == volume)
            {
                return;
            }

            m_BGMVolume = volume;
            UpdateMusicVolumes();
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        public void SetAudioEffectVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (AudioEffectVolume == volume)
            {
                return;
            }

            AudioEffectVolume = volume;
            UpdateEffectVolumes();
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        private void PlayBackgroundMusic()
        {
            var audioSource = GetMusicSource();
            audioSource.loop = true;
            audioSource.volume = m_BGMVolume * TotalVolume;
            audioSource.Play();
        }

        /// <summary>
        /// 获取一个未播放状态的音乐AudioSource
        /// </summary>
        private AudioSource GetMusicSource()
        {
            AudioSource audioSource = null;
            foreach (var item in m_MusicSources)
            {
                if (!item.isPlaying)
                {
                    audioSource = item;
                    break;
                }
            }

            if (audioSource == null)
            {
                GameObject newSourceObject = new GameObject($"MusicSource_{m_MusicSources.Count + 1}");
                newSourceObject.transform.SetParent(transform);
                audioSource = newSourceObject.AddComponent<AudioSource>();
                m_MusicSources.Add(audioSource);
            }
            return audioSource;
        }

        /// <summary>
        /// 获取一个未播放状态的音效AudioSource
        /// </summary>
        private AudioSource GetEffectSource()
        {
            AudioSource audioSource = null;
            foreach (var item in m_EffectSources)
            {
                if (!item.isPlaying)
                {
                    audioSource = item;
                    break;
                }
            }

            if (audioSource == null)
            {
                GameObject newSourceObject = new GameObject($"EffectSource_{m_EffectSources.Count + 1}");
                newSourceObject.transform.SetParent(transform);
                audioSource = newSourceObject.AddComponent<AudioSource>();
                m_EffectSources.Add(audioSource);
            }
            return audioSource;
        }

        /// <summary>
        /// 直接播放一个音效片段
        /// </summary>
        public void PlayAudioEffect(AudioClip audioClip, float delay = 0)
        {
            var audioSource = GetEffectSource();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.volume = AudioEffectVolume * TotalVolume;
            audioSource.PlayDelayed(delay);
        }

        /// <summary>
        /// 播放同一个音频，但随机音量大小和pitch
        /// </summary>
        public void PlayAudioEffectRandomized(AudioClip audioClip, float volumeMin = 0.6f, float volumeMax = 1f, float pitchMin = 0.6f, float pitchMax = 1f)
        {
            var audioSource = GetEffectSource();
            audioSource.clip = audioClip;
            audioSource.loop = false;

            // 随机设置音量
            float randomVolume = Random.Range(volumeMin, volumeMax) * AudioEffectVolume * TotalVolume;
            audioSource.volume = Mathf.Clamp(randomVolume, 0f, 1f);

            // 随机设置pitch
            audioSource.pitch = Random.Range(pitchMin, pitchMax);

            audioSource.Play();
        }

        /// <summary>
        /// 在指定位置播放一个音效片段
        /// </summary>
        public void PlayAudioEffect(AudioClip audioClip, Vector3 pos)
        {
            var audioSource = GetEffectSource();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.transform.position = pos;
            audioSource.spatialBlend = 1;
            audioSource.Play();
        }

        /// <summary>
        /// 带冷却时间的音效播放（限制播放频率）
        /// </summary>
        /// <param name="audioClip">音效片段</param>
        /// <param name="cooldown">冷却时间（秒）</param>
        /// <param name="delay">延迟播放时间（秒）</param>
        /// <param name="volumeMin">最小音量比例</param>
        /// <param name="volumeMax">最大音量比例</param>
        /// <returns>是否成功播放</returns>
        public bool PlayAudioEffectWithCooldown(AudioClip audioClip, float cooldown, float delay = 0, float volumeMin = 1f, float volumeMax = 1f)
        {
            if (audioClip == null) return false;

            float currentTime = Time.time;
            // 检查是否在冷却时间内
            if (_audioLastPlayTime.TryGetValue(audioClip, out float lastPlayTime))
            {
                if (currentTime - lastPlayTime < cooldown)
                {
                    return false; // 冷却中，不播放
                }
            }

            // 更新最后播放时间（加上延迟时间，确保冷却从实际播放时开始计算）
            _audioLastPlayTime[audioClip] = currentTime + delay;

            // 播放音效
            var audioSource = GetEffectSource();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.volume = Random.Range(volumeMin, volumeMax) * AudioEffectVolume * TotalVolume;
            audioSource.volume = Mathf.Clamp(audioSource.volume, 0f, 1f);

            if (delay > 0)
            {
                audioSource.PlayDelayed(delay);
            }
            else
            {
                audioSource.Play();
            }

            return true;
        }

        /// <summary>
        /// 更新所有音乐的音量
        /// </summary>
        private void UpdateMusicVolumes()
        {
            foreach (var source in m_MusicSources)
            {
                source.volume = m_BGMVolume * TotalVolume;
            }
        }

        /// <summary>
        /// 更新所有音效的音量
        /// </summary>
        private void UpdateEffectVolumes()
        {
            foreach (var source in m_EffectSources)
            {
                source.volume = AudioEffectVolume * TotalVolume;
            }
        }

        /// <summary>
        /// 更新所有音频源的音量
        /// </summary>
        private void UpdateAllVolumes()
        {
            UpdateMusicVolumes();
            UpdateEffectVolumes();
        }
    }

    /// <summary>
    /// 单个音频的数据格式
    /// </summary>
    [System.Serializable]
    public struct AudioInfo
    {
        public int key;
        public string audioPath;
        public AudioClip audioClip;

        public AudioInfo(int key, string audioPath, AudioClip audioClip)
        {
            this.key = key;
            this.audioPath = audioPath;
            this.audioClip = audioClip;
        }
    }
}