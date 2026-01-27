// from http://sourcechord.hatenablog.com/entry/20130303/1362315081
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace NeeLaboratory.ComponentModel
{
    /// <summary>
    /// モデルを簡略化するための <see cref="INotifyPropertyChanged"/> の実装。
    /// </summary>
    [DataContract]
    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// プロパティの変更を通知するためのマルチキャスト イベント。
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティの変更通知を購読。
        /// 購読解除するDisposableオブジェクトを返す。
        /// </summary>
        public IDisposable SubscribePropertyChanged(PropertyChangedEventHandler handler)
        {
            PropertyChanged += handler;
            return new AnonymousDisposable(() => PropertyChanged -= handler);
        }

        public IDisposable SubscribePropertyChanged(string? propertyName, PropertyChangedEventHandler handler)
        {
            return SubscribePropertyChanged(PropertyChangedTools.CreateChangedEventHandler(propertyName, handler));
        }

        /// <summary>
        /// プロパティが既に目的の値と一致しているかどうかを確認します。必要な場合のみ、
        /// プロパティを設定し、リスナーに通知します。
        /// </summary>
        /// <typeparam name="T">プロパティの型。</typeparam>
        /// <param name="storage">get アクセス操作子と set アクセス操作子両方を使用したプロパティへの参照。</param>
        /// <param name="value">プロパティに必要な値。</param>
        /// <param name="propertyName">リスナーに通知するために使用するプロパティの名前。
        /// この値は省略可能で、
        /// CallerMemberName をサポートするコンパイラから呼び出す場合に自動的に指定できます。</param>
        /// <returns>値が変更された場合は true、既存の値が目的の値に一致した場合は
        /// false です。</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// プロパティ値が変更されたことをリスナーに通知します。
        /// </summary>
        /// <param name="propertyName">リスナーに通知するために使用するプロパティの名前。
        /// この値は省略可能で、
        /// <see cref="CallerMemberNameAttribute"/> をサポートするコンパイラから呼び出す場合に自動的に指定できます。</param>
        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// プロパティ値変更イベントの受信
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="handler"></param>
        public PropertyChangedEventHandler AddPropertyChanged(string propertyName, PropertyChangedEventHandler handler)
        {
            var eventHander = PropertyChangedTools.CreateChangedEventHandler(propertyName, handler);
            PropertyChanged += eventHander;
            return eventHander;
        }


        /// <summary>
        /// プロパティ変更イベントクリア
        /// </summary>
        protected void ResetPropertyChanged()
        {
            this.PropertyChanged = null;
        }
    }

}
