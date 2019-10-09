using TweetMonitorLib.Interfaces;

namespace TweetMonitorLib.PolicyValidators
{
    public class BannedWordsCheckAhoCorasick : IPolicyValidator
    {
        private AhoCorasickMatcher matcher;

        public string PolicyIdentifier { get; private set; }

        public BannedWordsCheckAhoCorasick(AhoCorasickMatcher matcher)
        {
            this.matcher = matcher;
            PolicyIdentifier = "BannedWordsCheck";
        }

        public PolicyValidation Validate(Tweet tweet)
        {
            string bannedWordsMatched;
            matcher.RunMatch(tweet.TweetText, out bannedWordsMatched);

            bool violationsPresent = !string.IsNullOrEmpty(bannedWordsMatched);
            PolicyValidation policyValidation = new PolicyValidation(PolicyIdentifier, tweet, violationsPresent, bannedWordsMatched);

            return policyValidation;
        }
    }
}
