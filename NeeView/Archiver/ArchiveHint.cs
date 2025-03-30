
namespace NeeView
{
    /// <summary>
    /// アーカイバー選択用ヒント
    /// </summary>
    /// <param name="Archiver">優先アーカイバー</param>
    public record ArchiveHint(ArchiverIdentifier Archiver)
    {
        public static ArchiveHint None { get; } = new ArchiveHint(ArchiverIdentifier.None);
    }
}

