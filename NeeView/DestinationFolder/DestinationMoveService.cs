using NeeView.IO;
using NeeView.Properties;
using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// 目标文件夹移动、撤销和重做的会话级协调服务。
    /// </summary>
    public sealed class DestinationMoveService
    {
        private static DestinationMoveService? _current;
        private readonly object _syncRoot = new();
        private readonly List<DestinationMoveRecord> _undoHistory = new();
        private readonly List<DestinationMoveRecord> _redoHistory = new();
        private int _isBusy;

        /// <summary>
        /// 初始化会话级移动历史，并监听容量配置变化。
        /// </summary>
        private DestinationMoveService()
        {
            Config.Current.Panels.SubscribePropertyChanged(nameof(PanelsConfig.DestinationMoveHistoryCapacity),
                (s, e) => ApplyHistoryCapacity());
        }

        /// <summary>
        /// 获取当前进程共享的目标文件夹移动服务。
        /// </summary>
        public static DestinationMoveService Current => _current ??= new DestinationMoveService();

        /// <summary>
        /// 在忙碌状态或历史栈变化时触发。
        /// </summary>
        public event EventHandler? StateChanged;

        /// <summary>
        /// 获取是否正在执行文件移动。
        /// </summary>
        public bool IsBusy => Volatile.Read(ref _isBusy) != 0;

        /// <summary>
        /// 获取当前是否可以撤销最近一次成功移动。
        /// </summary>
        public bool CanUndo
        {
            get
            {
                lock (_syncRoot)
                {
                    return !IsBusy && _undoHistory.Count > 0;
                }
            }
        }

        /// <summary>
        /// 获取当前是否可以重做最近一次撤销。
        /// </summary>
        public bool CanRedo
        {
            get
            {
                lock (_syncRoot)
                {
                    return !IsBusy && _redoHistory.Count > 0;
                }
            }
        }

        /// <summary>
        /// 将指定文件移动到目标文件夹，并记录 Shell 返回的真实落点。
        /// </summary>
        /// <param name="paths">待移动的真实文件路径</param>
        /// <param name="destinationDirectory">目标文件夹</param>
        /// <param name="token">取消令牌</param>
        /// <returns>至少有一个文件成功移动时返回 true</returns>
        public async Task<bool> TryMoveAsync(IEnumerable<string> paths, string destinationDirectory, CancellationToken token)
        {
            // 先清理无效输入并按 Windows 路径规则去重，避免向 Shell 重复提交同一文件。
            var sourcePaths = paths.Where(e => !string.IsNullOrWhiteSpace(e)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (sourcePaths.Count == 0 || !TryBeginOperation()) return false;

            try
            {
                // 目标目录必须在提交 Shell 操作前存在，防止意外把目录字符串当成文件名。
                if (!FileIO.DirectoryExists(destinationDirectory))
                {
                    throw new DirectoryNotFoundException(destinationDirectory);
                }

                var result = await FileIO.SHMoveToFolderAsync(sourcePaths, destinationDirectory, token);
                var records = result.Items
                    .Where(e => !string.Equals(e.Source, e.Destination, StringComparison.OrdinalIgnoreCase))
                    .Select(e => new DestinationMoveRecord(e.Source, e.Destination))
                    .ToList();

                if (records.Count == 0)
                {
                    // A Shell provider can report neither success nor an exception; make that failure visible.
                    throw new IOException(TextResources.GetString("DestinationMove.NoFilesMoved"));
                }

                lock (_syncRoot)
                {
                    // 每个 Shell 成功项独立入栈，撤销时不会被部分失败拖累。
                    foreach (var record in records)
                    {
                        PushWithCapacity(_undoHistory, record);
                    }
                    _redoHistory.Clear();
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ShowError(ex, TextResources.GetString("Message.MoveFailed"));
                return false;
            }
            finally
            {
                EndOperation();
            }
        }

        /// <summary>
        /// 撤销最近一次成功的目标文件夹移动。
        /// </summary>
        /// <param name="token">取消令牌</param>
        /// <returns>文件成功恢复到原路径时返回 true</returns>
        public async Task<bool> UndoAsync(CancellationToken token)
        {
            return await ReplayAsync(true, token);
        }

        /// <summary>
        /// 重做最近一次成功撤销的目标文件夹移动。
        /// </summary>
        /// <param name="token">取消令牌</param>
        /// <returns>文件成功移回目标路径时返回 true</returns>
        public async Task<bool> RedoAsync(CancellationToken token)
        {
            return await ReplayAsync(false, token);
        }

        /// <summary>
        /// 执行撤销或重做，并仅在文件移动成功后变更历史栈。
        /// </summary>
        /// <param name="isUndo">true 表示撤销，false 表示重做</param>
        /// <param name="token">取消令牌</param>
        /// <returns>目标动作成功时返回 true</returns>
        private async Task<bool> ReplayAsync(bool isUndo, CancellationToken token)
        {
            if (!TryBeginOperation()) return false;

            try
            {
                DestinationMoveRecord? record;
                lock (_syncRoot)
                {
                    var sourceHistory = isUndo ? _undoHistory : _redoHistory;
                    record = sourceHistory.Count > 0 ? sourceHistory[^1] : null;
                }
                if (record is null) return false;

                var source = isUndo ? record.Destination : record.Source;
                var destination = isUndo ? record.Source : record.Destination;
                if (!FileIO.FileExists(source))
                {
                    ToastService.Current.Show(new Toast(
                        TextResources.GetFormatString("DestinationMove.SourceMissing", source),
                        TextResources.GetString("DestinationMove.Error.Title"),
                        ToastIcon.Warning));
                    return false;
                }

                if (FileIO.DirectoryExists(destination))
                {
                    ToastService.Current.Show(new Toast(
                        TextResources.GetFormatString("DestinationMove.TargetIsDirectory", destination),
                        TextResources.GetString("DestinationMove.Error.Title"),
                        ToastIcon.Warning));
                    return false;
                }

                var overwrite = FileIO.FileExists(destination);
                if (overwrite && !ConfirmOverwrite(destination)) return false;

                var result = await FileIO.SHMoveAsync(source, destination, overwrite, token);
                var moved = result.Items.Any(e =>
                    string.Equals(e.Source, source, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(e.Destination, destination, StringComparison.OrdinalIgnoreCase));
                if (!moved) return false;

                lock (_syncRoot)
                {
                    var sourceHistory = isUndo ? _undoHistory : _redoHistory;
                    var destinationHistory = isUndo ? _redoHistory : _undoHistory;

                    // 忙碌锁保证栈顶不变，但仍校验记录，避免异常路径误弹栈。
                    if (sourceHistory.Count == 0 || sourceHistory[^1] != record) return false;
                    sourceHistory.RemoveAt(sourceHistory.Count - 1);
                    PushWithCapacity(destinationHistory, record);
                }

                if (isUndo)
                {
                    RestoreVisiblePage(destination);
                }

                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (Exception ex)
            {
                ShowError(ex, TextResources.GetString("DestinationMove.Error.Title"));
                return false;
            }
            finally
            {
                EndOperation();
            }
        }

        /// <summary>
        /// 在覆盖已有文件前显示明确的覆盖或取消确认。
        /// </summary>
        /// <param name="destination">将被覆盖的文件路径</param>
        /// <returns>用户确认覆盖时返回 true</returns>
        private static bool ConfirmOverwrite(string destination)
        {
            var dialog = new MessageDialog(
                TextResources.GetString("DestinationMove.Overwrite.Title"),
                TextResources.GetFormatString("DestinationMove.Overwrite.Message", destination),
                MessageDialogIcon.Warning);
            var overwriteCommand = new UICommand("Word.Overwrite") { IsPossible = true, IsDanger = true };
            dialog.Commands.Add(overwriteCommand);
            dialog.Commands.Add(UICommands.Cancel);
            dialog.DefaultCommandIndex = 1;
            dialog.CancelCommandIndex = 1;
            return dialog.ShowDialog(MainViewComponent.Current.GetWindow()).Command == overwriteCommand;
        }

        /// <summary>
        /// 若用户仍浏览原文件夹，则重载并定位恢复的图片；否则仅提示恢复结果。
        /// </summary>
        /// <param name="restoredPath">已恢复的原始文件路径</param>
        private static void RestoreVisiblePage(string restoredPath)
        {
            var restoredDirectory = Path.GetDirectoryName(restoredPath);
            var currentBook = BookOperation.Current.Address;
            var currentBookDirectory = string.IsNullOrWhiteSpace(currentBook) ? null : Path.GetDirectoryName(currentBook);
            if (IsSamePath(currentBook, restoredDirectory) || IsSamePath(currentBookDirectory, restoredDirectory))
            {
                BookHub.Current.RequestReLoad(Current, Path.GetFileName(restoredPath));
                return;
            }

            ToastService.Current.Show(new Toast(
                TextResources.GetFormatString("DestinationMove.Restored", restoredPath),
                TextResources.GetString("UndoDestinationMoveCommand"),
                ToastIcon.Information));
        }

        /// <summary>
        /// 比较两个可能包含相对片段或尾部分隔符的 Windows 路径。
        /// </summary>
        /// <param name="left">左侧路径</param>
        /// <param name="right">右侧路径</param>
        /// <returns>规范化后路径相同时返回 true</returns>
        private static bool IsSamePath(string? left, string? right)
        {
            if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right)) return false;

            try
            {
                var normalizedLeft = Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                var normalizedRight = Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return string.Equals(normalizedLeft, normalizedRight, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试取得非排队操作锁。
        /// </summary>
        /// <returns>当前没有其他移动操作时返回 true</returns>
        private bool TryBeginOperation()
        {
            if (Interlocked.CompareExchange(ref _isBusy, 1, 0) != 0) return false;

            RaiseStateChanged();
            return true;
        }

        /// <summary>
        /// 释放操作锁并通知命令和面板刷新状态。
        /// </summary>
        private void EndOperation()
        {
            Interlocked.Exchange(ref _isBusy, 0);
            RaiseStateChanged();
        }

        /// <summary>
        /// 在容量上限内向历史列表压入一条记录。
        /// </summary>
        /// <param name="history">目标历史列表</param>
        /// <param name="record">待压入记录</param>
        private static void PushWithCapacity(List<DestinationMoveRecord> history, DestinationMoveRecord record)
        {
            var capacity = Config.Current.Panels.DestinationMoveHistoryCapacity;
            if (capacity <= 0) return;

            if (history.Count >= capacity)
            {
                history.RemoveRange(0, history.Count - capacity + 1);
            }
            history.Add(record);
        }

        /// <summary>
        /// 配置容量缩小时立即裁剪撤销和重做历史，并刷新命令状态。
        /// </summary>
        private void ApplyHistoryCapacity()
        {
            var capacity = Config.Current.Panels.DestinationMoveHistoryCapacity;
            lock (_syncRoot)
            {
                TrimToCapacity(_undoHistory, capacity);
                TrimToCapacity(_redoHistory, capacity);
            }
            RaiseStateChanged();
        }

        /// <summary>
        /// 从最旧记录开始裁剪历史列表到指定容量。
        /// </summary>
        /// <param name="history">待裁剪的历史列表</param>
        /// <param name="capacity">允许保留的最大记录数</param>
        private static void TrimToCapacity(List<DestinationMoveRecord> history, int capacity)
        {
            var removeCount = Math.Max(0, history.Count - capacity);
            if (removeCount > 0)
            {
                history.RemoveRange(0, removeCount);
            }
        }

        /// <summary>
        /// 通知界面和全局命令系统重新计算可执行状态。
        /// </summary>
        private void RaiseStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// 以 Toast 展示文件操作错误。
        /// </summary>
        /// <param name="exception">捕获到的异常</param>
        /// <param name="caption">错误标题</param>
        private static void ShowError(Exception exception, string caption)
        {
            ToastService.Current.Show(new Toast(exception.Message, caption, ToastIcon.Error));
        }

        /// <summary>
        /// 保存一条成功文件移动的真实起点和落点。
        /// </summary>
        /// <param name="Source">移动前路径</param>
        /// <param name="Destination">移动后真实路径</param>
        private sealed record DestinationMoveRecord(string Source, string Destination);
    }
}
