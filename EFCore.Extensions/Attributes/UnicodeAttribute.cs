namespace EFCore.Extensions.Attributes
{
    /// <summary>
    /// Specifies that a string property be stored as unicode
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class UnicodeAttribute : System.Attribute
    {
        public UnicodeAttribute(bool isUnicode = true)
            : base()
        {
            this.IsUnicode = isUnicode;
        }

        public bool IsUnicode { get; protected set; }
    }
}
