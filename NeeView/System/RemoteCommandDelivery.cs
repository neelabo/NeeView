using System.Diagnostics;

namespace NeeView
{
    public class RemoteCommandDelivery
    {
        public static RemoteCommandDelivery Latest { get; } = new RemoteCommandDelivery(RemoteCommandDeliveryType.Latest);
        public static RemoteCommandDelivery Previous { get; } = new RemoteCommandDelivery(RemoteCommandDeliveryType.Previous);
        public static RemoteCommandDelivery Next { get; } = new RemoteCommandDelivery(RemoteCommandDeliveryType.Next);


        public RemoteCommandDelivery(RemoteCommandDeliveryType type)
        {
            Type = type;
        }

        public RemoteCommandDelivery(Process process)
        {
            Type = RemoteCommandDeliveryType.Custom;
            Process = process;
        }

        public RemoteCommandDeliveryType Type { get; private set; }
        public Process? Process { get; private set; }
    }


    /// <summary>
    /// 配信先ターゲット
    /// </summary>
    public enum RemoteCommandDeliveryType
    {
        /// <summary>
        /// 自身を除く最新プロセス
        /// </summary>
        Latest,

        /// <summary>
        /// 指定のプロセス
        /// </summary>
        Custom,

        /// <summary>
        /// 前のプロセス
        /// </summary>
        Previous,

        /// <summary>
        /// 次のプロセス
        /// </summary>
        Next,
    }
}
