using System;

namespace NeeView
{
    /// <summary>
    /// スクリプトの開発環境。
    /// ErrorLevelに応じて例外処理を切り替える。
    /// </summary>
    public class ScriptAccessDiagnostics : IAccessDiagnostics
    {
        public T? Throw<T>(Exception ex)
        {
            Throw(ex);
            return default;
        }

        public object? Throw(Exception ex, Type type)
        {
            Throw(ex);
            return type.GetDefaultValue();
        }

        public void Throw(Exception ex)
        {
            Throw(ex, ScriptErrorLevel.Error);
        }

        public void Throw(Exception ex, ScriptErrorLevel errorLevel)
        {
            var _engine = JavaScriptEngineMap.Current.GetCurrentEngine();

            var message = _engine.CreateScriptErrorMessage(ex);

            var level = Config.Current.Script.ErrorLevel < errorLevel ? Config.Current.Script.ErrorLevel : errorLevel;

            switch (level)
            {
                case ScriptErrorLevel.Info:
                    ConsoleWindowManager.Current.InforMessage(message.ToString(), false);
                    break;

                case ScriptErrorLevel.Warning:
                    ConsoleWindowManager.Current.WarningMessage(message.ToString(), _engine.IsToastEnable);
                    break;

                case ScriptErrorLevel.Error:
                default:
                    throw new ScriptException(message, ex);
            }
        }

    }

}
