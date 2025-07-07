using System.Linq;

namespace NeeView
{
    [WordNodeMember]
    public class ExternalAppCollectionAccessor
    {
        private readonly ExternalAppCollection _collection;

        public ExternalAppCollectionAccessor()
        {
            _collection = Config.Current.System.ExternalAppCollection;
        }

        [WordNodeMember]
        public ExternalAppAccessor[] Items
        {
            get { return _collection.Select(e => new ExternalAppAccessor(e)).ToArray(); }
        }


        [WordNodeMember]
        public ExternalAppAccessor CreateNew()
        {
            return AppDispatcher.Invoke(() => { return new ExternalAppAccessor(_collection.CreateNew()); });
        }

        [WordNodeMember]
        public void Remove(ExternalAppAccessor item)
        {
            AppDispatcher.Invoke(() => _collection.Remove(item.Source));
        }

    }
}
