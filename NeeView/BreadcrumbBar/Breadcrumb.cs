using NeeLaboratory.ComponentModel;
using NeeLaboratory.Generators;
using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace NeeView
{
    public class Breadcrumb : BindableBase
    {
        private readonly IBreadcrumbProfile _profile;
        private readonly QueryPath _queryPath;
        private readonly int _index;
        private readonly string _path;
        private readonly string _name;
        private readonly bool _terminal;
        private BreadcrumbTrimMode _trimMode;


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
        }

        public string Path => _path;
        public string Name => TrimMode == BreadcrumbTrimMode.Trim ? "..." : _name;
        public bool IsTerminal => _terminal;

        // TODO: サブディレク状態の更新を反映させる。_profile の機能？
        public List<string> Children => _profile.GetDirectories(_path);
        public bool HasChildren => !_terminal || Children.Count > 0;


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
    }

}
