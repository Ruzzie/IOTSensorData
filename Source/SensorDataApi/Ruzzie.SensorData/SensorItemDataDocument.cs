using System;

namespace Ruzzie.SensorData
{
    [Serializable]  
    public class SensorItemDataDocument
    {
        public string ThingName { get; set; }
        public DateTime Created { get; set; }
        public string Id { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public DynamicObjectDictionary Content { get; set; }                
    }
}