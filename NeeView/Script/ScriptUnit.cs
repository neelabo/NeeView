using NeeView.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    public class ScriptUnit
    {
        private readonly ScriptUnitPool _pool;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public ScriptUnit(ScriptUnitPool pool)
        {
            if (pool is null) throw new ArgumentNullException(nameof(pool));

            _pool = pool;
        }

        public void Execute(object? sender, string path, string? name, List<object>? args)
        {
            Task.Run(() => ExecuteInner(sender, path, name, args));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        private void ExecuteInner(object? sender, string path, string? name, List<object>? args)
        {
            var engine = new JavaScriptEngine() { IsToastEnable = true };

            JavaScriptEngineMap.Current.Add(engine);
            try
            {
                ////engine.Log($"Script: {LoosePath.GetFileName(path)} ...");
                engine.SetCommandName(name);
                engine.SetArgs(args ?? []);
                engine.ExecuteFile(path, _cancellationTokenSource.Token);
                ////engine.Log($"Script: {LoosePath.GetFileName(path)} done.");
            }
            catch (Exception ex)
            {
                engine.ExceptionProcess(ex);
                ////engine.Log($"Script: {LoosePath.GetFileName(path)} stopped.");
            }
            finally
            {
                JavaScriptEngineMap.Current.Remove(engine);
                AppDispatcher.BeginInvoke(() => CommandTable.Current.FlushInputGesture());
                _pool.Remove(this);
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
