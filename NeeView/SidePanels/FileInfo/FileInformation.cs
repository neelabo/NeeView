using NeeLaboratory.ComponentModel;
using NeeLaboratory.Linq;
using NeeView.PageFrames;
using NeeView.Windows.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class FileInformation : BindableBase
    {
        public static FileInformation Current { get; }
        static FileInformation() => Current = new FileInformation();


        private readonly DelayValue<List<FileInformationSource>> _viewContentsDelay;
        private List<FileInformationSource>? _fileInformationCollection;


        private FileInformation()
        {
            var mainViewComponent = MainViewComponent.Current;

            mainViewComponent.PageFrameBoxPresenter.ViewContentChanged +=
                (s, e) => Update(e);

            _viewContentsDelay = new DelayValue<List<FileInformationSource>>();
            _viewContentsDelay.ValueChanged += ViewContentsDelay_ValueChanged;
        }


        public List<FileInformationSource>? FileInformationCollection
        {
            get { return _fileInformationCollection; }
            set { SetProperty(ref _fileInformationCollection, value); }
        }


        public FileInformationSource? GetMainFileInformation()
        {
            return FileInformationCollection?.OrderBy(e => e.Page?.Index ?? int.MaxValue).FirstOrDefault();
        }

        public void Update(FrameViewContentChangedEventArgs e)
        {
            if (e.Action < ViewContentChangedAction.ContentLoading) return;

            //var pageFrameContent = e.PageFrameContent;

            var viewContents = e.ViewContents;
            var direction = e.Direction; // 1; // pageFrameContent?.ViewContentsDirection ?? 1;

            var fileInformationCollection = viewContents
                .Where(e => !e.Element.IsDummy)
                .Direction(direction)
                //.Where(e => e.IsInformationValid)
                .Select(e => new FileInformationSource(e))
                .ToList();

            _viewContentsDelay.SetValue(fileInformationCollection, 100); // 100ms delay
        }

        private void ViewContentsDelay_ValueChanged(object? sender, EventArgs _)
        {
            this.FileInformationCollection = _viewContentsDelay.Value;
        }

        public void Update()
        {
            if (_fileInformationCollection is null) return;

            foreach (var item in _fileInformationCollection)
            {
                item.Update();
            }
        }

    }

}
