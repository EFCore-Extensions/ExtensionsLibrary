using EFCore.Extensions;
using EFCore.Extensions.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTest.EFDAL.Entity
{
    /// <summary>
    /// A entity in a different schema
    /// </summary>
    [Table("SchemaTest", Schema = "MySchema")]
    public class SchemaTest : IEntity
    {
        [PrimaryKey]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; protected set; }

        [StringLength(50)]
        [Nullable(false)]
        public virtual string Name { get; set; }

    }
}