using NeeView.ComponentModel;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// ViewContent を生成するのに必要十分な情報のセット (Immutable)
    /// </summary>
    public record class PageDataSource : IDataSource
    {
        public object? Data { get; init; }
        public long DataSize { get; init; }
        public string? ErrorMessage { get; init; }
        public bool IsLoaded => Data is not null || IsFailed;
        public bool IsFailed => ErrorMessage is not null;
        public DataState DataState => IsFailed ? DataState.Failed : IsLoaded ? DataState.Loaded : DataState.None;

        public PictureInfo? PictureInfo { get; init; }
        public Size Size { get; init; }
        public Size AspectSize { get; init; }
    }

}
