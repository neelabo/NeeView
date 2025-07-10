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

        public override ValueTask<bool> RenameAsync(string name)
        {
            return ValueTask.FromResult(false);
        }
    }

}
