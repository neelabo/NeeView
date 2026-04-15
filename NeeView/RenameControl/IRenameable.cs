using System.Threading.Tasks;

namespace NeeView
{
    public interface IRenameable
    {
        /// <summary>
        /// rename source text
        /// </summary>
        string GetRenameText();

        /// <summary>
        /// can rename?
        /// </summary>
        /// <returns>can rename</returns>
        bool CanRename();

        /// <summary>
        /// rename
        /// </summary>
        /// <param name="name">new name</param>
        /// <returns>is success</returns>
        Task<bool> RenameAsync(string name);
    }
}
