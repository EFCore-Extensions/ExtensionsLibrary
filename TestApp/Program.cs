using EFCoreTest.EFDAL;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EFCore.Extensions;
using EFCore.Extensions.Scripting;
using EFCoreTest.EFDAL.Entity;
using System.IO;

namespace ConsoleApp1
{
    class Program
    {
        //Create a database and get the create script from the "GenerateSqlScript" method below
        private const string connectionString = @"server=.;initial catalog=EFCoreTest;integrated security=SSPI;";

        //Create 2 tenants
        private const string tenantId1 = "Tenant1";
        private const string tenantId2 = "Tenant2";

        static void Main(string[] args)
        {
            GenerateSqlScript();
            TestCodeManaged();
            TestSoftDelete();
            TestBasicTenant();
            TestMultipleTenant();
            TestStaticData();
            ConcurrencyTest();

            Console.WriteLine("Press <ENTER> to end...");
            Console.ReadLine();
        }

        private static void GenerateSqlScript()
        {
            //Create an audit tracking configuration
            var startup = new TenantContextStartup("jsmith", tenantId1);

            //Create SQL Script
            using (var context = new DataContext(startup, "NO_CONNECTIONSTRING"))
            {
                //Force model load
                context.ChangeTracker.AcceptAllChanges();

                var rootPath = @"C:\code\ExtensionsLibrary\EFCoreTest.SqlInstaller\";
                var modelPath = Path.Combine(rootPath, "Models");
                var scriptPath = Path.Combine(rootPath, "Migrations");
                var createPath = Path.Combine(rootPath, "Create");
                if (!Directory.Exists(modelPath)) Directory.CreateDirectory(modelPath);
                if (!Directory.Exists(scriptPath)) Directory.CreateDirectory(scriptPath);
                if (!Directory.Exists(createPath)) Directory.CreateDirectory(createPath);

                var gen = new SqlServerGeneration(context);

                //Create SQL generation object
                var sqlCreate = gen.GenerateCreateScript(); //EXECUTE THIS SQL BLOCK TO CREATE DATABASE
                File.WriteAllText(Path.Combine(createPath, "Create.sql"), sqlCreate);

                //Load version file
                var versionFile = Path.Combine(modelPath, "version.json");
                var oldVersion = new Versioning();
                if (File.Exists(versionFile))
                {
                    oldVersion = ScriptingExtensions.FromJson<Versioning>(File.ReadAllText(versionFile));
                }
                var newVersion = new Versioning(oldVersion.ToString());
                newVersion.Increment();
                File.WriteAllText(versionFile, newVersion.ToJson());

                //Load last model (if one)
                DataModel oldModel = null;
                var oldVersionFile = Path.Combine(modelPath, oldVersion.GetDiffFileName()) + ".model";
                if (File.Exists(oldVersionFile))
                {
                    oldModel = ScriptingExtensions.FromJson<DataModel>(File.ReadAllText(oldVersionFile)); 
                }

                //Diff Script
                if (oldModel != null)
                {
                    var sqlDiff = gen.GenerateDiffScript(oldModel);
                    File.WriteAllText(Path.Combine(scriptPath, newVersion.GetDiffFileName() + ".sql"), sqlDiff);
                }

                //Write model to installer project
                var modelJson = gen.Model.ToJson();
                File.WriteAllText(Path.Combine(modelPath, newVersion.GetDiffFileName()) + ".model", modelJson);

                //Save this model
                //TODO

                //Save to file for review
                //System.IO.File.WriteAllText(@"c:\temp\test.sql", sql);

                //This will create a class diagram "dgml" file.
                //var classGen = new ClassDiagramGeneration(context);
                //var xml = classGen.Generate();
                //System.IO.File.WriteAllText(@"c:\temp\test8.dgml", xml);
            }

        }

        private static void TestCodeManaged()
        {
            var rnd = new Random();
            var startup = new TenantContextStartup("jsmith", tenantId1);
            using (var context = new DataContext(startup, connectionString))
            {
                for (var ii = 0; ii < 10; ii++)
                {
                    //The primary key is not an identity. It must be set in code
                    var newItem = new CodeManagedKey
                    {
                        ID = rnd.Next(10, 999999),
                        Name = $"Hello {DateTime.Now.Ticks}",
                        Data = "hello",
                    };
                    context.Add(newItem);
                }
                context.SaveChanges();

                //Test Versioning. It goes up by +1 on each save
                var item = context.CodeManagedKey.FirstOrDefault();
                Console.WriteLine($"Version #: {item.Version}");
                item.Name = DateTime.Now.Ticks.ToString();
                context.SaveChanges();
                Console.WriteLine($"Version #: {item.Version}");
                item.Name = DateTime.Now.Ticks.ToString();
                context.SaveChanges();
                Console.WriteLine($"Version #: {item.Version}");

            }
        }

        private static void TestSoftDelete()
        {
            var startup = new TenantContextStartup("jsmith", tenantId1);
            using (var context = new DataContext(startup, connectionString))
            {
                //Create 10 cars and save
                for (var ii = 1; ii <= 10; ii++)
                {
                    context.Add(new Car { Name = $"Car {ii}" });
                }
                context.SaveChanges();

                //Load all cars from database
                var carList = context.Car.ToList();
                Console.WriteLine($"Car count={carList.Count}");

                //Delete first car and save
                carList.First().Delete();
                context.SaveChanges();

                //Select all cars again with no where clause and notice there is 1 less
                carList = context.Car.ToList();
                Console.WriteLine($"Car count={carList.Count}");
            }

        }

        private static void TestMultipleTenant()
        {
            //Tenant 1 insert 10 items
            using (var context = new DataContext(new TenantContextStartup("jsmith", tenantId1), connectionString))
            {
                if (!context.Customer.Any())
                {
                    for (var ii = 1; ii <= 10; ii++)
                    {
                        context.Add(new Customer { Name1 = $"Customer {ii}", CustomerId = Guid.NewGuid(), CustomerTypeValue = CustomerTypeConstants.Big });
                        Console.WriteLine($"Added Customer {ii}");
                    }
                    context.SaveChanges();
                }
            }

            //Tenant 2 insert 10 items
            using (var context = new DataContext(new TenantContextStartup("jsmith", tenantId2), connectionString))
            {
                if (!context.Customer.Any())
                {
                    for (var ii = 1; ii <= 10; ii++)
                    {
                        context.Add(new Customer { Name1 = $"Customer {ii}", CustomerId = Guid.NewGuid(), CustomerTypeValue = CustomerTypeConstants.Little });
                        Console.WriteLine($"Added Customer {ii}");
                    }
                    context.SaveChanges();
                }
            }

            //Select all records from Tenant 1 with no where statement.
            using (var context = new DataContext(new TenantContextStartup("jsmith", tenantId1), connectionString))
            {
                var list = context.Customer
                    .Include(x => x.OrderList)
                    .Include(x => x.CustomerType)
                    .ToList();

                //The records for Tenant 2 are not found
                Console.WriteLine($"Found in Tenant 1, Count={list.Count}");
                context.SaveChanges();
            }

        }

        private static void TestStaticData()
        {
            //Test Static data table. This is an immuatable entity that cannot be updated
            var startup = new TenantContextStartup("jsmith", tenantId1);
            using (var context = new DataContext(startup, connectionString))
            {
                var list = context.CustomerType.ToList();
                Console.WriteLine($"Static data CustomerType has {list.Count} items");
                foreach (var item in list)
                {
                    Console.WriteLine($"Static data CustomerType value={item.CustomerTypeValue}");
                }
            }

        }

        private static void ConcurrencyTest()
        {
            //Concurrency Test
            var startup = new TenantContextStartup("jsmith", tenantId1);
            using (var context = new DataContext(startup, connectionString))
            {
                //Select the first item
                var item = context.Customer.First();

                //Simulate the item changing in another context (or user, or machine, etc)
                ChangeCustomer(startup, item.CustomerId);

                //The item has been changed now so this object is stale and should throw a concurrency exception
                item.Name1 = DateTime.Now.Ticks.ToString();
                try
                {
                    //The item has changed so it throws a concurrency exception
                    context.SaveChanges();
                    Console.WriteLine("You should never see this!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Concurrency failed as expected.");
                }
            }

        }

        private static void TestBasicTenant()
        {
            //Test a simple tenant table. The discriminator field is handled by the framework
            //The tenant ID is picked up from the "TenantContextStartup" object
            //The entity "TenantId" should never be set manually.
            var startup = new TenantContextStartup("jsmith", tenantId1);
            using (var context = new DataContext(startup, connectionString))
            {
                context.Add(new BasicTenant { Name = $"Hello {DateTime.Now.Ticks}" });
                context.SaveChanges();
            }

        }

        /// <summary>
        /// Helper function to modify a customer to simulate database changes
        /// </summary>
        private static void ChangeCustomer(TenantContextStartup startup, Guid id)
        {
            //Change a Customer item and save to database
            using (var context = new DataContext(startup, connectionString))
            {
                context.Customer.First(x => x.CustomerId == id).Name1 = (DateTime.Now.Ticks - 99999).ToString();
                context.SaveChanges();
            }
        }
    }

}