using System;

namespace H.Tests.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class TestsExtensions
    {
        private class Consumer
        {
            #region Properties

            private string ObjectName { get; }
            private string EventName { get; }
            private Action<string> Action { get; }

            #endregion

            #region Constructors

            public Consumer(string objectName, string eventName, Action<string> action)
            {
                ObjectName = objectName;
                EventName = eventName;
                Action = action;
            }

            #endregion

            #region Methods

            // ReSharper disable once UnusedParameter.Local
            public void HandleEvent(object _, object args)
            {
                var objectName = string.IsNullOrWhiteSpace(ObjectName)
                    ? string.Empty
                    : $"{ObjectName}.";
                Action($"{objectName}{EventName}: {args}");
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="name">Default: Name of type.</param>
        /// <param name="action"></param>
        public static T WithEventLogging<T>(
            this T obj, 
            string? name = null,
            Action<string>? action = null)
            where T : notnull
        {
            obj = obj ?? throw new ArgumentNullException(nameof(obj));
            action ??= Console.WriteLine;

            foreach (var eventInfo in typeof(T).GetEvents())
            {
                var consumer = new Consumer(
                    name ?? typeof(T).Name, 
                    eventInfo.Name, 
                    action);
                var method = consumer.GetType().GetMethod(nameof(Consumer.HandleEvent)) ?? 
                             throw new InvalidOperationException("HandleEvent method is not found");
                var handlerType = eventInfo.EventHandlerType ?? 
                                       throw new InvalidOperationException("Event Handler Type is null");
                var @delegate = Delegate.CreateDelegate(
                    handlerType, consumer, method, true);

                eventInfo.AddEventHandler(obj, @delegate);
            }

            return obj;
        }
    }
}
