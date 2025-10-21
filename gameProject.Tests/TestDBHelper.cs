using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using gameProject.Data;
using Moq;

namespace gameProject.Tests.Helpers
{
    public class TestDbHelper
    {
        public static ApplicationDbContext CreateInMemoryDbContext(string databaseName = "TestDb")
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            return new ApplicationDbContext(options);
        }

        public static ILogger<T> CreateMockLogger<T>()
        {
            return new Mock<ILogger<T>>().Object;
        }
    }
}