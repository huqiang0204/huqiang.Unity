using System;
using System.Threading;
using UnityEngine;

namespace huqiang
{
    struct Mission
    {
        public Action<object> action;
        public object data;
    }
    class ThreadMission
    {
        public int start = 0;
        public int end = 0;
        Mission[] missions;
        Thread thread;
        AutoResetEvent are;
        bool run;
        bool pause;
        public ThreadMission()
        {
            thread = new Thread(Run);
            are = new AutoResetEvent(false);
            missions = new Mission[1024];
            run = true;
            thread.Start();
        }
        void Run()
        {
            while (run)
            {
                try
                {
                    if (start == end)
                    {
                        are.WaitOne();
                    }
                    else
                    {
                        missions[start].action(missions[start].data);
                        start++;
                        if (start >= 1024)
                            start = 0;
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.Log(ex.StackTrace);
#endif
                }
            }
        }
        public void AddMission(Action<object> action, object dat)
        {
            missions[end].action = action;
            missions[end].data = dat;
            end++;
            if (end >= 1024)
                end = 0;
            if (thread.ThreadState == ThreadState.WaitSleepJoin)
                are.Set();
        }
        public void Dispose()
        {
            run = false;
            are.Set();
            thread.Abort();
        }
    }
    public class ThreadPool
    {
        static ThreadMission[] threads;
        static int size;
        public static void Initial(int buffsize = 4)
        {
            if (threads != null)
                Dispose();
            size = buffsize;
            threads = new ThreadMission[size];
            for (int i = 0; i < buffsize; i++)
                threads[i] = new ThreadMission();
            missions = new Mission[1024];
        }
        static int point;
        public static void AddMission(Action<object> action, object dat,int index=-1)
        {
            if (threads == null)
            {
                return;
            }
            if(index<0|index>=size)
            {
                threads[point].AddMission(action, dat);
                point++;
                if (point >= size)
                    point = 0;
            }
            else
            {
                threads[index].AddMission(action, dat);
            }
          
        }
        static Mission[] missions;
        static int start, end;
        public static void InvokeToMain(Action<object> action, object dat)
        {
            missions[end].action = action;
            missions[end].data = dat;
            end++;
            if (end >= 1024)
                end = 0;
        }
        public static void ExtcuteMain()
        {
            for (int i = 0; i < 1024; i++)
            {
                try
                {
                    if (start == end)
                    {
                        break;
                    }
                    else
                    {
                        missions[start].action(missions[start].data);
                        start++;
                        if (start >= 1024)
                            start = 0;
                    }
                }
                catch (Exception ex)
                {
                    start++;
                    if (start >= 1024)
                        start = 0;
                }
            }
        }
        public static void Dispose()
        {
            if (threads == null)
                return;
            for (int i = 0; i < threads.Length; i++)
                threads[i].Dispose();
        }
    }
}