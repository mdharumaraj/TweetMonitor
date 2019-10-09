using System;

namespace TweetMonitorLib.Interfaces
{
    public interface IPolicyValidator
    {
        string PolicyIdentifier { get; }
        PolicyValidation Validate(Tweet tweet);
    }

    public class PolicyValidation
    {
        public string PolicyIdentifier { get; private set; }
        public Tweet TweetData { get; private set; }
        public bool ViolationsPresent { get; private set; }
        public string ViolationData { get; private set; }

        public PolicyValidation(string policyIdentifier, Tweet tweetData, bool violationsPresent, string violationData)
        {
            if (string.IsNullOrEmpty(policyIdentifier))
            {
                throw new ArgumentException(policyIdentifier);
            }

            this.PolicyIdentifier = policyIdentifier;
            this.TweetData = tweetData ?? throw new ArgumentNullException(policyIdentifier);
            this.ViolationsPresent = violationsPresent;
            this.ViolationData = violationData;
        }
    }
}
