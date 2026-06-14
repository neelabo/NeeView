namespace NeeView
{
    /// <summary>
    /// コマンドパラメータ引数管理用
    /// </summary>
    public class CommandParameterArgs
    {
        public CommandParameterArgs(object? param)
        {
            Parameter = param;
            AllowReverse = true;
        }

        public CommandParameterArgs(object? param, bool allowReverse)
        {
            Parameter = param;
            AllowReverse = allowReverse;
        }

        /// <summary>
        /// パラメータ本体
        /// </summary>
        public object? Parameter { get; private set; }

        /// <summary>
        /// スライダー方向でのコマンド入れ替え許可
        /// </summary>
        public bool AllowReverse { get; set; }


        public static CommandParameterArgs Create(object param)
        {
            if (param is CommandParameterArgs parameterArgs)
            {
                return parameterArgs;
            }
            else
            {
                return new CommandParameterArgs(param);
            }
        }
    }
}
