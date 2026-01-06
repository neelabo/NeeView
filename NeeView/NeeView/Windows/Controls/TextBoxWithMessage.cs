using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView.Windows.Controls
{
    public class TextBoxWithMessage : Control
    {
        static TextBoxWithMessage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxWithMessage), new FrameworkPropertyMetadata(typeof(TextBoxWithMessage)));
        }


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextBoxWithMessage), new PropertyMetadata(null));


        public string EmptyMessage
        {
            get { return (string)GetValue(EmptyMessageProperty); }
            set { SetValue(EmptyMessageProperty, value); }
        }

        public static readonly DependencyProperty EmptyMessageProperty =
            DependencyProperty.Register(nameof(EmptyMessage), typeof(string), typeof(TextBoxWithMessage), new PropertyMetadata(null));


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateTextBinding();
        }

        // NOTE: Text の Binding 自体の変更には対応していない。現状では初期化時だけ。
        private void UpdateTextBinding()
        {
            var textBox = GetTemplateChild("PART_InputTextBox") as TextBox;
            if (textBox is null) return;

            var textBinding = BindingOperations.GetBinding(this, TextProperty);
            var updateSourceTrigger = textBinding?.UpdateSourceTrigger ?? UpdateSourceTrigger.Default;

            var binding = new Binding(nameof(Text))
            {
                Source = this,
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = updateSourceTrigger,
            };

            textBox.SetBinding(TextBox.TextProperty, binding);
        }

    }
}
