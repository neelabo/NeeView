using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NeeView.Setting
{
    /// <summary>
    /// ResolveConflictDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ResolveConflictDialog : Window
    {
        //
        public ResolveConflictDialog(ResolveConflictDialogContext context)
        {
            InitializeComponent();

            this.DataContext = new ResolveConflictDialogVM(context);

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }

        //
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        //
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    /// <summary>
    /// 競合しているコマンド情報
    /// </summary>
    public class ConflictItem
    {
        public string CommandName { get; set; }
        public bool IsChecked { get; set; }

        public string Text => CommandTable.Current.GetElement(CommandName).Text;

        public ConflictItem(string commandName, bool isChecked)
        {
            CommandName = commandName;
            IsChecked = isChecked;
        }
    }

    /// <summary>
    /// ConflictDialog ViewModel
    /// </summary>
    public class ResolveConflictDialogVM : BindableBase
    {
        private readonly ResolveConflictDialogContext _context;

        // window title
        public string Title => $"{CommandTable.Current.GetElement(_context.Command).Text} - {TextResources.GetString("ResolveConflictDialog.Title")}";

        ////public string Gesture => _context.Gesture;
        public string Note => string.Format(CultureInfo.InvariantCulture, TextResources.GetString("ResolveConflictDialog.Message"), _context.Gesture.GetDisplayString());

        public List<ConflictItem> Conflicts => _context.Conflicts;

        //
        public ResolveConflictDialogVM(ResolveConflictDialogContext context)
        {
            _context = context;
        }
    }

    /// <summary>
    /// ConflictDialog Model
    /// </summary>
    public class ResolveConflictDialogContext
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="command">current command</param>
        /// <param name="gesture">conflict gesture</param>
        /// <param name="commands">conflict commands</param>
        public ResolveConflictDialogContext(InputGestureSource gesture, List<string> commands, string command)
        {
            Command = command;
            Gesture = gesture;

            Conflicts = commands
                .Select(e => new ConflictItem(e, e == command))
                .ToList();
        }

        public string Command { get; set; }
        public InputGestureSource Gesture { get; set; }
        public List<ConflictItem> Conflicts { get; set; }

    }
}
