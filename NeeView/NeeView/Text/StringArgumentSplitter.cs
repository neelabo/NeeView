using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NeeView.Text
{
    public class StringArgumentSplitter
    {

        delegate void StateFunc(Context context);

        private enum State
        {
            S00 = 0,
            S01,
            S02,
            S03,
            S04,
            S05,
            S06,
            S07,
            S08,
            END,
        }

        private enum Trigger
        {
            End = 0,
            Space,
            DoubleQuote,
            Any,
        }

        private readonly State[,] _table = new State[,]
        {
            // End, Space, DoubleQuote, Any
            {State.END, State.S01, State.S06, State.S03, }, // S00
            {State.END, State.S01, State.S06, State.S03, }, // S01
            {State.S05, State.S04, State.S06, State.S03, }, // S02
            {State.S02, State.S02, State.S02, State.S02, }, // S03
            {State.S01, State.S01, State.S01, State.S01, }, // S04
            {State.END, State.END, State.END, State.END, }, // S05
            {State.S05, State.S07, State.S08, State.S07, }, // S06
            {State.S06, State.S06, State.S06, State.S06, }, // S07
            {State.S05, State.S04, State.S07, State.S03, }, // S08
        };

        private readonly List<StateFunc> _stateMap;

        public StringArgumentSplitter()
        {
            _stateMap = new List<StateFunc>
            {
                State00,
                State01,
                State02,
                State03,
                State04,
                State05,
                State06,
                State07,
                State08,
            };
        }


        public List<string> Split(string? source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return new List<string>();
            }

            var context = new Context(source);

            var state = State.S00;
            while (state != State.END)
            {
                //Debug.WriteLine($"{state}: {context.StateString()}");

                var func = _stateMap[(int)state];
                func.Invoke(context);
                var trigger = context.ReadTrigger();
                state = _table[(int)state, (int)trigger];
            }

            return context.Result;
        }


        private void State00(Context context)
        {
        }

        private void State01(Context context)
        {
            context.Next();
        }

        private void State02(Context context)
        {
            context.Next();
        }

        private void State03(Context context)
        {
            context.Push();
        }

        private void State04(Context context)
        {
            context.Answer();
        }

        private void State05(Context context)
        {
            context.Answer();
        }

        private void State06(Context context)
        {
            context.Next();
        }

        private void State07(Context context)
        {
            context.Push();
        }

        private void State08(Context context)
        {
            context.Next();
        }


        private class Context
        {
            private readonly string _source;
            private readonly StringBuilder _stringBuilder = new();
            private int _index;

            public Context(string source)
            {
                _source = source;
                _index = 0;
            }

            public bool IsEnd => _index >= _source.Length;

            public List<string> Result { get; private set; } = new List<string>();


            public void Next()
            {
                _index = (_index < _source.Length) ? _index + 1 : _index;
            }

            public void Back()
            {
                _index = (_index > 0) ? _index - 1 : _index;
            }

            public char Read()
            {
                return (_index < _source.Length) ? _source[_index] : '\0';
            }

            public Trigger ReadTrigger()
            {
                var c = Read();
                if (char.IsWhiteSpace(c))
                {
                    return Trigger.Space;
                }
                return c switch
                {
                    '\0' => Trigger.End,
                    '"' => Trigger.DoubleQuote,
                    _ => Trigger.Any,
                };
            }

            public void Push()
            {
                _stringBuilder.Append(Read());
            }

            public void Answer()
            {
                var s = _stringBuilder.ToString();
                _stringBuilder.Clear();

                Result.Add(s);
            }


            public string StateString()
            {
                return $"Index={_index}, Char={Read()}, Work={_stringBuilder}";
            }


        }
    }

    public static class StringTools
    {
        private static readonly StringArgumentSplitter _argumentSplitter = new();

        public static List<string> SplitArgument(string? s)
        {
            return _argumentSplitter.Split(s);
        }



        [Conditional("DEBUG")]
        public static void TestStringArgumentSplitter()
        {
            var argument = new StringArgumentSplitter();

            var samples = new string?[]
            {
                null,
                "",
                "    \n",
                @"a b c",
                @"a ""b c"" d e",
                @"a """" b c",
                @"a b ""c",
                @"a ""b c""d",
                @"a b""c d""",
                @"  a b c  ",
                @" a ""b """" c"" d",
                @" a """""""" c d",
            };

            foreach (var s in samples)
            {
                var result = argument.Split(s);
                var result2 = string.Join(",", result.Select(e => $"[{e}]"));
                Debug.WriteLine($"{s} => {result2}");
            }
        }
    }
}
