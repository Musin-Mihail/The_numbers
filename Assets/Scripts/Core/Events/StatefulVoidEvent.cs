using System;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Событие без параметров, которое "запоминает" свой вызов.
    /// Если слушатель подписывается после вызова, он немедленно получает уведомление.
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Stateful Void Event")]
    public class StatefulVoidEvent : ScriptableObject
    {
        private Action _action = delegate { };
        private bool _isFired;

        /// <summary>
        /// Вызывает событие.
        /// </summary>
        public void Raise()
        {
            _isFired = true;
            _action.Invoke();
        }

        /// <summary>
        /// Добавляет слушателя. Если событие уже было вызвано, слушатель вызывается немедленно.
        /// </summary>
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

        /// <summary>
        /// Удаляет слушателя.
        /// </summary>
        public void RemoveListener(Action listener)
        {
            _action -= listener;
        }

        /// <summary>
        /// Сбрасывает состояние события, как будто оно еще не было вызвано.
        /// </summary>
        public void Reset()
        {
            _isFired = false;
        }
    }
}