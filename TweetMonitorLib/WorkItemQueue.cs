using System;
using System.Collections.Generic;
using System.Threading;

namespace TweetMonitorLib
{
    public class WorkItemQueue<T>
    {
        private int maximumItems;
        LinkedList<T> workItemStore;
        SemaphoreSlim empty, full;

        public WorkItemQueue(int maximumItems)
        {
            this.maximumItems = maximumItems;
            workItemStore = new LinkedList<T>();
            this.empty = new SemaphoreSlim(maximumItems, maximumItems);
            this.full = new SemaphoreSlim(0, maximumItems);
        }

        public void Enqueue(T value)
        {
            this.empty.Wait();

            lock (this)
            {
                workItemStore.AddLast(value);
            }

            this.full.Release();
        }

        public T Dequeue(TimeSpan timeout)
        {
            if (!this.full.Wait(timeout))
            {
                return default(T);
            }

            T item = default(T);

            lock (this)
            {
                if (workItemStore.First != null)
                {
                    item = workItemStore.First.Value;
                    workItemStore.RemoveFirst();
                }
            }

            this.empty.Release();

            return item;
        }
    }
}
