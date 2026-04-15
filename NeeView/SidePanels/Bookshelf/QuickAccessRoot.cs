using NeeView.Properties;
using System.Threading.Tasks;

namespace NeeView
{
    public class QuickAccessRoot : QuickAccessFolder
    {
        public QuickAccessRoot()
        {
            Name = TextResources.GetString("Word.QuickAccess");
        }

        public override bool CanRename()
        {
            return false;
        }

        public override Task<bool> RenameAsync(string name)
        {
            return Task.FromResult(false);
        }
    }

}
