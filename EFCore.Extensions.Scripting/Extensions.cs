using System;

namespace EFCore.Extensions.Scripting
{
    internal class StringHelper
    {
        public static bool Match(string s1, string s2, bool ignoreCase = true)
        {
            if (s1 == null)
                if (s2 == null) return true;
                else return false;
            else
            if (s2 == null) return false;
            else if (s1.Length != s2.Length) return false;
            else if (s1.Length == 0) return true;

            return (String.Compare(s1, s2, ignoreCase) == 0);
        }
	}
}
