using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> executionQueue = new Queue<Action>();

    private static MainThreadDispatcher instance;

    public static void Enqueue(Action action)
    {
        if (instance == null)
        {
            var go = new GameObject("MainThreadDispatcher");
            instance = go.AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }

        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }
    }

    private void Update()
    {
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                var action = executionQueue.Dequeue();
                action?.Invoke();
            }
        }
    }
}