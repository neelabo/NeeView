using System;

namespace NeeLaboratory.Resources
{
    public class TextResourceString
    {
        private string _string;
        private TextResourceStringAttribute _attributes;

        public TextResourceString()
        {
            _string = "";
        }

        public TextResourceString(string s)
        {
            _string = s;
        }

        public TextResourceString(string s, TextResourceStringAttribute attributes)
        {
            _string = s;
            _attributes = attributes;
        }

        public string String
        {
            get
            {
#if DEBUG
                IsUsed = true;
#endif
                return _string;
            }
            set
            {
                _string = value;
            }
        }

        public TextResourceStringAttribute Attributes
        {
            get => _attributes;
            set => _attributes = value;
        }

        public bool IsExpanded
        {
            get => GetFlag(TextResourceStringAttribute.IsExpanded);
            set => SetFlag(TextResourceStringAttribute.IsExpanded, value);
        }

#if DEBUG
        public bool IsUsed
        {
            get => GetFlag(TextResourceStringAttribute.IsUsed);
            set => SetFlag(TextResourceStringAttribute.IsUsed, value);
        }
#endif

        private bool GetFlag(TextResourceStringAttribute flag)
        {
            return _attributes.HasFlag(flag);
        }

        private void SetFlag(TextResourceStringAttribute flag, bool value)
        {
            if (value)
            {
                _attributes |= flag;
            }
            else
            {
                _attributes &= ~flag;
            }
        }

        public override string ToString()
        {
            return String;
        }
    }


    [Flags]
    public enum TextResourceStringAttribute
    {
        None = 0,

        /// <summary>
        /// 展開済
        /// </summary>
        IsExpanded = 0x0001,

#if DEBUG
        /// <summary>
        /// 使用済
        /// </summary>
        IsUsed = 0x8000,
#endif
    }
}
