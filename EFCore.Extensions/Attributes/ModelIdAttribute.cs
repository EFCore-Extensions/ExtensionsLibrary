using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Extensions.Attributes
{
    /// <summary>
    /// Uniquely identifier a property for migration tracking
    /// </summary>
    public partial class ModelIdAttribute : System.Attribute
    {
        public ModelIdAttribute(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new Exception("Invalid model id");

            this.ModelId = id;
        }

        /// <summary>
        /// Any unique non-empty string that identifies a field across the entire model context
        /// </summary>
        public virtual string ModelId { get; }
    }
}
