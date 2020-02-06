using EFCore.Extensions;
using EFCore.Extensions.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EFCoreTest.EFDAL.Entity
{
    [HasNoKey]
    public class HeapTable : IEntity
    {
        [Required]
        public int ID { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }
    }
}