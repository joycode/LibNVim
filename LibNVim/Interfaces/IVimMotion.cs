using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Interfaces
{
    interface IVimMotion : IVimAction
    {
        /// <summary>
        /// moving in specified vim host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        VimPoint Move(IVimHost host);
    }
}
