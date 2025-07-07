using System;
using System.Collections.Generic;
using System.Reflection;

#pragma warning disable CA1822

namespace NeeView
{
    [WordNodeMember]
    public class BookConfigAccessor
    {
        public BookConfigAccessor()
        {
        }

        // PageMode
        [WordNodeMember]
        public int ViewPageSize
        {
            get { return (int)BookSettings.Current.PageMode + 1; }
            set { AppDispatcher.Invoke(() => BookSettings.Current.SetPageMode(((PageMode)value - 1).Validate())); }
        }

        // [Parameter(typeof(BookReadOrder))]
        [WordNodeMember(DocumentType = typeof(PageReadOrder))]
        public string BookReadOrder
        {
            get { return BookSettings.Current.BookReadOrder.ToString(); }
            set { AppDispatcher.Invoke(() => BookSettings.Current.SetBookReadOrder(value.ToEnum<PageReadOrder>())); }
        }

        [WordNodeMember]
        public bool IsSupportedDividePage
        {
            get { return BookSettings.Current.IsSupportedDividePage; }
            set { AppDispatcher.Invoke(() => BookSettings.Current.SetIsSupportedDividePage(value)); }
        }

        [WordNodeMember]
        public bool IsSupportedSingleFirstPage
        {
            get { return BookSettings.Current.IsSupportedSingleFirstPage; }
            set { AppDispatcher.Invoke(() => BookSettings.Current.SetIsSupportedSingleFirstPage(value)); }
        }

        [WordNodeMember]
        public bool IsSupportedSingleLastPage
        {
            get { return BookSettings.Current.IsSupportedSingleLastPage; }
            set { AppDispatcher.Invoke(() => BookSettings.Current.SetIsSupportedSingleLastPage(value)); }
        }

        [WordNodeMember]
        public bool IsSupportedWidePage
        {
            get { return BookSettings.Current.IsSupportedWidePage; }
            set { AppDispatcher.Invoke(() => BookSettings.Current.SetIsSupportedWidePage(value)); }
        }

        [WordNodeMember]
        public bool IsRecursiveFolder
        {
            get { return BookSettings.Current.IsRecursiveFolder; }
            set { AppDispatcher.Invoke(() => BookSettings.Current.SetIsRecursiveFolder(value)); }
        }

        // [Parameter(typeof(PageSortMode))]
        [WordNodeMember(DocumentType = typeof(PageSortMode))]
        public string SortMode
        {
            get { return BookSettings.Current.SortMode.ToString(); }
            set { AppDispatcher.Invoke(() => BookSettings.Current.SetSortMode(value.ToEnum<PageSortMode>())); }
        }

        [WordNodeMember(DocumentType = typeof(AutoRotateType))]
        public string AutoRotate
        {
            get { return BookSettings.Current.AutoRotate.ToString(); }
            set { AppDispatcher.Invoke(() => BookSettings.Current.SetAutoRotate(value.ToEnum<AutoRotateType>())); }
        }

        [WordNodeMember]
        public double BaseScale
        {
            get { return BookSettings.Current.BaseScale; }
            set { AppDispatcher.Invoke(() => BookSettings.Current.SetBaseScale(value)); }
        }
    }
}
