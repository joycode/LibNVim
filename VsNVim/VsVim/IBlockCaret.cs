using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;
using LibNVim;

namespace NVimVS.VsVim
{
    public interface IBlockCaret
    {
        ITextView TextView { get; }
        VimCaretShape CaretShape { get; set; }
        double CaretOpacity { get; set; }
        void Destroy();
    }
}
