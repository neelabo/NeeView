using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// 画像情報項目キー (Immutable)
    /// </summary>
    public record class FileInformationKey
    {
        public FileInformationKey(InformationKey key)
        {
            Debug.Assert(key != InformationKey.ExtraValue);

            Key = key;
            Name = "";
        }

        public FileInformationKey(string name)
        {
            Key = InformationKey.ExtraValue;
            Name = name;
        }


        public InformationKey Key { get; private set; }

        public string Name { get; private set; }



        public bool IsExtraValue()
        {
            return Key == InformationKey.ExtraValue;
        }


        public override string ToString()
        {
            if (IsExtraValue())
            {
                return Name;
            }
            else
            {
                return AliasNameExtensions.GetAliasName(Key) ?? Key.ToString();
            }
        }
    }
}
