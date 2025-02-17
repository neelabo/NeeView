namespace NeeView
{
    public interface ICopyPolicy
    {
        ArchivePolicy ArchiveCopyPolicy { get; }
        TextCopyPolicy TextCopyPolicy { get; }
    }

    public record class CopyPolicy(ArchivePolicy ArchiveCopyPolicy, TextCopyPolicy TextCopyPolicy) : ICopyPolicy
    {
    }
}
