using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Editions
{
    class EditionChangeLine : AbstractVimEditionInsertText
    {
        public EditionChangeLine(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        protected override void OnBeforeInsert(Interfaces.IVimHost host)
        {
            if (this.Repeat == 1) {
                VimRegister.YankLineToDefaultRegister(host, new VimSpan(host.CurrentPosition, host.CurrentPosition));
                host.DeleteLine();
                host.OpenLineAbove();
            }
            else {
                VimPoint from = host.CurrentPosition;
                int dst_line = Math.Min(from.X + this.Repeat - 1, host.TextLineCount - 1);
                VimPoint to = new VimPoint(dst_line, 0);

                VimSpan span = new VimSpan(from, to);
                VimRegister.YankLineToDefaultRegister(host, span);
                host.DeleteLineRange(span);

                // here will result in a annoying thing, you need to undo 2 times to undo this edition
                // not worth to fix it, too complex for this single one thing to do a big change
                host.OpenLineAbove();
            }
        }
    }
}
