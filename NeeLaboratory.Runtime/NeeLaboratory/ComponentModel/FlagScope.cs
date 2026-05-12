using System;

namespace NeeLaboratory.ComponentModel
{
    public sealed class FlagScope : IDisposable
    {
        private readonly Action<bool> _setter;

        public FlagScope(Action<bool> setter)
        {
            _setter = setter;
            _setter(true);
        }

        public void Dispose()
        {
            _setter(false);
        }
    }
}