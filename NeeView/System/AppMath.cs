using System.Runtime.CompilerServices;

namespace NeeView
{
    public static class AppMath
    {
        /// <summary>
        /// アプリ用の浮動小数の丸め
        /// </summary>
        /// <remarks>
        /// 既定で 5 桁
        /// </remarks>
        /// <param name="value"></param>
        /// <param name="digits"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Round(double value, int digits = 5)
        {
            return double.Round(value, digits);
        }
    }
}


