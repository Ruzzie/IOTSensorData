﻿using System;
using System.Threading;
using System.Web;
using NUnit.Framework;
using Ruzzie.SensorData.Web;

namespace Ruzzie.SensorData.UnitTests
{
    [TestFixture]
    public class WebJobTests
    {
        [Test]
        public void CallCallbackEveryInterval()
        {
            System.Web.Caching.Cache cache = HttpRuntime.Cache ?? new System.Web.Caching.Cache();

            int executeCounter = 0;
            Action actionToExecute = () => executeCounter++;
            IWebJob webJob = new WebJob(new TimeSpan(0, 0, 0, 0, 100), actionToExecute, cache);
            
            webJob.Start();
            
            Thread.Sleep(100);
            cache.Get(webJob.JobId);

            Thread.Sleep(100);
            cache.Get(webJob.JobId);

            Assert.That(executeCounter, Is.EqualTo(2));
        }
    }
}
