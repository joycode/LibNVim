using System;
using System.Collections.Generic;

using System.Text;
using LibNVim.Interfaces;

namespace LibNVim.Editions
{
    abstract class AbstractVimEditionRedoable : AbstractVimEdition, IVimEditionRedoable
    {
        protected AbstractVimEditionRedoable(IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public virtual bool Redo()
        {
            return this.Apply();
        }
    }
}
