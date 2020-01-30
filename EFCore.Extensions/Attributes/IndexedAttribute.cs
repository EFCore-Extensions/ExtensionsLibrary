namespace EFCore.Extensions.Attributes
{
    /// <summary>
    /// Creates a database index on one or more properties.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class IndexedAttribute : System.Attribute
    {
        /// <summary>
        /// Determines if the index is created as unique
        /// </summary>
        public bool IsUnique { get; protected set; }

        /// <summary>
        /// The name is only needed if more than 1 property defines an index.
        /// This allows for multiple indexes to be created on a single entity using any set of columns.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The column order in which the key is created
        /// </summary>
        public int ColumnIndex { get; protected set; }

        /// <summary>
        /// Create an index on a single property
        /// </summary>
        public IndexedAttribute(bool isUnique = false)
        {
            this.IsUnique = isUnique;
        }

        /// <summary>
        /// Create an index on a set of fields.
        /// </summary>
        /// <param name="name">A grouping indentifier such that multiple fields can added to a named index group.</param>
        /// <param name="columnIndex">The property index in the group.</param>
        /// <param name="isUnique">Determines if the index is unique.</param>
        public IndexedAttribute(string name, int columnIndex, bool isUnique = false)
            : this()
        {
            if (string.IsNullOrEmpty(name?.Trim()))
                throw new System.Exception("The name is required");

            this.Name = name;
            this.ColumnIndex = columnIndex;
            this.IsUnique = IsUnique;
        }

    }
}
