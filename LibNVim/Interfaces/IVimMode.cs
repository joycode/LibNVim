using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Interfaces
{
    public interface IVimMode
    {
        IVimHost Host { get; }
        VimCaretShape CaretShape { get; }

        void KeyDown(VimKeyEventArgs args);
    }
}
