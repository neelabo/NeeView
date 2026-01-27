using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace NeeView
{
    public class ConsoleHost : IConsoleHost
    {
        private readonly Window _owner;
        private JavaScriptEngine _engine;
        private WordTree _wordTree;


        public ConsoleHost(Window owner)
        {
            _owner = owner;

            _engine = CreateJavaScriptEngine();
            _wordTree = CreateWordTree(_engine);
        }


#pragma warning disable CS0067
        public event EventHandler<ConsoleHostOutputEventArgs>? Output;
#pragma warning restore CS0067


        public WordTree WordTree
        {
            get
            {
                UpdateEngine();
                return _wordTree;
            }
        }


        private void UpdateEngine()
        {
            if (_engine != null && !_engine.IsDirty) return;

            _engine = CreateJavaScriptEngine();
            _wordTree = CreateWordTree(_engine);
        }

        private static JavaScriptEngine CreateJavaScriptEngine()
        {
            var engine = new JavaScriptEngine();
            engine.ScriptDirectory = Config.Current.Script.ScriptFolder;

            return engine;
        }

        private static WordTree CreateWordTree(JavaScriptEngine engine)
        {
            var wordTreeRoot = new WordNode()
            {
                Children = new List<WordNode>()
                {
                    new WordNode("cls"),
                    new WordNode("help"),
                    new WordNode("exit"),
                    new WordNode("log"),
                    new WordNode("system"),
                    new WordNode("include"),
                    engine.CreateWordNode("nv"),
                },
            };

            return new WordTree(wordTreeRoot);
        }

        public string? Execute(string input, CancellationToken token)
        {
            switch (input.Trim())
            {
                case "?":
                case "help":
                    new ScriptManual().OpenScriptManual();
                    return null;

                case "exit":
                    AppDispatcher.Invoke(() => _owner.Close());
                    return null;

                default:
                    UpdateEngine();
                    JavaScriptEngineMap.Current.Add(_engine);
                    try
                    {
                        var result = _engine.Execute(null, input, token);
                        return ToJavaScriptString(result);
                    }
                    catch (Exception ex)
                    {
                        _engine.ExceptionProcess(ex);
                        return null;
                    }
                    finally
                    {
                        JavaScriptEngineMap.Current.Remove(_engine);
                        CommandTable.Current.FlushInputGesture();
                    }
            }
        }

        private static string ToJavaScriptString(object? source)
        {
            var builder = new JsonStringBuilder();
            return builder.AppendObject(source).ToString();
        }
    }
}
