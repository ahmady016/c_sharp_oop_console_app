using System.Collections;

namespace BooksStore;

public class BooksStore: ICloneable, IEnumerable<Book>
{
    #region private fields to store the properties of the BooksStore
    private readonly string _id;
    private readonly string _name;
    private readonly string _location;
    private readonly Dictionary<string, Book> _books = [];
    #endregion

    #region public properties to expose the fields of the BooksStore
    public string Id => _id;
    public string Name => _name;
    public string Location => _location;
    public IReadOnlyDictionary<string, Book> Books => _books;
    #endregion

    #region public computed properties to provide aggregated information about the books
    public List<Book> List => [..this];
    public int BooksCount => _books.Count;
    public int AuthorsCount => GetAuthors().Count;
    public int PublishersCount => GetPublishers().Count;
    public int TotalPages => this.Sum(book => book.Pages);
    public double TotalPrice => this.Sum(book => book.Price);
    public int AveragePages => BooksCount > 0 ? TotalPages / BooksCount : 0;
    public double AveragePrice => BooksCount > 0 ? TotalPrice / BooksCount : 0.0;
    #endregion

    #region public properties to perform various queries on the books in the store
    // get the most expensive, cheapest, thickest, thinnest, oldest, and newest books
    public Book? MostExpensiveBook => (BooksCount > 0)
        ? (from book in this orderby book.Price descending select book).FirstOrDefault()
        : null;
    public Book? CheapestBook => (BooksCount > 0)
        ? (from book in this orderby book.Price ascending select book).FirstOrDefault()
        : null;
    public Book? ThickestBook => (BooksCount > 0)
        ? (from book in this orderby book.Pages descending select book).FirstOrDefault()
        : null;
    public Book? ThinnestBook => (BooksCount > 0)
        ? (from book in this orderby book.Pages ascending select book).FirstOrDefault()
        : null;
    public Book? OldestBook => (BooksCount > 0)
        ? (from book in this orderby book.PublishedYear ascending select book).FirstOrDefault()
        : null;
    public Book? NewestBook => (BooksCount > 0)
        ? (from book in this orderby book.PublishedYear descending select book).FirstOrDefault()
        : null;
    #endregion

    #region indexer to get and set books in the store indexed by their id
    public Book this[string id]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(id.Trim(), nameof(id));
            if (!_books.TryGetValue(id.Trim(), out var book))
                throw new KeyNotFoundException($"Book with id '{id}' not found in the store.");
            return book;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(id.Trim(), nameof(id));
            ArgumentNullException.ThrowIfNull(value, nameof(value));
            if(id.Trim() != value.Id)
                throw new ArgumentException($"The indexer key [{id}] does not match the book's id '{value.Id}'.");
            // add new book if the id does not exist
            // or update the existing book with the new value
            _books[id.Trim()] = value;
        }
    }
    #endregion

    #region primary constructor to initialize the name and location
    public BooksStore(string name, string location)
    {
        _id = Helpers.GenerateId(10);
        _name = name;
        _location = location;
    }
    #endregion

    #region secondary constructor to initialize the name, location, and books
    public BooksStore(
        string name,
        string location,
        Dictionary<string, Book> books
    ) : this(name, location)
    {
        _books = books;
    }
    #endregion

    #region overriding Object methods (string representation, hashing, equality comparison)
    // implementing ToString method to provide a string representation of the BooksStore
    public override string ToString() =>
        $"{Name} located at {Location} and holds ({Books.Count}) books from ({PublishersCount}) publishers";
    // implementing GetHashCode method to generate a hash code for the BooksStore
    public override int GetHashCode() => HashCode.Combine(_id, _name, _location, _books.Count);
    // implementing Equals method to compare two BooksStores instances
    // based on their properties and the number of books they hold
    // ignoring the actual book details for simplicity
    public override bool Equals(object? obj)
    {
        if (obj is BooksStore store)
            return store._id == _id && store._name == _name &&
                store._location == _location && store._books.Count == _books.Count;
        return false;
    }
    #endregion

    #region implementing equality and comparison operators for BooksStores
    // based on the number of books they hold, and equality based on their properties
    public static bool operator ==(BooksStore? left, BooksStore? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }
    public static bool operator !=(BooksStore? left, BooksStore? right) => !(left == right);
    public static bool operator >(BooksStore left, BooksStore right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.Books.Count > right.Books.Count;
    }
    public static bool operator <(BooksStore left, BooksStore right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.Books.Count < right.Books.Count;
    }
    public static bool operator >=(BooksStore left, BooksStore right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.Books.Count >= right.Books.Count;
    }
    public static bool operator <=(BooksStore left, BooksStore right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return left.Books.Count <= right.Books.Count;
    }
    #endregion

    #region implementing binary operators for combining or subtracting BooksStores
    // private helper method to merge the books of two BooksStores into a new dictionary
    private Dictionary<string, Book> MergeBooksWith(BooksStore right)
    {
        var mergedBooks = new Dictionary<string, Book>(_books);
        foreach (var pair in right._books)
            mergedBooks[pair.Key] = pair.Value;
        return mergedBooks;
    }
    public static BooksStore operator +(BooksStore left, BooksStore right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        string newName = (left._name == right._name)
            ? left._name
            : $"{left._name} & {right._name}";
        string newLocation = (left._location == right._location)
            ? left._location
            : $"{left._location} & {right._location}";
        return new BooksStore(newName, newLocation, left.MergeBooksWith(right));
    }
    public static BooksStore operator -(BooksStore left, BooksStore right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        string newName = left._name.Contains(right._name) && left._name.Contains(" & ")
            ? left._name.Replace($" & {right._name}", "")
            : $"{left._name} - {right._name}";
        string newLocation = left._location.Contains(right._location) && left._location.Contains(" & ")
            ? left._location.Replace($" & {right._location}", "")
            : $"{left._location} - {right._location}";
        var newBooks = new Dictionary<string, Book>(left._books);
        foreach (var pair in right._books)
            newBooks.Remove(pair.Key);
        return new BooksStore(newName, newLocation, newBooks);
    }
    #endregion

    #region implementing ICloneable to allow cloning of the BooksStore instance
    public object Clone() =>
        new BooksStore(_name, _location, new Dictionary<string, Book>(_books));
    #endregion

    #region implementing IEnumerable<Book> to allow iteration over the books in the store
    public IEnumerator<Book> GetEnumerator()
    {
        foreach (var book in _books.Values)
            yield return book;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion

    #region public operations based on indexer and enumerable implementation
    public Book GetBook(string id) => this[id];
    public void AddBook(Book book) => this[book.Id] = book;
    public void AddBooks(IEnumerable<Book> books)
    {
        ArgumentNullException.ThrowIfNull(books, nameof(books));
        foreach (var book in books)
            this[book.Id] = book;
    }
    public bool RemoveBook(string id)
    {
        ArgumentNullException.ThrowIfNull(id.Trim(), nameof(id));
        return _books.Remove(id.Trim());
    }
    public bool RemoveBooks(IEnumerable<string> ids)
    {
        ArgumentNullException.ThrowIfNull(ids, nameof(ids));
        bool allRemoved = true;
        foreach (var id in ids)
            allRemoved &= _books.Remove(id.Trim());
        return allRemoved;
    }
    #endregion

    #region public helper methods for mapping, searching, and filtering books
    // helper method to get a list of all unique authors from the books in the store
    // using LINQ query syntax to iterate over the books and their authors
    // and collect them in a HashSet to ensure uniqueness
    public List<string> GetAuthors()
    {
        HashSet<string> authorsSet = [..
            from book in this
            from author in book.AuthorsList
            where !string.IsNullOrWhiteSpace(author)
            select author.Trim()
        ];
        return [..authorsSet];
    }
    // helper method to get a list of all unique publishers from the books in the store
    // using LINQ query syntax to iterate over the books and select their publishers
    // and collect them in a HashSet to ensure uniqueness
    public List<string> GetPublishers()
    {
        HashSet<string> publishersSet = [..
            from book in this
            where !string.IsNullOrWhiteSpace(book.Publisher)
            select book.Publisher.Trim()
        ];
        return [..publishersSet];
    }

    // additional helper methods for searching and filtering books
    // by title, author, publisher, published year, and price range
    public List<Book> BooksByTitle(string title)
    {
        ArgumentNullException.ThrowIfNull(title, nameof(title));
        return [..
            from book in this
            where book.Title.Contains(title, StringComparison.OrdinalIgnoreCase)
            select book
        ];
    }
    public List<Book> AuthorBooks(string author)
    {
        ArgumentNullException.ThrowIfNull(author.Trim(), nameof(author));
        return [..
            from book in this
            where book.AuthorsList.Any(existedAuthor => author.Trim().Equals(existedAuthor, StringComparison.OrdinalIgnoreCase))
            select book
        ];
    }
    public List<Book> PublisherBooks(string publisher)
    {
        ArgumentNullException.ThrowIfNull(publisher.Trim(), nameof(publisher));
        return [..
            from book in this
            where book.Publisher.Equals(publisher, StringComparison.OrdinalIgnoreCase)
            select book
        ];
    }
    public List<Book> PublishedInYear(int? year)
    {
        int yearToSearch = year ?? DateTime.Now.Year;
        return [..
            from book in this
            where book.PublishedYear == yearToSearch
            select book
        ];
    }
    public List<Book> BooksInPriceRange(double? min, double? max)
    {
        double minToSearch = min ?? 0.0;
        double maxToSearch = max ?? 100.0;
        return [..
            from book in this
            where book.Price >= minToSearch && book.Price <= maxToSearch
            select book
        ];
    }
    #endregion

}
