using System;
using System.IO;
using TweetMonitorLib.Interfaces;

namespace TweetMonitorLib.AlertDispatchers
{
    public class LogFileDispatcher : IAlertDispatcher, IDisposable
    {
        StreamWriter writer;

        public LogFileDispatcher()
        {
        }

        public void Initialize(string filename)
        {
            Stream stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            writer = new StreamWriter(stream);
        }

        public void DispatchAlert(PolicyValidation policyValidation)
        {
            writer.Write(
                string.Format(
                    "{0},{1},{2},{3}",
                    policyValidation.TweetData.UserHandle,
                    policyValidation.TweetData.Timestamp,
                    policyValidation.ViolationData,
                    Environment.NewLine));
            writer.Flush();
        }

        public void Dispose()
        {
            writer.Dispose();
        }
    }
}
