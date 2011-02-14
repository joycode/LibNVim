using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Interfaces
{
    /// <summary>
    /// every behaviours in vim: cursor motions, editions, modes switch
    /// </summary>
    public interface IVimAction : IVimRepeatable
    {
    }
}
