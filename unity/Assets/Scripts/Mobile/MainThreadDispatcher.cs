using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * @brief Executes queued actions on Unity's main thread.
 */
public class MainThreadDispatcher : MonoBehaviour
{
    /**
     * @brief Thread-safe queue of actions to execute on the main thread.
     */
    private static readonly Queue<Action> _executionQueue = new();

    /**
     * @brief Adds an action to the execution queue to be run on the main thread.
     * @param action The Action delegate to enqueue.
     */
    public static void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    /**
     * @brief Unity event called once per frame; executes all queued actions on the main thread.
     */
    void Update()
    {
        while (_executionQueue.Count > 0)
        {
            _executionQueue.Dequeue()?.Invoke();
        }
    }
}
