using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Editions
{
    class EditionJoinLine : AbstractVimEditionRedoable
    {
        public EditionJoinLine(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override bool Apply(Interfaces.IVimHost host)
        {
            if (host.IsCurrentPositionAtLastLine()) {
                return true;
            }

            int begin_line = host.CurrentPosition.X;
            int end_line = Math.Min(begin_line + this.Repeat, host.TextLineCount - 1);
            
            host.JoinLines(begin_line, end_line);

            return true;
        }
    }
}
