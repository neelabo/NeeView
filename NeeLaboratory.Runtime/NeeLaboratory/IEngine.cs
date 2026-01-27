namespace NeeLaboratory
{
    /// <summary>
    /// Engine interface.
    /// 非同期に駆動し続けるものをEngineとし、必須のインターフェイスを定義する
    /// </summary>
    public interface IEngine
    {
        /// <summary>
        /// エンジン始動
        /// </summary>
        void StartEngine();

        /// <summary>
        /// エンジン停止
        /// </summary>
        void StopEngine();
    }
}
