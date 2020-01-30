namespace EFCore.Extensions.Attributes
{
    /// <summary>
    /// Marks an entity as a database static data holder.
    /// It must have no public properties that can be set.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class StaticDataAttribute : System.Attribute
    {
    }

    /// <summary>
    /// Marks an integer property as the key for the data item
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class StaticDataIdFieldAttribute : System.Attribute
    {
    }

    /// <summary>
    /// Marks a string property as the name for data item
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class StaticDataNameFieldAttribute : System.Attribute
    {
    }
}
