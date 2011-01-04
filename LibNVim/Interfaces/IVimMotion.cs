using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Interfaces
{
    interface IVimMotion : IVimAction
    {
        /// <summary>
        /// motion move
        /// </summary>
        /// <returns></returns>
        VimPoint Move();
    }
}
