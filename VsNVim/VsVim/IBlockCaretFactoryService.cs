using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace NVimVS.VsVim
{
    public interface IBlockCaretFactoryService
    {
        IBlockCaret CreateBlockCaret(IWpfTextView textView);
    }
}
