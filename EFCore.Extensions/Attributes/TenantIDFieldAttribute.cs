namespace EFCore.Extensions.Attributes
{
    /// <summary>
    /// Identifies the tenant discriminator property.
    /// The default is "TenantId" and will be handled by convention if the entity implements ITenantTable.
    /// This attribute can be used to override the default proeprty and specify a custom column.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class TenantIDFieldAttribute : System.Attribute
    {
    }
}
