using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ruzzie.SensorData.Web.PushData;

namespace Ruzzie.SensorData.Web.Cache
{
    public class WriteThroughCacheLocal : IWriteThroughCache
    {
        private readonly IUpdateSensorDocumentMessageChannel _updateSensorDocumentMessageChannel;
        private static readonly ConcurrentDictionary<string, SensorItemDataDocument> LatestEntryCache;
        private static readonly TimeSpan DefaultCacheDurationForLatestItems = new TimeSpan(0, 4, 0, 0, 0);
        private static readonly TimeSpan DefaultPruneInterval = new TimeSpan(0, 0, 0, 5, 0);
        private static DateTime _lastPruneDateTime = DateTime.Now;
        private static PruneJobStatus _pruneJobStatus = PruneJobStatus.Idle;
        private static readonly ReaderWriterLockSlim PruneTaskLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);


        static WriteThroughCacheLocal()
        {
            //create level one cache 
            LatestEntryCache = new ConcurrentDictionary<string, SensorItemDataDocument>(StringComparer.InvariantCultureIgnoreCase);
        }

        public WriteThroughCacheLocal()
        {
            
        }

        public WriteThroughCacheLocal(IUpdateSensorDocumentMessageChannel updateSensorDocumentMessageChannel)
        {
            _updateSensorDocumentMessageChannel = updateSensorDocumentMessageChannel;
            updateSensorDocumentMessageChannel.Subscribe(LatestThingIsUpdatedNotification);
        }

        private void LatestThingIsUpdatedNotification(string message)
        {
            SensorItemDataDocument document;
            LatestEntryCache.TryRemove(message, out document);
        }

        public async Task Update(SensorItemDataDocument dataDocument)
        {
            if (dataDocument == null)
            {
                return;
            }

            await Task.Run(() =>
            {
                LatestEntryCache.AddOrUpdate(dataDocument.ThingName, dataDocument, (key, oldValue) =>
                {
                    if (dataDocument.Created > oldValue.Created)
                    {
                        return dataDocument;
                    }
                    return oldValue;
                });
                
            });
        }

        public async Task<SensorItemDataDocument> GetLatest(string thingName)
        {
            return await Task.Run(() =>
            {
                SensorItemDataDocument document;
                if (LatestEntryCache.TryGetValue(thingName, out document))
                {
                    return document;
                }
                return null;
            });
        }

        public async Task<int> PruneOldestItemCacheForItemsOlderThan(TimeSpan age)
        {
            ParallelQuery<string> itemsToPrune =
                LatestEntryCache.AsParallel().Where(item => item.Value.Created < DateTime.Now.Subtract(age)).Select(item => item.Key);

            return await RemoveItems(itemsToPrune);
        }

        public void ResetLatestEntryCache()
        {
            LatestEntryCache.Clear();
        }


        private async Task<int> RemoveItems(ParallelQuery<string> itemsToPrune)
        {
            return await Task.Run(() =>
            {
                return itemsToPrune.Count(key =>
                {
                    SensorItemDataDocument document;
                    return LatestEntryCache.TryRemove(key, out document);
                });
            });
        }


        public void PruneCache()
        {
            Task.Run(() =>
            {
                bool locked;
                locked = PruneTaskLock.TryEnterWriteLock(1);
                Task<int> pruneOldestItemCacheForItemsOlderThan = null;
                try
                {
                    if (!locked)
                    {
                        return;
                    }

                    if (_lastPruneDateTime <= DateTime.Now.Subtract(DefaultPruneInterval))
                    {
                        if (_pruneJobStatus == PruneJobStatus.Idle)
                        {
                            _pruneJobStatus = PruneJobStatus.Pruning;
                            _lastPruneDateTime = DateTime.Now;
                            pruneOldestItemCacheForItemsOlderThan =
                                PruneOldestItemCacheForItemsOlderThan(DefaultCacheDurationForLatestItems);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
                finally
                {
                    if (locked)
                    {
                        _pruneJobStatus = PruneJobStatus.Idle;
                        PruneTaskLock.ExitWriteLock();
                    }
                    if (pruneOldestItemCacheForItemsOlderThan != null)
                    {
                        pruneOldestItemCacheForItemsOlderThan.Wait(DefaultPruneInterval);
                    }
                }
            });
        }
    }

    public enum PruneJobStatus
    {
        Idle,
        Unavailable,
        Pruning
    }
}