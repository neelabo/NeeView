using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace NeeLaboratory.Resources
{
    public class TextResourceItem
    {
        private struct CaseText
        {
            public CaseText(string text, Regex regex)
            {
                Text = new TextResourceString(text);
                Regex = regex;
            }

            public TextResourceString Text { get; }
            public Regex Regex { get; }
        }

        private List<CaseText>? _caseTexts;


        public TextResourceItem()
        {
            SetText("");
        }

        public TextResourceItem(string text)
        {
            SetText(text);
        }


        public TextResourceString Text { get; private set; }

#if DEBUG
        // [DEV] リソース使用フラグ
        public bool Used => Text.IsUsed || (_caseTexts is not null && _caseTexts.Any(e => e.Text.IsUsed));
#endif

        [MemberNotNull(nameof(Text))]
        public void SetText(string text)
        {
            Text = new TextResourceString(text);
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

        public TextResourceString GetCaseText(string pattern)
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
