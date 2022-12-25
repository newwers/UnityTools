using Greet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zdq
{


    public class DelayAction : BaseMonoSingleClass<DelayAction>
    {
        struct ActionData
        {
            public float time;
            public Action action;
        }


        List<ActionData> m_vActions = new List<ActionData>();

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(this.gameObject);
        }



        // Update is called once per frame
        void Update()
        {
            for (int i = m_vActions.Count - 1; i >= 0; i--)
            {
                ActionData data = m_vActions[i];
                data.time -= Time.unscaledDeltaTime;
                if (data.time <= 0)
                {
                    data.action?.Invoke();
                    m_vActions.Remove(data);
                }
                else
                {
                    m_vActions[i] = data;
                }
            }
        }

        private void OnDestroy()
        {
            m_vActions.Clear();
        }

        public static void AddAction(Action aciton, float time)
        {
            ActionData data = new ActionData();
            data.time = time;
            data.action = aciton;
            Instance.m_vActions.Add(data);
        }
    }
}