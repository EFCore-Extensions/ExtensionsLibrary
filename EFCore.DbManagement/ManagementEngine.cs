using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace EFCore.DbManagement
{
    public abstract class ManagementEngine
    {
        public virtual void Install(Guid modelKey, string connectionString)
        {
            if (this.Options == null)
                throw new Exception("The 'Options' parameter cannot be null.");

            using (var context = new DbManagementContext(this.Options))
            {
                context.Database.EnsureCreated();

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

        protected virtual void RunScripts(System.Reflection.Assembly assem)
        {
            var scripts = GetScripts(assem);
            foreach (var resourceFileName in scripts)
            {
                var sql = string.Empty;
                var manifestStream = assem.GetManifestResourceStream(resourceFileName);
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
            foreach (var resourceName in resourceNames.Where(x=>x.EndsWith(".sql")))
            {
                retval.Add(resourceName);
            }
            return retval;
        }

        public abstract void ExecuteScript(string sql);
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
        [Required]
        [MaxLength(50)]
        public string Version { get; set; }
        [Required]
        public DateTime LastUpdated { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ModelKey { get; set; }
    }

    public class VersionObject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RowId { get; set; }
        [Required]
        public Guid ID { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [Required]
        [MaxLength(50)]
        public string Type { get; set; }
        [Required]
        [MaxLength(50)]
        public string Schema { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public DateTime ModifiedDate { get; set; }
        [Required]
        [MaxLength(50)]
        public string Hash { get; set; }
        [Required]
        [MaxLength(50)]
        public string Status { get; set; }
        [Required]
        public Guid ModelKey { get; set; }
    }
}