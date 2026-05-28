using System;
using System.Diagnostics;

namespace NeeView
{
    public static class DragActionTools
    {
        private const string _postfix = "DragAction";

        /// <summary>
        /// コマンド名をクラスタイプから生成
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string CreateName<T>()
            where T : DragAction
        {
            return CreateName(typeof(T));
        }

        /// <summary>
        /// コマンド名をクラスタイプから生成
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string CreateName(Type type)
        {
            return CreateName(type.Name);
        }

        /// <summary>
        /// コマンド名をクラス名から生成
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public static string CreateName(string className)
        {
            Debug.Assert(className.EndsWith(_postfix));
            return className[..^_postfix.Length];
        }

    }
}
