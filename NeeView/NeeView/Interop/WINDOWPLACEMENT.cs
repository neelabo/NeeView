using System.Runtime.InteropServices;

namespace NeeView.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT
    {
        public int length;
        public int flags;
        public ShowWindowCommands showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;

        public int Length
        {
            get => length;
            set => length = value;
        }

        public int Flags
        {
            get => flags;
            set => flags = value;
        }

        public ShowWindowCommands ShowCmd
        {
            get => showCmd;
            set => showCmd = value;
        }

        public POINT MinPosition
        {
            get => minPosition;
            set => minPosition = value;
        }

        public POINT MaxPosition
        {
            get => maxPosition;
            set => maxPosition = value;
        }

        public RECT NormalPosition
        {
            get => normalPosition;
            set => normalPosition = value;
        }

        public bool IsValid() => length == Marshal.SizeOf(typeof(WINDOWPLACEMENT));

        public override string ToString()
        {
            return $"{ShowCmd},{MinPosition.x},{MinPosition.y},{MaxPosition.x},{MaxPosition.y},{NormalPosition.left},{NormalPosition.top},{NormalPosition.right},{NormalPosition.bottom}";
        }
    }

}
