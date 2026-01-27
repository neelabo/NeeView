using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;

namespace NeeView
{
    public class KeyExConverter : TypeConverter
    {
        public static Dictionary<Key, string> DefaultKeyStringMap { get; } = new()
        {
            [Key.Back] = "Backspace",
            [Key.LineFeed] = "Clear",
            [Key.Return] = "Enter",
            [Key.Capital] = "CapsLock",
            [Key.HangulMode] = "KanaMode",
            [Key.HanjaMode] = "KanjiMode",
            [Key.Escape] = "Esc",
            [Key.Prior] = "PageUp",
            [Key.Next] = "PageDown",
            [Key.Snapshot] = "PrintScreen",
            [Key.Oem1] = "OemSemicolon",
            [Key.Oem2] = "OemQuestion",
            [Key.Oem3] = "OemTilde",
            [Key.Oem4] = "OemOpenBrackets",
            [Key.Oem5] = "OemPipe",
            [Key.Oem6] = "OemCloseBrackets",
            [Key.Oem7] = "OemQuotes",
            [Key.Oem102] = "OemBackslash",
            [Key.OemAttn] = "DbeAlphanumeric",
            [Key.OemFinish] = "DbeKatakana",
            [Key.OemCopy] = "DbeHiragana",
            [Key.OemAuto] = "DbeSbcsChar",
            [Key.OemEnlw] = "DbeDbcsChar",
            [Key.OemBackTab] = "DbeRoman",
            [Key.Attn] = "DbeNoRoman",
            [Key.CrSel] = "DbeEnterWordRegisterMode",
            [Key.ExSel] = "DbeEnterImeConfigureMode",
            [Key.EraseEof] = "DbeFlushString",
            [Key.Play] = "DbeCodeInput",
            [Key.Zoom] = "DbeNoCodeInput",
            [Key.NoName] = "DbeDetermineString",
            [Key.Pa1] = "DbeEnterDialogConversionMode",
        };

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
            if (destinationType == typeof(string) && context?.Instance is Key key)
            {
                return IsDefinedKey(key);
            }
            else
            {
                return false;
            }
        }


        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object source)
        {
            if (source is string s)
            {
                string fullName = s.Trim();
                object? key = GetKey(fullName, CultureInfo.InvariantCulture);
                if (key != null)
                {
                    return ((Key)key);
                }
                else
                {
                    throw new NotSupportedException($"Unsupported key literal: {fullName}");
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

            if (destinationType == typeof(string) && value != null)
            {
                Key key = (Key)value;
                if (key == Key.None)
                {
                    return string.Empty;
                }

                if (key >= Key.D0 && key <= Key.D9)
                {
                    return char.ToString((char)(int)(key - Key.D0 + '0'));
                }

                if (key >= Key.A && key <= Key.Z)
                {
                    return char.ToString((char)(int)(key - Key.A + 'A'));
                }

                string? strKey = MatchKey(key, culture);
                if (strKey != null && (strKey.Length != 0 || strKey == string.Empty))
                {
                    return strKey;
                }
            }
            throw GetConvertToException(value, destinationType);
        }


        private static object? GetKey(string keyToken, CultureInfo? culture)
        {
            if (keyToken == string.Empty)
            {
                return Key.None;
            }
            else
            {
                keyToken = keyToken.ToUpper(culture);
                if (keyToken.Length == 1 && char.IsLetterOrDigit(keyToken[0]))
                {
                    if (char.IsDigit(keyToken[0]) && (keyToken[0] >= '0' && keyToken[0] <= '9'))
                    {
                        return ((int)(Key)(Key.D0 + keyToken[0] - '0'));
                    }
                    else if (char.IsLetter(keyToken[0]) && (keyToken[0] >= 'A' && keyToken[0] <= 'Z'))
                    {
                        return ((int)(Key)(Key.A + keyToken[0] - 'A'));
                    }
                    else
                    {
                        throw new ArgumentException($"Cannot convert string {keyToken} to type {typeof(Key)}");
                    }
                }
                else
                {
                    var keyFound = keyToken switch
                    {
                        "ENTER" => Key.Return,
                        "ESC" => Key.Escape,
                        "PGUP" => Key.PageUp,
                        "PGDN" => Key.PageDown,
                        "PRTSC" => Key.PrintScreen,
                        "INS" => Key.Insert,
                        "DEL" => Key.Delete,
                        "WINDOWS" => Key.LWin,
                        "WIN" => Key.LWin,
                        "LEFTWINDOWS" => Key.LWin,
                        "RIGHTWINDOWS" => Key.RWin,
                        "APPS" => Key.Apps,
                        "APPLICATION" => Key.Apps,
                        "BREAK" => Key.Cancel,
                        "BACKSPACE" => Key.Back,
                        "BKSP" => Key.Back,
                        "BS" => Key.Back,
                        "SHIFT" => Key.LeftShift,
                        "LEFTSHIFT" => Key.LeftShift,
                        "RIGHTSHIFT" => Key.RightShift,
                        "CONTROL" => Key.LeftCtrl,
                        "CTRL" => Key.LeftCtrl,
                        "LEFTCTRL" => Key.LeftCtrl,
                        "RIGHTCTRL" => Key.RightCtrl,
                        "ALT" => Key.LeftAlt,
                        "LEFTALT" => Key.LeftAlt,
                        "RIGHTALT" => Key.RightAlt,
                        "SEMICOLON" => Key.OemSemicolon,
                        "PLUS" => Key.OemPlus,
                        "COMMA" => Key.OemComma,
                        "MINUS" => Key.OemMinus,
                        "PERIOD" => Key.OemPeriod,
                        "QUESTION" => Key.OemQuestion,
                        "TILDE" => Key.OemTilde,
                        "OPENBRACKETS" => Key.OemOpenBrackets,
                        "PIPE" => Key.OemPipe,
                        "CLOSEBRACKETS" => Key.OemCloseBrackets,
                        "QUOTES" => Key.OemQuotes,
                        "BACKSLASH" => Key.OemBackslash,
                        "FINISH" => Key.OemFinish,
                        "ATTN" => Key.Attn,
                        "CRSEL" => Key.CrSel,
                        "EXSEL" => Key.ExSel,
                        "ERASEEOF" => Key.EraseEof,
                        "PLAY" => Key.Play,
                        "ZOOM" => Key.Zoom,
                        "PA1" => Key.Pa1,
                        _ => (Key)Enum.Parse(typeof(Key), keyToken, true),
                    };
                    if ((int)keyFound != -1)
                    {
                        return keyFound;
                    }
                    return null;
                }
            }
        }

        private static string? MatchKey(Key key, CultureInfo? _)
        {
            if (key == Key.None)
            {
                return string.Empty;
            }
            else
            {
                if (DefaultKeyStringMap.TryGetValue(key, out var result))
                {
                    return result;
                }
            }

            if (IsDefinedKey(key))
            {
                return key.ToString();
            }
            else
            {
                return null;
            }
        }


        public static bool IsDefinedKey(Key key)
        {
            return (int)key >= (int)Key.None && (int)key <= (int)Key.DeadCharProcessed;
        }
    }
}
