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
            
            private string Name { get; }
            private Action<string> Action { get; }

            #endregion

            #region Constructors

            public Consumer(string name, Action<string> action)
            {
                Name = name;
                Action = action;
            }

            #endregion

            #region Methods

            // ReSharper disable once UnusedParameter.Local
            public void HandleEvent(object _, object args)
            {
                Action($"{Name}: {args}");
            }

            #endregion
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="action"></param>
        public static T WithEventLogging<T>(
            this T obj, 
            Action<string>? action = null)
            where T : notnull
        {
            obj = obj ?? throw new ArgumentNullException(nameof(obj));
            action ??= Console.WriteLine;

            foreach (var eventInfo in typeof(T).GetEvents())
            {
                var consumer = new Consumer(eventInfo.Name, action);
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
