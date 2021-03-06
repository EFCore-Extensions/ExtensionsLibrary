﻿using System;
using System.Linq;

namespace EFCore.Extensions.Scripting
{
    public class Versioning
    {
        public Versioning() { }

        public Versioning(string version)
            : this()
        {
            this.Version = version;
        }

        public DateTime LastGeneration => DateTime.Now;
        public string Version { get; protected set; } = "0.0.0.0";
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

        public virtual string GetDiffFileName()
        {
            var arr = this.Version
                .Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Convert.ToInt32(x))
                .ToList();

            if (arr.Count != 4)
                throw new Exception("Invalid version format");

            return arr[0].ToString("#######000") + "." +
                arr[1].ToString("#######000") + "." +
                arr[2].ToString("#######000") + "." +
                arr[3].ToString("#####00000");
        }

        public override string ToString() => this.Version;
    }
}
