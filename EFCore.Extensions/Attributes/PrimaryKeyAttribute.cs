namespace EFCore.Extensions.Attributes
{
    /// <summary>
    /// Creates a multi-column primary key
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class PrimaryKeyAttribute : System.Attribute
    {
        public PrimaryKeyAttribute()
        {
            this.Clustered = true;
        }

        public PrimaryKeyAttribute(int columnIndex)
            : this()
        {
            this.ColumnIndex = columnIndex;
        }

        public PrimaryKeyAttribute(bool clustered, int columnIndex)
            : this(columnIndex)
        {
            this.Clustered = clustered;
        }

        public int ColumnIndex { get; protected set; }

        public bool Clustered { get; protected set; }
    }
}
