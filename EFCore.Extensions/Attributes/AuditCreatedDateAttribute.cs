namespace EFCore.Extensions.Attributes
{
	/// <summary>
	/// Marks a string field to hold the CreatedDate audit information
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Property)]
	public partial class AuditCreatedDateAttribute : System.Attribute
	{
	}

}
