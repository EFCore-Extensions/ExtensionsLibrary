using EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using EFCoreTest.EFDAL.Entity;

namespace EFCoreTest.EFDAL
{
    /// <summary>
    /// This is static data for the 'CustomerType' entity
    /// </summary>
    public enum CustomerTypeConstants
    {
        Big = 1,
        Little = 2,
    }

    public class DataContext : EFCore.Extensions.ContextBase
    {
        public DataContext()
            : base()
        {
        }

        public DataContext(string connectionString)
            : base(connectionString)
        {
        }

        public DataContext(ContextStartup startup)
            : base(startup)
        {
        }

        public DataContext(ContextStartup startup, string connectionString)
            : base(startup, connectionString)
        {
        }

        public override Guid ModelKey => new Guid("6A423908-E7C7-4896-9B66-E1DE56F84042");

        public virtual DbSet<Customer> Customer { get; protected set; }
        public virtual DbSet<Order> Order { get; protected set; }
        public virtual DbSet<BasicTenant> BasicTenant { get; protected set; }
        public virtual DbSet<Category> TenantMaster { get; protected set; }
        public virtual DbSet<CustomerType> CustomerType { get; protected set; }
        public virtual DbSet<Car> Car { get; protected set; }
        public virtual DbSet<CodeManagedKey> CodeManagedKey { get; protected set; }
        public virtual DbSet<BigEntity> BigEntity { get; protected set; }
        public virtual DbSet<HeapTable> HeapTable { get; protected set; }
        public virtual DbSet<CompositeStuff> CompositeStuff { get; protected set; }
        public virtual DbSet<SchemaTest> SchemaTest { get; protected set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseLazyLoadingProxies()
                    .UseSqlServer(_connectionString);
            }
        }

    }

}