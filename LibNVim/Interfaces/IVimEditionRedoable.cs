using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Interfaces
{
    public interface IVimEditionRedoable : IVimEdititon
    {
        /// <summary>
        /// Editon should be redoable cross Vim Host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        bool Redo(IVimHost host);
    }
}
