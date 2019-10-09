using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TweetMonitorLib;

namespace TweetMonitorLibTests
{
    [TestClass]
    public class WorkItemQueueTests
    {
        [TestMethod]
        [Timeout(200)]
        public void DequeTimesOutWhenQueueIsEmpty()
        {
            WorkItemQueue<object> queue = new WorkItemQueue<object>(1);
            object workItem = queue.Dequeue(TimeSpan.FromMilliseconds(100));

            Assert.IsTrue(workItem == null);
        }
    }
}
