static class Program
{
    static async Task Main()
    {
        await ResumesBuilder.ResumeManager.Run();
        // HttpErrors.HttpErrorsManager.Run();
        // await HotelManagement.HotelManager.Run();
        // await BooksStore.BooksStoreManager.Run();
        // CourseManagement.CourseManager.Run();
        // PersonTest.Run();
    }
}
