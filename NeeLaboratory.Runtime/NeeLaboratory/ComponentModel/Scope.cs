using System;

namespace NeeLaboratory.ComponentModel
{
    public sealed class Scope : IDisposable
    {
        private readonly Action _onExit;

        public Scope(Action onEnter, Action onExit)
        {
            onEnter?.Invoke();
            _onExit = onExit;
        }

        public void Dispose()
        {
            _onExit?.Invoke();
        }
    }
}