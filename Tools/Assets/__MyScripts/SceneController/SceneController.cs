using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project001.Core
{
    public enum EScene
    {
        Main=0,
        Loading=1,
        Menu=2,
        Hall
    }
    public class SceneController : BaseMonoSingleClass<SceneController>
    {
        AsyncOperation m_pAsyncLoader = null;
        float m_pProgress = 0;

        static SceneController()
        {
            
        }
        //------------------------------------------------------
        public void Update()
        {
            if (m_pAsyncLoader != null && !m_pAsyncLoader.isDone)
            {
                SetLoadingProgress(m_pAsyncLoader.progress);
            }
        }
        //------------------------------------------------------
        public void LoadScene(EScene scene)
        {
            if (m_pAsyncLoader != null)
            {
                //todo:上一个加载还在情况

                m_pAsyncLoader = null;
            }

            m_pAsyncLoader = SceneManager.LoadSceneAsync((int)scene);
            if (m_pAsyncLoader != null)
            {
                m_pAsyncLoader.completed += M_pAsyncLoader_completed;
                m_pProgress = 0;
                ShowLoadingPanel();
            }
        }
        //------------------------------------------------------
        private void M_pAsyncLoader_completed(AsyncOperation operation)
        {
            //隐藏loading界面
            HideLoadingPanel();
            m_pAsyncLoader = null;
        }
        //------------------------------------------------------
        void ShowLoadingPanel()
        {
            SceneManager.LoadScene((int)EScene.Loading);
        }
        //------------------------------------------------------
        void HideLoadingPanel()
        {
            SceneManager.UnloadSceneAsync((int)EScene.Loading);
        }
        //------------------------------------------------------
        void SetLoadingProgress(float progress)
        {
            m_pProgress = progress;
        }
        //------------------------------------------------------
        public float GetProgress()
        {
            return m_pProgress;
        }
    }
}