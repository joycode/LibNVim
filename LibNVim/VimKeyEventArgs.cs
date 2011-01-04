using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace LibNVim
{
    public class VimKeyEventArgs
    {
        public VimKeyInput KeyInput { get; private set; }

        [DefaultValue(false)]
        public bool Handled { get; set; }

        public VimKeyEventArgs(VimKeyInput keyInput)
        {
            this.KeyInput = keyInput;
        }
    }
}
