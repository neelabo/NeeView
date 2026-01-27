using NeeLaboratory.ComponentModel;
using System.Diagnostics;

namespace NeeView
{
    public class FileInformationRecord : BindableBase, IHasKey<FileInformationKey>
    {
        private object? _value;


        public FileInformationRecord(InformationKey key, object? value)
        {
            Debug.Assert(key != InformationKey.ExtraValue);

            Key = new FileInformationKey(key);
            Group = key.ToInformationGroup();
            _value = value;
        }

        public FileInformationRecord(string name, InformationGroup group, object? value)
        {
            Key = new FileInformationKey(name);
            Group = group;
            Value = value;
        }

        public FileInformationKey Key { get; private set; }

        public InformationGroup Group { get; private set; }

        public object? Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value); }
        }

        public FileInformationRecord Clone()
        {
            return (FileInformationRecord)MemberwiseClone();
        }

        public override string ToString()
        {
            return $"Key = {Key}, Value = {Value}";
        }
    }
}
