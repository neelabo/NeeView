using Generator.Equals;
using NeeView.Windows.Property;

namespace NeeView
{
    [Equatable(Explicit = true)]
    public partial class ExportBookParameter : ExportImageParameter
    {
        [DefaultEquality] private ExportBookType _bookType = ExportBookType.Zip;

        public const string DefaultBookFileNameFormat0 = "{Index:000}";
        public const string DefaultBookFileNameFormat1 = "{Index:000}";
        public const string DefaultBookFileNameFormat2 = "";

        public ExportBookParameter()
        {
            OverwriteMode = ExportImageOverwriteMode.AddNumber;

            FileNameFormat0 = DefaultBookFileNameFormat0;
            FileNameFormat1 = DefaultBookFileNameFormat1;
            FileNameFormat2 = DefaultBookFileNameFormat2;
        }

        public ExportBookParameter(IExportImageParameter parameter, ExportBookType bookType) : base(parameter)
        {
            _bookType = bookType;
        }

        [PropertyMember]
        public ExportBookType BookType
        {
            get => _bookType;
            set => SetProperty(ref _bookType, value);
        }
    }
}
