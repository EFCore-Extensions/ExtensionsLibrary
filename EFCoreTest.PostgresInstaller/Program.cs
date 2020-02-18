using System;

namespace EFCoreTest.PostgresInstaller
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = @"server=.;initial catalog=Test79;Integrated Security=SSPI;";
            var installer = new InstallerEngine(System.Reflection.Assembly.GetExecutingAssembly());
            var modelKey = new Guid("6A423908-E7C7-4896-9B66-E1DE56F84042");
            installer.Install(modelKey, connectionString);
            Console.WriteLine("Install Complete");
        }
    }
}
