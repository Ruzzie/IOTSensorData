using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;

namespace Ruzzie.SensorData.Web
{   
    [Serializable]     
    public sealed class DynamicObjectDictionary : DynamicObject, IDictionary<string,object>
    {
        private readonly ConcurrentDictionary<string, dynamic> _internalMembers;

        public DynamicObjectDictionary()
        {
            _internalMembers =
                new ConcurrentDictionary<string, dynamic>(StringComparer.OrdinalIgnoreCase);
        }

        public DynamicObjectDictionary(IEnumerable<KeyValuePair<string,dynamic>> internalMembers)
        {
            _internalMembers =
                new ConcurrentDictionary<string, dynamic>(internalMembers,(StringComparer.OrdinalIgnoreCase));
        }

        private ConcurrentDictionary<string, dynamic> InternalMembers
        {
            get { return _internalMembers; }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = CreateOrAddMember(binder.Name);
            return true;
        }

        private object CreateOrAddMember(string memberName)
        {
            memberName = memberName.ToLowerInvariant();

            object resultVal;
            bool exists = InternalMembers.TryGetValue(memberName, out resultVal);
            //Auto create nested non initialized properties
            if (!exists)
            {
                resultVal = new DynamicObjectDictionary();
                InternalMembers.GetOrAdd(memberName, resultVal);
            }
            return resultVal;
        }

        private void CreateOrAddMember(string memberName, dynamic value)
        {
            memberName = memberName.ToLowerInvariant();

            object resultVal;
            bool exists = InternalMembers.TryGetValue(memberName, out resultVal);
            //Auto create nested non initialized properties
            if (!exists)
            {
                InternalMembers.GetOrAdd(memberName, value);
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return InternalMembers.TryUpdate(binder.Name, value, CreateOrAddMember(binder.Name));
        }

        /// <summary>
        ///     Set a property via the index. Only indexes[0] as a string will be used.
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="indexes">Only [0] will be used as string.</param>
        /// <param name="value"></param>
        /// <returns>true if successful; otherwise false.</returns>
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            return InternalMembers.TryUpdate((string) indexes[0], value, CreateOrAddMember((string) indexes[0]));
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = CreateOrAddMember((string) indexes[0]);
            return true;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return InternalMembers.Keys;
        }

        public static implicit operator DynamicObjectDictionary(ExpandoObject expandoObject)
        {
            return new DynamicObjectDictionary(expandoObject);            
        }

        public static DynamicObjectDictionary FromExpandoObject(ExpandoObject expandoObject)
        {
            return expandoObject;
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return InternalMembers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InternalMembers.GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            CreateOrAddMember(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            CreateOrAddMember(item.Key, item.Value);
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            InternalMembers.Clear();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return InternalMembers.ContainsKey(item.Key);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            (InternalMembers as ICollection<KeyValuePair<string, object>>).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            return (InternalMembers as ICollection<KeyValuePair<string, object>>).Remove(item);
        }

        int ICollection<KeyValuePair<string, object>>.Count
        {
            get { return InternalMembers.Count; }
        }

        public int MemberCount
        {
            get { return InternalMembers.Count; }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get { return (InternalMembers as ICollection<KeyValuePair<string, dynamic>>).IsReadOnly; }
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            return InternalMembers.ContainsKey(key);
        }

        void IDictionary<string, object>.Add(string key, object value)
        {
            CreateOrAddMember(key, value);
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            dynamic value;
            return InternalMembers.TryRemove(key, out value);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            return InternalMembers.TryGetValue(key, out value);
        }

        object IDictionary<string, object>.this[string key]
        {
            get { return InternalMembers[key]; }
            set { InternalMembers[key] = value; }
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get { return InternalMembers.Keys; }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get { return InternalMembers.Values; }
        }
    }

    

    
}