using System.Windows;


namespace NeeView.PageFrames
{
    public class TerminalPageFrameContent : DummyPageFrameContent
    {
        public TerminalPageFrameContent(PageRange frameRange, PageFrameActivity activity) : base(activity)
        {
            FrameRange = frameRange;
        }

        public override FrameworkElement? Content => null;

        public override bool IsLocked => true;
        public override PageRange FrameRange { get; }

        public override string ToString()
        {
            return "T" + base.ToString();
        }
    }
}
