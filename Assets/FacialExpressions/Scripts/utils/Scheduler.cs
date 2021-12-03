using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace utils
{
    public class Scheduler : MonoBehaviour
    {
        private static Scheduler instance = null;
        private static void CheckSchedulerState()
        {
            if (instance == null)
            {
                instance = new GameObject("Scheduler holder").AddComponent<Scheduler>();
            }
        }



        public static void RunAsync(Action r)
        {
            new Thread(new ThreadStart(r)).Start();
        }


        public static void RunSyncRepeated(float delay, float interval, Action a)
        {
            CheckSchedulerState();
            requestedTaskQueue.Enqueue(new RequestedTask
            {
                delay = delay,
                interval = interval,
                action = a
            });
        }

        public static void RunSyncLater(float delay, Action a)
        {
            CheckSchedulerState();
            requestedTaskQueue.Enqueue(new RequestedTask
            {
                delay = delay,
                action = a
            });
        }

        public static void RunSync(Action a, bool nowIfPossible = false)
        {
            CheckSchedulerState();
            if (nowIfPossible && IsMainThread())
                a();
            else
                RunSyncLater(0, a);
        }

        public static bool IsMainThread() => mainThread.Equals(Thread.CurrentThread);



        private static readonly ConcurrentQueue<RequestedTask> requestedTaskQueue = new ConcurrentQueue<RequestedTask>();
        private static readonly List<Task> taskQueue = new List<Task>();

        private static Thread mainThread;




        public void Start()
        {
            if (instance != null && instance != this)
                Debug.LogWarning("Please instanciate only one Scheduler. It may have unintended consequences otherwise.");
            instance = this;
            mainThread = Thread.CurrentThread;
        }

        public void OnDestroy()
        {
            instance = null;
        }


        void Update()
        {
            while (requestedTaskQueue.TryDequeue(out RequestedTask t))
                taskQueue.Add(t.AsTask);

            float time = Time.time;
            List<Task> toRemove = new List<Task>();
            foreach (Task t in taskQueue)
            {
                if (t.nextTime <= time)
                {
                    try
                    {
                        t.action();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }

                    if (t.interval != null)
                        t.nextTime += t.interval.Value;
                }
            }
            taskQueue.RemoveAll(t => t.nextTime <= time);
        }
    }


    internal class RequestedTask
    {
        public float delay = 0;
        public float? interval = null;
        public Action action;

        /// <summary>
        /// Must be called on main thread
        /// </summary>
        public Task AsTask => new Task
        {
            nextTime = Time.time + delay,
            interval = interval,
            action = action
        };
    }


    internal class Task
    {
        public float nextTime;
        public float? interval;
        public Action action;
    }
}

