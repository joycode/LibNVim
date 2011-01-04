using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim
{
    public class VimKeyInput
    {
        public static string Escape = "Esc";

        public string Value { get; private set; }

        public VimKeyInput(string value)
        {
            this.Value = value;
        }
    }
}
