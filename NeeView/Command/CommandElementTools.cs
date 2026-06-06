using NeeView.Windows;
using System;
using System.Diagnostics;
using System.Globalization;

namespace NeeView
{
    public static partial class CommandElementTools
    {
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
            return CreateCommandName(type.Name);
        }

        /// <summary>
        /// コマンド名をクラス名から生成
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string CreateCommandName(string className)
        {
            Debug.Assert(className.EndsWith("Command"));
            return className[..^"Command".Length];
        }

        /// <summary>
        /// CommandContext から VisibilityRequest を取得
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static StateRequest GetStateRequest(CommandContext e)
        {
            return CommandTools.GetToggleMode(e).ToStateRequest();
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

