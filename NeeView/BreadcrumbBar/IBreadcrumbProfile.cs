using System.Collections.Generic;
using System.Threading;


namespace NeeView
{
    public interface IBreadcrumbProfile
    {
        string GetDisplayName(string s, int index)
        {
            return s;
        }

        List<BreadcrumbToken> GetChildren(string path, int index, CancellationToken token);

        bool CanHasChild(string path, int index)
        {
            return true;
        }
    }

}
