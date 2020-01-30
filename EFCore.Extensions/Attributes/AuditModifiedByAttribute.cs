namespace EFCore.Extensions.Attributes
{
	/// <summary>
	/// Marks a string field to hold the ModifiedBy audit information
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Property)]
	public partial class AuditModifiedByAttribute : System.Attribute
	{
	}

}
