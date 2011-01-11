using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Interfaces
{
    /// <summary>
    /// range edition operates on open end spans
    /// </summary>
    interface IVimRangeEdition
    {
        IVimMotion Motion { get; }
    }
}
