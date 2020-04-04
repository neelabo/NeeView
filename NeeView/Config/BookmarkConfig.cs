﻿using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class BookmarkConfig : FolderListConfig
    {
        private bool _isVisible;
        private bool _isSelected;
        private bool _isSaveBookmark = true;
        private string _bookmarkFilePath;
        private bool _isSyncBookshelfEnabled = true;

        [JsonIgnore]
        [PropertyMapReadOnly]
        [PropertyMember("@WordIsPanelVisible")]
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        [JsonIgnore]
        [PropertyMember("@WordIsPanelSelected")]
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// 本の読み込みで本棚の更新を要求する
        /// </summary>
        [PropertyMember("@ParamIsSyncBookshelfEnabled")]
        public bool IsSyncBookshelfEnabled
        {
            get { return _isSyncBookshelfEnabled; }
            set { SetProperty(ref _isSyncBookshelfEnabled, value); }
        }

        // ブックマークの保存
        [PropertyMember("@ParamIsSaveBookmark")]
        public bool IsSaveBookmark
        {
            get { return _isSaveBookmark; }
            set { SetProperty(ref _isSaveBookmark, value); }
        }

        // ブックマークの保存場所
        [PropertyPath("@ParamBookmarkFilePath", FileDialogType = FileDialogType.SaveFile, Filter = "JSON|*.json")]
        public string BookmarkFilePath
        {
            get { return _bookmarkFilePath; }
            set { SetProperty(ref _bookmarkFilePath, string.IsNullOrWhiteSpace(value) || value == SaveData.DefaultBookmarkFilePath ? null : value); }
        }
    }
}

