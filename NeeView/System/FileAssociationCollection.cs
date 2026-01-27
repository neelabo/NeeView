using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class FileAssociationCollection : List<FileAssociation>
    {
        public bool TryAdd(FileAssociation association)
        {
            if (this.Any(e => e.Extension == association.Extension)) return false;
            this.Add(association);
            return true;
        }

        public bool TryAdd(string extension, FileAssociationCategory category)
        {
            if (this.Any(e => e.Extension == extension)) return false;
            this.Add(FileAssociationFactory.Create(extension, category));
            return true;
        }
    }

}