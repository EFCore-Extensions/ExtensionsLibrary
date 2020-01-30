namespace EFCore.Extensions.Attributes
{
	/// <summary>
	/// Marks a string field to hold the CreateBy audit information
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Property)]
	public partial class AuditCreatedByAttribute : System.Attribute
	{
	}

}
