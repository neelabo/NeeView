﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// ブックの現在ページに対する操作
    /// </summary>

    public interface IBookPageActionControl : IDisposable
    {
        bool CanDeleteFile();
        ValueTask DeleteFileAsync();

        bool CanDeleteFile(List<Page> pages);
        ValueTask DeleteFileAsync(List<Page> pages);
        ValueTask RemovePagesAsync(List<Page> pages);

        bool CanExport();
        void Export(ExportImageCommandParameter parameter);
        void ExportDialog(ExportImageAsCommandParameter parameter);
        
        bool CanOpenFilePlace();
        void OpenFilePlace();

        bool CanCutToClipboard(CopyFileCommandParameter parameter);
        void CutToClipboard(CopyFileCommandParameter parameter);

        bool CanCopyToClipboard(CopyFileCommandParameter parameter);
        void CopyToClipboard(CopyFileCommandParameter parameter);

        bool CanCopyToFolder(DestinationFolder parameter, MultiPagePolicy multiPagePolicy);
        void CopyToFolder(DestinationFolder parameter, MultiPagePolicy multiPagePolicy);

        bool CanMoveToFolder(DestinationFolder parameter, MultiPagePolicy multiPagePolicy);
        void MoveToFolder(DestinationFolder parameter, MultiPagePolicy multiPagePolicy);

        bool CanOpenApplication(IExternalApp parameter, MultiPagePolicy multiPagePolicy);
        void OpenApplication(IExternalApp parameter, MultiPagePolicy multiPagePolicy);

        //void OpenExternalApp(object? sender, OpenExternalAppCommandParameter parameter);
        //void ExportImage(object? sender, ExportImageCommandParameter parameter);
        //void CopyFile(object? sender);
        //void CopyImage(object? sender);
        //void DeleteFile(object? sender);

        //.. ページ依存だが、ここ？
        //void MoveToChildBook(object? sender);
    }
}
