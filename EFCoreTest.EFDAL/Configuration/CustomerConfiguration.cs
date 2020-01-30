using System;
using System.Collections.Generic;
using System.Text;
using EFCoreTest.EFDAL.Entity;
using Microsoft.EntityFrameworkCore;

namespace EFCoreTest.EFDAL.Configuration
{
    internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Customer> modelBuilder)
        {
            //Relations
            modelBuilder
                .HasMany(a => a.OrderList)
                .WithOne(b => b.Customer)
                .IsRequired(true)
                .HasPrincipalKey(q => new { q.CustomerId })
                .HasForeignKey(u => new { u.CustomerFkId })
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .HasOne(a => a.CustomerType)
                .WithMany()
                .HasPrincipalKey(q => new { q.ID })
                .HasForeignKey(u => new { u.CustomerTypeId })
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
