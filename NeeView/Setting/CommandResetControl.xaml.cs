using NeeView.Properties;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace NeeView.Setting
{
    /// <summary>
    /// CommandResetControl.xaml の相互作用ロジック
    /// </summary>
    public partial class CommandResetControl : UserControl
    {
        private static Dictionary<InputScheme, string> _inputSchemeNoteList { get; } = new Dictionary<InputScheme, string>
        {
            [InputScheme.TypeA] = TextResources.Replace(TextResources.GetString("InputScheme.TypeA.Remarks"), true),
            [InputScheme.TypeB] = TextResources.Replace(TextResources.GetString("InputScheme.TypeB.Remarks"), true),
            [InputScheme.TypeC] = TextResources.Replace(TextResources.GetString("InputScheme.TypeC.Remarks"), true),
        };

        public CommandResetControl()
        {
            InitializeComponent();
            UpdateInputSchemeNote();

            this.Root.DataContext = this;
        }

        public Dictionary<InputScheme, string> InputSchemeList { get; } = new Dictionary<InputScheme, string>
        {
            [InputScheme.TypeA] = TextResources.GetString("InputScheme.TypeA"),
            [InputScheme.TypeB] = TextResources.GetString("InputScheme.TypeB"),
            [InputScheme.TypeC] = TextResources.GetString("InputScheme.TypeC")
        };

        public InputScheme InputScheme
        {
            get { return (InputScheme)GetValue(InputSchemeProperty); }
            set { SetValue(InputSchemeProperty, value); }
        }

        public static readonly DependencyProperty InputSchemeProperty =
            DependencyProperty.Register("InputScheme", typeof(InputScheme), typeof(CommandResetControl),
                new FrameworkPropertyMetadata(InputScheme.TypeA, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, InputSchemeChanged));


        private static void InputSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CommandResetControl control)
            {
                control.UpdateInputSchemeNote();
            }
        }

        private void UpdateInputSchemeNote()
        {
            this.InputSchemeNote.Text = _inputSchemeNoteList[InputScheme];
        }
    }
}
