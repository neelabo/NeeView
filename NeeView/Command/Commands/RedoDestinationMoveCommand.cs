using NeeView.Properties;
using System.Threading;

namespace NeeView
{
    /// <summary>
    /// 重做最近一次被撤销的目标文件夹移动。
    /// </summary>
    public sealed class RedoDestinationMoveCommand : CommandElement
    {
        /// <summary>
        /// 初始化可由用户编辑的默认快捷键和命令分组。
        /// </summary>
        public RedoDestinationMoveCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.File");
            this.ShortCutKey = new ShortcutKey("Ctrl+Y");
            this.IsShowMessage = true;
        }

        /// <summary>
        /// 检查重做历史是否存在且移动服务是否空闲。
        /// </summary>
        /// <param name="sender">命令发送者</param>
        /// <param name="e">命令上下文</param>
        /// <returns>可以重做时返回 true</returns>
        public override bool CanExecute(object? sender, CommandContext e)
        {
            return DestinationMoveService.Current.CanRedo;
        }

        /// <summary>
        /// 启动重做；服务内部负责串行化、错误提示和历史一致性。
        /// </summary>
        /// <param name="sender">命令发送者</param>
        /// <param name="e">命令上下文</param>
        public override void Execute(object? sender, CommandContext e)
        {
            _ = DestinationMoveService.Current.RedoAsync(CancellationToken.None);
        }
    }
}
