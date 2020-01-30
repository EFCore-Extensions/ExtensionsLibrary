namespace EFCore.Extensions.Attributes
{
	/// <summary>
	/// Marks a string field to hold the ModifiedDate audit information
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Property)]
	public partial class AuditModifiedDateAttribute : System.Attribute
	{
	}
}
