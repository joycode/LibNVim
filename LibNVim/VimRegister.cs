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
        public string Text { get; set; }
        public bool IsTextLines { get; set; }

        public VimRegister(string name)
        {
            this.Name = name;
        }
    }
}
