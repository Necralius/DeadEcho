using Project.Core.Services;

namespace Project.Infrastructure.SceneTransitions
{
    public sealed class ScenePayloadStore : IScenePayloadStore
    {
        private ScenePayload _payload;

        public bool HasPayload => _payload != null;

        public ScenePayload Peek()
        {
            return _payload;
        }

        public ScenePayload Consume()
        {
            ScenePayload payload = _payload;
            _payload = null;
            return payload;
        }

        public void Store(ScenePayload payload)
        {
            _payload = payload;
        }

        public void Clear()
        {
            _payload = null;
        }
    }
}
