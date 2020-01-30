namespace EFCore.Extensions.Attributes
{
    /// <summary>
    /// Marks an integer field as an auto-incremented version field.
    /// The field will be updated by 1 each time the the entity is updated.
    /// </summary>
    public class VersionFieldAttribute : System.Attribute
    {
    }
}
