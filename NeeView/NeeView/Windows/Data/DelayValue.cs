using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.Windows.Threading;

// TODO: 遅延処理を停止するための終了処理が必要。Disposableにするべきか？
// TODO: WPF非依存にするため、Timers.Timeに変更する？

namespace NeeView.Windows.Data
{
    /// <summary>
    /// 値の遅延反映
    /// </summary>
    public partial class DelayValue<T> : BindableBase, IDisposable
    {
        private T? _value;
        private T? _delayValue;
        private DateTime _delayTime = DateTime.MaxValue;
        private readonly Dispatcher _dispatcher;
        private readonly DispatcherTimer _timer;
        private bool _disposedValue;


        public DelayValue() : this(default, App.Current.Dispatcher)
        {
        }

        public DelayValue(Dispatcher dispatcher) : this(default, dispatcher)
        {
        }

        public DelayValue(T? value) : this(value, App.Current.Dispatcher)
        {
        }

        public DelayValue(T? value, Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _timer = new DispatcherTimer(DispatcherPriority.Normal, _dispatcher);
            _timer.Interval = TimeSpan.FromSeconds(0.1);
            _timer.Tick += Tick;

            _value = value;
            _delayValue = value;
        }


        /// <summary>
        /// 値が反映されたときのイベント
        /// </summary>
        [Subscribable]
        public event EventHandler? ValueChanged;


        /// <summary>
        /// 現在値
        /// </summary>
        public T? Value => _value;


        /// <summary>
        /// タイマー精度変更
        /// </summary>
        /// <remarks>
        /// TODO: 遅延時間で自動で変更されるようにする
        /// </remarks>
        /// <param name="ms"></param>
        public void SetInterval(double ms)
        {
            if (_disposedValue) return;

            _timer.Interval = TimeSpan.FromMilliseconds(ms);
        }

        public void SetValue(T value)
        {
            if (_disposedValue) return;

            SetValue(value, 0.0);
        }

        /// <summary>
        /// 遅延値設定
        /// </summary>
        /// <param name="value">目的値</param>
        /// <param name="ms">反映遅延時間</param>
        /// <param name="overwriteOption">遅延実行中の上書き判定</param>
        public void SetValue(T value, double ms, DelayValueOverwriteOption overwriteOption = DelayValueOverwriteOption.None)
        {
            if (_disposedValue) return;

            if (EqualityComparer<T>.Default.Equals(_delayValue, value))
            {
                switch (overwriteOption)
                {
                    case DelayValueOverwriteOption.None:
                        return;

                    case DelayValueOverwriteOption.Force:
                        break;

                    case DelayValueOverwriteOption.Shorten:
                        if (_delayTime < DateTime.Now + TimeSpan.FromMilliseconds(ms)) return;
                        break;

                    case DelayValueOverwriteOption.Extend:
                        if (_delayTime > DateTime.Now + TimeSpan.FromMilliseconds(ms)) return;
                        break;
                }
            }

            _delayValue = value;

            if (ms <= 0.0)
            {
                _dispatcher.Invoke(() => Flush());
            }
            else
            {
                _delayTime = DateTime.Now + TimeSpan.FromMilliseconds(ms);
                _timer.Start();
            }
        }

        /// <summary>
        /// 目的値を現在値に反映
        /// </summary>
        private void Flush()
        {
            if (_disposedValue) return;

            _timer.Stop();

            if (!EqualityComparer<T>.Default.Equals(_delayValue, _value))
            {
                _value = _delayValue;
                ValueChanged?.Invoke(this, EventArgs.Empty);
                RaisePropertyChanged(nameof(Value));
            }
        }

        /// <summary>
        /// タイマー処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tick(object? sender, EventArgs e)
        {
            if (_disposedValue) return;

            if (_delayTime <= DateTime.Now)
            {
                Flush();
            }
        }

        /// <summary>
        /// 開発用：詳細状態取得
        /// </summary>
        /// <returns></returns>
        public string ToDetail()
        {
            return _timer.IsEnabled ? $"{_value} ({_delayValue}, {(_delayTime - DateTime.Now).TotalMilliseconds}ms)" : $"{_value}";
        }

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
                    _timer.Stop();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


    /// <summary>
    /// 遅延実行中の上書きオプション
    /// </summary>
    public enum DelayValueOverwriteOption
    {
        /// <summary>
        /// 遅延実行中の場合は上書きしない
        /// </summary>
        None,

        /// <summary>
        /// 常に上書き
        /// </summary>
        Force,

        /// <summary>
        /// 遅延時間を縮める方向で適用する
        /// </summary>
        Shorten,

        /// <summary>
        /// 遅延時間を伸ばす方向で適用する
        /// </summary>
        Extend,
    }

}
