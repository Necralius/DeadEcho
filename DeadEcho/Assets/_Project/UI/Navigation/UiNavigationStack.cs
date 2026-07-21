using System.Collections.Generic;

namespace Project.UI.Navigation
{
    public sealed class UiNavigationStack
    {
        private readonly Stack<IUiScreenController> _screens = new();

        public bool CanGoBack => _screens.Count > 0;
        public int Count => _screens.Count;

        public void Push(IUiScreenController screen)
        {
            _screens.Push(screen);
        }

        public bool TryPop(out IUiScreenController screen)
        {
            if (_screens.Count == 0)
            {
                screen = null;
                return false;
            }

            screen = _screens.Pop();
            return true;
        }

        public void Clear()
        {
            while (_screens.Count > 0)
                _screens.Pop().Dispose();
        }
    }
}
