using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Extensions.Scripting
{
    public class Versioning
    {
        public Versioning()
        {
            this.Version = "0.0.0.0";
        }

        public string Version { get; set; }

        public void Increment()
        {
            if (string.IsNullOrEmpty(this.Version))
                throw new Exception("Invalid version format");

            var arr = this.Version
                .Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToList();

            if (arr.Count != 4)
                throw new Exception("Invalid version format");

            arr[3]++;
            this.Version = string.Join(".", arr);
        }
    }
}
