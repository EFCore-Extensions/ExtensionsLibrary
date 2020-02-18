using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EFCore.DbManagement
{
    public abstract class ManagementEngine
    {
        protected ManagementEngine(System.Reflection.Assembly assembly)
        {
            this.Assembly = assembly;
        }

        protected System.Reflection.Assembly Assembly { get; set; }
        protected string ConnectionString { get; private set; } = null;

        public virtual void Install(Guid modelKey, string connectionString)
        {
            if (this.Options == null)
                throw new Exception("The 'Options' parameter cannot be null.");

            this.ConnectionString = connectionString;

            //Ensure that there is a database
            using (var context = new DbManagementContext(this.Options))
            {
                context.Database.EnsureCreated();
            }

            //Run all scripts
            this.RunScripts();

            //Update management tables
            using (var context = new DbManagementContext(this.Options))
            {
                var version = context.VersionModel.FirstOrDefault(x => x.ModelKey == modelKey);
                if (version == null)
                {
                    version = new VersionModel {ModelKey = modelKey};
                    context.Add(version);
                }

                version.LastUpdated = DateTime.Now;
                version.Version = "0.0.0.0";
                context.SaveChanges();
            }
        }

        protected virtual void RunScripts()
        {
            var scripts = GetScripts(this.Assembly);
            foreach (var resourceFileName in scripts)
            {
                var sql = string.Empty;
                var manifestStream = this.Assembly.GetManifestResourceStream(resourceFileName);
                using (var sr = new System.IO.StreamReader(manifestStream))
                {
                    sql = sr.ReadToEnd();
                }

                this.ExecuteScript(sql);
            }
        }

        protected DbContextOptions<DbManagementContext> Options { get; set; }

        private IEnumerable<string> GetScripts(System.Reflection.Assembly assem)
        {
            var retval = new List<string>();
            var resourceNames = assem.GetManifestResourceNames();
            foreach (var resourceName in resourceNames.Where(x => x.EndsWith(".sql")))
            {
                retval.Add(resourceName);
            }
            return retval;
        }

        private void ExecuteScript(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                return;

            //Break into lines
            var lines = BreakLines(sql);
            var blocks = new List<string>();
            var lastBlock = new List<string>();
            foreach (var ss in lines)
            {
                if (ss == this.BreakLine)
                {
                    blocks.Add(string.Join("\r\n", lastBlock));
                    lastBlock.Clear();
                }
                else
                    lastBlock.Add(ss);
            }

            blocks.RemoveAll(x => string.IsNullOrEmpty(x));

            foreach (var line in blocks)
            {
                using (var context = new DbManagementContext(this.Options))
                {
                    using (var command = context.Database.GetDbConnection().CreateCommand())
                    {
                        command.CommandText = line;
                        context.Database.OpenConnection();
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        protected abstract string BreakLine { get; }

        private static List<string> BreakLines(string text)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();
            return text.Split(new[] {"\r\n"}, StringSplitOptions.None).ToList();
        }
    }

    public class DbManagementContext : DbContext
    {
        public DbSet<VersionModel> VersionModel { get; protected set; }
        public DbSet<VersionObject> VersionObject { get; protected set; }

        public DbManagementContext(DbContextOptions<DbManagementContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<VersionModel>().ToTable("__versionModel");
            modelBuilder.Entity<VersionObject>().ToTable("__versionObject");
        }
    }

    public class VersionModel
    {
        [Required] [MaxLength(50)] public string Version { get; set; }
        [Required] public DateTime LastUpdated { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ModelKey { get; set; }
    }

    public class VersionObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RowId { get; set; }

        [Required] public Guid ID { get; set; }
        [Required] [MaxLength(50)] public string Name { get; set; }
        [Required] [MaxLength(50)] public string Type { get; set; }
        [Required] [MaxLength(50)] public string Schema { get; set; }
        [Required] public DateTime CreatedDate { get; set; }
        [Required] public DateTime ModifiedDate { get; set; }
        [Required] [MaxLength(50)] public string Hash { get; set; }
        [Required] [MaxLength(50)] public string Status { get; set; }
        [Required] public Guid ModelKey { get; set; }
    }
}