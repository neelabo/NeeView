using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;


namespace NeeView
{
    public class Breadcrumb : BindableBase
    {
        private static readonly List<BreadcrumbToken> _loadingChildren = [new LoadingBreadcrumbToken()];
        private static readonly List<BreadcrumbToken> _emptyChildren = [new EmptyBreadcrumbToken()];

        private readonly IBreadcrumbProfile _profile;
        private readonly QueryPath _queryPath;
        private readonly int _index;
        private readonly string _path;
        private readonly string _name;
        private readonly bool _terminal;
        private BreadcrumbTrimMode _trimMode;
        private List<BreadcrumbToken> _children = new();
        private CancellationTokenSource? _cancellationTokenSource;


        public Breadcrumb(IBreadcrumbProfile profile, QueryPath queryPath, int index)
        {
            if (index >= queryPath.Tokens.Length) throw new ArgumentOutOfRangeException(nameof(index));

            _profile = profile;
            _queryPath = queryPath;
            _index = index;
            _terminal = _index + 1 == _queryPath.Tokens.Length;

            // TODO: File のような SimplePath 構造に対応
            _path = _queryPath.Substring(0, _index + 1).SimplePath;
            _name = profile.GetDisplayName(_queryPath.Tokens[_index], index);
            IsVisibleName = index != 0 || !string.IsNullOrEmpty(_path);
        }

        public string Path => _path;
        public string Name => TrimMode == BreadcrumbTrimMode.Trim ? "..." : _name;
        public bool IsTerminal => _terminal;
        public bool IsVisibleName { get; }
        public bool HasChildren => !_terminal || _profile.CanHasChild(_path, _index);

        public List<BreadcrumbToken> Children
        {
            get { return _children; }
            set { SetProperty(ref _children, value); }
        }

        public BreadcrumbTrimMode TrimMode
        {
            get { return _trimMode; }
            set
            {
                if (SetProperty(ref _trimMode, value))
                {
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }


        public void LoadChildren()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            Children = _loadingChildren;

            Task.Run(() =>
            {
                try
                {
                    var children = _profile.GetChildren(_path, _index, _cancellationTokenSource.Token);
                    Children = children.Count > 0 ? children : _emptyChildren;
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch
                {
                    Children = _emptyChildren;
                }
            }, _cancellationTokenSource.Token);
        }

        public void CancelLoadChildren()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }
    }


}
