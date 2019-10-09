namespace TweetMonitorLib
{
    public class Configuration
    {
        public int PolicyValidatorThreadCount { get; set; }
        public int AlertGeneratorThreadCount { get; set; }
        public int AlertDispatcherThreadCount { get; set; }
        public int MaxQueuedItemsPolicyValidator { get; set; }
        public int MaxQueuedItemsAlertGenerator { get; set; }
        public int MaxQueuedItemsAlertDispatcher { get; set; }

        public Configuration()
        {
            PolicyValidatorThreadCount = 3; // Set to one less than the number of physical cores
            AlertGeneratorThreadCount = 1;
            AlertDispatcherThreadCount = 1;

            MaxQueuedItemsPolicyValidator = 512;
            MaxQueuedItemsAlertGenerator = 32;
            MaxQueuedItemsAlertDispatcher = 32;
        }
    }
}
