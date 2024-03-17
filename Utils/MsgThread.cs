using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CConn
{
    public class MsgThread<T> where T : new()
    {
        public bool IsRunning { get; private set; }
        private LinkedList<T> queue = new LinkedList<T>();
        private bool key = false;
        private Action<T> onMsgArrived = null;
        private readonly object syncLock = new object();
        private AutoResetEvent resetEvent = new AutoResetEvent(false);
        private Task runningTask = null;

        public MsgThread(Action<T> onMsgArrived = null)
        {
            SetOnMsgArrivedListener(onMsgArrived);
        }

        public void SetOnMsgArrivedListener(Action<T> onMsgArrived)
        {
            this.onMsgArrived = onMsgArrived;
        }

        public void Start()
        {
            if (IsRunning)
            {
                return;
            }

            runningTask = Task.Factory.StartNew(Run, TaskCreationOptions.LongRunning);
        }

        public void Stop()
        {
            if (IsRunning)
            {
                Enqueue(default);
                if (runningTask != null)
                {
                    runningTask.Wait();
                    runningTask = null;
                }
            }
        }

        public void Run()
        {
            IsRunning = true;
            while (IsRunning)
            {
                WaitForKey();

                if (!IsEmpty())                 
                {
                    var msg = PopMsg();
                    if (msg == null || msg.Equals(default(T)))
                    {
                        ClearQueue();
                        UnityEngine.Debug.Log("end");
                        break;
                    }
                    else
                    {
                        if (onMsgArrived != null)
                        {
                            onMsgArrived.Invoke(msg);
                        }
                    }
                }

                ResetKeyIfEmpty();
            }
        }

        public void Enqueue(T msg)
        {
            if (IsRunning)
            {
                lock (syncLock)
                {
                    queue.AddLast(msg);
                }

                SetKey();
            }
        }

        private void WaitForKey()
        {
            if (!key)
            {
                resetEvent.WaitOne();
            }
        }

        private void ResetKey()
        {
            key = false;
        }

        private void SetKey()
        {
            if (!key)
            {
                key = true;
                resetEvent.Set();
            }
        }

        private T PopMsg()
        {
            lock (syncLock)
            {
                var msg = queue.First.Value;
                queue.RemoveFirst();
                return msg;
            }
        }

        private void ClearQueue()
        {
            lock (syncLock)
            {
                queue.Clear();
            }
        }

        private bool IsEmpty()
        {
            lock (syncLock)
            {
                return queue.Count == 0;
            }
        }

        private void ResetKeyIfEmpty()
        {
            lock (syncLock)
            {
                if (queue.Count == 0)
                {
                    ResetKey();
                }
            }
        }
    }
}
