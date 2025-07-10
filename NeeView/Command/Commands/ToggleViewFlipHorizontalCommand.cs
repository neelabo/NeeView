using NeeView.Properties;
using System;
using System.Globalization;
using System.Windows.Data;


namespace NeeView
{
    public class ToggleViewFlipHorizontalCommand : CommandElement
    {
        public ToggleViewFlipHorizontalCommand()
        {
            this.Group = TextResources.GetString("CommandGroup.ViewManipulation");
            this.IsShowMessage = false;
        }

        // NOTE: パノラマモードでかつカーソル位置の画像に対する操作の場合、フラグが確定できないためメニュー用のフラグ表示は無効にした
        //public override Binding CreateIsCheckedBinding()
        //{
        //    return new Binding(nameof(IViewTransformControl.IsFlipHorizontal)) { Source = MainViewComponent.Current.ViewTransformControl, Mode = BindingMode.OneWay };
        //}

        [MethodArgument("ToggleCommand.Execute.Remarks")]
        public override void Execute(object? sender, CommandContext e)
        {
            if (e.Args.Length > 0)
            {
                MainViewComponent.Current.ViewTransformControl.FlipHorizontal(Convert.ToBoolean(e.Args[0], CultureInfo.InvariantCulture));
            }
            else
            {
                MainViewComponent.Current.ViewTransformControl.ToggleFlipHorizontal();
            }
        }
    }
}
