using EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
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