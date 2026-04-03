using Generator.Equals;
using NeeView.Windows.Property;
using System.Text.Json.Serialization;

namespace NeeView
{
    [Equatable(Explicit = true)]
    public partial class ExportBookParameter : ExportImageParameter
    {
        [DefaultEquality] private ExportBookType _bookType = ExportBookType.Zip;

        public ExportBookParameter()
        {
            OverwriteMode = ExportImageOverwriteMode.AddNumber;
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
