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

        public override bool Apply(Interfaces.IVimHost host)
        {
            if (this.Repeat == 1) {
                VimRegister.YankLineToDefaultRegister(host, new VimSpan(host.CurrentPosition, host.CurrentPosition));
                host.DeleteLine();
                return true;
            }

            VimPoint from = host.CurrentPosition;
            int dst_line = Math.Min(from.X + this.Repeat - 1, host.TextLineCount - 1);
            VimPoint to = new VimPoint(dst_line, 0);

            VimSpan span = new VimSpan(from, to);
            VimRegister.YankLineToDefaultRegister(host, span);
            host.DeleteLineRange(span);

            return true;
        }
    }
}
