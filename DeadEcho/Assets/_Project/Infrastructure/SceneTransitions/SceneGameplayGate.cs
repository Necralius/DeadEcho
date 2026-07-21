using Project.Core.Services;

namespace Project.Infrastructure.SceneTransitions
{
    public sealed class SceneGameplayGate : ISceneGameplayGate
    {
        private int _blockCount;

        public bool IsGameplayBlocked => _blockCount > 0;

        public void Block()
        {
            _blockCount++;
        }

        public void Release()
        {
            if (_blockCount > 0)
                _blockCount--;
        }
    }
}
