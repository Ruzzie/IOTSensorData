using System;
using System.Web.Caching;

namespace Ruzzie.SensorData.Web
{
    public class WebJob : IWebJob
    {
        private readonly TimeSpan _interval;
        private readonly Action _actionToExecute;
        private readonly System.Web.Caching.Cache _cache;

        public WebJob(TimeSpan interval, Action actionToExecute, System.Web.Caching.Cache cacheToUseForExpiryTimer)
        {
            _interval = interval;
            _actionToExecute = actionToExecute;
            _cache = cacheToUseForExpiryTimer;
            JobId = Guid.NewGuid().ToString();
        }

        public string JobId { get; }

        public void Start()
        {
            _cache.Add(JobId, JobId, null, System.Web.Caching.Cache.NoAbsoluteExpiration, _interval, CacheItemPriority.Normal, Callback);
        }

        private void Callback(string key, object value, CacheItemRemovedReason reason)
        {
            if (reason == CacheItemRemovedReason.Expired)
            {
                _actionToExecute.Invoke();
                Start();
            }            
        }
    }
}