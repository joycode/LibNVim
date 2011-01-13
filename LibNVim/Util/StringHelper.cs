using System;
using System.Collections.Generic;
using System.Text;

namespace LibNVim.Util
{
    class StringHelper
    {
        private readonly static List<char> White_Space_Chars = new List<char>() { ' ', '\t', '\r', '\n' };

        public static bool IsNullOrWhiteSpace(string str)
        {
            if (str == null) {
                return true;
            }

            if (str.Length == 0) {
                return true;
            }

            foreach (char ch in str) {
                if (!White_Space_Chars.Contains(ch)) {
                    return false;
                }
            }

            return true;
        }
    }
}
