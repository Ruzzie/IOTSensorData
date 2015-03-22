using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;

namespace Ruzzie.SensorData.Web
{
    [Serializable]  
    public class SensorItemDataDocument
    {
        public string ThingName { get; set; }
        public DateTime Created { get; set; }
        public string Id { get; set; }
        public DynamicDictionaryObject Content { get; set; }                
    }
}