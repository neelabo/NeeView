using NeeLaboratory.ComponentModel;
using NeeView.Windows.Data;
using System;
using System.Windows;

namespace NeeView.Threading
{
    public class DelayVisibility : BindableBase
    {
        private readonly DelayValue<Visibility> _visibility;


        public DelayVisibility() : this(Visibility.Collapsed)
        {
        }

        public DelayVisibility(Visibility visibility)
        {
            _visibility = new DelayValue<Visibility>(visibility);
            _visibility.ValueChanged += (s, e) =>
            {
                Changed?.Invoke(s, e);
                RaisePropertyChanged(nameof(Visibility));
            };
        }


        public event EventHandler? Changed;


        public Visibility Visibility
        {
            get { return Get(); }
            set { Set(value); }
        }

        public double DefaultDelayTime { get; set; } = 1.0;


        public Visibility Get()
        {
            return _visibility.Value;
        }

        public void Set(Visibility visibility)
        {
            var delay = this.DefaultDelayTime * 1000;
            _visibility.SetValue(visibility, visibility == Visibility.Visible ? 0 : delay);
        }

        public void SetDelayVisibility(Visibility visibility, int ms)
        {
            _visibility.SetValue(visibility, visibility == Visibility.Visible ? 0 : ms);
        }

        public void SetDelayVisibility(Visibility visibility, int ms, DelayValueOverwriteOption overwriteOption)
        {
            _visibility.SetValue(visibility, visibility == Visibility.Visible ? 0 : ms, overwriteOption);
        }

        public string ToDetail()
        {
            return _visibility.ToDetail();
        }
    }
}
