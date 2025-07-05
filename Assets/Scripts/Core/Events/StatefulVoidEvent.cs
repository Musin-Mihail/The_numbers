using System;
using UnityEngine;

namespace Core.Events
{
    [CreateAssetMenu(menuName = "Events/Stateful Void Event")]
    public class StatefulVoidEvent : ScriptableObject
    {
        private Action _action = delegate { };
        private bool _isFired;

        public void Raise()
        {
            _isFired = true;
            _action.Invoke();
        }

        public void AddListener(Action listener)
        {
            if (_isFired)
            {
                listener.Invoke();
            }
            else
            {
                _action += listener;
            }
        }

        public void RemoveListener(Action listener)
        {
            _action -= listener;
        }

        public void Reset()
        {
            _isFired = false;
        }
    }
}