namespace NeeView.ComponentModel
{
    public enum DataState
    {
        None,
        Loaded,
        Failed,
    }

    public interface IDataSource
    {
        public object? Data { get; }
        public long DataSize { get; }
        public string? ErrorMessage { get; }

        public bool IsLoaded => Data is not null || IsFailed;
        public bool IsFailed => ErrorMessage is not null;
        public DataState DataState => IsFailed ? DataState.Failed : IsLoaded ? DataState.Loaded : DataState.None;
    }


    public interface IDataSource<T>
    {
        public T? Data { get; }

        public long DataSize { get; }
        public string? ErrorMessage { get; }

        public bool IsLoaded => Data is not null || IsFailed;
        public bool IsFailed => ErrorMessage is not null;
        public DataState DataState => IsFailed ? DataState.Failed : IsLoaded ? DataState.Loaded : DataState.None;
    }


    public static class IDataSourceExtensions
    {
        public static DataSource CreateDataSource(this IDataSource source)
        {
            return new DataSource(source.Data, source.DataSize, source.ErrorMessage);
        }
    }
}
