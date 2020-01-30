using EFCore.Extensions.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EFCore.Extensions
{
	/// <summary>
	/// A base entity with all of the audit attributes implemented
	/// </summary>
	public abstract class AuditableBaseEntity : BaseEntity
	{
		[ConcurrencyCheck]
		[Nullable(false)]
		protected byte[] Timestamp { get; set; }

		[AuditCreatedBy]
		[StringLength(50)]
		[Nullable(true)]
		protected string CreatedBy { get; set; }

		[AuditCreatedDate]
		[Nullable(false)]
		protected DateTime CreatedDate { get; set; }

		[AuditModifiedBy]
		[StringLength(50)]
		[Nullable(true)]
		protected string ModifiedBy { get; set; }

		[AuditModifiedDate]
		[Nullable(false)]
		protected DateTime ModifiedDate { get; set; }

	}
}
