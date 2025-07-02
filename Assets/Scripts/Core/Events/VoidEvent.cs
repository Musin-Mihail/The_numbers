using System;
using UnityEngine;

namespace Core.Events
{
    [CreateAssetMenu(menuName = "Events/Void Event")]
    public class VoidEvent : ScriptableObject
    {
        private Action _action = delegate { };

        public void Raise()
        {
            _action.Invoke();
        }

        public void AddListener(Action listener)
        {
            _action += listener;
        }

        public void RemoveListener(Action listener)
        {
            _action -= listener;
        }
    }
}