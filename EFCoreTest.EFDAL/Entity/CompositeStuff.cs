using EFCore.Extensions;
using EFCore.Extensions.Attributes;
using System.ComponentModel.DataAnnotations;

namespace EFCoreTest.EFDAL.Entity
{
    /// <summary>
    /// This entity has a composite primary key with 2 fields
    /// This entity has a composite index with 2 fields
    /// </summary>
    public class CompositeStuff : IEntity
    {
        [PrimaryKey(2)]
        public int ID1 { get; protected set; }

        [PrimaryKey(1)]
        public int ID2 { get; protected set; }

        [MaxLength(50)]
        [Indexed("I1", 2)]
        public string Name1 { get; set; }

        [MaxLength(50)]
        [Indexed("I1", 1)]
        public string Name2 { get; set; }

        [MaxLength(50)]
        public string Name3 { get; set; }
    }
}