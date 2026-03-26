using Generator.Equals;
using NeeView.Windows.Property;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// スケールモードトグル用設定
    /// </summary>
    [Equatable(Explicit = true)]
    public partial class ToggleStretchModeCommandParameter : CommandParameter
    {
        [DefaultEquality] private bool _isLoop = true;
        [DefaultEquality] private bool _isEnableNone = true;
        [DefaultEquality] private bool _isEnableUniform = true;
        [DefaultEquality] private bool _isEnableUniformToFill = true;
        [DefaultEquality] private bool _isEnableUniformToSize = true;
        [DefaultEquality] private bool _isEnableUniformToVertical = true;
        [DefaultEquality] private bool _isEnableUniformToHorizontal = true;

        // ループ
        [PropertyMember]
        public bool IsLoop
        {
            get => _isLoop;
            set => SetProperty(ref _isLoop, value);
        }

        // 表示名
        [PropertyMember]
        public bool IsEnableNone
        {
            get => _isEnableNone;
            set => SetProperty(ref _isEnableNone, value);
        }

        [PropertyMember]
        public bool IsEnableUniform
        {
            get => _isEnableUniform;
            set => SetProperty(ref _isEnableUniform, value);
        }

        [PropertyMember]
        public bool IsEnableUniformToFill
        {
            get => _isEnableUniformToFill;
            set => SetProperty(ref _isEnableUniformToFill, value);
        }

        [PropertyMember]
        public bool IsEnableUniformToSize
        {
            get => _isEnableUniformToSize;
            set => SetProperty(ref _isEnableUniformToSize, value);
        }

        [PropertyMember]
        public bool IsEnableUniformToVertical
        {
            get => _isEnableUniformToVertical;
            set => SetProperty(ref _isEnableUniformToVertical, value);
        }

        [PropertyMember]
        public bool IsEnableUniformToHorizontal
        {
            get => _isEnableUniformToHorizontal;
            set => SetProperty(ref _isEnableUniformToHorizontal, value);
        }


        public IReadOnlyDictionary<PageStretchMode, bool> GetStretchModeDictionary()
        {
            return new Dictionary<PageStretchMode, bool>()
            {
                [PageStretchMode.None] = IsEnableNone,
                [PageStretchMode.Uniform] = IsEnableUniform,
                [PageStretchMode.UniformToFill] = IsEnableUniformToFill,
                [PageStretchMode.UniformToSize] = IsEnableUniformToSize,
                [PageStretchMode.UniformToVertical] = IsEnableUniformToVertical,
                [PageStretchMode.UniformToHorizontal] = IsEnableUniformToHorizontal,
            };
        }
    }

}
