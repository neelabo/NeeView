using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public class FileAssociationAccessorCollection : List<FileAssociationAccessor>
    {
        private readonly FileAssociationIconBitmapCache _cache = new();

        public FileAssociationAccessorCollection(FileAssociationCollection source)
        {
            this.AddRange(source.Select(e => new FileAssociationAccessor(e, _cache)));
        }


        public void Flush()
        {
            List<Exception> exceptions = new();
            bool isChanged = false;
            foreach (var item in this)
            {
                try
                {
                    isChanged |= item.Flush();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            if (isChanged)
            {
                FileAssociationTools.RefreshShellIcons();
            }
            if (exceptions.Count > 0)
            {
                var message = string.Join(System.Environment.NewLine, exceptions.Select(e => e.Message));
                ToastService.Current.Show("FileAssociation", new Toast(message, null, ToastIcon.Error));
            }
        }

        public void InitializeValue()
        {
            foreach (var item in this)
            {
                item.InitializeValue();
            }
        }
    }

}