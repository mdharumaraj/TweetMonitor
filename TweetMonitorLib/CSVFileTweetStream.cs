using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TweetMonitorLib.Interfaces;

namespace TweetMonitorLib
{
    public class CSVFileTweetStream : ITweetStream
    {
        StreamReader fileReader;
        IList<Tweet> tweets;
        int i = 0;

        public CSVFileTweetStream()
        {
        }

        public void InitializeStream(string pathToTweetsFile)
        {
            fileReader = new StreamReader(pathToTweetsFile);
            tweets = new List<Tweet>();

            //Tweet tweet = GetNextInternal();
            //while (tweet != null)
            //{
            //    tweets.Add(tweet);
            //    tweet = GetNextInternal();
            //}
        }

        //public Tweet GetNext()
        //{
        //    Tweet result = null;
        //    if (i < tweets.Count)
        //    {
        //        result = tweets[i];
        //        i++;
        //    }

        //    return result;
        //}

        public Tweet GetNext()
        {
            ThrowIfNotInitialized();

            if (fileReader.EndOfStream)
            {
                return null;
            }

            string tweetAsCSV = fileReader.ReadLine();
            string formattedTime = tweetAsCSV.Substring(17, 30).Trim(new char[] {'"'});
            int index = tweetAsCSV.IndexOf(',', 59);
            string userHandle = tweetAsCSV.Substring(59, index - 59).Trim(new char[] { '"' });
            string tweetText = tweetAsCSV.Substring(index + 1).Trim(new char[] { '"' }).ToLowerInvariant();

            tweetText = tweetText.Replace(",", " ");
            tweetText = tweetText.Replace("!", " ");
            tweetText = tweetText.Replace(".", " ");
            tweetText = tweetText.Replace("(", " ");
            tweetText = tweetText.Replace(")", " ");
            tweetText = tweetText.Replace("'", " ");
            tweetText = " " + tweetText + " ";

            DateTimeOffset dto;
            //"Mon Apr 06 23:43:23 PDT 2009"
            formattedTime = formattedTime.Replace("PDT", "-07:00");
            bool result = DateTimeOffset.TryParseExact(formattedTime, "ddd MMM dd HH:mm:ss K yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dto);
            long timestamp = dto.ToUnixTimeSeconds();

            return new Tweet(userHandle, timestamp, tweetText);
        }

        private void ThrowIfNotInitialized()
        {
            if (fileReader == null)
            {
                throw new Exception("Instance is not initialized");
            }
        }
    }
}
