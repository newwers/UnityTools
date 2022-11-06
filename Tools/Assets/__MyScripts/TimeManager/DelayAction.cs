using Greet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayAction : MonoBehaviour
{
    struct ActionData
    {
        public float time;
        public Action action;
    }

    static DelayAction Instance;

    List<ActionData> m_vActions = new List<ActionData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = m_vActions.Count-1; i >=0; i--)
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

    public static void AddAction(Action aciton,float time)
    {
        ActionData data = new ActionData();
        data.time = time;
        data.action = aciton;
        Instance.m_vActions.Add(data);
    }
}
