using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Extensions.Attributes
{
    public partial class ModelIdAttribute : System.Attribute
    {
        public ModelIdAttribute(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new Exception("Invalid model id");

            this.ModelId = id;
        }

        public virtual string ModelId { get; set; }
    }
}
