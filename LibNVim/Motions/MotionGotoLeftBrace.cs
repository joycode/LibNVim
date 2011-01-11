using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibNVim.Motions
{
    class MotionGotoLeftBrace : AbstractVimMotion
    {
        public MotionGotoLeftBrace(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move()
        {
            if (this.Host.IsCurrentPositionAtStartOfDocument()) {
                return this.Host.CurrentPosition;
            }

            VimPoint start_pos = this.Host.CurrentPosition;
            VimPoint pos = null;

            for (int i = 0; i < this.Repeat; i++) {
                if (!this.Host.FindLeftBrace(start_pos, out pos)) {
                    return this.Host.CurrentPosition;
                }

                start_pos = pos;
            }

            this.Host.MoveCursor(pos);
            this.Host.GotoLine(pos.X);

            return this.Host.CurrentPosition;
        }
    }
}
