using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// TinyInfomessage : ViewModel
    /// </summary>
    public class TinyInfoMessageViewModel : BindableBase
    {
        /// <summary>
        /// ChangeCount property.
        /// 表示の更新通知に利用される。
        /// </summary>
        public int ChangeCount
        {
            get { return _changeCount; }
            set { if (_changeCount != value) { _changeCount = value; RaisePropertyChanged(); } }
        }

        private int _changeCount;


        /// <summary>
        /// Model property.
        /// </summary>
        public TinyInfoMessage Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private TinyInfoMessage _model;


        //
        public TimeSpan DisplayTime => TimeSpan.FromSeconds(_model.DisplayTime);

        //
        public Visibility Visibility => string.IsNullOrEmpty(_model.Message) ? Visibility.Collapsed : Visibility.Visible;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="model"></param>
        public TinyInfoMessageViewModel(TinyInfoMessage model)
        {
            _model = model;

            _model.AddPropertyChanged(nameof(_model.Message),
                (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(_model.Message)) ChangeCount++;
                    RaisePropertyChanged(nameof(Visibility));
                });

            _model.AddPropertyChanged(nameof(_model.DisplayTime),
                (s, e) =>
                {
                    RaisePropertyChanged(nameof(DisplayTime));
                });
        }
    }
}
