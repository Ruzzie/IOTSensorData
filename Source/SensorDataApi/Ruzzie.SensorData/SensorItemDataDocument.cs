using System;
using System.Diagnostics.CodeAnalysis;

namespace Ruzzie.SensorData
{
    [Serializable]  
    public class SensorItemDataDocument
    {
        public string ThingName { get; set; }
        public DateTime Created { get; set; }
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public string Id { get; set; }
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public DynamicObjectDictionary Content { get; set; }                
    }
}