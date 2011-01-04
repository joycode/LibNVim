using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionInsertToLineStart : AbstractVimEditionRedoable
    {
        public EditionInsertToLineStart(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply()
        {
            this.Host.MoveToStartOfLineText();

            Modes.ModeInsert mode = new Modes.ModeInsert(this.Host, this.Host.CurrentMode, this);
            this.Host.CurrentMode = mode;
            return true;
        }
    }
}
