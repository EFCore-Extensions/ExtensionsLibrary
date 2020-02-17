using System;
using System.Collections.Generic;
using System.Text;
using EFCore.DbManagement;
using Microsoft.EntityFrameworkCore;

namespace EFCoreTest.SqlInstaller
{
    public class InstallerEngine : ManagementEngine
    {
        public override void Install(Guid modelKey, string connectionString)
        {
            this.Options = new DbContextOptionsBuilder<DbManagementContext>()
                .UseSqlServer(connectionString)
                .Options;

            this.RunScripts(System.Reflection.Assembly.GetExecutingAssembly());
            base.Install(modelKey, connectionString);
        }

        public override void ExecuteScript(string sql)
        {
            //TODO
        }
    }
}
