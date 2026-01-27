using NeeLaboratory.Windows.Input;
using NeeView.Properties;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    public class InformationGroupButton : Button
    {

        public InformationGroupButton() : base()
        {
            OpenFolderCommand = new RelayCommand(OpenFolderCommand_Execute);
            OpenMapCommand = new RelayCommand(OpenMapCommand_Execute);
        }


        public FileInformationSource Source
        {
            get { return (FileInformationSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(FileInformationSource), typeof(InformationGroupButton), new PropertyMetadata(null, AnyPropertyChanged));


        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(InformationGroupButton), new PropertyMetadata(null, AnyPropertyChanged));


        public RelayCommand OpenFolderCommand { get; }

        public RelayCommand OpenMapCommand { get; }


        private static void AnyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is InformationGroupButton control)
            {
                control.Update();
            }
        }

        private void Update()
        {
            if (GroupName == InformationGroup.File.ToAliasName())
            {
                this.Content = TextResources.GetString("Information.OpenFolder");
                this.Command = OpenFolderCommand;
                this.Visibility = OpenFolderCommand_CanExecute() ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (GroupName == InformationGroup.Gps.ToAliasName())
            {
                this.Content = TextResources.GetString("Information.OpenMap");
                this.Command = OpenMapCommand;
                this.Visibility = OpenMapCommand_CanExecute() ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }
        }

        private bool OpenFolderCommand_CanExecute()
        {
            return Source?.CanOpenPlace() == true;
        }

        private void OpenFolderCommand_Execute()
        {
            Source?.OpenPlace();
        }

        private bool OpenMapCommand_CanExecute()
        {
            return Source?.CanOpenMap() == true;
        }

        void OpenMapCommand_Execute()
        {
            Source?.OpenMap();
        }
    }

}
