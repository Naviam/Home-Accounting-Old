using System.Collections.Generic;

namespace Naviam.UnitTests.Mocks
{
    public class MockCacheWrapper : ICacheWrapper
    {
        public T Get<T>(string key)
        {
            return Get<T>(key, null);
        }

        public T Get<T>(string key, params int?[] id)
        {
            var res = default(T);
            return res;
        }

        public T GetAndSet<T>(string key, T val)
        {
            return GetAndSet(key, val, null);
        }

        public T GetAndSet<T>(string key, T val, params int?[] id)
        {
            var res = default(T);
            return res;
        }

        public void ProlongKey(string key)
        {
        }

        public void Set<T>(string key, T val)
        {
        }

        public void Set<T>(string key, T val, bool forceExpire, params int?[] id)
        {
        }

        public List<T> GetList<T>(string key)
        {
            return GetList<T>(key, null);
        }

        public List<T> GetList<T>(string key, params int?[] id)
        {
            return null;
        }

        public void SetList<T>(string key, List<T> val)
        {
        }

        public void SetList<T>(string key, List<T> val, params int?[] id)
        {
        }

        public void AddToList<T>(string key, T val)
        {
        }

        public void AddToList<T>(string key, T val, params int?[] id)
        {
        }

        public void UpdateList<T>(string key, T val)
        {
        }

        public void UpdateList<T>(string key, T val, params int?[] id)
        {
        }

        public void RemoveFromList<T>(string key, T val)
        {
        }

        public void RemoveFromList<T>(string key, T val, params int?[] id)
        {
        }

        public T GetFromList<T>(string key, T val)
        {
            return GetFromList(key, val, null);
        }

        public T GetFromList<T>(string key, T val, params int?[] id)
        {
            var res = default(T);
            return res;
        }
    }
}
