namespace NeeView
{

    public static class BookSettingConfigExtensions
    {
        public static BookMemento ToBookMemento(this BookSettingConfig self)
        {
            var memento = new BookMemento();

            memento.Page = self.Page;
            memento.PageMode = self.PageMode;
            memento.BookReadOrder = self.BookReadOrder;
            memento.IsSupportedDividePage = self.IsSupportedDividePage;
            memento.IsSupportedSingleFirstPage = self.IsSupportedSingleFirstPage;
            memento.IsSupportedSingleLastPage = self.IsSupportedSingleLastPage;
            memento.IsSupportedWidePage = self.IsSupportedWidePage;
            memento.IsRecursiveFolder = self.IsRecursiveFolder;
            memento.SortMode = self.SortMode;
            memento.SortSeed = 0;
            memento.AutoRotate = self.AutoRotate;
            memento.BaseScale = self.BaseScale;

            return memento;
        }

        public static void Restore(this BookSettingConfig self, BookMemento? memento)
        {
            if (memento == null) return;

            self.Page = memento.Page ?? "";
            self.PageMode = memento.PageMode;
            self.BookReadOrder = memento.BookReadOrder;
            self.IsSupportedDividePage = memento.IsSupportedDividePage;
            self.IsSupportedSingleFirstPage = memento.IsSupportedSingleFirstPage;
            self.IsSupportedSingleLastPage = memento.IsSupportedSingleLastPage;
            self.IsSupportedWidePage = memento.IsSupportedWidePage;
            self.IsRecursiveFolder = memento.IsRecursiveFolder;
            self.SortMode = memento.SortMode;
            self.AutoRotate = memento.AutoRotate;
            self.BaseScale = memento.BaseScale;
        }

        public static void CopyTo(this BookSettingConfig self, BookSettingConfig target)
        {
            target.Page = self.Page;
            target.PageMode = self.PageMode;
            target.BookReadOrder = self.BookReadOrder;
            target.IsSupportedDividePage = self.IsSupportedDividePage;
            target.IsSupportedSingleFirstPage = self.IsSupportedSingleFirstPage;
            target.IsSupportedSingleLastPage = self.IsSupportedSingleLastPage;
            target.IsSupportedWidePage = self.IsSupportedWidePage;
            target.IsRecursiveFolder = self.IsRecursiveFolder;
            target.SortMode = self.SortMode;
            target.AutoRotate = self.AutoRotate;
            target.BaseScale = self.BaseScale;
        }
    }

}
