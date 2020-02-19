using EFCore.Extensions;
using EFCore.Extensions.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTest.EFDAL.Entity
{
    /// <summary>
    /// A basic tenant table that is handled by the framework
    /// </summary>
    public class BasicTenant : ITenantEntity
    {
        [PrimaryKey]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; protected set; }

        [MaxLengthUnbounded]
        [Nullable(false)]
        public virtual string Name { get; set; }

        /// <summary>
        /// This field will automatically be hooked up as the tenant field. It is also hidden from the developer
        /// This field can be private/protected if desired
        /// </summary>
        public virtual string TenantId { get; protected set; }
    }
}