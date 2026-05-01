using NeeView.Windows;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NeeView
{
    public static partial class CommandElementTools
    {
        [GeneratedRegex(@"Command$")]
        private static partial Regex _termCommandRegex { get; }

        /// <summary>
        /// コマンド名をクラスタイプから生成
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string CreateCommandName<T>()
            where T : CommandElement
        {
            return CreateCommandName(typeof(T));
        }

        /// <summary>
        /// コマンド名をクラスタイプから生成
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string CreateCommandName(Type type)
        {
            return _termCommandRegex.Replace(type.Name, "");
        }

        /// <summary>
        /// CommandContext から VisibilityRequest を取得
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static StateRequest GetStateRequest(CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                return Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture).ToStateRequest();
            }
            else
            {
                return StateRequest.Toggle;
            }
        }

        public static bool GetState(CommandContext e)
        {
            var state = GetStateRequest(e);
            var byMenu = e.Options.HasFlag(CommandOption.ByMenu);
            return GetState(state, byMenu);
        }

        public static bool GetState(StateRequest state, bool byMenu)
        {
            var current = byMenu ? Config.Current.FilmStrip.IsEnabled : MainWindow.Current.IsFilmStripVisible;
            return state.ToIsEnabled(current);
        }

        /// <summary>
        /// CommandContext から遷移状態を取得
        /// </summary>
        /// <param name="e"></param>
        /// <param name="byMenuState">Menu呼び出しで参照する現在状態</param>
        /// <param name="currentState">通常呼び出しで参照する現在状態</param>
        /// <returns></returns>
        public static bool GetState(CommandContext e, bool byMenuState, bool currentState)
        {
            var state = GetStateRequest(e);
            var byMenu = e.Options.HasFlag(CommandOption.ByMenu);
            return GetState(state, byMenu, byMenuState, currentState);
        }

        public static bool GetState(CommandContext e, bool isVisibleState)
        {
            var state = GetStateRequest(e);
            var byMenu = e.Options.HasFlag(CommandOption.ByMenu);
            return GetState(state, byMenu, isVisibleState, isVisibleState);
        }

        public static bool GetState(StateRequest state, bool byMenu, bool byMenuState, bool currentState)
        {
            var current = byMenu ? byMenuState : currentState;
            return state.ToIsEnabled(current);
        }

        public static bool GetState(CommandContext e, Func<bool> byMenuState, Func<bool> currentState)
        {
            var state = GetStateRequest(e);
            var byMenu = e.Options.HasFlag(CommandOption.ByMenu);
            return GetState(state, byMenu, byMenuState, currentState);
        }

        public static bool GetState(StateRequest state, bool byMenu, Func<bool> byMenuState, Func<bool> currentState)
        {
            var current = byMenu ? byMenuState() : currentState();
            return state.ToIsEnabled(current);
        }
    }
}

