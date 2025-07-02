using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public static class ServiceProvider
    {
        private static readonly Dictionary<Type, object> Services = new();

        public static void Register<T>(T service)
        {
            var type = typeof(T);
            if (Services.TryAdd(type, service)) return;
            Debug.LogWarning($"[ServiceProvider] Сервис типа {type.Name} уже зарегистрирован. Предыдущий экземпляр будет перезаписан.");
            Services[type] = service;
        }

        public static T GetService<T>()
        {
            var type = typeof(T);
            if (Services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"[ServiceProvider] Сервис типа {type.Name} не зарегистрирован.");
        }

        public static void Clear()
        {
            Services.Clear();
        }
    }
}