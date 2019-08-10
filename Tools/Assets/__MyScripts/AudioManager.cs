/*
	newwer
    管理音频的加载和播放
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Tools.FileTool;
using UnityEngine;
using UnityEngine.Networking;

namespace Asset.Core
{
    /// <summary>
    /// 管理所有音频
    /// 在一开始加载所有的音频,
    /// todo:如果音频是不常用音频,可以在后面打开某个场景的时候进行加载(未实现)
    /// </summary>
	public class AudioManager : MonoBehaviour {


        public static AudioManager Instance;

        [Header("配置所有音频路径的文件")]
        public string m_AudioConfigPath = "";
        /// <summary>
        /// 存放根据配置文件加载的所有音频
        /// </summary>
        public Dictionary<int, AudioInfo> m_AllAudio = new Dictionary<int, AudioInfo>();


        [Header("----------BGM相关设置---------")]
        [Header("BGM的路径(相对Assets)")]
        public string m_BGMAudioClipPath = "";

        [Header("背景音频的格式,好像不支持MP3格式")]
        public AudioType m_BGMAudioType = AudioType.WAV;

        [Header("BGM初始化时,音量大小")]
        public float m_BGMvInitVolume = 0.5f;

        [Header("-------------------")]
        [Space(50)]

        //当播放时,获取当前是否有audiosource组件,如果有,判断是否正在播放,如果没有,就播放,有就创建新的

        public List<AudioSource> m_AudioSources;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            StartCoroutine(LoadAudioConfig());
        }

        void Start () {
            PlayBackgroundMusic();
            DontDestroyOnLoad(this.gameObject);
        }
	
        /// <summary>
        /// 加载配置文件中所有的音频资源
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadAudioConfig()
        {
            if (string.IsNullOrEmpty(m_AudioConfigPath))
            {
                Debug.LogError("配置文件路径没填");
                yield break;
            }
            var uri = Path.Combine(Application.dataPath, m_AudioConfigPath);
            if (!FileTools.ExistFile(uri))
            {
                Debug.LogError("文件不存在:" + uri);
                yield break;
            }

            string[] audioConfig = FileTools.ReadFileLine(uri);
            foreach (var item in audioConfig)
            {
                //读取配置的格式:唯一序号 空格 文件路径
                string[] temp = item.Split(' ');
                int key = int.Parse(temp[0]);
                string value = temp[1];
                print("key:" + key + ",value:" + value);
                if (!m_AllAudio.ContainsKey(key))
                {
                    AudioClip audioClip = null;
                    var audioUri = new System.Uri(Path.Combine(Application.dataPath, value));
                    print(audioUri);

                    using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(audioUri, m_BGMAudioType))
                    {
                        yield return request.SendWebRequest();
                        if (request.isNetworkError)
                        {
                            print(request.error);
                        }
                        else
                        {
                            audioClip = DownloadHandlerAudioClip.GetContent(request);
                            AudioInfo audioInfo = new AudioInfo(key, value, audioClip);
                            m_AllAudio.Add(key, audioInfo);
                        }
                    }
                }
                else
                {
                    print("重复的key:" + key);
                }
            }
            
        }


        /// <summary>
        /// 播放背景音乐
        /// </summary>
        private void PlayBackgroundMusic()
        {
            //获取audiosource组件
            var audioSource = GetAudioSource();

            AudioClip audioClip = m_AllAudio[1].audioClip;
            audioSource.clip = audioClip;
            audioSource.loop = true;
            audioSource.volume = m_BGMvInitVolume;
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
            foreach (var item in m_AudioSources)
            {
                if (item.isPlaying == false)
                {
                    audioSource = item;//如果有空闲的 AudioSource 组件,就取
                }
            }

            //所有 AudioSource 都播放的情况下,新增一个 AudioSource 组件
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                m_AudioSources.Add(audioSource);
            }
            return audioSource;
        }


        /// <summary>
        /// 直接播放一个音频片段,调用者无需处理AudioSource的组件
        /// 只需要传递进来要播放的音频即可
        /// </summary>
        public void PlayAudio(AudioClip audioClip,ulong deley = 0)
        {
            var audioSource = GetAudioSource();
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.Play(deley);
        }

        //TODO:有一个需求,根据传递进来的音频文件名称,找到对应文件位置,进行加载文件

        public void Dispose()
        {
            m_AllAudio.Clear();
            m_AllAudio = null;
            m_AudioSources.Clear();
            m_AudioSources = null;
            Instance = null;
        }

        private void OnDestroy()
        {
            Dispose();
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
