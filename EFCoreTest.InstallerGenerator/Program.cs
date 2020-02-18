using EFCore.Extensions;
using EFCoreTest.EFDAL;

namespace EFCoreTest.InstallerGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var startup = new TenantContextStartup("jsmith", "qqq");
            using (var context = new DataContext(startup, "NO_CONNECTIONSTRING"))
            {
                //Force model load
                context.ChangeTracker.AcceptAllChanges();

                var generator = new EFCore.Extensions.DbManagement.InstallerGenerator();

                //SQL
                using (var scriptProvider = new EFCore.Extensions.Scripting.SqlServer.SqlServerGeneration(context)) 
                {
                    var rootPath = @"C:\code\ExtensionsLibrary\EFCoreTest.SqlInstaller\";
                    generator.Run(rootPath, context, scriptProvider);
                }

                //Postgres
                using (var scriptProvider = new EFCore.Extensions.Scripting.Postgres.PostgresGeneration(context))
                {
                    var rootPath = @"C:\code\ExtensionsLibrary\EFCoreTest.PostgresInstaller\";
                    generator.Run(rootPath, context, scriptProvider);
                }
            }
        }
    }
}
