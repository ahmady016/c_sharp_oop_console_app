using System.Runtime.CompilerServices;

namespace BooksStore;

public static class BooksStoreManager
{
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
            ? [..from author in authorsArray where author != null select author]
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

    public static async Task Run()
    {
        try
        {
            var rawJsonBooks = await Helpers.ReadJsonFile(SameDirectory("books.json"));
            List<Book> books = [..from dict in rawJsonBooks select MapToBook(dict)];

            Console.WriteLine("-------------------------");
            Console.WriteLine($"({books.Count}) Books loaded from JSON file:");
            Console.WriteLine("-------------------------");
            foreach (var book in books.Take(10))
                Console.WriteLine(book);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

}
