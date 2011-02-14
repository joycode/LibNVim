using System;
using System.Collections.Generic;

using System.Text;

namespace LibNVim.Motions
{
    class MotionGotoLeftBrace : AbstractVimMotion
    {
        public MotionGotoLeftBrace(Interfaces.IVimHost host, int repeat)
            : base(host, repeat)
        {
        }

        public override VimPoint Move(Interfaces.IVimHost host)
        {
            if (host.IsCurrentPositionAtStartOfDocument()) {
                return host.CurrentPosition;
            }

            VimPoint start_pos = host.CurrentPosition;
            VimPoint pos = null;

            for (int i = 0; i < this.Repeat; i++) {
                if (!host.FindLeftBrace(start_pos, out pos)) {
                    return host.CurrentPosition;
                }

                start_pos = pos;
            }

            host.GotoLine(pos.X);
            host.MoveCursor(pos);

            return host.CurrentPosition;
        }
    }
}
