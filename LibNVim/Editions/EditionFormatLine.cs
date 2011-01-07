using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Editions
{
    class EditionFormatLine : AbstractVimEditionRedoable
    {
        public EditionFormatLine(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply()
        {
            if (this.Repeat == 1) {
                this.Host.FormatLine();
            }
            else {
                VimPoint from = this.Host.CurrentPosition;
                int dst_line = Math.Min(from.X + this.Repeat - 1, this.Host.TextLineCount - 1);
                VimPoint to = new VimPoint(dst_line, 0);

                this.Host.FormatLineRange(new VimSpan(from, to));
            }

            return true;
        }
    }
}
