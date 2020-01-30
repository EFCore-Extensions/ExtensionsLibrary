namespace EFCore.Extensions.Attributes
{
    /// <summary>
    /// Specifies that the property default value is a database specific default like time or user.
    /// </summary>
    public class DefaultValueSpecialAttribute : System.ComponentModel.DefaultValueAttribute
    {
        public enum DefaultValueTypeConstants
        {
            CurrentTime = 1,
            CurrentTimeUTC = 2,
            DbUser = 3,
            AppName = 4,
        }

        public DefaultValueSpecialAttribute(DefaultValueTypeConstants type)
            : base(typeof(DefaultValueTypeConstants), type.ToString("d"))
        {
        }

    }
}