using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Editions
{
    abstract class AbstractVimEditionRedoable : AbstractVimEdition, IVimEditionRedoable
    {
        public AbstractVimEditionRedoable(IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

    }
}
