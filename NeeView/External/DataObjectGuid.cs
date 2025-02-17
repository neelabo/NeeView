using System;
using System.Windows;

namespace NeeView
{
    public static class DataObjectGuid
    {
        public static string Format { get; } = "NeeView.Guid";

        public static void SetGuid(this IDataObject data, Guid guid)
        {
            data.SetData(Format, guid.ToString());
        }

        public static Guid GetGuid(this IDataObject data)
        {
            if (data.GetDataPresent(Format))
            {
                return Parse(data.GetData(Format));
            }
            return Guid.Empty;
        }

        public static Guid Parse(object? raw)
        {
            if (raw is string s)
            {
                if (Guid.TryParse(s, out Guid guid))
                {
                    return guid;
                }
            }
            return Guid.Empty;
        }
    }
}
