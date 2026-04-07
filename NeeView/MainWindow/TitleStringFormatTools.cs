using NeeView.Properties;

namespace NeeView
{
    public static class TitleStringFormatTools
    {
        public static string CreateHelpText()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine(Bold(TextResources.GetString("StringFormat.Note.Basic")));

            sb.AppendLine();

            sb.AppendLine(GetHelpWordText(TitleStringFormatter.BookKey));
            sb.AppendLine(GetHelpWordText(TitleStringFormatter.PageKey));
            sb.AppendLine(GetHelpWordText(TitleStringFormatter.PagePartKey) + " (L, R)");
            sb.AppendLine(GetHelpWordText(TitleStringFormatter.PageMaxKey));
            sb.AppendLine(GetHelpWordText(TitleStringFormatter.NameKey));
            sb.AppendLine(GetHelpWordText(TitleStringFormatter.EntryPathKey));
            sb.AppendLine(GetHelpWordText(TitleStringFormatter.FullPathKey));
            sb.AppendLine(GetHelpWordText(TitleStringFormatter.SizeKey) + " (e.g., 640 × 480)");
            sb.AppendLine(GetHelpWordText(TitleStringFormatter.BitsKey));
            sb.AppendLine(GetHelpWordText(TitleStringFormatter.ViewScaleKey));
            sb.AppendLine(GetHelpWordText(TitleStringFormatter.ScaleKey));
            sb.AppendLine();
            sb.AppendLine("e.g., {Page:000} → 001");
            sb.AppendLine("e.g., {Scale:#%} → 100%");
            sb.AppendLine();
            sb.AppendLine(Bold(TextResources.GetString("StringFormat.Note.Suffix")));
            sb.AppendLine();
            sb.AppendLine(GetHelpSuffixText("1"));
            sb.AppendLine(GetHelpSuffixText("2"));
            sb.AppendLine(GetHelpSuffixText("L"));
            sb.AppendLine(GetHelpSuffixText("R"));
            sb.AppendLine();
            sb.AppendLine("e.g., {Book}_{PageL}-{PageR}");
            sb.AppendLine();
            sb.AppendLine(Bold(TextResources.GetString("StringFormat.Note.String")));
            sb.AppendLine();
            sb.AppendLine("- / ... " + TextResources.GetString("StringFormat.String.Separator"));
            sb.AppendLine("- # ... " + TextResources.GetString("StringFormat.String.Format"));
            sb.AppendLine();
            sb.AppendLine("e.g., {FullPath:/ > } → C: > Foo > Bar.jpg");
            sb.AppendLine("e.g., {Name}{PagePart:(#)} →  Bar.jpg(L)");
            sb.Append("e.g., {Size}{Bits: x #} → 640 x 480 x 24");

            return sb.ToString();
        }

        private static string Bold(string s)
        {
            return "<b>" + s + "</b>";
        }

        private static string GetHelpWordText(string word)
        {
            return $"- {word} ... {TextResources.GetString("StringFormat." + word)}";
        }

        private static string GetHelpSuffixText(string suffix)
        {
            return $"- {suffix} ... {TextResources.GetString("StringFormat.Suffix." + suffix)}";
        }
    }

}
