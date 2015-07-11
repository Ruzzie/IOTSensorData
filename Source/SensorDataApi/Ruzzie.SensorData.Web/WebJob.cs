using System;
using System.Web.Caching;

namespace Ruzzie.SensorData.Web
{
    public class WebJob : IWebJob
    {
        private readonly TimeSpan _interval;
        private readonly Action _actionToExecute;
        private readonly System.Web.Caching.Cache _cache;
        private readonly string _jobId;

        public WebJob(TimeSpan interval, Action actionToExecute, System.Web.Caching.Cache cacheToUseForExpiryTimer)
        {
            _interval = interval;
            _actionToExecute = actionToExecute;
            _cache = cacheToUseForExpiryTimer;
            _jobId = Guid.NewGuid().ToString();
        }

        public string JobId
        {
            get { return _jobId; }
        }

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