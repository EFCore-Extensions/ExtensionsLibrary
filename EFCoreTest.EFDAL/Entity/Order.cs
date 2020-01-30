using EFCore.Extensions;
using EFCore.Extensions.Attributes;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EFCoreTest.EFDAL.Entity
{
    /// <summary>
    /// This entity has its audit fields inherited from the AuditableBaseEntity
    /// The tenant table mapping is handled manually
    /// </summary>
    public class Order : AuditableBaseEntity, ITenantEntity
    {
        /// <summary>
        /// PK field is oddly named
        /// </summary>
        [PrimaryKey]
        public virtual Guid OrderId5 { get; protected set; }

        /// <summary>
        /// This property has the base event handling functionality implemented so a developer can cancel the set if desired
        /// </summary>
        [DefaultValue(5)]
        public int Quantity
        {
            get { return _Quantity; }
            set
            {
                if (value == _Quantity) return;
                var eventArg = new ChangingEventArgs<int>(value, nameof(this.Quantity));
                this.OnPropertyChanging(eventArg);
                if (eventArg.Cancel) return;
                _Quantity = eventArg.Value;
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Quantity)));
            }
        }
        protected int _Quantity = 0;

        [Required]
        [DefaultValueSpecial(DefaultValueSpecialAttribute.DefaultValueTypeConstants.CurrentTime)]
        public DateTime DefaultedTime { get; protected set; }

        public virtual Guid CustomerFkId { get; set; }

        public virtual Customer Customer { get; set; }

        protected virtual string TenantId { get; set; }

        string ITenantEntity.TenantId { get => this.TenantId; }
    }
}
