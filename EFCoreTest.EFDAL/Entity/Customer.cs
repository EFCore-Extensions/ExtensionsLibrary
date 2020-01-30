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
    /// This entity has all fields manually added
    /// The tenant table mapping is handled manually
    /// </summary>
    public class Customer : ITenantEntity
    {
        /// <summary>
        /// This is the primary key
        /// </summary>
        [Description("Some Description")]
        [PrimaryKey]
        public virtual Guid CustomerId { get; set; }

        [StringLengthUnbounded]
        [Nullable(false)]
        [Column("Name")]
        [MaxLength(50)]
        public virtual string Name1 { get; set; }

        public virtual List<Order> OrderList { get; set; }

        [Required]
        public virtual int CustomerTypeId { get; set; }

        [NotMapped]
        public CustomerTypeConstants CustomerTypeValue
        {
            get { return (CustomerTypeConstants)this.CustomerTypeId; }
            set { this.CustomerTypeId = (int)value; }
        }

        public virtual CustomerType CustomerType { get; set; }

        /// <summary>
        /// This is hidden from public view in this example and as such needs an explicit interface declaration below
        /// </summary>
        protected virtual string TenantId { get; set; }

        //This is used so that the field can be protected
        string ITenantEntity.TenantId { get => this.TenantId; }

        //All of these audit fields can be added manaually like this
        //Or the entity can inherit from 'AuditableBaseEntity'

        [AuditCreatedBy]
        public virtual string CreatedBy { get; protected set; }

        [AuditCreatedDate]
        public virtual DateTime CreatedDate { get; protected set; }

        [AuditModifiedBy]
        public virtual string ModifiedBy { get; protected set; }

        [AuditModifiedDate]
        public virtual DateTime ModifiedDate { get; protected set; }

        /// <summary>
        /// This can be public but there is most likely no reason for a user to see or manipulate it.
        /// </summary>
        [ConcurrencyCheck]
        protected virtual byte[] Timestamp { get; set; }

        public override string ToString()
        {
            return this.Name1;
        }

    }

}