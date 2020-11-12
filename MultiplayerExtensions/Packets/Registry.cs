using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerExtensions.Packets
{
    public class Registry<T> : ICollection, IEnumerable where T : class
    {
        private T?[] _registry;

        public Registry(int size)
        {
            _registry = new T[size];
        }

        public void Register(T obj)
        {
            for (int i = 0; i < _registry.Length; i++)
            {
                if (_registry[i] == null)
                {
                    _registry[i] = obj;
                    break;
                }
            }
        }

        public void Unregister(T obj)
        {
            int index = _registry.IndexOf(obj);
            _registry[index] = null;
        }

        public bool Contains(object value)
        {
            for (int i = 0; i < _registry.Length; i++)
            {
                if (_registry[i] == value)
                {
                    return true;
                }
            }

            return false;
        }

        public int IndexOf(T value)
        {
            for (int i = 0; i < _registry.Length; i++)
            {
                if (_registry[i] == value)
                {
                    return i;
                }
            }

            throw new KeyNotFoundException();
        }

        public int Count => ((ICollection)_registry).Count;
        public object SyncRoot => _registry.SyncRoot;
        public bool IsSynchronized => _registry.IsSynchronized;
        public bool IsReadOnly => _registry.IsReadOnly;
        public bool IsFixedSize => _registry.IsFixedSize;
        public T? this[int index] { get => _registry[index]; set => _registry[index] = value; }

        public void CopyTo(Array array, int index) => _registry.CopyTo(array, index);
        public IEnumerator GetEnumerator() => _registry.GetEnumerator();
    }
}
