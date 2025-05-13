using System.Collections.Generic;


namespace NeeView
{
    public interface IBreadcrumbProfile
    {
        string GetDisplayName(string s, int index)
        {
            return s;
        }

        List<string> GetDirectories(string path);
    }

}
