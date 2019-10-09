using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using TweetMonitorLib.AlertGenerators;
using TweetMonitorLib.Interfaces;

namespace TweetMonitorLibTests
{
    [TestClass]
    public class PolicyViolationAlertGeneratorTests
    {
        [TestMethod]
        public void ViolationWithinTimeWindowIsReported()
        {
            PolicyViolationAlertGenerator alertGenerator = new PolicyViolationAlertGenerator();
            alertGenerator.MonitoredTimeWindow = TimeSpan.FromMinutes(5);
            alertGenerator.ViolationCountThreshold = 3;

            Tweet tweet1 = new Tweet("darth_coder", TimestampFromDateString("Wed Oct 09 05:01:00 PDT 2019"), "chocolate");
            Tweet tweet2 = new Tweet("darth_coder", TimestampFromDateString("Wed Oct 09 05:02:00 PDT 2019"), "chocolate");
            Tweet tweet3 = new Tweet("darth_coder", TimestampFromDateString("Wed Oct 09 05:06:00 PDT 2019"), "chocolate");

            PolicyValidation validation1 = new PolicyValidation("TestPolicy", tweet1, true, "chocolate");
            PolicyValidation validation2 = new PolicyValidation("TestPolicy", tweet2, true, "chocolate");
            PolicyValidation validation3 = new PolicyValidation("TestPolicy", tweet3, true, "chocolate");

            AlertCallback alertCallback = new AlertCallback();
            alertCallback.Reset();

            alertGenerator.RecordPolicyViolations(validation1, alertCallback.RaiseAlert);
            Assert.IsFalse(alertCallback.AlertGenerated, "Alert is not generated");
            Assert.IsNull(alertCallback.ValidationReportedOnAlert, "Validation is expected to be null");

            alertGenerator.RecordPolicyViolations(validation2, alertCallback.RaiseAlert);
            Assert.IsFalse(alertCallback.AlertGenerated, "Alert is not generated");
            Assert.IsNull(alertCallback.ValidationReportedOnAlert, "Validation is expected to be null");

            alertGenerator.RecordPolicyViolations(validation3, alertCallback.RaiseAlert);
            Assert.IsTrue(alertCallback.AlertGenerated, "Alert must be generated");
            Assert.IsTrue(alertCallback.ValidationReportedOnAlert == validation3, "Validation is expected to set");
        }

        [TestMethod]
        public void ViolationOutsideTimeWindowIsIgnored()
        {
            PolicyViolationAlertGenerator alertGenerator = new PolicyViolationAlertGenerator();
            alertGenerator.MonitoredTimeWindow = TimeSpan.FromMinutes(5);
            alertGenerator.ViolationCountThreshold = 3;

            Tweet tweet1 = new Tweet("darth_coder", TimestampFromDateString("Wed Oct 09 05:01:00 PDT 2019"), "chocolate");
            Tweet tweet2 = new Tweet("darth_coder", TimestampFromDateString("Wed Oct 09 05:02:00 PDT 2019"), "chocolate");
            Tweet tweet3 = new Tweet("darth_coder", TimestampFromDateString("Wed Oct 09 05:06:01 PDT 2019"), "chocolate");

            PolicyValidation validation1 = new PolicyValidation("TestPolicy", tweet1, true, "chocolate");
            PolicyValidation validation2 = new PolicyValidation("TestPolicy", tweet2, true, "chocolate");
            PolicyValidation validation3 = new PolicyValidation("TestPolicy", tweet3, true, "chocolate");

            AlertCallback alertCallback = new AlertCallback();
            alertCallback.Reset();

            alertGenerator.RecordPolicyViolations(validation1, alertCallback.RaiseAlert);
            Assert.IsFalse(alertCallback.AlertGenerated, "Alert is not generated");
            Assert.IsNull(alertCallback.ValidationReportedOnAlert, "Validation is expected to be null");

            alertGenerator.RecordPolicyViolations(validation2, alertCallback.RaiseAlert);
            Assert.IsFalse(alertCallback.AlertGenerated, "Alert is not generated");
            Assert.IsNull(alertCallback.ValidationReportedOnAlert, "Validation is expected to be null");

            alertGenerator.RecordPolicyViolations(validation3, alertCallback.RaiseAlert);
            Assert.IsFalse(alertCallback.AlertGenerated, "Alert is not generated");
            Assert.IsNull(alertCallback.ValidationReportedOnAlert, "Validation is expected to be null");
        }

        [TestMethod]
        public void TimeWindowSlides()
        {
            PolicyViolationAlertGenerator alertGenerator = new PolicyViolationAlertGenerator();
            alertGenerator.MonitoredTimeWindow = TimeSpan.FromMinutes(5);
            alertGenerator.ViolationCountThreshold = 3;

            Tweet tweet1 = new Tweet("darth_coder", TimestampFromDateString("Wed Oct 09 05:01:00 PDT 2019"), "chocolate");
            Tweet tweet2 = new Tweet("darth_coder", TimestampFromDateString("Wed Oct 09 05:05:00 PDT 2019"), "chocolate");
            Tweet tweet3 = new Tweet("darth_coder", TimestampFromDateString("Wed Oct 09 05:06:00 PDT 2019"), "chocolate");
            Tweet tweet4 = new Tweet("darth_coder", TimestampFromDateString("Wed Oct 09 05:07:00 PDT 2019"), "chocolate");

            PolicyValidation validation1 = new PolicyValidation("TestPolicy", tweet1, true, "chocolate");
            PolicyValidation validation2 = new PolicyValidation("TestPolicy", tweet2, true, "chocolate");
            PolicyValidation validation3 = new PolicyValidation("TestPolicy", tweet3, true, "chocolate");
            PolicyValidation validation4 = new PolicyValidation("TestPolicy", tweet4, true, "chocolate");

            AlertCallback alertCallback = new AlertCallback();
            alertCallback.Reset();

            alertGenerator.RecordPolicyViolations(validation1, alertCallback.RaiseAlert);
            Assert.IsFalse(alertCallback.AlertGenerated, "Alert is not generated");
            Assert.IsNull(alertCallback.ValidationReportedOnAlert, "Validation is expected to be null");

            alertGenerator.RecordPolicyViolations(validation2, alertCallback.RaiseAlert);
            Assert.IsFalse(alertCallback.AlertGenerated, "Alert is not generated");
            Assert.IsNull(alertCallback.ValidationReportedOnAlert, "Validation is expected to be null");

            alertGenerator.RecordPolicyViolations(validation3, alertCallback.RaiseAlert);
            Assert.IsTrue(alertCallback.AlertGenerated, "Alert must be generated");
            Assert.IsTrue(alertCallback.ValidationReportedOnAlert == validation3, "Validation is expected to set");

            alertGenerator.RecordPolicyViolations(validation4, alertCallback.RaiseAlert);
            Assert.IsTrue(alertCallback.AlertGenerated, "Alert must be generated");
            Assert.IsTrue(alertCallback.ValidationReportedOnAlert == validation4, "Validation is expected to set");
        }

        private long TimestampFromDateString(string dateString)
        {
            DateTimeOffset dto;
            //"Mon Apr 06 23:43:23 PDT 2009"
            dateString = dateString.Replace("PDT", "-07:00");
            bool result = DateTimeOffset.TryParseExact(dateString, "ddd MMM dd HH:mm:ss K yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dto);

            long timestamp = 0;
            if (result)
            {
                timestamp = dto.ToUnixTimeSeconds();
            }

            return timestamp;
        }

        class AlertCallback
        {
            public bool AlertGenerated { get; set; }
            public PolicyValidation ValidationReportedOnAlert { get; set; }

            public void Reset()
            {
                AlertGenerated = false;
                ValidationReportedOnAlert = null;
            }

            public void RaiseAlert(PolicyValidation validation)
            {
                this.AlertGenerated = true;
                this.ValidationReportedOnAlert = validation;
            }
        }
    }
}
