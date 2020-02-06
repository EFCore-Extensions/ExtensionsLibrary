using EFCore.Extensions;
using EFCore.Extensions.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace EFCoreTest.EFDAL.Entity
{
    /// <summary>
    /// The PK is not an identity and must be set in code.
    /// </summary>
    public class CodeManagedKey : IEntity
    {
        [PrimaryKey]
        public int ID { get; set; }

        [MaxLength(50)]
        [Required]
        [Unicode]
        public string Name { get; set; }

        [MaxLengthUnbounded]
        [Nullable(true)]
        public string Data { get; set; }

        [Required]
        [VersionField]
        public int Version { get; protected set; }

        [ConcurrencyCheck]
        protected Guid Concurrency { get; set; }
    }
}