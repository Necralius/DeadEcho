using System;
using System.Collections.Generic;

namespace Project.Core.Services
{
    public sealed class GameEventBus : IGameEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Publish<TEvent>(TEvent gameEvent)
        {
            Type eventType = typeof(TEvent);

            if (!_handlers.TryGetValue(eventType, out List<Delegate> handlers))
                return;

            Delegate[] handlersSnapshot = handlers.ToArray();

            foreach (Delegate handler in handlersSnapshot)
                ((Action<TEvent>)handler).Invoke(gameEvent);
        }

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            Type eventType = typeof(TEvent);

            if (!_handlers.TryGetValue(eventType, out List<Delegate> handlers))
            {
                handlers = new List<Delegate>();
                _handlers.Add(eventType, handlers);
            }

            handlers.Add(handler);
            return new Subscription(() => handlers.Remove(handler));
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _dispose;
            private bool _isDisposed;

            public Subscription(Action dispose)
            {
                _dispose = dispose;
            }

            public void Dispose()
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;
                _dispose.Invoke();
            }
        }
    }
}
