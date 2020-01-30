using EFCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace UnitTestProject
{
    /// <summary>
    /// Create a wrapper context that uses the in-memory database provider
    /// </summary>
    public class TestContext : EFCoreTest.EFDAL.DataContext
    {
        public TestContext()
            : base()
        {
        }

        public TestContext(string connectionString)
            : base(connectionString)
        {
        }

        public TestContext(ContextStartup startup)
            : base(startup)
        {
        }

        public TestContext(ContextStartup startup, string connectionString)
            : base(startup, connectionString)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLazyLoadingProxies()
                .UseInMemoryDatabase(_connectionString);

            //This this afterwards as then this object is already configured
            base.OnConfiguring(optionsBuilder);
        }
    }
}
