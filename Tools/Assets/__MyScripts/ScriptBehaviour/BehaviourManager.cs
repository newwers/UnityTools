/*
 负责管理脚本生命周期,
 使用方式:需要实现生命周期的,就注册到这个类里面进行统一管理调用
 */
using System.Collections.Generic;

namespace zdq.Behaviour
{
    public class BehaviourManager
    {
        Dictionary<int, ScriptBehaviour> m_ScriptBehaviours;

        public BehaviourManager()
        {
            Init();
        }

        ~BehaviourManager()
        {
            OnDestroy();
        }

        void Init()
        {
            m_ScriptBehaviours = new Dictionary<int, ScriptBehaviour>();
        }

        void OnDestroy()
        {
            Clear();
        }
        /// <summary>
        /// 提供给外部调用清理的一个方法
        /// </summary>
        public void Clear()
        {
            foreach (var script in m_ScriptBehaviours)
            {
                script.Value.OnDisable();
                script.Value.OnDestroy();
            }
            m_ScriptBehaviours.Clear();
        }

        public void RegisterScript(ScriptBehaviour script)
        {
            //int key = script.GetHashCode();//通过类的对象进行获取的hashcode,每个对象的hash值都是不同的
            int key = script.GetType().GetHashCode();//通过获取类的hashcode,保证同样的类,不同对象有一样的hash值
            if (!m_ScriptBehaviours.ContainsKey(key))
            {
                m_ScriptBehaviours.Add(key, script);
                script.Awake();
                script.OnEnable();
                script.Start();
            }
            //todo:怎么处理已经注册过的情况?
            //什么情况下会重复注册?
        }

        public void RemoveScript(ScriptBehaviour script)
        {
            int key = script.GetHashCode();
            if (m_ScriptBehaviours.ContainsKey(key))
            {
                script.OnDisable();
                script.OnDestroy();
                m_ScriptBehaviours.Remove(key);
            }
        }

        public void Update(float frame)
        {
            for(int i = 0;i< m_ScriptBehaviours.Count;i++)
            {
                m_ScriptBehaviours[i].Update(frame);
            }
        }
        /// <summary>
        /// 根据类名,获取到对应脚本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="script"></param>
        /// <returns></returns>
        public T GetScript<T>(ScriptBehaviour script) where T:ScriptBehaviour
        {
            int key = script.GetType().GetHashCode();
            ScriptBehaviour scriptBehaviour = null;
            if (m_ScriptBehaviours.TryGetValue(key,out scriptBehaviour))
            {
                return scriptBehaviour as T;
            }
            return null;
        }
    }
}