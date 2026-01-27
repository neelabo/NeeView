using NeeLaboratory;
using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace NeeView.Data
{
    /// <summary>
    /// テーブル値インターフェイス
    /// </summary>
    public interface IIndexValue
    {
        /// <summary>
        /// 現在インデックス
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// 最大インデックス
        /// </summary>
        int IndexMax { get; }

        /// <summary>
        /// 現在値での表示文字列
        /// </summary>
        string ValueString { get; }
    }

    /// <summary>
    /// テーブル値 基底
    /// </summary>
    /// <typeparam name="T"></typeparam>
    abstract public class IndexValue<T> : BindableBase, IIndexValue
        where T : struct
    {
        private readonly List<T> _values;
        private int _index;
        private T _value;
        public PropertyValue<T>? _property;


        public IndexValue(List<T> values)
        {
            _values = values;
            _index = 0;
            _value = _values[_index];

            FormatConverter = new ValueToFormatString<T>(GetValueString);
        }


        public event EventHandler<ValueChangedEventArgs<T>>? ValueChanged;


        public bool IsValueSyncIndex { get; set; } = true;

        /// <summary>
        /// Max Index
        /// </summary>
        public int IndexMax => _values.Count - 1;

        /// <summary>
        /// 現在のインデックス値
        /// </summary>
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = MathUtility.Clamp<int>(value, 0, IndexMax);
                SetValue(_values[_index]);
            }
        }

        /// <summary>
        /// 現在値。
        /// PropertyValueが設定されているときはラッパーとして動作する
        /// </summary>
        public T Value
        {
            get { return _property != null ? _property.Value : _value; }

            set
            {
                _index = IndexOfNear(value, _values);
                SetValue(IsValueSyncIndex ? _values[_index] : value);
            }
        }

        /// <summary>
        /// 値に関連させるPropertyValue
        /// </summary>
        public PropertyValue<T>? Property
        {
            get { return _property; }
            set
            {
                _property = value;
                if (_property is not null)
                {
                    this.Value = _property.Value;
                }
            }
        }

        public string ValueString => GetValueString(Value);
        public virtual IValueConverter? Converter => null;
        public virtual IValueConverter? FormatConverter { get; }


        public override string? ToString()
        {
            return ValueString;
        }

        /// <summary>
        /// 近似値の取得
        /// </summary>
        abstract protected int IndexOfNear(T value, IEnumerable<T> values);

        /// <summary>
        /// 値の設定処理
        /// </summary>
        private void SetValue(T value)
        {
            _value = value;

            if (_property != null)
            {
                _property.Value = value;
            }

            RaisePropertyChanged(null);
            ValueChanged?.Invoke(this, new ValueChangedEventArgs<T>() { NewValue = value });
        }

        protected virtual string GetValueString(T value)
        {
            return value.ToString() ?? "";
        }

        public void Update()
        {
            _index = IndexOfNear(Value, _values);
            RaisePropertyChanged(null);
        }
    }

    /// <summary>
    /// 値変更イベント引数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public T? NewValue { get; set; }
    }

    /// <summary>
    /// テーブル値(int)
    /// </summary>
    public class IndexIntValue : IndexValue<int>
    {
        public IndexIntValue(List<int> values) : base(values)
        {
        }

        protected override int IndexOfNear(int value, IEnumerable<int> values)
        {
            var diff = values.Select((x, index) =>
            {
                var diffX = Math.Abs(x - value);
                return new { index, diffX };
            });
            return diff.OrderBy(d => d.diffX).First().index;
        }
    }

    /// <summary>
    /// テーブル値(double)
    /// </summary>
    public class IndexDoubleValue : IndexValue<double>
    {
        public IndexDoubleValue(List<double> values) : base(values)
        {
        }

        protected override int IndexOfNear(double value, IEnumerable<double> values)
        {
            var diff = values.Select((x, index) =>
            {
                var diffX = Math.Abs(x - value);
                return new { index, diffX };
            });
            return diff.OrderBy(d => d.diffX).First().index;
        }
    }

    /// <summary>
    /// テーブル値(TimeSpan)
    /// </summary>
    public class IndexTimeSpanValue : IndexValue<TimeSpan>
    {
        public IndexTimeSpanValue(List<TimeSpan> values) : base(values)
        {
        }

        protected override int IndexOfNear(TimeSpan value, IEnumerable<TimeSpan> values)
        {
            var diff = values.Select((x, index) =>
            {
                var diffX = (x - value).Duration();
                return new { index, diffX };
            });
            return diff.OrderBy(d => d.diffX).First().index;
        }
    }

}
