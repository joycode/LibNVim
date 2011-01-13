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

        public override bool Apply()
        {
            if (this.Host.IsCurrentPositionAtLastLine()) {
                return true;
            }

            int begin_line = this.Host.CurrentPosition.X;
            int end_line = Math.Min(begin_line + this.Repeat, this.Host.TextLineCount - 1);
            
            this.Host.JoinLines(begin_line, end_line);

            return true;
        }
    }
}
