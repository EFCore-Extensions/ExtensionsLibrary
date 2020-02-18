using EFCore.Extensions;
using EFCoreTest.EFDAL;
using EFCoreTest.EFDAL.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace UnitTestProject
{
    [TestClass]
    public class DatabaseTests
    {
        private string connectionString = "qqq";
        private string TenantA = "AAA";
        private string TenantB = "BBB";

        [TestMethod]
        public void TestSoftDelete()
        {
            var startup = new TenantContextStartup("jsmith", TenantA);
            using (var context = new TestContext(startup, connectionString))
            {
                var startCount = context.Car.Count();

                //Create 10 cars and save
                for (var ii = 1; ii <= 10; ii++)
                {
                    context.Add(new Car { Name = $"Car {ii}" });
                }
                context.SaveChanges();

                //Load all cars from database
                var carList = context.Car.ToList();
                Assert.AreEqual(10 + startCount, carList.Count);

                //Delete first car and save
                carList.First().Delete();
                context.SaveChanges();

                //Select all cars again with no where clause and notice there is 1 less
                carList = context.Car.ToList();
                Assert.AreEqual(9 + startCount, carList.Count);
            }
            
        }

        [TestMethod]
        public void TestTenant1()
        {
            const int TenantACount = 13;
            const int TenantBCount = 8;

            //Create objects for TenantA
            var startup = new TenantContextStartup("jsmith", TenantA);
            using (var context = new TestContext(startup, connectionString))
            {
                //Create cars and save
                for (var ii = 1; ii <= TenantACount; ii++)
                {
                    context.Add(new BasicTenant { Name = $"Car {ii}" });
                }
                context.SaveChanges();
            }

            //Create objects for TenantB
            startup = new TenantContextStartup("jsmith", TenantB);
            using (var context = new TestContext(startup, connectionString))
            {
                //Create 10 cars and save
                for (var ii = 1; ii <= TenantBCount; ii++)
                {
                    context.Add(new BasicTenant { Name = $"Car {ii}" });
                }
                context.SaveChanges();
            }

            //Now select all objects for TenantA
            startup = new TenantContextStartup("jsmith", TenantA);
            using (var context = new TestContext(startup, connectionString))
            {
                var list = context.BasicTenant.ToList();
                Assert.AreEqual(TenantACount, list.Count);
            }

            //Now select all objects for TenantB
            startup = new TenantContextStartup("jsmith", TenantB);
            using (var context = new TestContext(startup, connectionString))
            {
                var list = context.BasicTenant.ToList();
                Assert.AreEqual(TenantBCount, list.Count);
            }

            //Now select all objects for missing tenant
            startup = new TenantContextStartup("jsmith", "some string");
            using (var context = new TestContext(startup, connectionString))
            {
                var list = context.BasicTenant.ToList();
                Assert.AreEqual(0, list.Count);
            }

        }

        [TestMethod]
        public void TestVersion()
        {
            var startup = new TenantContextStartup("jsmith", TenantA);
            using (var context = new TestContext(startup, connectionString))
            {
                var newItem = new CodeManagedKey();
                newItem.Name = DateTime.Now.Ticks.ToString();
                context.Add(newItem);
                context.SaveChanges();
                Assert.AreEqual(1, newItem.Version);

                newItem.Name = DateTime.Now.Ticks.ToString();
                context.SaveChanges();
                Assert.AreEqual(2, newItem.Version);

                newItem.Name = DateTime.Now.Ticks.ToString();
                context.SaveChanges();
                Assert.AreEqual(3, newItem.Version);
            }
        }

        [TestMethod]
        public void TestUnicode()
        {
            //This will never fail on memory tests
            //Must SQL Server or real database to test as memory is always unicode

            var startup = new TenantContextStartup("jsmith", TenantA);
            using (var context = new TestContext(startup, connectionString))
            {
                var cc = "汉字";
                var newItem = new Car { Name = cc };
                context.Add(newItem);
                context.SaveChanges();
                Assert.AreEqual(cc, newItem.Name);
            }
        }

    }
}
