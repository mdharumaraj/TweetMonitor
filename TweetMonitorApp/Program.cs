using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using TweetMonitorLib;
using TweetMonitorLib.AlertDispatchers;
using TweetMonitorLib.AlertGenerators;
using TweetMonitorLib.API;
using TweetMonitorLib.Interfaces;
using TweetMonitorLib.PolicyValidators;

namespace TweetMonitorApp
{
    class Program
    {
        static string logFileForAlerts = @"C:\Projects\Exercises\TweetMonitor\Alerts\bannedwords.csv"; //Directory must exist!
        static string tweetStreamFile = @"C:\Projects\Exercises\TweetMonitor\Data\training.1600000.processed.noemoticon.csv";
        static string pathToKeywordFile = @"C:\Projects\Exercises\TweetMonitor\Data\keywords.txt";

        static void Main(string[] args)
        {
            bool useAhoCorasickMatcher = true;

            ITweetStream tweetStreams = GetTweetStream();

            IPolicyValidator policyValidator = null;
            if (useAhoCorasickMatcher)
            {
                policyValidator = GetAhoCorasickPolicyValidator();
            }
            else
            {
                policyValidator = GetNaivePolicyValidator();
            }

            IAlertGenerator alertGenerator = GetAlertGenerator();
            IAlertDispatcher alertDispatcher = GetFileSinkDispatcher();

            TweetMonitor tweetMonitor =
                new TweetMonitor()
                    .WithTweetStreams(new ITweetStream[] { tweetStreams })
                    .WithPolicyValidators(new IPolicyValidator[] { policyValidator })
                    .WithAlertGenerator(alertGenerator)
                    .WithAlertDispatcher(alertDispatcher)
                    .Run();

            Console.WriteLine("Tweets Processed:{0}", tweetMonitor.TweetsProcessed);
            Console.WriteLine("Processing Time:{0}", tweetMonitor.ProcessingTime);
            Console.WriteLine("Done");
            Console.Read();
        }

        private static IAlertDispatcher GetFileSinkDispatcher()
        {
            LogFileDispatcher alertDispatcher = new LogFileDispatcher();
            alertDispatcher.Initialize(logFileForAlerts);
            return alertDispatcher;
        }

        private static ITweetStream GetTweetStream()
        {
            Console.Write("Initializing tweetstream ...");
            CSVFileTweetStream tweetStream = new CSVFileTweetStream();
            tweetStream.InitializeStream(tweetStreamFile);
            Console.WriteLine("Done");
            return tweetStream;
        }

        private static IPolicyValidator GetNaivePolicyValidator()
        {
            Console.Write("Initializing string matcher ...");
            string[] bannedWords = GetBannedWords();
            BannedWordsCheckNaive policyValidator = new BannedWordsCheckNaive(bannedWords);
            Console.WriteLine("Done");
            return policyValidator;
        }

        private static IPolicyValidator GetAhoCorasickPolicyValidator()
        {
            Console.Write("Initializing string matcher ...");
            string[] bannedWords = GetBannedWords();
            AhoCorasickMatcher matcher = new AhoCorasickMatcher(bannedWords);

            BannedWordsCheckAhoCorasick policyValidator = new BannedWordsCheckAhoCorasick(matcher);
            Console.WriteLine("Done");
            return policyValidator;
        }

        private static IAlertGenerator GetAlertGenerator()
        {
            PolicyViolationAlertGenerator alertGenerator = new PolicyViolationAlertGenerator();
            return alertGenerator;
        }

        private static string[] GetBannedWords()
        {
            List<string> bannedWords = new List<string>();
            using (StreamReader reader = new StreamReader(pathToKeywordFile))
            {
                while (!reader.EndOfStream)
                {
                    string word = reader.ReadLine();
                    word = " " + word.ToLowerInvariant() + " ";
                    bannedWords.Add(word);
                }
            }

            return bannedWords.ToArray();
        }

        private static void RunStartupTests()
        {
            try
            {
                ValidateKeywordFile();
            }
            catch (Exception)
            {
                Console.Write("Fix errors and rerun");
            }
        }

        private static void ValidateKeywordFile()
        {
            try
            {
                string pathToKeywordFile = ConfigurationManager.AppSettings["KeywordFile"];
                using (File.Open(pathToKeywordFile, FileMode.Open, FileAccess.Read, FileShare.Read)) ;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error opening keyword file; Error:{0}", ex.ToString());
                throw;
            }
        }
    }
}
