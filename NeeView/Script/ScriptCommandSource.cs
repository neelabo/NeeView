using NeeLaboratory.Text;
using NeeView.Properties;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        [GeneratedRegex(@"^\s*/{2,}\s*(@\w+)(.*)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
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
        public string ArgsDescription { get; private set; } = "";

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
                var args = new List<string>();
                var argsDescriotion = new StringBuilder();
                var description = new StringBuilder();
                var mouseGesture = new List<string>();
                var name = "";
                var shortcutKey = new List<string>();
                var touchGesture = new List<string>();

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
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        args.Add(value);
                                    }
                                    break;
                                case "@argsdescription":
                                    argsDescriotion.AppendLine(value.Unescape());
                                    break;
                                case "@description":
                                    description.AppendLine(value.Unescape());
                                    break;
                                case "@mousegesture":
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        mouseGesture.Add(value);
                                    }
                                    break;
                                case "@name":
                                    if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(name))
                                    {
                                        name = value;
                                    }
                                    break;
                                case "@shortcutkey":
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        shortcutKey.Add(value);
                                    }
                                    break;
                                case "@touchgesture":
                                    if (!string.IsNullOrEmpty(value))
                                    {
                                        touchGesture.Add(value);
                                    }
                                    break;
                            }
                        }
                    }
                    else if (isComment && !string.IsNullOrWhiteSpace(line))
                    {
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(name))
                {
                    source.Text = name;
                }

                var remarks = description.ToString().Trim();
                if (!string.IsNullOrEmpty(remarks))
                {
                    source.Remarks = remarks;
                }

                source.Args = string.Join(' ', args);

                source.ArgsDescription = argsDescriotion.ToString().Trim();

                source.ShortCutKey = string.Join(',', shortcutKey);
                source.MouseGesture = string.Concat(mouseGesture);
                source.TouchGesture = string.Join(',', touchGesture);
            }

            return source;
        }
    }
}
