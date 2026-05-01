using System;
using System.Collections.Generic;

namespace Squadstorm.Core
{
    /// <summary>
    /// Центральний реєстр сервісів (Service Locator).
    /// Замінює Singleton патерн для глобального доступу до менеджерів.
    /// </summary>
    public static class Services
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Реєстрація нового сервісу.
        /// </summary>
        public static void Register<T>(T service)
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                // Попередження, якщо сервіс вже зареєстрований, але ми його перезаписуємо
                UnityEngine.Debug.LogWarning($"Service {type.Name} is being overwritten.");
            }
            _services[type] = service;
        }

        /// <summary>
        /// Отримання сервісу. Викидає виключення, якщо сервіс не знайдено.
        /// </summary>
        public static T Get<T>()
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var obj))
            {
                return (T)obj;
            }
            
            throw new Exception($"Service {type.Name} is not registered in the ServiceLocator!");
        }

        /// <summary>
        /// Безпечна спроба отримати сервіс.
        /// </summary>
        public static bool TryGet<T>(out T service)
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }

            service = default;
            return false;
        }

        /// <summary>
        /// Очищення всіх сервісів (корисно при перезавантаженні сцени або гри).
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}
