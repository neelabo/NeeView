//#define LOCAL_DEBUG

using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;

namespace NeeLaboratory.ComponentModel
{
    /// <summary>
    /// 最新値の保持
    /// </summary>
    /// <remarks>
    /// 更新したときに操作子を返す。操作子を介して最新値を変更できるが、別処理ですでに更新されていた場合は変更できない。
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    [LocalDebug]
    public partial class LatestValue<T>
        where T : class
    {
        public record class Operation(LatestValue<T> LatestValue, T Value) : IDisposable
        {
            public void Dispose() => LatestValue.Reset(this);
        }

        private readonly System.Threading.Lock _lock = new();
        private Operation? _operation;

        public T? Value => _operation?.Value;

        public Operation? CompareSet(T value)
        {
            lock (_lock)
            {
                if (EqualityComparer<T>.Default.Equals(Value, value))
                {
                    LocalDebug.WriteLine($"Skip CompareSet {value}");
                    return null;
                }
                return Set(value);
            }
        }

        public Operation Set(T value)
        {
            lock (_lock)
            {
                LocalDebug.WriteLine($"Set {value}");
                _operation = new Operation(this, value);
                return _operation;
            }
        }

        public void Reset(Operation operation)
        {
            lock (_lock)
            {
                if (_operation != operation)
                {
                    LocalDebug.WriteLine($"Skip Remove {Value}");
                    return;
                }
                LocalDebug.WriteLine($"Remove {Value}");
                _operation = null;
            }
        }

    }

}
