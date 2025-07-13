using System;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Простое игровое событие без параметров.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Void Event")]
    public class VoidEvent : ScriptableObject
    {
        private Action _action = delegate { };

        /// <summary>
        /// Вызывает событие.
        /// </summary>
        public void Raise()
        {
            _action.Invoke();
        }

        /// <summary>
        /// Добавляет слушателя.
        /// </summary>
        public void AddListener(Action listener)
        {
            _action += listener;
        }

        /// <summary>
        /// Удаляет слушателя.
        /// </summary>
        public void RemoveListener(Action listener)
        {
            _action -= listener;
        }
    }
}