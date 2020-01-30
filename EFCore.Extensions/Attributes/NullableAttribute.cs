namespace EFCore.Extensions.Attributes
{
    /// <summary>
    /// This is a recipricoll attribute to the IsRequired. 
    /// It can be used on every property as it can be toggled true/false unlike IsRequired
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class NullableAttribute : System.Attribute
    {
        public NullableAttribute()
            : base()
        {
            this.AllowNull = true;
        }

        public NullableAttribute(bool allowNull = true)
            : this()
        {
            this.AllowNull = allowNull;
        }

        public bool AllowNull { get; protected set; }
    }

}
