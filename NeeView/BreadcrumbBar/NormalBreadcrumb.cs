using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace NeeView
{
    public class NormalBreadcrumb : Breadcrumb
    {
        private static readonly List<BreadcrumbToken> _loadingChildren = [new LoadingBreadcrumbToken()];
        private static readonly List<BreadcrumbToken> _emptyChildren = [new EmptyBreadcrumbToken()];

        private readonly IBreadcrumbProfile _profile;
        private readonly QueryPath _path;
        private readonly string _name;
        private readonly bool _terminal;
        private List<BreadcrumbToken> _children = new();
        private CancellationTokenSource? _cancellationTokenSource;


        public NormalBreadcrumb(IBreadcrumbProfile profile, QueryPath query, int index)
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
        }


        public override QueryPath Path => _path;

        public override string Name => _name;

        public override bool HasName => !string.IsNullOrEmpty(Name);

        public override List<BreadcrumbToken> Children
        {
            get { return _children; }
            set { SetProperty(ref _children, value); }
        }

        public override bool HasChildren => !_terminal || _profile.CanHasChild(_path);


        public override void LoadChildren()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            Children = _loadingChildren;

            Task.Run(async () =>
            {
                try
                {
                    var children = await _profile.GetChildrenAsync(_path, _cancellationTokenSource.Token);
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

        public override void CancelLoadChildren()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }
    }

}
