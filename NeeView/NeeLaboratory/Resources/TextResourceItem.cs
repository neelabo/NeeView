using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NeeLaboratory.Resources
{
    public class TextResourceItem
    {
        private struct CaseText
        {
            public CaseText(string text, Regex regex)
            {
                Text = text;
                Regex = regex;
            }

            public string Text { get; }
            public Regex Regex { get; }
        }

        private List<CaseText>? _caseTexts;


        public TextResourceItem()
        {
        }

        public TextResourceItem(string text)
        {
            SetText(text);
        }


        public string Text { get; private set; } = "";

#if DEBUG
        // [DEV] リソース使用フラグ
        public bool Used { get; set; }
#endif

        public void SetText(string text)
        {
            Text = text;
        }

        public void AddCaseText(string text, Regex regex)
        {
            if (_caseTexts == null)
            {
                _caseTexts = new();
            }
            _caseTexts.Add(new CaseText(text, regex));
        }

        public void AddText(string text, Regex? regex)
        {
            if (regex is not null)
            {
                AddCaseText(text, regex);
            }
            else
            {
                SetText(text);
            }
        }

        public string GetCaseText(string pattern)
        {
            if (_caseTexts is null) return Text;

            foreach (var caseText in _caseTexts)
            {
                if (caseText.Regex.IsMatch(pattern))
                {
                    return caseText.Text;
                }
            }
            return Text;
        }
    }
}
