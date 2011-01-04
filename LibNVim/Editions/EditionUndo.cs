using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionUndo : AbstractVimEdition
    {
        public EditionUndo(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply()
        {
            this.Host.Undo();

            return true;
        }
    }
}
