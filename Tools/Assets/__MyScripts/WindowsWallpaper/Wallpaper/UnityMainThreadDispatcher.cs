using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200000A RID: 10
public class UnityMainThreadDispatcher : MonoBehaviour
{
    // Token: 0x0600003F RID: 63 RVA: 0x0000310B File Offset: 0x0000130B
    public static UnityMainThreadDispatcher Instance()
    {
        if (UnityMainThreadDispatcher._instance == null)
        {
            GameObject gameObject = new GameObject("UnityMainThreadDispatcher");
            UnityMainThreadDispatcher._instance = gameObject.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(gameObject);
        }
        return UnityMainThreadDispatcher._instance;
    }

    // Token: 0x06000040 RID: 64 RVA: 0x0000313C File Offset: 0x0000133C
    public void Enqueue(Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException("action");
        }
        Queue<Action> executionQueue = UnityMainThreadDispatcher._executionQueue;
        lock (executionQueue)
        {
            UnityMainThreadDispatcher._executionQueue.Enqueue(action);
        }
    }

    // Token: 0x06000041 RID: 65 RVA: 0x00003190 File Offset: 0x00001390
    private void Update()
    {
        for (; ; )
        {
            Queue<Action> executionQueue = UnityMainThreadDispatcher._executionQueue;
            Action action;
            lock (executionQueue)
            {
                if (UnityMainThreadDispatcher._executionQueue.Count == 0)
                {
                    break;
                }
                action = UnityMainThreadDispatcher._executionQueue.Dequeue();
            }
            action();
        }
    }

    // Token: 0x04000040 RID: 64
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    // Token: 0x04000041 RID: 65
    private static UnityMainThreadDispatcher _instance;
}
