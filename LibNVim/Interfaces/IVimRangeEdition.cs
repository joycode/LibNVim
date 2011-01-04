using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Interfaces
{
    interface IVimRangeEdition : IVimEditionRedoable
    {
        IVimMotion Motion { get; }
    }
}
