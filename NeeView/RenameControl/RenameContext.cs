using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Linq;

namespace NeeView
{
    public class RenameContext : BindableBase
    {
        private static readonly char[] _invalidChars = System.IO.Path.GetInvalidFileNameChars();

        private bool _isInvalidFileNameChars;
        private bool _isInvalidSeparatorChars;
        private bool _isSelectFileNameBody;
        private bool _isHideExtension;

        private readonly string _oldValue;
        private string _extension = "";
        private string _text;
        private string _oldText;

        public RenameContext(string text)
        {
            _text = text;
            _oldText = _text;
            _oldValue = _text;
        }


        // ファイル名禁則文字制御
        public bool IsInvalidFileNameChars
        {
            get { return _isInvalidFileNameChars; }
            set { SetProperty(ref _isInvalidFileNameChars, value); }
        }

        // パス区切り文字制御
        public bool IsInvalidSeparatorChars
        {
            get { return _isInvalidSeparatorChars; }
            set { SetProperty(ref _isInvalidSeparatorChars, value); }
        }

        // 拡張子を除いた部分を選択
        public bool IsSelectFileNameBody
        {
            get { return _isSelectFileNameBody && !_isHideExtension; }
            set { SetProperty(ref _isSelectFileNameBody, value); }
        }

        // 拡張子を非表示
        public bool IsHideExtension
        {
            get { return _isHideExtension; }
            set
            {
                if (SetProperty(ref _isHideExtension, value))
                {
                    if (_isHideExtension)
                    {
                        _extension = System.IO.Path.GetExtension(_oldValue);
                        Text = System.IO.Path.GetFileNameWithoutExtension(_oldValue);
                        _oldText = Text;
                    }
                    else
                    {
                        _extension = "";
                        Text = _oldValue;
                        _oldText = Text;
                    }
                }
            }
        }

        // 編集文字列
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, GetFixedText(value, true)); }
        }

        // 元の文字列
        public string OldText => _oldValue;

        // 変更後の文字列
        public string NewText => string.IsNullOrWhiteSpace(_text) ? "" : _text.Trim() + _extension;

        // 選択する文字列部分
        public string BodyText => _isSelectFileNameBody ? LoosePath.GetFileNameWithoutExtension(_text) : _text;

        public bool IsTextChanged => string.CompareOrdinal(OldText, NewText) != 0;


        private string GetFixedText(string source, bool withToast)
        {
            if (_isInvalidFileNameChars)
            {
                return GetFixedInvalidFileNameCharsText(source, withToast);
            }
            else if (_isInvalidSeparatorChars)
            {
                return GetFixedInvalidSeparatorCharsText(source, withToast);
            }
            else
            {
                return source;
            }
        }

        private static string GetFixedInvalidFileNameCharsText(string source, bool withToast)
        {
            var text = new string(source.Where(e => !_invalidChars.Contains(e)).ToArray());
            if (withToast && text != source)
            {
                ToastService.Current.Show(new Toast(TextResources.GetString("Notice.InvalidFileNameChars"), "", ToastIcon.Information));
            }
            return text;
        }

        private static string GetFixedInvalidSeparatorCharsText(string source, bool withToast)
        {
            var text = new string(source.Where(e => !LoosePath.Separators.Contains(e)).ToArray());
            if (withToast && text != source)
            {
                ToastService.Current.Show(new Toast(TextResources.GetString("Notice.InvalidSeparatorChars"), "", ToastIcon.Information));
            }
            return text;
        }

    }

}
