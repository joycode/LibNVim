using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Interfaces
{
    public interface IVimMode
    {
        IVimHost Host { get; }
        VimCaretShape CaretShape { get; }

        bool CanProcess(VimKeyInput keyInput);
        void KeyDown(VimKeyEventArgs args);
    }
}
