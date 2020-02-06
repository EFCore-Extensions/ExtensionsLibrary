using EFCore.Extensions;
using EFCore.Extensions.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTest.EFDAL.Entity
{
    /// <summary>
    /// This entity is inherited from the base entity which provides some base functionality and audit fields.
    /// </summary>
    public class Category : BaseEntity
    {
        [PrimaryKey]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int CategoryId { get; protected set; }

        [Required]
        [MaxLength(50)]
        [ModelId("aytwgsgw6")]
        public virtual string Name { get; set; }

    }

}