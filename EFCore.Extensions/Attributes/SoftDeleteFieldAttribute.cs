namespace EFCore.Extensions.Attributes
{
    /// <summary>
    /// This defines the field to be used as the soft delete discriminator property.
    /// The default is "IsDeleted" and will be handled by convention if the entity implements the ISoftDelete.
    /// This attribute can be used to override the default proeprty and specify a custom column.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class SoftDeleteFieldAttribute : System.Attribute
    {
    }

}
