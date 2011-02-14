using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Editions
{
    class EditionFormatLine : AbstractVimEditionRedoable
    {
        public EditionFormatLine(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply(Interfaces.IVimHost host)
        {
            if (this.Repeat == 1) {
                host.FormatLine();
            }
            else {
                VimPoint from = host.CurrentPosition;
                int dst_line = Math.Min(from.X + this.Repeat - 1, host.TextLineCount - 1);
                VimPoint to = new VimPoint(dst_line, 0);

                host.FormatLineRange(new VimSpan(from, to));
            }

            return true;
        }
    }
}
