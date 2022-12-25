/*
	newwer
    管理音频的加载和播放
*/
using Greet;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
//using Tools.FileTool;
using UnityEngine;
using UnityEngine.Networking;

namespace Asset.Core
{
    /// <summary>
    /// 管理所有音频
    /// 在一开始加载所有的音频,
    /// todo:如果音频是不常用音频,可以在后面打开某个场景的时候进行加载(未实现)
    /// </summary>
	public class AudioManager : BaseMonoSingleClass<AudioManager>
    {



        [Header("BGM初始化时,音量大小")]
        public float m_BGMvInitVolume = 0.5f;

        public List<AudioSource> m_AudioSources;

        [Header("打招呼音频")]
        public GreetAudioClips GreetAudioClips;

        GreetLogic m_pGreetLogic = null;

        public GreetLogic greetLogic
        {
            get
            {
                return m_pGreetLogic;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (m_pGreetLogic == null)
            {
                m_pGreetLogic = new GreetLogic();
                m_pGreetLogic.Awake();
            }

            DontDestroyOnLoad(this.gameObject);
        }

        void Start () {
            m_pGreetLogic?.Start();
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
            m_pGreetLogic?.OnDestroy();
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
            if(m_AudioSources != null)
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
                audioSource = gameObject.AddComponent<AudioSource>();
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
        public void PlayAudio(AudioClip audioClip,float deley = 0)
        {
            var audioSource = GetAudioSource();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.Play((ulong)(audioClip.frequency * deley));
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
