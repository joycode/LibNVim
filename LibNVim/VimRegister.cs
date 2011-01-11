using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim
{
    public class VimRegister
    {
        public static VimRegister DefaultRegister = new VimRegister("");

        public string Name { get; private set; }
        public string Text { get; private set; }
        public bool IsTextLines { get; private set; }

        public VimRegister(string name)
        {
            this.Name = name;
        }

        public void Remember(string text, bool isTextLines)
        {
            this.Text = text;
            this.IsTextLines = isTextLines;
        }
    }
}
