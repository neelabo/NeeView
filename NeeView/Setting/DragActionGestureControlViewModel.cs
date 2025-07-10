using NeeLaboratory.Generators;
using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NeeView.Setting
{
    /// <summary>
    /// MouseDragSetting ViewModel
    /// </summary>
    [NotifyPropertyChanged]
    public partial class DragActionGestureControlViewModel : INotifyPropertyChanged
    {
        private readonly DragActionCollection _sources;
        private readonly string _key;
        private DragToken _dragToken = new();
        private DragKey _originalDrag = DragKey.Empty;
        private DragKey _newDrag = DragKey.Empty;
        private RelayCommand? _clearCommand;


        public DragActionGestureControlViewModel(DragActionCollection memento, string key, FrameworkElement gestureSender)
        {
            _sources = memento;
            _key = key;

            gestureSender.MouseDown += GestureSender_MouseDown;

            OriginalDrag = NewDrag = _sources[_key].MouseButton ?? DragKey.Empty;
            UpdateGestureToken(NewDrag);
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        public DragToken DragToken
        {
            get { return _dragToken; }
            set { SetProperty(ref _dragToken, value); }
        }

        public DragKey OriginalDrag
        {
            get { return _originalDrag; }
            set { SetProperty(ref _originalDrag, value); }
        }

        public DragKey NewDrag
        {
            get { return _newDrag; }
            set { SetProperty(ref _newDrag, value); }
        }


        private void GestureSender_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var dragKey = new DragKey(MouseButtonBitsExtensions.Create(e), Keyboard.Modifiers);

            UpdateGestureToken(dragKey);
        }


        /// <summary>
        /// Update Gesture Information
        /// </summary>
        public void UpdateGestureToken(DragKey dragKey)
        {
            NewDrag = dragKey;

            // Check Conflict
            var token = new DragToken();
            token.Gesture = dragKey;

            if (token.Gesture.IsValid)
            {
                token.Conflicts = _sources 
                    .Where(i => i.Key != _key && i.Value.MouseButton == token.Gesture)
                    .Select(i => i.Key)
                    .ToList();

                if (token.Conflicts.Count > 0)
                {
                    token.OverlapsText = string.Format(CultureInfo.InvariantCulture, TextResources.GetString("Notice.Conflict"), TextResources.Join(token.Conflicts.Select(i => DragActionTable.Current.Elements[i].Note)));
                }
            }

            DragToken = token;
        }


        /// <summary>
        /// 決定
        /// </summary>
        public void Decide()
        {
            _sources[_key].MouseButton = NewDrag;
        }


        /// <summary>
        /// Command: ClearCommand
        /// </summary>
        public RelayCommand ClearCommand
        {
            get { return _clearCommand = _clearCommand ?? new RelayCommand(ClearCommand_Executed); }
        }

        private void ClearCommand_Executed()
        {
            UpdateGestureToken(DragKey.Empty);
        }
    }
}
