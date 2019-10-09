using System;

namespace TweetMonitorLib.Interfaces
{
    public interface IAlertGenerator
    {
        void RecordPolicyViolations(PolicyValidation validation, Action<PolicyValidation> alertDispatchCallback);
    }
}
