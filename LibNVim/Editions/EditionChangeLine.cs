using System;
using System.Collections.Generic;
using System.Linq;
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
            int dst_line = Math.Min(this.Host.CurrentPosition.X + this.Repeat - 1,
                    this.Host.TextLineCount - 1);
            this.Host.DeleteLineRange(this.Host.CurrentPosition, new VimPoint(dst_line, 0));

            this.Host.OpenLineAbove();
        }
    }
}
