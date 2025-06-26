/*
	newwer
    管理音频的加载和播放
*/
using System.Collections.Generic;
using UnityEngine;

namespace Asset.Core
{
    /// <summary>
    /// 管理所有音频
    /// 在一开始加载所有的音频,
    /// todo:在某些需要播放时额外设置audiosource参数的时候,需要使用scriptableobject进行配置
    /// </summary>
	public class AudioManager : BaseMonoSingleClass<AudioManager>
    {



        [Header("BGM初始化时,音量大小")]
        public float m_BGMvInitVolume = 0.5f;

        public float AudioEffectVolume = 1.0f;

        public List<AudioSource> m_AudioSources;

        public AudioClip SpawnGiftAduio;
        public AudioClip ClickGiftAduio;
        public AudioClip PickItemAduio;
        public AudioClip DropItemAduio;
        public AudioClip ClickBalloonAduio;

        [Header("图鉴UI")]
        public AudioClip OpenCollectionAudio;
        public AudioClip ClickBalloonPageAduio;
        public AudioClip ClickButtonAduio;
        public AudioClip ClickBuyBtnAduio;





        protected override void Awake()
        {
            base.Awake();


            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
            PlayBackgroundMusic();
        }

        public void Dispose()
        {
            m_AudioSources.Clear();
            m_AudioSources = null;
            mInstance = null;
        }

        private void OnDestroy()
        {
            Dispose();
        }


        /// <summary>
        /// 播放背景音乐
        /// </summary>
        private void PlayBackgroundMusic()
        {
            //获取audiosource组件
            var audioSource = GetAudioSource();

            audioSource.loop = true;
            audioSource.volume *= m_BGMvInitVolume;
            audioSource.Play();

        }


        /// <summary>
        /// AudioSource组件管理
        /// 获取一个未播放状态的AudioSource
        /// </summary>
        /// <returns></returns>
        public AudioSource GetAudioSource()
        {
            AudioSource audioSource = null;
            if (m_AudioSources != null)
            {
                foreach (var item in m_AudioSources)
                {
                    if (item.isPlaying == false)
                    {
                        audioSource = item;//如果有空闲的 AudioSource 组件,就取
                        break;
                    }
                }
            }


            //所有 AudioSource 都播放的情况下,新增一个 AudioSource 组件
            if (audioSource == null)
            {
                GameObject newSourceObject = new GameObject($"AudioSource_{m_AudioSources.Count + 1}");
                newSourceObject.transform.SetParent(transform);

                audioSource = newSourceObject.AddComponent<AudioSource>();
                if (m_AudioSources == null)
                {
                    m_AudioSources = new List<AudioSource>();
                }
                m_AudioSources.Add(audioSource);
            }
            return audioSource;
        }


        /// <summary>
        /// 直接播放一个音频片段,调用者无需处理AudioSource的组件
        /// 只需要传递进来要播放的音频即可
        /// </summary>
        public void PlayAudio(AudioClip audioClip, float deley = 0)
        {
            var audioSource = GetAudioSource();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            //audioSource.Play((ulong)(audioClip.frequency * deley));
            audioSource.PlayDelayed(deley);
        }

        public void PlayAudio(AudioClip audioClip, Vector3 pos)
        {
            var audioSource = GetAudioSource();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.transform.position = pos;//设置位置
            audioSource.spatialBlend = 1;//3D音效
            audioSource.Play();
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
