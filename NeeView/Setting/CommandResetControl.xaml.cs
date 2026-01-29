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
        public CommandResetControl()
        {
            InitializeComponent();

            this.Root.DataContext = this;
            this.Root.ItemsSource = InputSchemeList;
            this.Root.SelectionChanged += Root_SelectionChanged;
            this.Root.SelectedIndex = 0;
        }

        public List<InputSchemeText> InputSchemeList { get; } = new List<InputSchemeText>
        {
            new InputSchemeText(InputScheme.TypeA, TextResources.GetString("InputScheme.TypeA"), TextResources.Replace(TextResources.GetString("InputScheme.TypeA.Remarks"), true)),
            new InputSchemeText(InputScheme.TypeB, TextResources.GetString("InputScheme.TypeB"), TextResources.Replace(TextResources.GetString("InputScheme.TypeB.Remarks"), true)),
            new InputSchemeText(InputScheme.TypeC, TextResources.GetString("InputScheme.TypeC"), TextResources.Replace(TextResources.GetString("InputScheme.TypeC.Remarks"), true)),
        };

        public InputScheme InputScheme
        {
            get { return (InputScheme)GetValue(InputSchemeProperty); }
            set { SetValue(InputSchemeProperty, value); }
        }

        public static readonly DependencyProperty InputSchemeProperty =
            DependencyProperty.Register("InputScheme", typeof(InputScheme), typeof(CommandResetControl),
                new FrameworkPropertyMetadata(InputScheme.TypeA, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private void Root_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Root.SelectedItem is InputSchemeText item)
            {
                InputScheme = item.Scheme;
            }
        }
    }


    public record class InputSchemeText(InputScheme Scheme, string Name, string Remarks)
    {
    }
}
