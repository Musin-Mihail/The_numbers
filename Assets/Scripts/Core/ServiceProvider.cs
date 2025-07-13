using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// Простой статический Service Locator для управления зависимостями в проекте.
    /// Позволяет регистрировать и получать экземпляры сервисов по их типу.
    /// </summary>
    public static class ServiceProvider
    {
        private static readonly Dictionary<Type, object> Services = new();

        /// <summary>
        /// Регистрирует экземпляр сервиса. Если сервис такого типа уже зарегистрирован, он будет перезаписан.
        /// </summary>
        /// <typeparam name="T">Тип сервиса.</typeparam>
        /// <param name="service">Экземпляр сервиса.</param>
        public static void Register<T>(T service)
        {
            var type = typeof(T);
            if (Services.TryAdd(type, service)) return;
            Debug.LogWarning($"[ServiceProvider] Сервис типа {type.Name} уже зарегистрирован. Предыдущий экземпляр будет перезаписан.");
            Services[type] = service;
        }

        /// <summary>
        /// Возвращает зарегистрированный экземпляр сервиса указанного типа.
        /// </summary>
        /// <typeparam name="T">Тип запрашиваемого сервиса.</typeparam>
        /// <returns>Экземпляр сервиса.</returns>
        /// <exception cref="InvalidOperationException">Выбрасывается, если сервис не зарегистрирован.</exception>
        public static T GetService<T>()
        {
            var type = typeof(T);
            if (Services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"[ServiceProvider] Сервис типа {type.Name} не зарегистрирован.");
        }

        /// <summary>
        /// Очищает все зарегистрированные сервисы.
        /// </summary>
        public static void Clear()
        {
            Services.Clear();
        }
    }
}