using System;
using UnityEngine;

namespace Core.Events
{
    public abstract class BaseGameEvent<T> : ScriptableObject
    {
        private Action<T> _action = delegate { };

        public void Raise(T item)
        {
            _action.Invoke(item);
        }

        public void AddListener(Action<T> listener)
        {
            _action += listener;
        }

        public void RemoveListener(Action<T> listener)
        {
            _action -= listener;
        }
    }
}