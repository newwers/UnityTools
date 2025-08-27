/*
    管理音频的加载和播放
*/
using System.Collections.Generic;
using UnityEngine;

namespace Z.Core.Audio
{
    /// <summary>
    /// 播放模式枚举
    /// </summary>
    public enum BGMPlayMode
    {
        Random,      // 随机播放
        LoopList,    // 列表循环
        SingleLoop   // 单曲循环
    }

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

        [Header("BGM")]
        public List<AudioClip> BGMAudioClips;

        [Header("音效")]
        public AudioClip SpawnGiftAduio;
        public AudioClip PickItemAduio;
        public AudioClip DropItemAduio;
        public AudioClip ClickBalloonAduio;
        public AudioClip PickCoinAduio;
        public AudioClip DragRopeAudio;
        public AudioClip LongClickAudio;
        public AudioClip ItemPopAudio_Small;
        public AudioClip ItemPopAudio_Large;

        [Header("图鉴UI")]
        public AudioClip OpenCollectionAudio;
        public AudioClip ClickBalloonPageAduio;
        public AudioClip UnlockHideAudio;//Unlock消失音效
        public AudioClip ClickButtonAduio;
        public AudioClip ClickBuyBtnAduio;
        public AudioClip NewHideAudio;

        [Header("礼盒音效")]
        public AudioClip BoxAudio1;
        public AudioClip BoxAudio2;
        public AudioClip BoxAudio3;
        public AudioClip BoxAudio4;
        public AudioClip GiftBoxAudio;

        private List<AudioSource> m_MusicSources = new List<AudioSource>();
        private List<AudioSource> m_EffectSources = new List<AudioSource>();
        // 存储音频片段的最后播放时间，用于限制播放频率
        private Dictionary<AudioClip, float> _audioLastPlayTime = new Dictionary<AudioClip, float>();

        // 添加BGM相关变量
        private AudioSource m_BGMSource;
        private int m_CurrentBGMIndex = -1;
        private AudioClip m_LastPlayedBGM;
        // 播放模式相关变量
        private BGMPlayMode m_PlayMode = BGMPlayMode.Random;
        private bool m_IsSingleLoop = false;


        protected override void Awake()
        {
            base.Awake();
            _audioLastPlayTime = new Dictionary<AudioClip, float>();

            // 创建专用的BGM AudioSource
            GameObject bgmSourceObject = new GameObject("BGM_Source");
            bgmSourceObject.transform.SetParent(transform);
            m_BGMSource = bgmSourceObject.AddComponent<AudioSource>();
            m_BGMSource.loop = false; // 不循环，我们将手动控制播放下一首
            m_BGMSource.volume = m_BGMVolume * TotalVolume;
        }

        private void Update()
        {
            // 检查BGM是否播放完毕，如果完毕则播放下一首
            if (m_BGMSource != null && !m_BGMSource.isPlaying && m_BGMSource.time >= m_BGMSource.clip.length - 0.1f)
            {
                PlayNextBGM();
            }
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
            if (BGMAudioClips == null || BGMAudioClips.Count == 0)
            {
                Debug.LogWarning("No BGM audio clips assigned!");
                return;
            }

            PlayNextBGM();
        }

        /// <summary>
        /// 播放上一首BGM
        /// </summary>
        internal void PlayPrevBGM()
        {
            if (BGMAudioClips == null || BGMAudioClips.Count == 0)
                return;

            // 单曲循环模式直接重新播放当前歌曲
            if (m_PlayMode == BGMPlayMode.SingleLoop && m_BGMSource.clip != null)
            {
                m_BGMSource.Stop();
                m_BGMSource.Play();
                return;
            }

            // 随机模式下，随机选择一首与当前不同的BGM
            if (m_PlayMode == BGMPlayMode.Random)
            {
                if (BGMAudioClips.Count == 1)
                {
                    PlayBGMByIndex(0);
                    return;
                }

                AudioClip prevBGM;
                do
                {
                    int randomIndex = Random.Range(0, BGMAudioClips.Count);
                    prevBGM = BGMAudioClips[randomIndex];
                } while (prevBGM == m_BGMSource.clip && BGMAudioClips.Count > 1);

                PlayBGMByIndex(BGMAudioClips.IndexOf(prevBGM));
                return;
            }

            // 列表循环模式
            if (m_PlayMode == BGMPlayMode.LoopList)
            {
                int prevIndex = m_CurrentBGMIndex - 1;
                if (prevIndex < 0)
                {
                    prevIndex = BGMAudioClips.Count - 1; // 循环到列表末尾
                }
                PlayBGMByIndex(prevIndex);
            }
        }

        /// <summary>
        /// 播放下一首BGM
        /// </summary>
        public void PlayNextBGM()
        {
            if (BGMAudioClips == null || BGMAudioClips.Count == 0)
                return;

            // 单曲循环模式直接重新播放当前歌曲
            if (m_PlayMode == BGMPlayMode.SingleLoop && m_BGMSource.clip != null)
            {
                m_BGMSource.Stop();
                m_BGMSource.Play();
                return;
            }

            // 随机模式下，使用原有逻辑
            if (m_PlayMode == BGMPlayMode.Random)
            {
                // 如果只有一首BGM，直接播放它
                if (BGMAudioClips.Count == 1)
                {
                    PlayBGMByIndex(0);
                    return;
                }

                // 随机选择一首与上一首不同的BGM
                AudioClip nextBGM;
                do
                {
                    int randomIndex = Random.Range(0, BGMAudioClips.Count);
                    nextBGM = BGMAudioClips[randomIndex];
                } while (nextBGM == m_LastPlayedBGM && BGMAudioClips.Count > 1);

                PlayBGMByIndex(BGMAudioClips.IndexOf(nextBGM));
                return;
            }

            // 列表循环模式
            if (m_PlayMode == BGMPlayMode.LoopList)
            {
                int nextIndex = m_CurrentBGMIndex + 1;
                if (nextIndex >= BGMAudioClips.Count)
                {
                    nextIndex = 0; // 循环到列表开头
                }
                PlayBGMByIndex(nextIndex);
            }
        }

        /// <summary>
        /// 根据索引播放BGM
        /// </summary>
        private void PlayBGMByIndex(int index)
        {
            if (index < 0 || index >= BGMAudioClips.Count) return;

            m_CurrentBGMIndex = index;
            m_BGMSource.clip = BGMAudioClips[index];
            m_BGMSource.volume = m_BGMVolume * TotalVolume;
            m_BGMSource.Play();
            m_LastPlayedBGM = m_BGMSource.clip;
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
        public AudioSource PlayAudioEffect(AudioClip audioClip, float delay = 0)
        {
            var audioSource = GetEffectSource();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.volume = AudioEffectVolume * TotalVolume;
            audioSource.pitch = 1f;//重置pitch，防止被修改过
            audioSource.PlayDelayed(delay);
            //LogManager.Log($"PlayAudioEffect:volume:{audioSource.volume}");
            return audioSource;
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

            // 更新BGM专用源的音量
            if (m_BGMSource != null)
            {
                m_BGMSource.volume = m_BGMVolume * TotalVolume;
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

        /// <summary>
        /// 切换单曲循环模式
        /// </summary>
        internal void ToggleSingleLoopBGM()
        {
            if (m_PlayMode == BGMPlayMode.SingleLoop)
            {
                // 如果已经是单曲循环，则切换回随机模式
                m_PlayMode = BGMPlayMode.Random;
                Debug.Log("切换到随机播放模式");
            }
            else
            {
                // 否则切换到单曲循环模式
                m_PlayMode = BGMPlayMode.SingleLoop;
                Debug.Log("切换到单曲循环模式");
            }
        }

        /// <summary>
        /// 切换列表循环模式
        /// </summary>
        internal void ToggleLoopBGM()
        {
            if (m_PlayMode == BGMPlayMode.LoopList)
            {
                // 如果已经是列表循环，则切换回随机模式
                m_PlayMode = BGMPlayMode.Random;
                Debug.Log("切换到随机播放模式");
            }
            else
            {
                // 否则切换到列表循环模式
                m_PlayMode = BGMPlayMode.LoopList;
                Debug.Log("切换到列表循环模式");
            }
        }

        /// <summary>
        /// 获取当前播放模式
        /// </summary>
        public BGMPlayMode GetCurrentPlayMode()
        {
            return m_PlayMode;
        }

        /// <summary>
        /// 设置播放模式
        /// </summary>
        public void SetPlayMode(BGMPlayMode mode)
        {
            m_PlayMode = mode;

            switch (mode)
            {
                case BGMPlayMode.Random:
                    Debug.Log("切换到随机播放模式");
                    break;
                case BGMPlayMode.LoopList:
                    Debug.Log("切换到列表循环模式");
                    break;
                case BGMPlayMode.SingleLoop:
                    Debug.Log("切换到单曲循环模式");
                    break;
            }
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