using System;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// (開発用) 簡易ログ
    /// </summary>
    public class DebugSimpleLog
    {
        private int _index;
        private int _count;
        private readonly int _maxLine;
        private readonly string[] _debugLogs;

        public DebugSimpleLog()
        {
            _maxLine = 64;
            _debugLogs = new string[_maxLine];
        }

        public DebugSimpleLog(int maxLine)
        {
            _maxLine = maxLine;
            _debugLogs = new string[_maxLine];
        }

        public string Last => _debugLogs[_index];

        public string All => string.Join("\n", Enumerable.Range((_index - _count + 1 + _maxLine) % _maxLine, _count).Select(e => _debugLogs[e % _maxLine]));

        public void WriteLine(string msg)
        {
            _index = (_index + 1) % _maxLine;
            _count = Math.Min(_count + 1, _maxLine);
            _debugLogs[_index] = msg;
        }
    }

}
