using System;

namespace Project.Core.Services
{
    public interface IGameEventBus
    {
        void Publish<TEvent>(TEvent gameEvent);
        IDisposable Subscribe<TEvent>(Action<TEvent> handler);
    }
}
