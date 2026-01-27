using System;

namespace NeeView.Windows.Property
{
    /// <summary>
    /// Setterメソッド装備
    /// </summary>
    public interface IValueSetter
    {
        string Name { get; }

        event EventHandler? ValueChanged;

        object? GetValue();

        void SetValue(object? value);
    }
}
