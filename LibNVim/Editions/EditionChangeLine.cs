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

        protected override void OnBeforeInsert()
        {
            if (this.Repeat == 1) {
                VimRegister.YankLineToDefaultRegister(this.Host, new VimSpan(this.Host.CurrentPosition, this.Host.CurrentPosition));
                this.Host.DeleteLine();
                this.Host.OpenLineAbove();
            }
            else {
                VimPoint from = this.Host.CurrentPosition;
                int dst_line = Math.Min(from.X + this.Repeat - 1, this.Host.TextLineCount - 1);
                VimPoint to = new VimPoint(dst_line, 0);

                VimSpan span = new VimSpan(from, to);
                VimRegister.YankLineToDefaultRegister(this.Host, span);
                this.Host.DeleteLineRange(span);

                // here will result in a annoying thing, you need to undo 2 times to undo this edition
                // not worth to fix it, too complex for this single one thing to do a big change
                this.Host.OpenLineAbove();
            }
        }
    }
}
