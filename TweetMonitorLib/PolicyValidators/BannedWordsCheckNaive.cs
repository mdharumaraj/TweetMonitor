using TweetMonitorLib.Interfaces;

namespace TweetMonitorLib.PolicyValidators
{
    public class BannedWordsCheckNaive : IPolicyValidator
    {
        private string[] keywords;

        public string PolicyIdentifier { get; private set; }

        public BannedWordsCheckNaive(string[] keywords)
        {
            this.keywords = keywords;
            PolicyIdentifier = "BannedWordsCheck";
        }

        public PolicyValidation Validate(Tweet tweet)
        {
            string bannedWordsUsed = string.Empty;

            foreach (string word in keywords)
            {
                if (tweet.TweetText.Contains(word))
                {
                    bannedWordsUsed += " " + word + " ";
                }
            }

            bool violationsPresent = !string.IsNullOrEmpty(bannedWordsUsed);
            PolicyValidation policyValidation = new PolicyValidation(PolicyIdentifier, tweet, violationsPresent, bannedWordsUsed);

            return policyValidation;
        }
    }
}
