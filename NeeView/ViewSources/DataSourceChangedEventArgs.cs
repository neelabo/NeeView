using System;

namespace NeeView
{
    public class DataSourceChangedEventArgs : EventArgs
    {
        public DataSourceChangedEventArgs(PageDataSource dataSource)
        {
            DataSource = dataSource;
        }

        public PageDataSource DataSource { get; }
    }

}
