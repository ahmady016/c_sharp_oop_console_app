static class Program
{
    static async Task Main()
    {
        HttpErrors.HttpErrorsManager.Run();
        // await HotelManagement.HotelManager.Run();
        // await BooksStore.BooksStoreManager.Run();
        // CourseManagement.CourseManager.Run();
        // PersonTest.Run();
    }
}
