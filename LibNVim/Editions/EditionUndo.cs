using System;
using System.Collections.Generic;

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
            for (int i = 0; i < this.Repeat; i++) {
                this.Host.Undo();
            }

            return true;
        }
    }
}
