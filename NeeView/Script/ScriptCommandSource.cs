using NeeView.Properties;
using System.IO;
using System.Text.RegularExpressions;

namespace NeeView
{
    public partial class ScriptCommandSource
    {
        public const string Extension = ".nvjs";
        public const string OnStartupFilename = "OnStartup";
        public const string OnBookLoadedFilename = "OnBookLoaded";
        public const string OnPageChangedFilename = "OnPageChanged";
        public const string OnPageEndFilename = "OnPageEnd";
        public const string OnWindowStateChangedFilename = "OnWindowStateChanged";

        [GeneratedRegex(@"^\s*/{2,}")]
        private static partial Regex _regexCommentLine { get; }

        [GeneratedRegex(@"^\s*/{2,}\s*(@\w+)\s+(.+)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
        private static partial Regex _regexDocComment { get; }


        public ScriptCommandSource(string path)
        {
            Path = path;
        }


        public string Path { get; private set; }
        public string Name => LoosePath.GetFileNameWithoutExtension(Path);
        public bool IsCloneable { get; private set; }
        public string Text { get; private set; } = "";
        public string Remarks { get; private set; } = "";
        public string ShortCutKey { get; private set; } = "";
        public string MouseGesture { get; private set; } = "";
        public string TouchGesture { get; private set; } = "";
        public string Args { get; private set; } = "";

        public static ScriptCommandSource Create(string path)
        {
            var source = new ScriptCommandSource(path);

            var filename = LoosePath.GetFileNameWithoutExtension(path);
            source.Text = filename;

            switch (filename)
            {
                case OnStartupFilename:
                    source.Remarks = TextResources.GetString("ScriptOnStartupCommand.Remarks");
                    break;

                case OnBookLoadedFilename:
                    source.Remarks = TextResources.GetString("ScriptOnBookLoadedCommand.Remarks");
                    break;

                case OnPageChangedFilename:
                    source.Remarks = TextResources.GetString("ScriptOnPageChangedCommand.Remarks");
                    break;

                case OnPageEndFilename:
                    source.Remarks = TextResources.GetString("ScriptOnPageEndCommand.Remarks");
                    break;

                case OnWindowStateChangedFilename:
                    source.Remarks = TextResources.GetString("ScriptOnWindowStateChangedCommand.Remarks");
                    break;

                default:
                    source.Remarks = TextResources.GetString("ScriptCommand.Remarks");
                    source.IsCloneable = true;
                    break;
            }

            using (var reader = new StreamReader(path))
            {
                bool isComment = false;
                string? line;
                while ((line = reader?.ReadLine()) != null)
                {
                    if (_regexCommentLine.IsMatch(line))
                    {
                        isComment = true;
                        var match = _regexDocComment.Match(line);
                        if (match.Success)
                        {
                            var key = match.Groups[1].Value.ToLowerInvariant();
                            var value = match.Groups[2].Value.Trim();
                            switch (key)
                            {
                                case "@args":
                                    source.Args = value;
                                    break;
                                case "@description":
                                    source.Remarks = value;
                                    break;
                                case "@mousegesture":
                                    source.MouseGesture = value;
                                    break;
                                case "@name":
                                    source.Text = value;
                                    break;
                                case "@shortcutkey":
                                    source.ShortCutKey = value;
                                    break;
                                case "@touchgesture":
                                    source.TouchGesture = value;
                                    break;
                            }
                        }
                    }
                    else if (isComment && !string.IsNullOrWhiteSpace(line))
                    {
                        break;
                    }
                }
            }

            return source;
        }
    }
}
