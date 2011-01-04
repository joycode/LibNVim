using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionOpenLineAbove : AbstractVimEditionRedoable
    {
        public EditionOpenLineAbove(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply()
        {
            // TODO ignore repeats now!
            this.Host.OpenLineAbove();

            Modes.ModeInsert mode = new Modes.ModeInsert(this.Host, this.Host.CurrentMode, this);
            this.Host.CurrentMode = mode;
            return true;
        }
    }
}
