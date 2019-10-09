using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TweetMonitorLib.Interfaces;

namespace TweetMonitorLibTests
{
    [TestClass]
    public class PolicyValidatorTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Exception must be thrown if policyIdentifier is null")]
        public void CannotConstructPolicyWithNullPolicyIdentifier()
        {
            Tweet tweet = new Tweet("userhandle", 0, "tweetext");
            PolicyValidation validation = new PolicyValidation(null, tweet, true, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "Exception must be thrown if policyIdentifier is empty")]
        public void CannotConstructPolicyWithEmptyPolicyIdentifier()
        {
            Tweet tweet = new Tweet("userhandle", 0, "tweetext");
            PolicyValidation validation = new PolicyValidation(string.Empty, tweet, true, string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException), "Exception must be thrown if tweetdata is null")]
        public void CannotConstructPolicyWithNullTweetData()
        {
            PolicyValidation validation = new PolicyValidation("aPolicy", null, true, string.Empty);
        }
    }
}
