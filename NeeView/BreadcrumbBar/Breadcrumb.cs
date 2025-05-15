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
        private readonly QueryPath _path;
        private readonly string _name;
        private readonly bool _terminal;
        private BreadcrumbTrimMode _trimMode;
        private List<BreadcrumbToken> _children = new();
        private CancellationTokenSource? _cancellationTokenSource;


        public Breadcrumb(IBreadcrumbProfile profile, QueryPath query, int index)
        {
            if (index >= query.Tokens.Length) throw new ArgumentOutOfRangeException(nameof(index));

            if (query.Scheme == QueryScheme.Root)
            {
                query = new QueryPath("");
            }

            _profile = profile;
            _terminal = index + 1 == query.Tokens.Length;

            _path = query.Substring(0, index + 1);

            _name = profile.GetDisplayName(query, index);
            IsVisibleName = index != 0 || !(_path.Scheme.CanOmit() && string.IsNullOrEmpty(_path.Path));
        }

        public QueryPath Path => _path;
        public string Name => TrimMode == BreadcrumbTrimMode.Trim ? "..." : _name;
        public bool IsTerminal => _terminal;
        public bool IsVisibleName { get; }
        public bool HasChildren => !_terminal || _profile.CanHasChild(_path);

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
                    var children = _profile.GetChildren(_path, _cancellationTokenSource.Token);
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
