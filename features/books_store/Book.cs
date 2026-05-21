namespace BooksStore;
public class Book
{
    private readonly string _id;
    private readonly string _title;
    private readonly string _subTitle;
    private readonly string[] _authors;
    private readonly string _publisher;
    private readonly int _publishedYear;
    private readonly int _pages;
    private readonly double _price;

    public string Id => _id;
    public string Title => _title;
    public string SubTitle => _subTitle;
    public string Authors => string.Join(", ", _authors);
    public string Publisher => _publisher;
    public int PublishedYear => _publishedYear;
    public int Pages => _pages;
    public double Price => _price;
    public int Age => DateTime.Now.Year - _publishedYear;

    public Book(
        string id,
        string title,
        string subTitle,
        string[] authors,
        string publisher,
        int publishedYear,
        int pages = 50,
        double price = 10.0
    )
    {
        _id = id;
        _title = title;
        _subTitle = subTitle;
        _authors = authors;
        _publisher = publisher;
        _publishedYear = publishedYear;
        _pages = pages;
        _price = price;
    }

    public override string ToString() =>
        $"{Title} - ({SubTitle}), authored by ({string.Join(", ", Authors)}), published by ({Publisher}), on [{PublishedYear}], with ({Pages}) pages and cost (${Price:F2}).";
    public override int GetHashCode()
    {
        return HashCode.Combine(_id, _title, _publisher, _publishedYear, _pages, _price);
    }
    public override bool Equals(object? obj)
    {
        if (obj is Book book)
            return book._id == _id && book._title == _title &&
                book._subTitle == _subTitle && book.Authors == Authors &&
                book._publisher == _publisher && book._publishedYear == _publishedYear &&
                book._pages == _pages && book._price == _price;
        return false;
    }

}
