namespace EFCore.Extensions.Attributes
{
    /// <summary>
    /// Indicates that a table has no primary key.
    /// This is required for tables that have no primary key defined.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class HasNoKeyAttribute : System.Attribute
    {
    }

}
