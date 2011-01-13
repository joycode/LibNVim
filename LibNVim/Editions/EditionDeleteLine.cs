using System;
using System.Collections.Generic;

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
            if (this.Repeat == 1) {
                VimRegister.YankLineToDefaultRegister(this.Host, new VimSpan(this.Host.CurrentPosition, this.Host.CurrentPosition));
                this.Host.DeleteLine();
                return true;
            }

            VimPoint from = this.Host.CurrentPosition;
            int dst_line = Math.Min(from.X + this.Repeat - 1, this.Host.TextLineCount - 1);
            VimPoint to = new VimPoint(dst_line, 0);

            VimSpan span = new VimSpan(from, to);
            VimRegister.YankLineToDefaultRegister(this.Host, span);
            this.Host.DeleteLineRange(span);

            return true;
        }
    }
}
