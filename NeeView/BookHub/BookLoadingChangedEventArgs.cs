using System;


namespace NeeView
{
    public class BookLoadingChangedEventArgs : EventArgs
    {
        public BookLoadingChangedEventArgs(bool isLoading)
        {
            IsLoading = isLoading;
        }
        public bool IsLoading { get; set; }
    }
}

