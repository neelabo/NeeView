using System;
using System.Diagnostics;

namespace NeeLaboratory.Diagnostics
{
    /// <summary>
    /// ログの管理を行う。
    /// SourceTraceのユーティリティクラスです。
    /// </summary>
    public class Log : IDisposable
    {
        private readonly TraceSource _traceSource;
        private readonly int _id;


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="name">ログの名前</param>
        /// <param name="id">ログ出力するときのイベント識別子</param>
        /// <param name="level">ログ出力レベル</param>
        public Log(string name, int id, SourceLevels level)
        {
            _traceSource = new TraceSource(name, level);
            _id = id;
        }

        public Log(string name, int id)
        {
            _traceSource = new TraceSource(name, SourceLevels.Error);
            _id = id;
        }

        public Log(TraceSource traceSource, int id)
        {
            _traceSource = traceSource;
            _id = id;
        }



        public TraceSource TraceSource => _traceSource;



        /// <summary>
        /// TraceSourceのログ出力レベル変更
        /// </summary>
        public void SetSourceLevel(SourceLevels level)
        {
            var sourceSwitch = new SourceSwitch($"{_traceSource.Name}.{level}")
            {
                Level = level
            };

            _traceSource.Switch = sourceSwitch;
        }


        public void Trace(string message)
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, _id, message);
        }

        public void Trace(string message, params object[] args)
        {
            _traceSource.TraceEvent(TraceEventType.Verbose, _id, message, args);
        }

        public void Trace(TraceEventType type, string message)
        {
            _traceSource.TraceEvent(type, _id, message);
        }

        public void Trace(TraceEventType type, string message, params object[] args)
        {
            _traceSource.TraceEvent(type, _id, message, args);
        }

        public void Flush()
        {
            _traceSource.Flush();
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }

                _traceSource.Flush();
                _traceSource.Close();

                _disposedValue = true;
            }
        }

        ~Log()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
