using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionChangeLine : AbstractVimEditionRedoable
    {
        public EditionChangeLine(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply()
        {
            int dst_line = Math.Min(this.Host.CurrentPosition.X + this.Repeat - 1,
                    this.Host.TextLineCount - 1);
            this.Host.DeleteLineRange(this.Host.CurrentPosition, new VimPoint(dst_line, 0));

            this.Host.OpenLineAbove();

            Modes.ModeInsert mode = new Modes.ModeInsert(this.Host, this.Host.CurrentMode, this);
            this.Host.CurrentMode = mode;
            return true;
        }
    }
}
