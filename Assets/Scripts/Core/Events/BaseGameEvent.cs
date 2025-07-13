using System;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// Абстрактный базовый класс для игровых событий с одним параметром.
    /// </summary>
    /// <typeparam name="T">Тип параметра события.</typeparam>
    public abstract class BaseGameEvent<T> : ScriptableObject
    {
        private Action<T> _action = delegate { };

        /// <summary>
        /// Вызывает событие, уведомляя всех подписчиков.
        /// </summary>
        /// <param name="item">Параметр для передачи подписчикам.</param>
        public void Raise(T item)
        {
            _action.Invoke(item);
        }

        /// <summary>
        /// Добавляет слушателя к событию.
        /// </summary>
        /// <param name="listener">Метод-слушатель.</param>
        public void AddListener(Action<T> listener)
        {
            _action += listener;
        }

        /// <summary>
        /// Удаляет слушателя из события.
        /// </summary>
        /// <param name="listener">Метод-слушатель.</param>
        public void RemoveListener(Action<T> listener)
        {
            _action -= listener;
        }
    }
}