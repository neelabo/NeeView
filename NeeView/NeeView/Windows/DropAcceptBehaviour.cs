// from https://github.com/takanemu/WPFDragAndDropSample

using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;

namespace NeeView.Windows
{
    public sealed class DropAcceptDescription
    {
        public event EventHandler<DragEventArgs>? DragEnter;
        public event EventHandler<DragEventArgs>? DragLeave;
        public event EventHandler<DragEventArgs>? DragOver;
        public event EventHandler<DragEventArgs>? DragDrop;

        public void OnDragEnter(object? sender, DragEventArgs dragEventArgs)
        {
            this.DragEnter?.Invoke(sender, dragEventArgs);
        }

        public void OnDragLeave(object? sender, DragEventArgs dragEventArgs)
        {
            this.DragLeave?.Invoke(sender, dragEventArgs);
        }

        public void OnDragOver(object? sender, DragEventArgs dragEventArgs)
        {
            this.DragOver?.Invoke(sender, dragEventArgs);
        }

        public void OnDrop(object? sender, DragEventArgs dragEventArgs)
        {
            this.DragDrop?.Invoke(sender, dragEventArgs);
        }
    }


    /// <summary>
    /// ドロップ対象オブジェクト用ビヘイビア
    /// </summary>
    public class DragAcceptBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// ドロップイベント処理セット
        /// </summary>
        public DropAcceptDescription Description
        {
            get { return (DropAcceptDescription)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(DropAcceptDescription), typeof(DragAcceptBehavior), new PropertyMetadata(null));


        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewDragEnter += DragEnterHandler;
            this.AssociatedObject.PreviewDragLeave += DragLeaveHandler;
            this.AssociatedObject.PreviewDragOver += DragOverHandler;
            this.AssociatedObject.Drop += DropHandler;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewDragEnter -= DragEnterHandler;
            this.AssociatedObject.PreviewDragLeave -= DragLeaveHandler;
            this.AssociatedObject.PreviewDragOver -= DragOverHandler;
            this.AssociatedObject.Drop -= DropHandler;
            base.OnDetaching();
        }

        private void DragEnterHandler(object sender, DragEventArgs e)
        {
            if (this.Description is null) return;

            this.Description.OnDragEnter(sender, e);
            this.Description.OnDragOver(sender, e);
        }

        private void DragLeaveHandler(object sender, DragEventArgs e)
        {
            if (this.Description is null) return;

            this.Description.OnDragLeave(sender, e);
        }

        private void DragOverHandler(object sender, DragEventArgs e)
        {
            if (this.Description is null) return;

            this.Description.OnDragOver(sender, e);
        }

        private void DropHandler(object sender, DragEventArgs e)
        {
            if (this.Description is null) return;

            this.Description.OnDrop(sender, e);
        }
    }
}

