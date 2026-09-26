using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NeeLaboratory.ComponentModel;
using NeeView.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// 目标文件夹面板的显示状态和交互命令。
    /// </summary>
    public partial class DestinationFolderPanelViewModel : ObservableObject
    {
        private readonly DestinationMoveService _moveService;
        private bool _isFolderOperationBusy;
        // 当前目录与递增版本号用于丢弃慢速网络枚举返回的过期结果。
        private string? _currentImageDirectory;
        private long _childFolderRefreshVersion;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CreateFolderCommand))]
        private string _newFolderName = "";

        /// <summary>
        /// 初始化目标文件夹列表，并订阅配置、页面和移动历史变化。
        /// </summary>
        public DestinationFolderPanelViewModel()
        {
            _moveService = DestinationMoveService.Current;
            ManagedItems = new ObservableCollection<DestinationFolderPanelItem>();
            CurrentFolderItems = new ObservableCollection<DestinationFolderPanelItem>();

            Config.Current.System.SubscribePropertyChanged(nameof(SystemConfig.DestinationFolderCollection),
                (s, e) => Refresh());
            Config.Current.Panels.SubscribePropertyChanged(nameof(PanelsConfig.IsDestinationFolderCopyMode),
                (s, e) => UpdateCommandStates());
            Config.Current.Panels.SubscribePropertyChanged(nameof(PanelsConfig.IsDestinationFolderAutoRefreshEnabled),
                (s, e) => OnAutoRefreshChanged());
            PageFrameBoxPresenter.Current.ViewPageChanged += (s, e) => OnCurrentPageChanged();
            BookOperation.Current.BookChanged += (s, e) => OnCurrentPageChanged();
            _moveService.StateChanged += (s, e) => UpdateCommandStates();

            Refresh();
            UpdateCurrentDirectory();
        }

        /// <summary>
        /// 获取手动管理且持久化的全部目标文件夹。
        /// </summary>
        public ObservableCollection<DestinationFolderPanelItem> ManagedItems { get; }

        /// <summary>
        /// 获取当前图片目录下非递归枚举的子文件夹；此集合不写入全局配置。
        /// </summary>
        public ObservableCollection<DestinationFolderPanelItem> CurrentFolderItems { get; }

        /// <summary>
        /// 获取手动管理区域是否有目标文件夹。
        /// </summary>
        public bool HasManagedItems => ManagedItems.Count > 0;

        /// <summary>
        /// 获取当前目录区域是否有子文件夹。
        /// </summary>
        public bool HasCurrentFolderItems => CurrentFolderItems.Count > 0;

        /// <summary>
        /// 获取当前是否正在执行目标文件夹移动。
        /// </summary>
        public bool IsBusy => _moveService.IsBusy;

        /// <summary>
        /// 从持久化配置重新构建手动管理的目标文件夹列表。
        /// </summary>
        public void Refresh()
        {
            ManagedItems.Clear();

            // 手动管理区域不限制数量；只有前九项与数字快捷键保持一致。
            foreach (var item in Config.Current.System.DestinationFolderCollection
                .Select((folder, index) => new DestinationFolderPanelItem(index + 1, folder)))
            {
                ManagedItems.Add(item);
            }

            OnPropertyChanged(nameof(HasManagedItems));
            UpdateCommandStates();
        }

        /// <summary>
        /// 响应书籍或可见页面切换；只有图片所在目录改变时才重建子目录列表。
        /// </summary>
        private void OnCurrentPageChanged()
        {
            UpdateCurrentDirectory();
            UpdateCommandStates();
        }

        /// <summary>
        /// 处理自动刷新开关；关闭时废弃未完成的枚举，开启时立即读取当前目录一次。
        /// </summary>
        private void OnAutoRefreshChanged()
        {
            ++_childFolderRefreshVersion;
            if (Config.Current.Panels.IsDestinationFolderAutoRefreshEnabled
                && _currentImageDirectory is { } currentDirectory)
            {
                _ = RefreshChildFoldersAsync(currentDirectory);
            }
        }

        /// <summary>
        /// 跟踪当前主图片目录，并在真正切换目录时按设置刷新直接子目录。
        /// </summary>
        private void UpdateCurrentDirectory()
        {
            var directory = TryGetCurrentImageDirectory(out var currentDirectory) ? currentDirectory : null;
            if (string.Equals(_currentImageDirectory, directory, StringComparison.OrdinalIgnoreCase)) return;

            _currentImageDirectory = directory;
            ++_childFolderRefreshVersion;
            CurrentFolderItems.Clear();
            OnPropertyChanged(nameof(HasCurrentFolderItems));

            // 普通翻页保持目录不变时不枚举；这里只响应目录切换，不进行定时轮询。
            if (directory is not null && Config.Current.Panels.IsDestinationFolderAutoRefreshEnabled)
            {
                _ = RefreshChildFoldersAsync(directory);
            }
        }

        /// <summary>
        /// 检查指定目标文件夹是否可以按当前模式接收主图片。
        /// </summary>
        /// <param name="item">面板目标项</param>
        /// <returns>当前主图片可按所选模式处理且服务空闲时返回 true</returns>
        private bool CanClassify(DestinationFolderPanelItem? item)
        {
            return item is not null
                && !_moveService.IsBusy
                && BookOperation.Current.Control.CanMoveToFolder(item.Folder, MultiPagePolicy.Once)
                && (!Config.Current.Panels.IsDestinationFolderCopyMode
                    || BookOperation.Current.Control.CanCopyToFolder(item.Folder, MultiPagePolicy.Once));
        }

        /// <summary>
        /// 按当前模式将主图片移动或复制到点击的目标文件夹。
        /// </summary>
        /// <param name="item">面板目标项</param>
        [RelayCommand(CanExecute = nameof(CanClassify))]
        private void Classify(DestinationFolderPanelItem? item)
        {
            if (item is null) return;

            // 只切换面板操作，复制保留源文件且不进入移动撤销历史。
            if (Config.Current.Panels.IsDestinationFolderCopyMode)
            {
                BookOperation.Current.Control.CopyToFolder(item.Folder, MultiPagePolicy.Once);
            }
            else
            {
                BookOperation.Current.Control.MoveToFolder(item.Folder, MultiPagePolicy.Once);
            }
        }

        /// <summary>
        /// 检查是否存在可撤销的成功移动。
        /// </summary>
        /// <returns>可以撤销时返回 true</returns>
        private bool CanUndo()
        {
            return _moveService.CanUndo;
        }

        /// <summary>
        /// 撤销最近一次成功移动。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanUndo))]
        private async Task Undo()
        {
            await _moveService.UndoAsync(CancellationToken.None);
        }

        /// <summary>
        /// 检查是否存在可重做的成功撤销。
        /// </summary>
        /// <returns>可以重做时返回 true</returns>
        private bool CanRedo()
        {
            return _moveService.CanRedo;
        }

        /// <summary>
        /// 重做最近一次成功撤销。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRedo))]
        private async Task Redo()
        {
            await _moveService.RedoAsync(CancellationToken.None);
        }

        /// <summary>
        /// 打开现有的目标文件夹管理对话框。
        /// </summary>
        [RelayCommand]
        private void Manage()
        {
            DestinationFolderDialog.ShowDialog(MainViewComponent.Current.GetWindow());
        }

        /// <summary>
        /// 检查是否可以从当前主图片所在文件夹刷新目标文件夹列表。
        /// </summary>
        /// <returns>存在普通文件系统图片且没有其他目录操作时返回 true</returns>
        private bool CanRefreshFromCurrentFolder()
        {
            return !_isFolderOperationBusy && TryGetCurrentImageDirectory(out _);
        }

        /// <summary>
        /// 手动刷新当前图片目录的直接子文件夹，不修改手动管理的目标目录。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRefreshFromCurrentFolder))]
        private async Task RefreshFromCurrentFolder()
        {
            if (!TryGetCurrentImageDirectory(out var currentDirectory)) return;

            SetFolderOperationBusy(true);
            try
            {
                await RefreshChildFoldersAsync(currentDirectory);
            }
            finally
            {
                SetFolderOperationBusy(false);
            }
        }

        /// <summary>
        /// 检查输入的名称是否可以在当前图片文件夹中创建直接子文件夹。
        /// </summary>
        /// <returns>名称有效、当前图片可定位且没有其他目录操作时返回 true</returns>
        private bool CanCreateFolder()
        {
            return !_isFolderOperationBusy
                && !string.IsNullOrWhiteSpace(NewFolderName)
                && TryGetCurrentImageDirectory(out _);
        }

        /// <summary>
        /// 在当前主图片所在文件夹创建直接子文件夹，刷新列表并按当前模式处理图片。
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCreateFolder))]
        private async Task CreateFolder()
        {
            if (!TryGetCurrentImageDirectory(out var currentDirectory)) return;
            var currentImagePath = BookOperation.Current.Book?.CurrentPage?.TargetPath;
            if (!TryCreateChildDirectoryPath(currentDirectory, NewFolderName, out var destinationPath))
            {
                ToastService.Current.Show(new Toast(
                    TextResources.GetString("DestinationFolderPanel.InvalidFolderName"),
                    TextResources.GetString("DestinationFolderPanel.CreateFailed"),
                    ToastIcon.Warning));
                return;
            }

            SetFolderOperationBusy(true);
            try
            {
                // 文件系统操作放到后台执行，避免网络映射目录阻塞面板界面。
                await Task.Run(() => Directory.CreateDirectory(destinationPath));
                NewFolderName = "";
                await RefreshChildFoldersAsync(currentDirectory);

                // 创建目录期间如果已切换图片，不要把后来显示的另一张图片误分类。
                if (!TryGetCurrentImageDirectory(out var activeDirectory)
                    || !string.Equals(activeDirectory, currentDirectory, StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(BookOperation.Current.Book?.CurrentPage?.TargetPath, currentImagePath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // 新建目录也使用当前模式；移动沿用下一页与撤销历史，复制保留源图。
                var destinationFolder = new DestinationFolder(Path.GetFileName(destinationPath), destinationPath);
                if (BookOperation.Current.Control.CanMoveToFolder(destinationFolder, MultiPagePolicy.Once))
                {
                    if (Config.Current.Panels.IsDestinationFolderCopyMode)
                    {
                        if (BookOperation.Current.Control.CanCopyToFolder(destinationFolder, MultiPagePolicy.Once))
                        {
                            BookOperation.Current.Control.CopyToFolder(destinationFolder, MultiPagePolicy.Once);
                        }
                    }
                    else
                    {
                        BookOperation.Current.Control.MoveToFolder(destinationFolder, MultiPagePolicy.Once);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowFolderOperationError(ex, "DestinationFolderPanel.CreateFailed");
            }
            finally
            {
                SetFolderOperationBusy(false);
            }
        }

        /// <summary>
        /// 获取当前主图片所在的真实文件系统目录。
        /// </summary>
        /// <param name="directory">成功时返回图片所在目录</param>
        /// <returns>当前页是普通文件系统图片且目录存在时返回 true</returns>
        private static bool TryGetCurrentImageDirectory(out string directory)
        {
            var page = BookOperation.Current.Book?.CurrentPage;
            if (page?.ArchiveEntry.Archive is not FolderArchive
                || !page.ArchiveEntry.IsFileSystem
                || page.ArchiveEntry.IsShortcut)
            {
                directory = "";
                return false;
            }

            directory = Path.GetDirectoryName(page.TargetPath) ?? "";
            return !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory);
        }

        /// <summary>
        /// 异步枚举当前目录的直接子文件夹，并仅更新临时面板列表。
        /// </summary>
        /// <param name="currentDirectory">当前主图片所在目录</param>
        private async Task RefreshChildFoldersAsync(string currentDirectory)
        {
            var refreshVersion = ++_childFolderRefreshVersion;
            try
            {
                var folders = await Task.Run(() => EnumerateDestinationFolders(currentDirectory));

                // 网络目录可能较慢；只接受最新请求且仍属于当前图片目录的结果。
                if (refreshVersion != _childFolderRefreshVersion
                    || !string.Equals(_currentImageDirectory, currentDirectory, StringComparison.OrdinalIgnoreCase)) return;

                CurrentFolderItems.Clear();
                foreach (var folder in folders)
                {
                    // 子目录不占用数字快捷键的序号；两组目录都可以无限显示。
                    CurrentFolderItems.Add(new DestinationFolderPanelItem(null, folder));
                }

                OnPropertyChanged(nameof(HasCurrentFolderItems));
                ClassifyCommand.NotifyCanExecuteChanged();
            }
            catch (Exception ex)
            {
                if (refreshVersion == _childFolderRefreshVersion)
                {
                    ShowFolderOperationError(ex, "DestinationFolderPanel.RefreshFailed");
                }
            }
        }

        /// <summary>
        /// 非递归枚举目录，并生成稳定排序的目标文件夹对象。
        /// </summary>
        /// <param name="currentDirectory">要读取的父目录</param>
        /// <returns>按文件夹名称排序的直接子文件夹列表</returns>
        private static List<DestinationFolder> EnumerateDestinationFolders(string currentDirectory)
        {
            return Directory.EnumerateDirectories(currentDirectory, "*", SearchOption.TopDirectoryOnly)
                .OrderBy(path => Path.GetFileName(path), StringComparer.CurrentCultureIgnoreCase)
                .Select(path => new DestinationFolder(Path.GetFileName(path), path))
                .ToList();
        }

        /// <summary>
        /// 验证输入名称并生成受限于当前图片目录的直接子目录路径。
        /// </summary>
        /// <param name="parentDirectory">当前图片所在目录</param>
        /// <param name="input">用户输入的文件夹名称</param>
        /// <param name="destinationPath">验证成功时返回完整目标路径</param>
        /// <returns>输入可安全用作直接子文件夹名称时返回 true</returns>
        private static bool TryCreateChildDirectoryPath(string parentDirectory, string input, out string destinationPath)
        {
            var name = input.Trim();
            destinationPath = "";
            if (string.IsNullOrEmpty(name)
                || name is "." or ".."
                || name.EndsWith(".", StringComparison.Ordinal)
                || FileIO.ContainsInvalidFileNameChars(name)
                || IsReservedWindowsName(name))
            {
                return false;
            }

            try
            {
                var fullParentPath = Path.GetFullPath(parentDirectory);
                var fullDestinationPath = Path.GetFullPath(Path.Combine(fullParentPath, name));
                var destinationParent = Path.GetDirectoryName(fullDestinationPath);

                // 规范化后再次核对父目录，确保输入不能越出当前图片目录。
                if (destinationParent is null
                    || !string.Equals(
                        destinationParent.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                        fullParentPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                destinationPath = fullDestinationPath;
                return true;
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                return false;
            }
        }

        /// <summary>
        /// 检查名称是否为 Windows 保留设备名。
        /// </summary>
        /// <param name="name">待检查的文件夹名称</param>
        /// <returns>名称不能用于普通目录时返回 true</returns>
        private static bool IsReservedWindowsName(string name)
        {
            var stem = Path.GetFileNameWithoutExtension(name).ToUpperInvariant();
            return stem is "CON" or "PRN" or "AUX" or "NUL"
                || (stem.Length == 4
                    && (stem.StartsWith("COM", StringComparison.Ordinal) || stem.StartsWith("LPT", StringComparison.Ordinal))
                    && stem[3] is >= '1' and <= '9');
        }

        /// <summary>
        /// 更新目录操作忙碌状态并重新计算相关按钮可用性。
        /// </summary>
        /// <param name="isBusy">是否正在执行目录操作</param>
        private void SetFolderOperationBusy(bool isBusy)
        {
            _isFolderOperationBusy = isBusy;
            RefreshFromCurrentFolderCommand.NotifyCanExecuteChanged();
            CreateFolderCommand.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// 以本地化标题显示目录操作错误。
        /// </summary>
        /// <param name="exception">文件系统异常</param>
        /// <param name="captionResourceKey">错误标题资源键</param>
        private static void ShowFolderOperationError(Exception exception, string captionResourceKey)
        {
            ToastService.Current.Show(new Toast(
                exception.Message,
                TextResources.GetString(captionResourceKey),
                ToastIcon.Error));
        }

        /// <summary>
        /// 通知面板属性和生成命令重新计算可执行状态。
        /// </summary>
        private void UpdateCommandStates()
        {
            OnPropertyChanged(nameof(IsBusy));
            ClassifyCommand.NotifyCanExecuteChanged();
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
            RefreshFromCurrentFolderCommand.NotifyCanExecuteChanged();
            CreateFolderCommand.NotifyCanExecuteChanged();
        }
    }
}
