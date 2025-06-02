namespace NeeView
{
    public static class BookmarkTools
    {
        public static string GetValidateName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            return name.Trim().Replace('/', '_').Replace('\\', '_');
        }
    }

}
