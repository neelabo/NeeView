using NeeLaboratory.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// 画面に表示する通知：小さく通知
    /// </summary>
    public class TinyInfoMessage : BindableBase
    {
        /// <summary>
        /// Message property.
        /// </summary>
        public string? Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        private string? _Message;


        /// <summary>
        /// DisplayTime property. (sec)
        /// </summary>
        public double DisplayTime
        {
            get { return _dispTime; }
            set { if (_dispTime != value) { _dispTime = value; RaisePropertyChanged(); } }
        }

        private double _dispTime = 1.0;


        /// <summary>
        /// 通知
        /// </summary>
        /// <param name="message"></param>
        public void SetMessage(string message)
        {
            this.Message = message;
            RaisePropertyChanged(nameof(Message));
        }
    }
}
