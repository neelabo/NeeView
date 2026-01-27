using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace NeeView
{
    public class KeyGestureSourceConverter : TypeConverter
    {
        private const char _modifiersDelimiter = '+';

        private static readonly KeyExConverter _keyConverter = new();
        private static readonly ModifierKeysConverter _modifierKeysConverter = new();

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            if (destinationType == typeof(string) && context?.Instance is KeyGestureSource)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object source)
        {
            if (source != null && source is string s)
            {
                string fullName = s.Trim();
                if (fullName == string.Empty)
                {
                    return new KeyGestureSource(Key.None);
                }

                string keyToken;
                string modifiersToken;

                int index = fullName.LastIndexOf(_modifiersDelimiter);
                if (index >= 0)
                {
                    modifiersToken = fullName[..index];
                    keyToken = fullName[(index + 1)..];
                }
                else
                {
                    modifiersToken = string.Empty;
                    keyToken = fullName;
                }

                ModifierKeys modifiers = ModifierKeys.None;
                object? resultKey = _keyConverter.ConvertFrom(context, culture, keyToken);
                if (resultKey != null)
                {
                    object temp = _modifierKeysConverter.ConvertFrom(context, culture, modifiersToken);
                    if (temp != null)
                    {
                        modifiers = (ModifierKeys)temp;
                    }
                    return new KeyGestureSource((Key)resultKey, modifiers);
                }
            }
            throw GetConvertFromException(source);
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException(nameof(destinationType));
            }

            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    if (value is KeyGestureSource keyGesture)
                    {
                        if (keyGesture.Key == Key.None)
                            return string.Empty;

                        string strBinding = "";
                        string? strKey = _keyConverter.ConvertTo(context, culture, keyGesture.Key, destinationType) as string;
                        if (strKey != string.Empty)
                        {
                            strBinding += _modifierKeysConverter.ConvertTo(context, culture, keyGesture.Modifiers, destinationType) as string;
                            if (strBinding != string.Empty)
                            {
                                strBinding += _modifiersDelimiter;
                            }
                            strBinding += strKey;
                        }
                        return strBinding;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
            throw GetConvertToException(value, destinationType);
        }


        internal static bool IsDefinedKey(Key key)
        {
            return (key >= Key.None && key <= Key.OemClear);
        }

    }

}
