using NeeView.Properties;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// 撤销最近一次目标文件夹移动。
    /// </summary>
    public sealed class UndoDestinationMoveCommand : CommandElement
    {
        /// <summary>
        /// 初始化可由用户编辑的默认快捷键和命令分组。
        /// </summary>
        public UndoDestinationMoveCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.ShortCutKey = new ShortcutKey("Ctrl+Z");
            this.IsShowMessage = true;
        }

        /// <summary>
        /// 检查撤销历史是否存在且移动服务是否空闲。
        /// </summary>
        /// <param name="sender">命令发送者</param>
        /// <param name="e">命令上下文</param>
        /// <returns>可以撤销时返回 true</returns>
        public override bool CanExecute(object? sender, CommandContext e)
        {
            return DestinationMoveService.Current.CanUndo;
        }

        /// <summary>
        /// 启动撤销；服务内部负责串行化、错误提示和历史一致性。
        /// </summary>
        /// <param name="sender">命令发送者</param>
        /// <param name="e">命令上下文</param>
        public override void Execute(object? sender, CommandContext e)
        {
            _ = DestinationMoveService.Current.UndoAsync(CancellationToken.None);
        }
    }
}
