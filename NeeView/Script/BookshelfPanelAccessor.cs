﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NeeView
{
    public class BookshelfPanelAccessor : LayoutPanelAccessor
    {
        private readonly FolderPanel _panel;
        private readonly BookshelfFolderList _model;

        public BookshelfPanelAccessor() : base(nameof(FolderPanel))
        {
            _panel = (FolderPanel)CustomLayoutPanelManager.Current.GetPanel(nameof(FolderPanel));
            _model = _panel.Presenter.FolderList;
        }


        [WordNodeMember]
        public string? Path
        {
            get { return _model.Place?.SimplePath; }
            set { AppDispatcher.Invoke(() => _model.RequestPlace(new QueryPath(value), null, FolderSetPlaceOption.UpdateHistory)); }
        }

        [WordNodeMember]
        public string SearchWord
        {
            get { return AppDispatcher.Invoke(() => _panel.Presenter.FolderListView.GetSearchBoxText()); }
            set { AppDispatcher.Invoke(() => _panel.Presenter.FolderListView.SetSearchBoxText(value)); }
        }

        [WordNodeMember(DocumentType = typeof(PanelListItemStyle))]
        public string Style
        {
            get { return _model.FolderListConfig.PanelListItemStyle.ToString(); }
            set { AppDispatcher.Invoke(() => _model.FolderListConfig.PanelListItemStyle = value.ToEnum<PanelListItemStyle>()); }
        }

        [WordNodeMember(DocumentType = typeof(FolderOrder))]
        public string FolderOrder
        {
            get { return _model.GetFolderOrder().ToString(); }
            set { AppDispatcher.Invoke(() => _model.SetFolderOrder(value.ToEnum<FolderOrder>())); }
        }

        [WordNodeMember]
        public BookshelfItemAccessor[] Items
        {
            get { return AppDispatcher.Invoke(() => GetItems()); }
        }

        [WordNodeMember]
        public BookshelfItemAccessor[] SelectedItems
        {
            get { return AppDispatcher.Invoke(() => GetSelectedItems()); }
            set { AppDispatcher.Invoke(() => SetSelectedItems(value)); }
        }

        [WordNodeMember]
        public string[] PreviousHistory
        {
            get => _model.GetPreviousHistory().Select(e => e.Value.SimpleQuery).ToArray();
        }

        [WordNodeMember]
        public string[] NextHistory
        {
            get => _model.GetNextHistory().Select(e => e.Value.SimpleQuery).ToArray();
        }

        [WordNodeMember]
        public bool IsFolderTreeVisible
        {
            get => _model.IsFolderTreeVisible;
            set => AppDispatcher.Invoke(() => _model.IsFolderTreeVisible = value);
        }

        [WordNodeMember(DocumentType = typeof(FolderTreeLayout))]
        public string FolderTreeLayout
        {
            get { return _model.FolderTreeLayout.ToString(); }
            set { AppDispatcher.Invoke(() => _model.FolderTreeLayout = value.ToEnum<FolderTreeLayout>()); }
        }

        [WordNodeMember]
        public bool IsSearchIncludeSubdirectories
        {
            get => _model.IsSearchIncludeSubdirectories;
            set => AppDispatcher.Invoke(() => _model.IsSearchIncludeSubdirectories = value);
        }


        private BookshelfItemAccessor[] GetItems()
        {
            return ToStringArray(_panel.Presenter.FolderListBox?.GetItems());
        }

        private BookshelfItemAccessor[] GetSelectedItems()
        {
            return ToStringArray(_panel.Presenter.FolderListBox?.GetSelectedItems());
        }

        private void SetSelectedItems(BookshelfItemAccessor[] selectedItems)
        {
            selectedItems = selectedItems ?? Array.Empty<BookshelfItemAccessor>();

            var listBox = _panel.Presenter.FolderListBox;
            listBox?.SetSelectedItems(selectedItems.Select(e => e.Source));
        }

        private static BookshelfItemAccessor[] ToStringArray(IEnumerable<FolderItem>? items)
        {
            return items?.Select(e => new BookshelfItemAccessor(e)).ToArray() ?? Array.Empty<BookshelfItemAccessor>();
        }

        [WordNodeMember]
        public void MoveToHome()
        {
            _model.MoveToHome();
        }

        [WordNodeMember]
        public void SetHome()
        {
            _model.SetHome();
        }

        [WordNodeMember]
        public void MoveTo(string path)
        {
            _model.MoveTo(new QueryPath(path));
        }

        [WordNodeMember]
        public void MoveToPrevious()
        {
            _model.MoveToPrevious();
        }

        [WordNodeMember]
        public void MoveToNext()
        {
            _model.MoveToNext();
        }

        [WordNodeMember]
        public void MoveToParent()
        {
            _model.MoveToParent();
        }

        [WordNodeMember]
        public void Sync()
        {
            _model.Sync();
        }

        [WordNodeMember]
        public void AddQuickAccess()
        {
            AppDispatcher.Invoke(() => _model.AddQuickAccess());
        }

        [WordNodeMember]
        public void ClearHistoryInPlace()
        {
            AppDispatcher.Invoke(() => _model.ClearHistoryInPlace());
        }


        internal WordNode CreateWordNode(string name)
        {
            return WordNodeHelper.CreateClassWordNode(name, this.GetType());
        }
    }

}
