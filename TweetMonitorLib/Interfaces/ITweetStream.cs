namespace TweetMonitorLib.Interfaces
{
    public interface ITweetStream
    {
        Tweet GetNext();
    }

    public class Tweet
    {
        public string UserHandle { get; private set; }
        public long Timestamp { get; private set; }
        public string TweetText { get; private set; }

        public Tweet(string userHandle, long timestamp, string tweetText)
        {
            this.UserHandle = userHandle;
            this.Timestamp = timestamp;
            this.TweetText = tweetText;
        }
    }
}
