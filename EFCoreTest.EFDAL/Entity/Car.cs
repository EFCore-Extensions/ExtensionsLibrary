using EFCore.Extensions;
using EFCore.Extensions.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFCoreTest.EFDAL.Entity
{
    /// <summary>
    /// This entity inherits from 'ISoftDeleted' and as such has soft delete functionality baked in by convention
    /// </summary>
    public class Car : ISoftDeleted
    {
        [PrimaryKey]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int CarId { get; protected set; }

        [StringLength(50)]
        [Required]
        [DefaultValue("New Car")]
        [Unicode]
        public virtual string Name { get; set; }

        /// <summary>
        /// This is non-public so that it cannot accidently be set.
        /// Calling the 'Delete' method will reset this property as a safe-guard.
        /// This property can be public, but it is better managed as protected
        /// </summary>
        protected virtual bool IsDeleted { get; set; }

        /// <summary>
        /// This is only necessary if the actual property is not public
        /// </summary>
        bool ISoftDeleted.IsDeleted => this.IsDeleted;

        /// <summary>
        /// Marks this object as deleted to better protect the real "IsDeleted" property
        /// </summary>
        public void Delete()
        {
            this.IsDeleted = true;
        }

    }

}