using System;
using System.Collections.Generic;
using System.Threading;
using TweetMonitorLib.Interfaces;

namespace TweetMonitorLib.API
{
    public class TweetMonitor
    {
        private readonly TimeSpan consumerTimeout = TimeSpan.FromSeconds(30);

        WorkItemQueue<Tweet> inputQueue;
        WorkItemQueue<PolicyValidation> violationsQueue;
        WorkItemQueue<PolicyValidation> alertDispatchQueue;

        Configuration monitorConfiguration;
        IPolicyValidator[] policyValidators;
        IAlertGenerator alertGenerator;
        IAlertDispatcher alertDispatcher;
        ITweetStream[] tweetStreams;

        List<Thread> policyValidatorThreads;
        List<Thread> alertGeneratorThreads;
        List<Thread> alertDispatcherThreads;
        List<Thread> producerThreads;

        // Stats
        public ulong TweetsProcessed { get; private set; }
        public TimeSpan ProcessingTime { get; private set; }

        public TweetMonitor()
        {
            policyValidatorThreads = new List<Thread>();
            alertGeneratorThreads = new List<Thread>();
            alertDispatcherThreads = new List<Thread>();
            producerThreads = new List<Thread>();

            monitorConfiguration = new Configuration();
        }

        public TweetMonitor WithPolicyValidators(IPolicyValidator[] policyValidators)
        {
            this.policyValidators = policyValidators;
            return this;
        }

        public TweetMonitor WithAlertGenerator(IAlertGenerator alertGenerator)
        {
            this.alertGenerator = alertGenerator;
            return this;
        }

        public TweetMonitor WithAlertDispatcher(IAlertDispatcher alertDispatcher)
        {
            this.alertDispatcher = alertDispatcher;
            return this;
        }

        public TweetMonitor WithTweetStreams(ITweetStream[] tweetStreams)
        {
            this.tweetStreams = tweetStreams;
            return this;
        }

        public TweetMonitor Run()
        {
            DateTime start = DateTime.Now;

            inputQueue = new WorkItemQueue<Tweet>(monitorConfiguration.MaxQueuedItemsPolicyValidator);
            violationsQueue = new WorkItemQueue<PolicyValidation>(monitorConfiguration.MaxQueuedItemsAlertGenerator);
            alertDispatchQueue = new WorkItemQueue<PolicyValidation>(monitorConfiguration.MaxQueuedItemsAlertDispatcher);

            CreatePolicyValidators();
            CreateAlertGenerators();
            CreateAlertDispatchers();
            CreateProducers();

            WaitForProducersToComplete();
            WaitForPolicyValidatorsToComplete();
            WaitForAlertGeneratorsToComplete();
            WaitForAlertDispatchersToComplete();

            DateTime end = DateTime.Now;

            this.ProcessingTime = end - start;

            return this;
        }

        private void CreatePolicyValidators()
        {
            for (int i = 0; i < monitorConfiguration.PolicyValidatorThreadCount; i++)
            {
                Thread consumer = new Thread(() => { PolicyValidatorProc(); });
                policyValidatorThreads.Add(consumer);

                consumer.Start();
            }
        }

        private void CreateAlertGenerators()
        {
            for (int i = 0; i < monitorConfiguration.AlertGeneratorThreadCount; i++)
            {
                Thread alertGenerator = new Thread(() => { AlertGeneratorProc(); });
                alertGeneratorThreads.Add(alertGenerator);

                alertGenerator.Start();
            }
        }

        private void CreateAlertDispatchers()
        {
            for (int i = 0; i < monitorConfiguration.AlertDispatcherThreadCount; i++)
            {
                Thread dispatcher = new Thread(() => { AlertDispatchProc(); });
                alertDispatcherThreads.Add(dispatcher);

                dispatcher.Start();
            }
        }

        private void CreateProducers()
        {
            for (int i = 0; i < tweetStreams.Length; i++)
            {
                Thread producerThread = new Thread((x) => { ProducerThreadProc(x); });

                producerThreads.Add(producerThread);
                producerThread.Start(tweetStreams[i]);
            }
        }

        private void WaitForProducersToComplete()
        {
            WaitForThreads(producerThreads);

            for (int i = 0; i < monitorConfiguration.PolicyValidatorThreadCount; i++)
            {
                inputQueue.Enqueue(null); //Poison message to terminate consumers
            }
        }

        private void WaitForPolicyValidatorsToComplete()
        {
            WaitForThreads(policyValidatorThreads);

            for (int i = 0; i < monitorConfiguration.AlertGeneratorThreadCount; i++)
            {
                violationsQueue.Enqueue(null); //Poison message to terminate consumers
            }
        }

        private void WaitForAlertGeneratorsToComplete()
        {
            WaitForThreads(alertGeneratorThreads);

            for (int i = 0; i < monitorConfiguration.AlertDispatcherThreadCount; i++)
            {
                alertDispatchQueue.Enqueue(null); //Poison message to terminate consumers
            }
        }

        private void WaitForAlertDispatchersToComplete()
        {
            WaitForThreads(alertDispatcherThreads);
        }

        private void WaitForThreads(List<Thread> threads)
        {
            foreach (Thread t in threads)
            {
                t.Join();
            }
        }

        private void ProducerThreadProc(object o)
        {
            ITweetStream tweetStream = (ITweetStream)o;
            Tweet tweet = tweetStream.GetNext();
            while (tweet != null)
            {
                this.TweetsProcessed++;
                inputQueue.Enqueue(tweet);
                tweet = tweetStream.GetNext();
            }
        }

        private void PolicyValidatorProc()
        {
            Tweet tweet = inputQueue.Dequeue(consumerTimeout);
            while (tweet != null)
            {
                foreach (IPolicyValidator validator in policyValidators)
                {
                    PolicyValidation validation = validator.Validate(tweet);
                    if (validation.ViolationsPresent)
                    {
                        violationsQueue.Enqueue(validation);
                    }
                }

                tweet = inputQueue.Dequeue(consumerTimeout);
            }
        }

        private void AlertGeneratorProc()
        {
            PolicyValidation policyValidation = violationsQueue.Dequeue(consumerTimeout);
            while (policyValidation != null)
            {
                alertGenerator.RecordPolicyViolations(policyValidation, (p)=>alertDispatchQueue.Enqueue(p));
                policyValidation = violationsQueue.Dequeue(consumerTimeout);
            }
        }

        private void AlertDispatchProc()
        {
            PolicyValidation policyValidation = alertDispatchQueue.Dequeue(consumerTimeout);
            while (policyValidation != null)
            {
                alertDispatcher.DispatchAlert(policyValidation);
                policyValidation = alertDispatchQueue.Dequeue(consumerTimeout);
            }
        }
    }
}
