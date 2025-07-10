namespace NeeLaboratory.Resources
{
    /// <summary>
    /// リソーステキストを再起展開する
    /// </summary>
    public partial class TextResourceExpand : ITextResource
    {
        private readonly ITextResource _resource;
        private readonly TextResourceReplacer _replacer;

        public TextResourceExpand(ITextResource resource)
        {
            _resource = resource;
            _replacer = new TextResourceReplacer(_resource);

        }

        public TextResourceString? GetResourceString(string name)
        {
            var s = _resource.GetResourceString(name);
            if (s is null) return null;
            return ValidateTextResourceString(s);
        }

        public TextResourceString? GetCaseResourceString(string name, string pattern)
        {
            var s = _resource.GetCaseResourceString(name, pattern);
            if (s is null) return null;
            return ValidateTextResourceString(s);
        }

        public TextResourceString? ValidateTextResourceString(TextResourceString s)
        {
            if (s.IsExpanded) return s;

            var result = _replacer.Replace(s.String, true);
            if (result.FileReplaceCount == 0)
            {
                s.IsExpanded = true;
                if (s.String != result.Text)
                {
                    s.String = result.Text;
                }
                return s;
            }
            else
            {
                return new TextResourceString(result.Text, TextResourceStringAttribute.IsExpanded);
            }
        }

        public string? Replace(string s, bool fallback)
        {
            return _replacer.Replace(s, fallback)?.Text;
        }
    }
}
