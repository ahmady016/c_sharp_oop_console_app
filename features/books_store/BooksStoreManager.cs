using System.Runtime.CompilerServices;

namespace BooksStore;

public static class BooksStoreManager
{
    // list to store the loaded books from the JSON file
    private static IReadOnlyList<Book> _books = [];

    // 3 different book stores to demonstrate the functionality of the BooksStore class
    // with be filled with some of the loaded books later in the FillBooksStores method
    private static readonly BooksStore _strand = new("The Strand Bookstore", "New York City, USA");
    private static readonly BooksStore _cityLights = new("City Lights Bookstore", "San Francisco, CA, USA");
    private static readonly BooksStore _elliott = new("Elliott Bay Book Company", "Seattle, WA, USA");

    // helper method to get the file path in the same directory as the caller
    // the compiler replaces callerPath with the full path of the caller file at compile time
    // so SameDirectory("books.json") always resolves to the directory of the caller file
    // regardless of where the code is executed (Base Directory, bin folder, etc.)
    // or from or the current working directory
    private static string SameDirectory(
        string fileName, [CallerFilePath] string callerPath = ""
    ) => Path.Combine(Path.GetDirectoryName(callerPath)!, fileName);

    // helper method to map a dictionary of book data to a Book object
    // handle missing keys with (default values or random values)
    // and perform type conversions
    private static Book MapToBook(Dictionary<string, object> bookData)
    {
        string _id = bookData.TryGetValue("id", out var idValue) && idValue is string idString
            ? idString
            : Helpers.GenerateId(12);

        string _title = bookData.TryGetValue("title", out var titleValue) && titleValue is string titleString
            ? titleString
            : "Unknown Title";

        string _subTitle = bookData.TryGetValue("subtitle", out var subTitleValue) && subTitleValue is string subTitleString
            ? subTitleString
            : $"{_title} and {_title}.";

        string _publisher = bookData.TryGetValue("publisher", out var publisherValue) && publisherValue is string publisherString
            ? publisherString
            : "Unknown Publisher";

        string[] _authors = bookData.TryGetValue("authors", out var authorsValue) && authorsValue is string[] authorsArray
            ? [.. from author in authorsArray where author != null select author]
            : [];

        int _publishedYear = bookData.TryGetValue("publishedDate", out var publishedDateValue) && publishedDateValue is string publishedDateString
            ? int.Parse(publishedDateString?[..4] ?? "2000")
            : Random.Shared.Next(1960, DateTime.Now.Year + 1);

        int _pages = bookData.TryGetValue("pageCount", out var pageCountValue) && pageCountValue is int pageCountInt
            ? pageCountInt
            : Random.Shared.Next(50, 2001);

        double _price = bookData.TryGetValue("price", out var priceValue) && priceValue is double priceDouble
            ? priceDouble
            : Random.Shared.NextDouble() * 100;

        return new Book(
            id: _id,
            title: _title,
            subTitle: _subTitle,
            authors: _authors,
            publisher: _publisher,
            publishedYear: _publishedYear,
            pages: _pages,
            price: _price
        );
    }

    // method to load books from a JSON file, map them to Book objects
    // and store them in the _books list
    private static async Task LoadBooks()
    {
        try
        {
            var rawJsonBooks = await Helpers.ReadJsonFile(SameDirectory("books.json"));
            _books = [.. from dict in rawJsonBooks select MapToBook(dict)];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    // method to generate 3 books stores with different names and locations
    // and fill them with some of the books loaded from the JSON file
    private static void FillBooksStores()
    {
        _strand.AddBooks(_books.Take(200));
        _cityLights.AddBooks(_books.Skip(150).Take(150));
        _elliott.AddBooks(_books.Skip(300).Take(150));
    }

    // method to print the given book store details
    private static void PrintBooksStoreDetails(BooksStore store)
    {
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"--- {store.Name} Details ---");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine(store);
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Unique Authors Count: {store.AuthorsCount}");
        Console.WriteLine($"Total Price of Books: ${store.TotalPrice:F2}");
        Console.WriteLine($"Average Price of Books: ${store.AveragePrice:F2}");
        Console.WriteLine($"Total Pages of Books: {store.TotalPages}");
        Console.WriteLine($"Average Pages of Books: {store.AveragePages}");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Most Expensive Book: {store.MostExpensiveBook}");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Cheapest Book: {store.CheapestBook}");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Thickest Book: {store.ThickestBook}");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Thinnest Book: {store.ThinnestBook}");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Oldest Book: {store.OldestBook}");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Newest Book: {store.NewestBook}");
        Console.WriteLine("-----------------------------------------");
    }

    // method to get all books published by a specific publisher
    // from all the stores and print their details
    private static void PrintPublisherBooksDetails(string publisher)
    {
        var booksInStrand = _strand.PublisherBooks(publisher);
        var booksInCityLights = _cityLights.PublisherBooks(publisher);
        var booksInElliott = _elliott.PublisherBooks(publisher);
        List<Book> allPublisherBooks = [..booksInStrand, ..booksInCityLights, ..booksInElliott];
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Books published by ({publisher}):");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Found ({allPublisherBooks.Count}) books published by ({publisher}):");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Found ({booksInStrand.Count}) books in {_strand.Name}.");
        Console.WriteLine($"Found ({booksInCityLights.Count}) books in {_cityLights.Name}.");
        Console.WriteLine($"Found ({booksInElliott.Count}) books in {_elliott.Name}.");
        Console.WriteLine("-----------------------------------------");
        foreach (var book in allPublisherBooks)
        {
            Console.WriteLine($"{book.Title} published in [{book.PublishedYear}] with (${book.Price:F2}) cost and ({book.Pages}) pages.");
            Console.WriteLine("-----------------------------------------");
        }
    }

    // method to get all books with a given price range
    // from all the stores and print their details
    private static void PrintPriceRangeBooksDetails(double minPrice, double maxPrice)
    {
        var booksInStrand = _strand.BooksInPriceRange(minPrice, maxPrice);
        var booksInCityLights = _cityLights.BooksInPriceRange(minPrice, maxPrice);
        var booksInElliott = _elliott.BooksInPriceRange(minPrice, maxPrice);
        List<Book> allPriceRangeBooks = [..booksInStrand, ..booksInCityLights, ..booksInElliott];
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Books with price between (${minPrice:F2}) and (${maxPrice:F2}):");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Found ({allPriceRangeBooks.Count}) books with price between (${minPrice:F2}) and (${maxPrice:F2}):");
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"Found ({booksInStrand.Count}) books in {_strand.Name}.");
        Console.WriteLine($"Found ({booksInCityLights.Count}) books in {_cityLights.Name}.");
        Console.WriteLine($"Found ({booksInElliott.Count}) books in {_elliott.Name}.");
        Console.WriteLine("-----------------------------------------");
        foreach (var book in allPriceRangeBooks)
        {
            Console.WriteLine($"{book.Title} published in [{book.PublishedYear}] with (${book.Price:F2}) cost and ({book.Pages}) pages.");
            Console.WriteLine("-----------------------------------------");
        }
    }
    public static async Task Run()
    {
        await LoadBooks();

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine($"({_books.Count}) Books loaded from JSON file:");
        Console.WriteLine("-----------------------------------------");

        FillBooksStores();

        PrintBooksStoreDetails(_strand);
        PrintBooksStoreDetails(_cityLights);
        PrintBooksStoreDetails(_elliott);

        BooksStore unitedStore = _strand + _cityLights + _elliott;
        PrintBooksStoreDetails(unitedStore);

        PrintPublisherBooksDetails("Packt Publishing Ltd");

        PrintPriceRangeBooksDetails(40.0, 60.0);

        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

}
