using System;
using EFCore.Extensions.DbManagement;
using Microsoft.EntityFrameworkCore;

namespace EFCoreTest.SqlInstaller
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
                .UseSqlServer(connectionString)
                .Options;

            base.Install(modelKey, connectionString);
        }

        protected override string BreakLine => "GO";
    }
}
