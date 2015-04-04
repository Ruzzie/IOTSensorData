using System;

namespace Ruzzie.SensorData.Web
{
    [Serializable]  
    public class SensorItemDataDocument
    {
        public string ThingName { get; set; }
        public DateTime Created { get; set; }
        public string Id { get; set; }
        public DynamicObjectDictionary Content { get; set; }                
    }
}