using EFCore.Extensions;
using EFCore.Extensions.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTest.EFDAL.Entity
{
    /// <summary>
    /// This is a static data entity with database backing.
    /// It is mapped to an enumeration by convention "CustomerTypeConstants".
    /// </summary>
    [ImmutableObject(true)]
    [StaticData]
    public class CustomerType : IEntity
    {
        [PrimaryKey]
        [StaticDataIdField]
        public int ID { get; protected set; }

        [NotMapped]
        public CustomerTypeConstants CustomerTypeValue { get { return (CustomerTypeConstants)this.ID; } }

        [StaticDataNameField]
        [MaxLength(50)]
        public string Name { get; protected set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}