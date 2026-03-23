namespace NeeView
{
    public class BookMementoSlim
    {
        public BookMementoSlim()
        {
        }

        public BookMementoSlim(BookMemento memento)
        {
            Path = memento.Path;
            Page = memento.Page;
            Props = memento.ToPropertiesString();
        }

        public string Path { get; set; } = "";

        public string Page { get; set; } = "";

        public string? Props { get; set; }

        public BookMemento? ToBookMemento()
        {
            return BookMemento.ParseWithProperties(Path, Page, Props);
        }

        public static BookMementoSlim? Create(BookMemento? memento)
        {
            if (memento is null)
            {
                return null;
            }

            return new BookMementoSlim(memento);
        }
    }

}

