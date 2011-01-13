using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Interfaces
{
    public interface IVimEditionRedoable : IVimEdititon
    {
        bool Redo();
    }
}
