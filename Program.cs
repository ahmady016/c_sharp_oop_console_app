static class Program
{
    static async Task Main()
    {
        await BooksStore.BooksStoreManager.Run();
        // CourseManagement.CourseManager.Run();
        // PersonTest.Run();
    }

}
