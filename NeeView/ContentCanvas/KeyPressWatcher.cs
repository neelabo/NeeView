using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// なにかキーが押されているかを監視する
    /// </summary>
    public partial class KeyPressWatcher : IDisposable
    {
        private readonly UIElement _target;
        private readonly LinkedList<Key> _keys;

        public KeyPressWatcher(UIElement target)
        {
            _target = target;
            _keys = new LinkedList<Key>();

            _target.PreviewKeyDown += Target_PreviewKeyDown;
            _target.PreviewKeyUp += Target_PreviewKeyUp;
        }


        [Subscribable]
        public event EventHandler<KeyEventArgs>? PreviewKeyDown;

        [Subscribable]
        public event EventHandler<KeyEventArgs>? PreviewKeyUp;


        public bool IsPressed
        {
            get
            {
                if (_disposedValue) return false;

                if (_keys.Any() && _keys.All(e => Keyboard.IsKeyUp(e)))
                {
                    _keys.Clear();
                    return IsModifierKeysPressed;
                }
                else
                {
                    ////if (_keys.Any()) Debug.WriteLine("AnyKey: " + string.Join(",", _keys));
                    return _keys.Any() || IsModifierKeysPressed;
                }
            }
        }

        private static bool IsModifierKeysPressed => Keyboard.Modifiers != ModifierKeys.None;


        private void Target_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_disposedValue) return;

            AddKey(e.Key);
            PreviewKeyDown?.Invoke(sender, e);
        }

        private void Target_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (_disposedValue) return;

            RemoveKey(e.Key);
            PreviewKeyUp?.Invoke(sender, e);
        }

        private void AddKey(Key key)
        {
            if (_disposedValue) return;

            if (!RoutedCommandTable.Current.IsUsedKey(key)) return;

            if (_keys.Contains(key)) return;
            _keys.AddLast(key);
        }

        private void RemoveKey(Key key)
        {
            if (_disposedValue) return;

            _keys.Remove(key);
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected void ThrowIfDisposed()
        {
            if (_disposedValue) throw new ObjectDisposedException(GetType().FullName);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _target.PreviewKeyDown -= Target_PreviewKeyDown;
                    _target.PreviewKeyUp -= Target_PreviewKeyUp;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
