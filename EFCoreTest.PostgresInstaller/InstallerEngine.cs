using System;
using EFCore.Extensions.DbManagement;
using Microsoft.EntityFrameworkCore;

namespace EFCoreTest.PostgresInstaller
{
    public class InstallerEngine : ManagementEngine
    {
        public InstallerEngine(System.Reflection.Assembly assembly)
            : base(assembly)
        {
        }

        public override void Install(Guid modelKey, string connectionString)
        {
            this.Options = new DbContextOptionsBuilder<DbManagementContext>()
                .UseNpgsql(connectionString)
                .Options;

            base.Install(modelKey, connectionString);
        }

        protected override string BreakLine => "--GO";
    }
}
