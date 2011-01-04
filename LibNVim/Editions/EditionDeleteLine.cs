using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionDeleteLine : AbstractVimEditionRedoable
    {
        public EditionDeleteLine(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply()
        {
            int dst_line = Math.Min(this.Host.CurrentPosition.X + this.Repeat - 1,
                    this.Host.TextLineCount - 1);
            this.Host.DeleteLineRange(this.Host.CurrentPosition, new VimPoint(dst_line, 0));

            return true;
        }
    }
}
