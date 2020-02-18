using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Extensions.Scripting
{
    internal static class Extensions
    {
    }

    internal class StringHelper
    {
        #region String Match
        public static bool Match(object s1, string s2, bool ignoreCase)
        {
            if (s1 == null)
                if (s2 == null) return true;
                else return false;
            else
            if (s2 == null) return false;
            else if (s1.ToString().Length != s2.Length) return false;
            else if (s1.ToString().Length == 0) return true;

            return (String.Compare(s1.ToString(), s2, ignoreCase) == 0);
        }

        public static bool Match(string s1, string s2, bool ignoreCase)
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

        public static bool Match(string s1, string s2)
        {
            return Match(s1, s2, true);
        }
        #endregion

	}
}
