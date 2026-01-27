namespace NeeView.Windows.Controls
{
    public static class Windows11Tools
    {
        /// <summary>
        /// Windows11であるかどうか
        /// </summary>
        /// <remarks>
        /// 注意：確かな判定方法ではない
        /// </remarks>
        public static bool IsWindows11 => System.Environment.OSVersion.Version.Major == 10 && System.Environment.OSVersion.Version.Minor == 0 && System.Environment.OSVersion.Version.Build >= 22000;

        /// <summary>
        /// Windows11以上であるかどうか
        /// </summary>
        /// <remarks>
        /// 注意：確かな判定方法ではない
        /// </remarks>
        public static bool IsWindows11OrGreater => System.Environment.OSVersion.Version.Major >= 10 && System.Environment.OSVersion.Version.Build >= 22000;
    }
}
