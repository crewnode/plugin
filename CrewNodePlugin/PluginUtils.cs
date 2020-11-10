using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CrewNodePlugin
{
    public static class PluginUtils
    {
        public static void RunTask(Action action, int seconds, CancellationToken token)
        {
            if (action == null) return;
            Task.Run(async () => {
                while (!token.IsCancellationRequested)
                {
                    action();
                    await Task.Delay(TimeSpan.FromSeconds(seconds), token);
                }
            }, token);
        }
    }

    public static class ExtensionMethods
    {
        public static async Task<object> InvokeAsync(this MethodInfo @this, object obj, params object[] parameters)
        {
            dynamic awaitable = @this.Invoke(obj, parameters);
            await awaitable;
            return awaitable.GetAwaiter().GetResult();
        }
    }

    public enum DictType { AddItem, RemoveItem };

    public class DictChangedEventArgs<K, V> : EventArgs
    {
        public DictType Type { get; set; }
        public K Key { get; set; }
        public V Value { get; set; }
    }

    public class CrewNodeDictionary<K, V> : IDictionary<K, V>
    {
        public delegate void DictionaryChanged(object sender, DictChangedEventArgs<K, V> e);

        public event DictionaryChanged OnDictionaryChanged;

        private IDictionary<K, V> innerDict;

        public ICollection<K> Keys => innerDict.Keys;

        public ICollection<V> Values => innerDict.Values;

        public int Count => innerDict.Count;

        public bool IsReadOnly => innerDict.IsReadOnly;

        public V this[K key] { get => innerDict[key]; set => innerDict[key] = value; }

        public CrewNodeDictionary()
        {
            innerDict = new Dictionary<K, V>();
        }

        public void Add(K key, V value)
        {
            if (OnDictionaryChanged != null)
            {
                OnDictionaryChanged(this, new DictChangedEventArgs<K, V>() { Type = DictType.AddItem, Key = key, Value = value });
            }

            innerDict.Add(key, value);
        }

        public bool ContainsKey(K key)
        {
            return innerDict.ContainsKey(key);
        }

        public bool Remove(K key)
        {
            if (OnDictionaryChanged != null)
            {
                OnDictionaryChanged(this, new DictChangedEventArgs<K, V>() { Type = DictType.RemoveItem, Key = key });
            }

            return innerDict.Remove(key);
        }

        public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
        {
            return innerDict.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<K, V> item)
        {
            if (OnDictionaryChanged != null)
            {
                OnDictionaryChanged(this, new DictChangedEventArgs<K, V>() { Type = DictType.AddItem, Key = item.Key, Value = item.Value });
            }

            innerDict.Add(item);
        }

        public void Clear()
        {
            if (OnDictionaryChanged != null)
            {
                OnDictionaryChanged(this, new DictChangedEventArgs<K, V>() { Type = DictType.RemoveItem });
            }

            innerDict.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return innerDict.Contains(item);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            innerDict.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            if (OnDictionaryChanged != null)
            {
                OnDictionaryChanged(this, new DictChangedEventArgs<K, V>() { Type = DictType.RemoveItem, Key = item.Key, Value = item.Value });
            }

            return innerDict.Remove(item);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return innerDict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)innerDict).GetEnumerator();
        }
    }
}
