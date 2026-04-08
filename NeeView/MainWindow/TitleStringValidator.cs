using System.Collections.Generic;

namespace NeeView
{
    public static class TitleStringValidator
    {
        private readonly record struct ReplaceItem(string Old, string New);

        private static readonly List<ReplaceItem> _replaceList = new()
        {
            new("$Book", "{Book}"),
            new("$PageMax", "{PageMax}"),
            new("$PageL", "{PageL}"),
            new("$PageR", "{PageR}"),
            new("$Page", "{Page}{Part: (#)}"),
            new("$FullPathL", "{FullPathL:/ > }"),
            new("$FullPathR", "{FullPathR:/ > }"),
            new("$FullPath", "{FullPath:/ > }"),
            new("$FullNameL", "{EntryPathL:/ > }"),
            new("$FullNameR", "{EntryPathR:/ > }"),
            new("$FullName", "{EntryPath:/ > }"),
            new("$NameL", "{NameL}"),
            new("$NameR", "{NameR}"),
            new("$Name", "{Name}"),
            new("$SizeExL", "{SizeL}{BitsL: x #}"),
            new("$SizeExR", "{SizeR}{BitsR: x #}"),
            new("$SizeEx", "{Size}{Bits: x #}"),
            new("$SizeL", "{SizeL}"),
            new("$SizeR", "{SizeR}"),
            new("$Size", "{Size}"),
            new("$ViewScale", "{ViewScale:#%}"),
            new("$ScaleL", "{ScaleL:#%}"),
            new("$ScaleR", "{ScaleR:#%}"),
            new("$Scale", "{Scale:#%}"),
        };


        public static string ValidateVersion46(string format)
        {
            // v46.0+
            foreach (var pair in _replaceList)
            {
                format = format.Replace(pair.Old, pair.New);
            }
            return format;
        }
    }
}
