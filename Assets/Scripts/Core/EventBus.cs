using System;
using System.Collections.Generic;

namespace Squadstorm.Core
{
    /// <summary>
    /// Типізована шина подій (Event Bus).
    /// Дозволяє системам спілкуватися без жорстких залежностей.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Підписка на подію типу T.
        /// </summary>
        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
            {
                _handlers[type] = new List<Delegate>();
            }

            if (!_handlers[type].Contains(handler))
            {
                _handlers[type].Add(handler);
            }
        }

        /// <summary>
        /// Відписка від події типу T.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var list))
            {
                list.Remove(handler);
            }
        }

        /// <summary>
        /// Публікація (виклик) події типу T. Всі підписники отримають ці дані.
        /// </summary>
        public static void Publish<T>(T evt)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var list))
            {
                // Створюємо копію списку, щоб уникнути помилок, якщо хтось відпишеться під час виклику
                var handlersCopy = new List<Delegate>(list);
                foreach (var handler in handlersCopy)
                {
                    ((Action<T>)handler)?.Invoke(evt);
                }
            }
        }

        /// <summary>
        /// Очищення всіх підписок.
        /// </summary>
        public static void ClearAll()
        {
            _handlers.Clear();
        }
    }
}
