namespace NeeView
{
    /// <summary>
    /// コマンドパラメータとして渡すオブジェクト。メニューからのコマンドであることを示す
    /// </summary>
    public class MenuCommandTag
    {
        public static MenuCommandTag Tag { get; } = new MenuCommandTag();
    }
}
