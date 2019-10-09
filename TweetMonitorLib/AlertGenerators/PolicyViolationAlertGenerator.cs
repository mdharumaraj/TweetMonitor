using System;
using System.Collections.Generic;
using TweetMonitorLib.Interfaces;

namespace TweetMonitorLib.AlertGenerators
{
    public class PolicyViolationAlertGenerator : IAlertGenerator
    {
        Dictionary<string, List<PolicyValidation>> violations;

        public TimeSpan MonitoredTimeWindow { get; set; }
        public int ViolationCountThreshold { get; set; }

        public PolicyViolationAlertGenerator()
        {
            violations = new Dictionary<string, List<PolicyValidation>>();
            this.MonitoredTimeWindow = TimeSpan.FromDays(1);
            this.ViolationCountThreshold = 2;
        }

        public void RecordPolicyViolations(PolicyValidation validation, Action<PolicyValidation> alertDispatchCallback)
        {
            string userHandle = validation.TweetData.UserHandle;
            if (!violations.ContainsKey(userHandle))
            {
                violations[userHandle] = new List<PolicyValidation>();
            }

            violations[userHandle].Add(validation);

            List<PolicyValidation> policyValidations = violations[userHandle];
            GenerateAlertIfRequired(validation, alertDispatchCallback, policyValidations);
        }

        private void GenerateAlertIfRequired(PolicyValidation validation, Action<PolicyValidation> alertDispatchCallback, List<PolicyValidation> policyValidations)
        {
            long timeWindowStart = validation.TweetData.Timestamp - (long)MonitoredTimeWindow.TotalSeconds;
            int violationCount = 0;

            for (int i = 0; i < policyValidations.Count; i++)
            {
                if (policyValidations[i].TweetData.Timestamp >= timeWindowStart)
                {
                    violationCount++;
                }
            }

            if (violationCount >= ViolationCountThreshold)
            {
                alertDispatchCallback(validation);
            }
        }
    }
}
