namespace NeeView
{
    /// <summary>
    /// QuickAccessNode と QuickAccessNodeFolder を DataObject の値にするためのアダプタ
    /// </summary>
    public class QuickAccessNodeBaseObject
    {
        public QuickAccessNodeBaseObject(QuickAccessNodeBase value)
        {
            Value = value;
        }

        public QuickAccessNodeBase Value { get; }
    }
}
