using System;
using System.Collections.Generic;

using System.Text;
using LibNVim.Interfaces;

namespace LibNVim
{
    class VimGlobalInfo
    {
        public static IVimEditionRedoable LastEdition = null;
        public static VimFindWordRecord FindWordRecord = null;
    }
}
